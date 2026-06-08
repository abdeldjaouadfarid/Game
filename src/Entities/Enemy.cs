using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    public enum EnemyType { Cultist, Gaunt, Ork, Chaos }

    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Knockback;
        public float Hp;
        public float MaxHp;
        public float Speed;
        public float Damage;
        public float Radius;
        public int XpValue;
        public EnemyType Type;
        public Texture2D Tex;
        public bool Alive = true;
        public float HitFlash = 0f;
        public float ContactCooldown = 0f;
        public int Facing = 1;
        public float Bob;

        // boss-only
        public bool IsBoss = false;
        public float BossFireTimer = 0f;
        public string Name = null;

        public void Init(EnemyType type, Vector2 pos, Texture2D tex, float hp, float speed, float damage, int xp, float radius)
        {
            Type = type;
            Position = pos;
            Tex = tex;
            Hp = MaxHp = hp;
            Speed = speed;
            Damage = damage;
            XpValue = xp;
            Radius = radius;
            Knockback = Vector2.Zero;
            Alive = true;
            HitFlash = 0f;
            ContactCooldown = 0f;
            IsBoss = false;
            BossFireTimer = 0f;
            Name = null;
            Bob = (float)(_seed.NextDouble() * 6.28);
        }

        static readonly System.Random _seed = new System.Random();

        public void Update(float dt, Vector2 target)
        {
            Vector2 toTarget = target - Position;
            if (toTarget != Vector2.Zero) toTarget.Normalize();

            Position += toTarget * Speed * dt;

            // knockback decays
            if (Knockback != Vector2.Zero)
            {
                Position += Knockback * dt;
                Knockback *= 0.86f;
                if (Knockback.LengthSquared() < 1f) Knockback = Vector2.Zero;
            }

            if (toTarget.X > 0.1f) Facing = 1;
            else if (toTarget.X < -0.1f) Facing = -1;

            Bob += dt * 8f;

            if (HitFlash > 0f) HitFlash -= dt;
            if (ContactCooldown > 0f) ContactCooldown -= dt;
        }

        public void Hurt(float dmg, Vector2 knockDir, float knockForce)
        {
            Hp -= dmg;
            HitFlash = 0.12f;
            Knockback += knockDir * knockForce;
            if (Hp <= 0f) Alive = false;
        }
    }
}
