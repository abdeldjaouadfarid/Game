using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Hereticide
{
    /// <summary>
    /// Unifies keyboard, mouse and touch into a single list of "pointers" plus key helpers.
    /// On desktop the mouse acts as one touch pointer so the virtual joystick / menu taps
    /// behave exactly like they will on Android.
    /// </summary>
    public class Input
    {
        public struct Pointer
        {
            public int Id;
            public Vector2 Pos;
            public bool Pressed;   // went down this frame
            public bool Down;      // currently held
            public bool Released;  // came up this frame
        }

        const int MouseId = -100;

        public readonly List<Pointer> Pointers = new List<Pointer>();

        KeyboardState _ks, _pks;
        MouseState _ms, _pms;

        public void Update()
        {
            _pks = _ks;
            _ks = Keyboard.GetState();
            _pms = _ms;
            _ms = Mouse.GetState();

            Pointers.Clear();

            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                foreach (var t in touches)
                {
                    if (t.State == TouchLocationState.Invalid) continue;
                    Pointers.Add(new Pointer
                    {
                        Id = t.Id,
                        Pos = t.Position,
                        Pressed = t.State == TouchLocationState.Pressed,
                        Down = t.State == TouchLocationState.Pressed || t.State == TouchLocationState.Moved,
                        Released = t.State == TouchLocationState.Released
                    });
                }
            }
            else
            {
                bool down = _ms.LeftButton == ButtonState.Pressed;
                bool wasDown = _pms.LeftButton == ButtonState.Pressed;
                if (down || wasDown)
                {
                    Pointers.Add(new Pointer
                    {
                        Id = MouseId,
                        Pos = new Vector2(_ms.X, _ms.Y),
                        Pressed = down && !wasDown,
                        Down = down,
                        Released = !down && wasDown
                    });
                }
            }
        }

        public bool KeyDown(Keys k) => _ks.IsKeyDown(k);
        public bool KeyPressed(Keys k) => _ks.IsKeyDown(k) && _pks.IsKeyUp(k);

        /// <summary>WASD + arrow keys as a normalized-ish direction (length 0..1).</summary>
        public Vector2 KeyboardMove()
        {
            Vector2 d = Vector2.Zero;
            if (_ks.IsKeyDown(Keys.W) || _ks.IsKeyDown(Keys.Up)) d.Y -= 1;
            if (_ks.IsKeyDown(Keys.S) || _ks.IsKeyDown(Keys.Down)) d.Y += 1;
            if (_ks.IsKeyDown(Keys.A) || _ks.IsKeyDown(Keys.Left)) d.X -= 1;
            if (_ks.IsKeyDown(Keys.D) || _ks.IsKeyDown(Keys.Right)) d.X += 1;
            if (d != Vector2.Zero) d.Normalize();
            return d;
        }

        /// <summary>True if any pointer pressed (used for "tap anywhere" prompts).</summary>
        public bool AnyPressed()
        {
            foreach (var p in Pointers) if (p.Pressed) return true;
            return false;
        }

        public bool TryGetPressInside(Rectangle rect, out Vector2 pos)
        {
            foreach (var p in Pointers)
            {
                if (p.Pressed && rect.Contains((int)p.Pos.X, (int)p.Pos.Y))
                {
                    pos = p.Pos;
                    return true;
                }
            }
            pos = Vector2.Zero;
            return false;
        }
    }
}
