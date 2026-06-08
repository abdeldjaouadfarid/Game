using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    /// <summary>
    /// Spawns the endless horde in a ring around the player. Density, toughness and the enemy
    /// mix all ramp up over time, with periodic swarm waves and elite Chaos Marines.
    /// </summary>
    public class Spawner
    {
        const int MaxEnemies = 340;

        float _spawnTimer;
        float _swarmTimer = 28f;
        float _eliteTimer = 70f;
        readonly Random _rng = new Random();

        public void Reset()
        {
            _spawnTimer = 0f;
            _swarmTimer = 28f;
            _eliteTimer = 70f;
        }

        public void Update(World w, float dt)
        {
            float minute = w.Time / 60f;

            _spawnTimer -= dt;
            float interval = MathHelper.Clamp(0.85f - minute * 0.12f, 0.16f, 0.85f);
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = interval;
                int cluster = 1 + (int)MathHelper.Clamp(minute, 0, 4);
                for (int i = 0; i < cluster; i++)
                    SpawnNormal(w, minute);
            }

            _swarmTimer -= dt;
            if (_swarmTimer <= 0f)
            {
                _swarmTimer = MathHelper.Clamp(34f - minute * 2f, 16f, 34f);
                SpawnSwarm(w, minute);
            }

            _eliteTimer -= dt;
            if (_eliteTimer <= 0f && minute >= 1f)
            {
                _eliteTimer = MathHelper.Clamp(60f - minute * 3f, 24f, 60f);
                Spawn(w, EnemyType.Chaos, minute, RingPos(w));
            }
        }

        void SpawnNormal(World w, float minute)
        {
            EnemyType type;
            double r = _rng.NextDouble();
            if (minute < 1f)
                type = r < 0.7 ? EnemyType.Cultist : EnemyType.Gaunt;
            else if (minute < 2.5f)
                type = r < 0.45 ? EnemyType.Cultist : (r < 0.8 ? EnemyType.Gaunt : EnemyType.Ork);
            else
                type = r < 0.3 ? EnemyType.Cultist : (r < 0.65 ? EnemyType.Gaunt : EnemyType.Ork);

            Spawn(w, type, minute, RingPos(w));
        }

        void SpawnSwarm(World w, float minute)
        {
            // a tight arc of fast gaunts pouring in from one direction
            int count = 12 + (int)(minute * 6);
            float baseAngle = (float)(_rng.NextDouble() * MathHelper.TwoPi);
            float radius = w.Camera.SpawnRadius();
            for (int i = 0; i < count; i++)
            {
                float a = baseAngle + (float)(_rng.NextDouble() - 0.5) * 1.6f;
                float d = radius + (float)_rng.NextDouble() * 60f;
                Vector2 pos = w.Player.Position + new Vector2(MathF.Cos(a), MathF.Sin(a)) * d;
                Spawn(w, EnemyType.Gaunt, minute, pos);
            }
        }

        Vector2 RingPos(World w)
        {
            float a = (float)(_rng.NextDouble() * MathHelper.TwoPi);
            float d = w.Camera.SpawnRadius() + (float)_rng.NextDouble() * 50f;
            return w.Player.Position + new Vector2(MathF.Cos(a), MathF.Sin(a)) * d;
        }

        void Spawn(World w, EnemyType type, float minute, Vector2 pos)
        {
            if (w.Enemies.Count >= MaxEnemies) return;

            float hpScale = 1f + minute * 0.55f;
            float dmgScale = 1f + minute * 0.22f;
            float spdScale = 1f + MathHelper.Clamp(minute * 0.04f, 0f, 0.5f);

            float hp, speed, dmg, radius;
            int xp;
            Texture2D tex;

            switch (type)
            {
                case EnemyType.Gaunt:
                    hp = 9f; speed = 78f; dmg = 5f; xp = 1; radius = 5f; tex = Art.Gaunt; break;
                case EnemyType.Ork:
                    hp = 46f; speed = 38f; dmg = 12f; xp = 3; radius = 7f; tex = Art.Ork; break;
                case EnemyType.Chaos:
                    hp = 170f; speed = 46f; dmg = 18f; xp = 14; radius = 7f; tex = Art.Chaos; break;
                default: // Cultist
                    hp = 14f; speed = 46f; dmg = 6f; xp = 1; radius = 6f; tex = Art.Cultist; break;
            }

            var e = w.GetPooledEnemy();
            e.Init(type, pos, tex,
                hp * hpScale,
                speed * spdScale,
                dmg * dmgScale,
                xp, radius);
            w.Enemies.Add(e);
        }
    }
}
