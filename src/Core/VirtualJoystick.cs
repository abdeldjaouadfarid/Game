using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    /// <summary>
    /// A floating virtual thumbstick. It anchors wherever the player first presses on the
    /// left half of the screen and reports a direction vector (length 0..1). Works with touch
    /// on Android and with the mouse on desktop.
    /// </summary>
    public class VirtualJoystick
    {
        public bool Active;
        public int PointerId = -1;
        public Vector2 Origin;
        public Vector2 Current;
        public float Radius = 80f;
        public Vector2 Direction;   // normalized * magnitude(0..1)

        public void Update(Input input, int screenW, int screenH)
        {
            if (!Active)
            {
                foreach (var p in input.Pointers)
                {
                    if (p.Pressed && p.Pos.X < screenW * 0.5f)
                    {
                        Active = true;
                        PointerId = p.Id;
                        Origin = p.Pos;
                        Current = p.Pos;
                        break;
                    }
                }
            }
            else
            {
                bool found = false;
                foreach (var p in input.Pointers)
                {
                    if (p.Id == PointerId)
                    {
                        found = true;
                        Current = p.Pos;
                        if (p.Released) { Active = false; PointerId = -1; }
                        break;
                    }
                }
                if (!found) { Active = false; PointerId = -1; }
            }

            if (Active)
            {
                Vector2 d = Current - Origin;
                float len = d.Length();
                if (len > Radius) d = d / len * Radius;
                Direction = d / Radius;
            }
            else
            {
                Direction = Vector2.Zero;
            }
        }

        public void Draw(SpriteBatch sb, Texture2D ring, Texture2D circle)
        {
            if (!Active) return;
            float baseScale = (Radius * 2f) / ring.Width;
            var ringOrigin = new Vector2(ring.Width / 2f, ring.Height / 2f);
            sb.Draw(ring, Origin, null, new Color(255, 255, 255, 70), 0f, ringOrigin, baseScale, SpriteEffects.None, 0f);

            Vector2 knob = Origin + Direction * Radius;
            float knobScale = (Radius * 0.7f) / circle.Width;
            var circOrigin = new Vector2(circle.Width / 2f, circle.Height / 2f);
            sb.Draw(circle, knob, null, new Color(255, 255, 255, 110), 0f, circOrigin, knobScale, SpriteEffects.None, 0f);
        }
    }
}
