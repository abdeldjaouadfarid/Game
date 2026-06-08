using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    public class Camera2D
    {
        public Vector2 Position;   // world point at the centre of the screen
        public float Zoom = 3f;
        public Viewport Viewport;
        public float Shake = 0f;
        Vector2 _shakeOffset;
        static System.Random _rng = new System.Random();

        public Matrix Transform =>
            Matrix.CreateTranslation(-Position.X - _shakeOffset.X, -Position.Y - _shakeOffset.Y, 0f) *
            Matrix.CreateScale(Zoom) *
            Matrix.CreateTranslation(Viewport.Width / 2f, Viewport.Height / 2f, 0f);

        public void Follow(Vector2 target, float dt)
        {
            // smooth chase
            Position = Vector2.Lerp(Position, target, MathHelper.Clamp(dt * 8f, 0f, 1f));

            if (Shake > 0f)
            {
                Shake -= dt * 30f;
                if (Shake < 0f) Shake = 0f;
                float mag = Shake;
                _shakeOffset = new Vector2(
                    ((float)_rng.NextDouble() * 2f - 1f) * mag,
                    ((float)_rng.NextDouble() * 2f - 1f) * mag);
            }
            else _shakeOffset = Vector2.Zero;
        }

        public void AddShake(float amount)
        {
            Shake = System.Math.Min(8f, Shake + amount);
        }

        public Vector2 ScreenToWorld(Vector2 screen)
        {
            return Vector2.Transform(screen, Matrix.Invert(Transform));
        }

        public Rectangle VisibleWorldBounds()
        {
            Vector2 tl = ScreenToWorld(Vector2.Zero);
            Vector2 br = ScreenToWorld(new Vector2(Viewport.Width, Viewport.Height));
            return new Rectangle((int)tl.X, (int)tl.Y, (int)(br.X - tl.X), (int)(br.Y - tl.Y));
        }

        public float SpawnRadius()
        {
            float halfW = Viewport.Width / 2f / Zoom;
            float halfH = Viewport.Height / 2f / Zoom;
            return MathF.Sqrt(halfW * halfW + halfH * halfH) + 40f;
        }
    }
}
