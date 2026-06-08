using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Damage;
        public float Life;
        public float Radius = 3f;
        public int Pierce;
        public float Knockback = 60f;
        public bool Alive = true;
        public Texture2D Tex;
        public Color Tint = Color.White;
        public float Rotation;
        public bool SpinSprite = false;
        public bool Hostile = false;   // true = damages the player (boss fire) instead of enemies

        // grenade / aoe behaviour
        public bool IsAoe = false;
        public float AoeRadius = 0f;
        public float AoeDamage = 0f;

        public readonly HashSet<Enemy> AlreadyHit = new HashSet<Enemy>();

        public void Update(float dt)
        {
            Position += Velocity * dt;
            Life -= dt;
            if (SpinSprite) Rotation += dt * 12f;
            else Rotation = MathF.Atan2(Velocity.Y, Velocity.X);
            if (Life <= 0f) Alive = false;
        }
    }
}
