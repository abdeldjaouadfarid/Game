using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    /// <summary>
    /// Owns one survival run: the player, the horde, projectiles, pickups, particles and the
    /// systems that drive them. Also exposes the small API the weapons call into.
    /// </summary>
    public class World
    {
        public readonly Player Player = new Player();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<Projectile> Projectiles = new List<Projectile>();
        public readonly List<XpGem> Gems = new List<XpGem>();
        public readonly List<Particle> Particles = new List<Particle>();
        public readonly Spawner Spawner = new Spawner();
        public Camera2D Camera;

        public float Time;
        public int Kills;
        public int PendingLevelUps;

        // companion / boss progression
        public Companion Companion;
        public Enemy Boss;
        bool _companionJoined;
        bool _bossTriggered;

        // on-screen event banner
        public string BannerText = "";
        public float BannerTimer;

        const int CompanionLevel = 5;
        const int BossLevel = 10;

        // debug/tuning: HERETICIDE_XPSCALE multiplies XP gain, HERETICIDE_BOSSHP scales boss HP (default 1)
        float _xpScale = 1f;
        float _bossHpScale = 1f;

        readonly Stack<Enemy> _enemyPool = new Stack<Enemy>();
        readonly Random Rng = new Random();

        // transient visual effects
        class Blast { public Vector2 Pos; public float Radius; public float Life; public float MaxLife; }
        class Slash { public Vector2 Pos; public float Radius; public int Facing; public float Life; public float MaxLife; }
        readonly List<Blast> _blasts = new List<Blast>();
        readonly List<Slash> _slashes = new List<Slash>();

        // scratch buffers for nearest-enemy queries (reused, read immediately)
        readonly List<Enemy> _near = new List<Enemy>();
        readonly List<float> _nearD = new List<float>();

        public void Start(Camera2D cam)
        {
            Camera = cam;
            Player.Reset(Vector2.Zero);
            Player.Weapons.Add(new BolterWeapon());
            Player.Weapons.Add(new ChainswordWeapon());
            Enemies.Clear();
            Projectiles.Clear();
            Gems.Clear();
            Particles.Clear();
            _blasts.Clear();
            _slashes.Clear();
            _enemyPool.Clear();
            Spawner.Reset();
            Time = 0f;
            Kills = 0;
            PendingLevelUps = 0;
            Companion = null;
            Boss = null;
            _companionJoined = false;
            _bossTriggered = false;
            BannerText = "";
            BannerTimer = 0f;

            _xpScale = ReadFloatEnv("HERETICIDE_XPSCALE", 1f);
            _bossHpScale = ReadFloatEnv("HERETICIDE_BOSSHP", 1f);
        }

        static float ReadFloatEnv(string name, float fallback)
        {
            var s = Environment.GetEnvironmentVariable(name);
            float v;
            if (!string.IsNullOrEmpty(s) &&
                float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) && v > 0f)
                return v;
            return fallback;
        }

        public Enemy GetPooledEnemy() => _enemyPool.Count > 0 ? _enemyPool.Pop() : new Enemy();

        // ----------------------------------------------------------------- update
        public void Update(float dt, Vector2 moveDir)
        {
            Time += dt;
            Player.Update(dt, moveDir);
            Spawner.Update(this, dt);

            for (int i = 0; i < Player.Weapons.Count; i++)
                Player.Weapons[i].Update(this, dt);

            UpdateProgression(dt);

            UpdateEnemies(dt);
            UpdateBoss(dt);
            UpdateProjectiles(dt);
            UpdateGems(dt);
            UpdateParticles(dt);
            UpdateEffects(dt);
            SweepEnemies();
        }

        void UpdateEnemies(float dt)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                e.Update(dt, Player.Position);

                float sumR = e.Radius + Player.Radius;
                float d2 = Vector2.DistanceSquared(e.Position, Player.Position);
                if (d2 < sumR * sumR)
                {
                    float dist = MathF.Sqrt(d2);
                    Vector2 n = dist > 0.01f ? (e.Position - Player.Position) / dist : new Vector2(1, 0);
                    float overlap = sumR - dist;
                    if (overlap > 0f) e.Position += n * overlap * 0.5f; // soft separation

                    if (Player.Alive && e.ContactCooldown <= 0f)
                    {
                        Player.TakeDamage(e.Damage);
                        e.ContactCooldown = 0.55f;
                        Camera.AddShake(0.7f);
                    }
                }
            }
        }

        void UpdateProjectiles(float dt)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.Update(dt);

                if (p.Alive && p.Hostile)
                {
                    float rr = p.Radius + Player.Radius;
                    if (Vector2.DistanceSquared(p.Position, Player.Position) <= rr * rr)
                    {
                        if (Player.Alive) Player.TakeDamage(p.Damage);
                        SpawnHit(p.Position, new Color(190, 110, 230));
                        p.Alive = false;
                    }
                }
                else if (p.Alive && !p.IsAoe)
                {
                    for (int j = 0; j < Enemies.Count; j++)
                    {
                        var e = Enemies[j];
                        if (!e.Alive || p.AlreadyHit.Contains(e)) continue;
                        float rr = p.Radius + e.Radius;
                        if (Vector2.DistanceSquared(p.Position, e.Position) <= rr * rr)
                        {
                            Vector2 kd = p.Velocity;
                            if (kd != Vector2.Zero) kd.Normalize();
                            e.Hurt(p.Damage, kd, p.Knockback);
                            SpawnHit(p.Position, new Color(255, 220, 120));
                            p.AlreadyHit.Add(e);
                            if (p.Pierce <= 0) { p.Alive = false; break; }
                            p.Pierce--;
                        }
                    }
                }

                if (!p.Alive)
                {
                    if (p.IsAoe) SpawnExplosion(p.Position, p.AoeRadius, p.AoeDamage);
                    Projectiles[i] = Projectiles[Projectiles.Count - 1];
                    Projectiles.RemoveAt(Projectiles.Count - 1);
                }
            }
        }

        void UpdateGems(float dt)
        {
            float pickR2 = Player.PickupRadius * Player.PickupRadius;
            for (int i = Gems.Count - 1; i >= 0; i--)
            {
                var g = Gems[i];
                g.Bob += dt * 4f;
                Vector2 toP = Player.Position - g.Position;
                float d2 = toP.LengthSquared();

                if (Player.MagnetPulse > 0f || d2 < pickR2) g.Attracted = true;

                if (g.Attracted)
                {
                    float d = MathF.Sqrt(d2);
                    Vector2 dir = d > 0.01f ? toP / d : Vector2.Zero;
                    float pull = MathHelper.Lerp(260f, 90f, MathHelper.Clamp(d / Player.PickupRadius, 0f, 1f));
                    g.Position += dir * pull * dt;
                }

                if (d2 < (Player.Radius + 5f) * (Player.Radius + 5f))
                {
                    AddXp(g.Value);
                    AddParticle(g.Position, Vector2.Zero, 0.25f, 6f, 0f,
                        new Color(150, 255, 180), new Color(150, 255, 180) * 0f, Art.Circle, 0.6f);
                    g.Alive = false;
                    Gems[i] = Gems[Gems.Count - 1];
                    Gems.RemoveAt(Gems.Count - 1);
                }
            }
        }

        void UpdateParticles(float dt)
        {
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                var p = Particles[i];
                p.Update(dt);
                if (!p.Alive)
                {
                    Particles[i] = Particles[Particles.Count - 1];
                    Particles.RemoveAt(Particles.Count - 1);
                }
            }
        }

        void UpdateEffects(float dt)
        {
            for (int i = _blasts.Count - 1; i >= 0; i--)
            {
                _blasts[i].Life -= dt;
                if (_blasts[i].Life <= 0f) { _blasts[i] = _blasts[_blasts.Count - 1]; _blasts.RemoveAt(_blasts.Count - 1); }
            }
            for (int i = _slashes.Count - 1; i >= 0; i--)
            {
                _slashes[i].Life -= dt;
                if (_slashes[i].Life <= 0f) { _slashes[i] = _slashes[_slashes.Count - 1]; _slashes.RemoveAt(_slashes.Count - 1); }
            }
        }

        void SweepEnemies()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                var e = Enemies[i];
                if (e.Alive) continue;
                OnEnemyDeath(e);
                Enemies[i] = Enemies[Enemies.Count - 1];
                Enemies.RemoveAt(Enemies.Count - 1);
                _enemyPool.Push(e);
            }
        }

        void OnEnemyDeath(Enemy e)
        {
            Kills++;
            Gems.Add(new XpGem(e.Position, e.XpValue));

            if (e.IsBoss)
            {
                Boss = null;
                Banner("THE TRAITOR IS PURGED - SHE IS REDEEMED");
                LogEvent("[EVENT] Boss purged at level " + Player.Level);
                Camera.AddShake(7f);
                Player.Hp = Math.Min(Player.MaxHp, Player.Hp + Player.MaxHp * 0.4f);
                for (int i = 0; i < 14; i++)
                {
                    float ang = (float)(Rng.NextDouble() * MathHelper.TwoPi);
                    float r = (float)Rng.NextDouble() * 30f;
                    Gems.Add(new XpGem(e.Position + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r, 6));
                }
                // the redeemed Sister rejoins the marine
                Companion = new Companion();
                Companion.Reset(Player.Position + new Vector2(-20f, -12f));
            }

            var blood = e.Type == EnemyType.Gaunt ? new Color(150, 90, 170)
                      : e.Type == EnemyType.Ork ? new Color(90, 150, 60)
                      : new Color(150, 30, 30);
            int n = e.Type == EnemyType.Chaos ? 14 : 6;
            for (int i = 0; i < n; i++)
            {
                float a = (float)(Rng.NextDouble() * MathHelper.TwoPi);
                float s = (float)Rng.NextDouble() * 70f + 20f;
                AddParticle(e.Position, new Vector2(MathF.Cos(a), MathF.Sin(a)) * s, 0.4f, 3f, 0f,
                    blood, blood * 0f, Art.Pixel, 0.88f);
            }
            if (e.Type == EnemyType.Chaos) Camera.AddShake(2.5f);
        }

        // ------------------------------------------------- companion / boss
        void UpdateProgression(float dt)
        {
            // Sister joins at level 5
            if (!_companionJoined && !_bossTriggered && Player.Level >= CompanionLevel)
            {
                _companionJoined = true;
                Companion = new Companion();
                Companion.Reset(Player.Position + new Vector2(-20f, -12f));
                Banner("HABIBTI NOURHAN MY LOVE 💋❤️ JOINed YOU");
                LogEvent("[EVENT] Companion joined at level " + Player.Level);
            }

            // ...and falls to Chaos at level 10, becoming the boss
            if (!_bossTriggered && Player.Level >= BossLevel)
            {
                _bossTriggered = true;
                Companion = null;
                SpawnSisterBoss();
                Banner("NOURHAN FALLS - ZINOU LE MBZEL RISES");
                LogEvent("[EVENT] Boss spawned (Fallen Sister) at level " + Player.Level);
            }

            if (Companion != null) Companion.Update(this, dt);

            if (BannerTimer > 0f) BannerTimer -= dt;
        }

        void SpawnSisterBoss()
        {
            float minute = Time / 60f;
            float a = (float)(Rng.NextDouble() * MathHelper.TwoPi);
            Vector2 pos = Player.Position + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (Camera.SpawnRadius() * 0.6f);

            var e = GetPooledEnemy();
            e.Init(EnemyType.Chaos, pos, Art.SisterBoss,
                1400f * (1f + minute * 0.35f) * _bossHpScale,  // hp
                52f,                             // speed
                26f,                             // contact damage
                60,                              // xp
                14f);                            // radius
            e.IsBoss = true;
            e.BossFireTimer = 2.0f;
            e.Name = "Zinou le mbzel ;)";
            Enemies.Add(e);
            Boss = e;
            Camera.AddShake(6f);
        }

        void UpdateBoss(float dt)
        {
            if (Boss == null) return;
            if (!Boss.Alive) return; // SweepEnemies will clear it

            Boss.BossFireTimer -= dt;
            if (Boss.BossFireTimer <= 0f)
            {
                Boss.BossFireTimer = 2.2f;
                Vector2 dir = Player.Position - Boss.Position;
                float baseAng = MathF.Atan2(dir.Y, dir.X);
                for (int i = -1; i <= 1; i++)
                {
                    float ang = baseAng + i * 0.22f;
                    AddProjectile(new Projectile
                    {
                        Position = Boss.Position,
                        Velocity = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 200f,
                        Damage = 14f,
                        Life = 2.6f,
                        Radius = 5f,
                        Pierce = 0,
                        Tex = Art.Plasma,
                        Tint = new Color(190, 100, 230),
                        Hostile = true
                    });
                }
                SpawnMuzzle(Boss.Position, new Color(170, 80, 220));
            }
        }

        public void SpawnHealSpark(Vector2 pos)
        {
            float a = (float)(Rng.NextDouble() * MathHelper.TwoPi);
            AddParticle(pos + new Vector2(MathF.Cos(a) * 6f, 4f),
                new Vector2(MathF.Cos(a) * 8f, -22f), 0.6f, 4f, 0f,
                new Color(120, 255, 150), new Color(120, 255, 150) * 0f, Art.Circle, 0.92f);
        }

        void Banner(string text)
        {
            BannerText = text;
            BannerTimer = 3.4f;
        }

        // Milestone logging. Writes to stderr, and (if HERETICIDE_EVENTLOG is set) appends to that
        // file too — used by the automated smoke test. No-op for normal players.
        static readonly string EventLogPath = Environment.GetEnvironmentVariable("HERETICIDE_EVENTLOG");
        static void LogEvent(string msg)
        {
            Console.Error.WriteLine(msg);
            if (!string.IsNullOrEmpty(EventLogPath))
            {
                try { System.IO.File.AppendAllText(EventLogPath, msg + Environment.NewLine); } catch { }
            }
        }

        // ----------------------------------------------------------- weapon API
        public void AddProjectile(Projectile p) => Projectiles.Add(p);

        public Enemy NearestEnemy(Vector2 pos, float maxDist = float.MaxValue)
        {
            Enemy best = null;
            float bestD = maxDist * maxDist;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                if (!e.Alive) continue;
                float d = Vector2.DistanceSquared(pos, e.Position);
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        public List<Enemy> NearestEnemies(Vector2 pos, int count)
        {
            _near.Clear();
            _nearD.Clear();
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                if (!e.Alive) continue;
                float d = Vector2.DistanceSquared(pos, e.Position);
                if (_near.Count < count)
                {
                    _near.Add(e); _nearD.Add(d);
                    int k = _near.Count - 1;
                    while (k > 0 && _nearD[k - 1] > _nearD[k]) { Swap(k); k--; }
                }
                else if (d < _nearD[count - 1])
                {
                    _near[count - 1] = e; _nearD[count - 1] = d;
                    int k = count - 1;
                    while (k > 0 && _nearD[k - 1] > _nearD[k]) { Swap(k); k--; }
                }
            }
            return _near;
        }

        void Swap(int k)
        {
            var te = _near[k]; _near[k] = _near[k - 1]; _near[k - 1] = te;
            var td = _nearD[k]; _nearD[k] = _nearD[k - 1]; _nearD[k - 1] = td;
        }

        public Enemy RandomEnemyInRange(Vector2 pos, float range)
        {
            float r2 = range * range;
            // reservoir pick among enemies in range
            Enemy chosen = null;
            int seen = 0;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                if (!e.Alive) continue;
                if (Vector2.DistanceSquared(pos, e.Position) > r2) continue;
                seen++;
                if (Rng.Next(seen) == 0) chosen = e;
            }
            return chosen;
        }

        public int DamageArea(Vector2 center, float radius, float damage, float knockback, Vector2 knockFrom)
        {
            int hits = 0;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                if (!e.Alive) continue;
                float rr = radius + e.Radius;
                if (Vector2.DistanceSquared(e.Position, center) <= rr * rr)
                {
                    Vector2 dir = e.Position - knockFrom;
                    if (dir != Vector2.Zero) dir.Normalize();
                    else dir = new Vector2(1, 0);
                    e.Hurt(damage, dir, knockback);
                    SpawnHit(e.Position, new Color(255, 180, 120));
                    hits++;
                }
            }
            return hits;
        }

        public void SpawnExplosion(Vector2 pos, float radius, float damage)
        {
            DamageArea(pos, radius, damage, 200f, pos);
            _blasts.Add(new Blast { Pos = pos, Radius = radius, Life = 0.35f, MaxLife = 0.35f });
            Camera.AddShake(3.2f);
            for (int i = 0; i < 16; i++)
            {
                float a = (float)(Rng.NextDouble() * MathHelper.TwoPi);
                float s = (float)Rng.NextDouble() * radius * 4f + 40f;
                var c = i % 2 == 0 ? new Color(255, 180, 60) : new Color(255, 110, 40);
                AddParticle(pos, new Vector2(MathF.Cos(a), MathF.Sin(a)) * s, 0.45f, radius * 0.5f, 2f,
                    c, c * 0f, Art.Circle, 0.86f);
            }
        }

        public void AddSlashEffect(Vector2 pos, float radius, int facing)
        {
            _slashes.Add(new Slash { Pos = pos, Radius = radius, Facing = facing, Life = 0.18f, MaxLife = 0.18f });
        }

        public void SpawnMuzzle(Vector2 pos, Color color)
        {
            AddParticle(pos, Vector2.Zero, 0.1f, 9f, 0f, color * 0.85f, color * 0f, Art.Circle, 0.5f);
        }

        void SpawnHit(Vector2 pos, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = (float)(Rng.NextDouble() * MathHelper.TwoPi);
                float s = (float)Rng.NextDouble() * 60f + 20f;
                AddParticle(pos, new Vector2(MathF.Cos(a), MathF.Sin(a)) * s, 0.22f, 2.5f, 0f,
                    color, color * 0f, Art.Pixel, 0.9f);
            }
        }

        void AddParticle(Vector2 pos, Vector2 vel, float life, float size, float endSize,
            Color c0, Color c1, Texture2D tex, float drag)
        {
            Particles.Add(new Particle
            {
                Position = pos,
                Velocity = vel,
                Life = life,
                MaxLife = life,
                Size = size,
                EndSize = endSize,
                ColorStart = c0,
                ColorEnd = c1,
                Tex = tex,
                Drag = drag,
                RotSpeed = (float)(Rng.NextDouble() * 6.0 - 3.0)
            });
        }

        // ----------------------------------------------------------------- xp
        public void AddXp(int amount)
        {
            Player.Xp += amount * _xpScale;
            while (Player.Xp >= Player.XpToNext)
            {
                Player.Xp -= Player.XpToNext;
                Player.Level++;
                PendingLevelUps++;
                Player.XpToNext = XpThreshold(Player.Level);
            }
        }

        static float XpThreshold(int level) => 5f + level * 4f + (level * level) / 2f;

        // ---------------------------------------------------------------- draw
        public void Draw(SpriteBatch sb)
        {
            DrawGround(sb);

            // aoe blasts under everything
            foreach (var b in _blasts)
            {
                float t = b.Life / b.MaxLife;
                float scale = (b.Radius * 2f) / Art.Circle.Width * (1.1f - t * 0.2f);
                var origin = new Vector2(Art.Circle.Width / 2f, Art.Circle.Height / 2f);
                sb.Draw(Art.Circle, b.Pos, null, new Color(255, 150, 60) * (t * 0.8f), 0f, origin, scale, SpriteEffects.None, 0f);
                float rs = (b.Radius * 2f) / Art.Ring.Width * (1.3f - t * 0.3f);
                var ro = new Vector2(Art.Ring.Width / 2f, Art.Ring.Height / 2f);
                sb.Draw(Art.Ring, b.Pos, null, new Color(255, 220, 150) * t, 0f, ro, rs, SpriteEffects.None, 0f);
            }

            // gems
            foreach (var g in Gems)
            {
                float yo = MathF.Sin(g.Bob) * 1.2f;
                DrawCentered(sb, Art.Gem, g.Position + new Vector2(0, yo), 1f, Color.White, 1);
            }

            // enemies
            foreach (var e in Enemies)
            {
                float yo = MathF.Sin(e.Bob) * 1.2f;
                float pop = e.HitFlash > 0f ? 1.18f : 1f;
                DrawCentered(sb, e.Tex, e.Position + new Vector2(0, yo), pop, Color.White, e.Facing);
                if (e.HitFlash > 0f)
                {
                    float a = e.HitFlash / 0.12f;
                    float gs = (e.Radius * 2.4f) / Art.Circle.Width;
                    var o = new Vector2(Art.Circle.Width / 2f, Art.Circle.Height / 2f);
                    sb.Draw(Art.Circle, e.Position, null, new Color(255, 255, 255) * (a * 0.7f), 0f, o, gs, SpriteEffects.None, 0f);
                }
            }

            // companion + healing aura
            if (Companion != null)
            {
                float aur = 0.22f + 0.10f * MathF.Sin(Time * 5f);
                float gs = 30f / Art.Ring.Width;
                var ro = new Vector2(Art.Ring.Width / 2f, Art.Ring.Height / 2f);
                sb.Draw(Art.Ring, Player.Position, null, new Color(120, 255, 150) * aur, 0f, ro, gs, SpriteEffects.None, 0f);

                float cyo = MathF.Sin(Companion.Bob) * 1.2f;
                DrawCentered(sb, Art.Sister, Companion.Position + new Vector2(0, cyo), 1f, Color.White, Companion.Facing);
            }

            // player
            {
                float yo = MathF.Sin(Player.WalkCycle) * 1.0f;
                DrawCentered(sb, Art.Marine, Player.Position + new Vector2(0, yo), 1f, Color.White, Player.Facing);
                if (Player.HurtFlash > 0f)
                {
                    float a = Player.HurtFlash / 0.22f;
                    float gs = 22f / Art.Circle.Width;
                    var o = new Vector2(Art.Circle.Width / 2f, Art.Circle.Height / 2f);
                    sb.Draw(Art.Circle, Player.Position, null, new Color(255, 60, 60) * (a * 0.6f), 0f, o, gs, SpriteEffects.None, 0f);
                }
            }

            // chainsword slashes
            foreach (var s in _slashes)
            {
                float t = s.Life / s.MaxLife;
                float scale = (s.Radius * 2f) / Art.Slash.Width;
                var origin = new Vector2(Art.Slash.Width / 2f, Art.Slash.Height / 2f);
                var fx = s.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(Art.Slash, s.Pos, null, new Color(220, 240, 255) * (t * 0.85f), 0f, origin, scale, fx, 0f);
            }

            // projectiles
            foreach (var p in Projectiles)
            {
                var origin = new Vector2(p.Tex.Width / 2f, p.Tex.Height / 2f);
                sb.Draw(p.Tex, p.Position, null, p.Tint, p.Rotation, origin, 1f, SpriteEffects.None, 0f);
            }

            // particles
            foreach (var p in Particles)
            {
                float scale = p.CurrentSize / p.Tex.Width;
                var origin = new Vector2(p.Tex.Width / 2f, p.Tex.Height / 2f);
                sb.Draw(p.Tex, p.Position, null, p.CurrentColor, p.Rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        void DrawGround(SpriteBatch sb)
        {
            var b = Camera.VisibleWorldBounds();
            int ts = Art.Ground.Width;
            int x0 = (int)Math.Floor(b.Left / (float)ts) * ts;
            int y0 = (int)Math.Floor(b.Top / (float)ts) * ts;
            for (int x = x0; x < b.Right + ts; x += ts)
                for (int y = y0; y < b.Bottom + ts; y += ts)
                    sb.Draw(Art.Ground, new Vector2(x, y), Color.White);
        }

        static void DrawCentered(SpriteBatch sb, Texture2D tex, Vector2 pos, float scale, Color color, int facing)
        {
            var origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            var fx = facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sb.Draw(tex, pos, null, color, 0f, origin, scale, fx, 0f);
        }
    }
}
