using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
        public float EndSize;
        public Color ColorStart;
        public Color ColorEnd;
        public float Drag = 0.9f;
        public float Rotation;
        public float RotSpeed;
        public bool Alive = true;
        public Texture2D Tex;

        public void Update(float dt)
        {
            Position += Velocity * dt;
            Velocity *= Drag;
            Rotation += RotSpeed * dt;
            Life -= dt;
            if (Life <= 0f) Alive = false;
        }

        public float T => 1f - (Life / MaxLife); // 0..1 progress

        public Color CurrentColor => Color.Lerp(ColorStart, ColorEnd, T);
        public float CurrentSize => MathHelper.Lerp(Size, EndSize, T);
    }
}
