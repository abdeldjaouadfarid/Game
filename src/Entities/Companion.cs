using System;
using Microsoft.Xna.Framework;

namespace Hereticide
{
    /// <summary>
    /// The Battle Sister companion (unlocks at player level 5). She follows the marine, fires a
    /// weak bolt pistol at nearby foes, and slowly heals the player over time. At level 10 the
    /// World replaces her with the Fallen Sister boss.
    /// </summary>
    public class Companion
    {
        public Vector2 Position;
        public int Facing = 1;
        public float Bob;

        public float HealRate = 1.4f;       // HP per second granted to the player (slow heal)
        public float FireCooldown = 0.8f;   // intentionally slow
        public float Range = 175f;

        float _fireTimer = 0.5f;
        float _healPulse;

        public void Reset(Vector2 pos)
        {
            Position = pos;
            _fireTimer = 0.5f;
            _healPulse = 0f;
        }

        public void Update(World w, float dt)
        {
            var p = w.Player;

            // follow a point just beside / behind the marine
            Vector2 target = p.Position + new Vector2(-p.Facing * 16f, -10f);
            Vector2 to = target - Position;
            float d = to.Length();
            if (d > 1f)
            {
                float spd = MathHelper.Clamp(d * 4f, 0f, 240f);
                Position += (to / d) * spd * dt;
            }
            if (to.X > 1f) Facing = 1; else if (to.X < -1f) Facing = -1;
            Bob += dt * 6f;

            // slow heal over time
            if (p.Hp < p.MaxHp)
            {
                p.Hp = Math.Min(p.MaxHp, p.Hp + HealRate * dt);
                _healPulse += dt;
                if (_healPulse >= 0.7f) { _healPulse = 0f; w.SpawnHealSpark(p.Position); }
            }

            // weak ranged fire
            _fireTimer -= dt;
            if (_fireTimer <= 0f)
            {
                var e = w.NearestEnemy(Position, Range);
                if (e != null)
                {
                    Vector2 dir = e.Position - Position;
                    if (dir != Vector2.Zero) dir.Normalize();
                    w.AddProjectile(new Projectile
                    {
                        Position = Position,
                        Velocity = dir * 320f,
                        Damage = 7f + p.Level * 0.6f,   // deliberately low damage
                        Life = 1.1f,
                        Radius = 3f,
                        Pierce = 0,
                        Knockback = 28f,
                        Tex = Art.Bolt,
                        Tint = new Color(180, 220, 255)
                    });
                    w.SpawnMuzzle(Position, new Color(150, 200, 255));
                    _fireTimer = FireCooldown;
                }
                else _fireTimer = 0.25f;
            }
        }
    }
}
