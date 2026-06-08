using Microsoft.Xna.Framework;

namespace Hereticide
{
    public class XpGem
    {
        public Vector2 Position;
        public int Value;
        public bool Alive = true;
        public float Bob;
        public bool Attracted;

        public XpGem(Vector2 pos, int value)
        {
            Position = pos;
            Value = value;
            Bob = (float)(_rng.NextDouble() * 6.28);
        }

        static readonly System.Random _rng = new System.Random();
    }
}
