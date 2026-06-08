using Microsoft.Xna.Framework;

namespace Hereticide
{
    public abstract class Weapon
    {
        public string Name;
        public int Level = 1;
        public int MaxLevel = 8;
        public float Timer = 0.25f;   // small initial delay so it fires shortly after pickup

        public bool IsMaxed => Level >= MaxLevel;

        public void Update(World w, float dt)
        {
            Timer -= dt;
            if (Timer <= 0f)
            {
                Fire(w);
                Timer = Cooldown(w.Player);
            }
        }

        protected abstract void Fire(World w);
        public abstract float Cooldown(Player p);

        /// <summary>One-line description of what the NEXT level (or initial pickup) grants.</summary>
        public abstract string NextLevelText();

        public virtual void LevelUp() { if (Level < MaxLevel) Level++; }
    }

    // ---------------------------------------------------------------- Bolter
    public class BolterWeapon : Weapon
    {
        public BolterWeapon() { Name = "BOLTER"; }

        public int ProjectileCount => 1 + Level / 3;
        public float DamageOf(Player p) => (8f + 3f * Level) * p.DamageMult;
        public int PierceOf => Level >= 5 ? 1 : 0;

        public override float Cooldown(Player p)
        {
            float lvlScale = MathHelper.Clamp(1f - 0.035f * (Level - 1), 0.6f, 1f);
            return 0.55f * p.CooldownMult * lvlScale;
        }

        protected override void Fire(World w)
        {
            var targets = w.NearestEnemies(w.Player.Position, ProjectileCount);
            if (targets.Count == 0) return;

            float dmg = DamageOf(w.Player);
            float speed = 360f * w.Player.ProjSpeedMult;
            for (int i = 0; i < targets.Count; i++)
            {
                Vector2 dir = targets[i].Position - w.Player.Position;
                if (dir != Vector2.Zero) dir.Normalize();
                w.AddProjectile(new Projectile
                {
                    Position = w.Player.Position,
                    Velocity = dir * speed,
                    Damage = dmg,
                    Life = 1.3f,
                    Radius = 4f,
                    Pierce = PierceOf,
                    Knockback = 55f,
                    Tex = Art.Bolt,
                    Tint = Color.White
                });
            }
            w.SpawnMuzzle(w.Player.Position, new Color(255, 210, 90));
        }

        public override string NextLevelText()
        {
            if (Level == 0) return "Auto-fires bolts at nearby foes";
            int nextCount = 1 + (Level + 1) / 3;
            if (nextCount > ProjectileCount) return "+1 bolt, +damage";
            if (Level + 1 == 5) return "Bolts now pierce, +damage";
            return "+damage, faster fire";
        }
    }

    // ------------------------------------------------------------ Chainsword
    public class ChainswordWeapon : Weapon
    {
        public ChainswordWeapon() { Name = "CHAINSWORD"; Timer = 0.4f; }

        public float DamageOf(Player p) => (12f + 5f * Level) * p.DamageMult;
        public float RadiusOf(Player p) => (28f + 4f * Level) * p.AreaMult;

        public override float Cooldown(Player p) => 0.7f * p.CooldownMult;

        protected override void Fire(World w)
        {
            float radius = RadiusOf(w.Player);
            float dmg = DamageOf(w.Player);
            int hits = w.DamageArea(w.Player.Position, radius, dmg, 130f, w.Player.Position);
            w.AddSlashEffect(w.Player.Position, radius, w.Player.Facing);
            if (hits > 0) w.Camera.AddShake(1.2f);
        }

        public override string NextLevelText()
        {
            return "+damage, wider arc";
        }
    }

    // ----------------------------------------------------------------- Plasma
    public class PlasmaWeapon : Weapon
    {
        public PlasmaWeapon() { Name = "PLASMA GUN"; Timer = 0.6f; }

        public float DamageOf(Player p) => (22f + 9f * Level) * p.DamageMult;
        public int PierceOf => 2 + Level / 2;

        public override float Cooldown(Player p) => 1.15f * p.CooldownMult;

        protected override void Fire(World w)
        {
            var t = w.NearestEnemy(w.Player.Position);
            Vector2 dir;
            if (t != null) { dir = t.Position - w.Player.Position; if (dir != Vector2.Zero) dir.Normalize(); }
            else dir = new Vector2(w.Player.Facing, 0);

            w.AddProjectile(new Projectile
            {
                Position = w.Player.Position,
                Velocity = dir * 240f * w.Player.ProjSpeedMult,
                Damage = DamageOf(w.Player),
                Life = 1.6f,
                Radius = 6f,
                Pierce = PierceOf,
                Knockback = 90f,
                Tex = Art.Plasma,
                Tint = Color.White
            });
            w.SpawnMuzzle(w.Player.Position, new Color(120, 200, 255));
        }

        public override string NextLevelText() => "+damage, +pierce";
    }

    // ------------------------------------------------------------ Frag Grenade
    public class FragWeapon : Weapon
    {
        public FragWeapon() { Name = "FRAG LAUNCHER"; Timer = 0.8f; }

        public float DamageOf(Player p) => (28f + 11f * Level) * p.DamageMult;
        public float AoeOf(Player p) => (38f + 5f * Level) * p.AreaMult;

        public override float Cooldown(Player p) => 2.0f * p.CooldownMult;

        protected override void Fire(World w)
        {
            var t = w.RandomEnemyInRange(w.Player.Position, 260f);
            Vector2 targetPos = t != null ? t.Position : w.Player.Position + new Vector2(w.Player.Facing * 120f, 0);
            Vector2 dir = targetPos - w.Player.Position;
            float dist = dir.Length();
            if (dir != Vector2.Zero) dir.Normalize();
            float speed = 150f;

            w.AddProjectile(new Projectile
            {
                Position = w.Player.Position,
                Velocity = dir * speed,
                Damage = 0f,
                Life = MathHelper.Clamp(dist / speed, 0.25f, 2.0f),
                Radius = 4f,
                Pierce = 9999,
                Tex = Art.Grenade,
                Tint = Color.White,
                SpinSprite = true,
                IsAoe = true,
                AoeRadius = AoeOf(w.Player),
                AoeDamage = DamageOf(w.Player)
            });
        }

        public override string NextLevelText() => "+blast damage, +radius";
    }
}
