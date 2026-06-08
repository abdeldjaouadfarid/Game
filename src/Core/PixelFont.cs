using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    /// <summary>
    /// A tiny 5x7 procedural bitmap font. No content pipeline / no font files needed.
    /// Each glyph is drawn pixel-by-pixel using a 1x1 white texture.
    /// </summary>
    public static class PixelFont
    {
        public const int GlyphW = 5;
        public const int GlyphH = 7;
        static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>();

        static void G(char c, params string[] rows)
        {
            var norm = new string[GlyphH];
            for (int i = 0; i < GlyphH; i++)
            {
                string r = i < rows.Length ? rows[i] : "";
                if (r.Length < GlyphW) r = r.PadRight(GlyphW, '.');
                else if (r.Length > GlyphW) r = r.Substring(0, GlyphW);
                norm[i] = r;
            }
            Glyphs[c] = norm;
        }

        static PixelFont()
        {
            G(' ', ".....", ".....", ".....", ".....", ".....", ".....", ".....");
            G('A', ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#");
            G('B', "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####.");
            G('C', ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###.");
            G('D', "###..", "#..#.", "#...#", "#...#", "#...#", "#..#.", "###..");
            G('E', "#####", "#....", "#....", "####.", "#....", "#....", "#####");
            G('F', "#####", "#....", "#....", "####.", "#....", "#....", "#....");
            G('G', ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".###.");
            G('H', "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#");
            G('I', ".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###.");
            G('J', "..###", "...#.", "...#.", "...#.", "#..#.", "#..#.", ".##..");
            G('K', "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#");
            G('L', "#....", "#....", "#....", "#....", "#....", "#....", "#####");
            G('M', "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#");
            G('N', "#...#", "##..#", "#.#.#", "#.#.#", "#..##", "#...#", "#...#");
            G('O', ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###.");
            G('P', "####.", "#...#", "#...#", "####.", "#....", "#....", "#....");
            G('Q', ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#");
            G('R', "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#");
            G('S', ".####", "#....", "#....", ".###.", "....#", "....#", "####.");
            G('T', "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#..");
            G('U', "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###.");
            G('V', "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#..");
            G('W', "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#");
            G('X', "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#");
            G('Y', "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#..");
            G('Z', "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####");
            G('0', ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###.");
            G('1', "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###.");
            G('2', ".###.", "#...#", "....#", "..##.", ".#...", "#....", "#####");
            G('3', "####.", "....#", "....#", ".###.", "....#", "....#", "####.");
            G('4', "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#.");
            G('5', "#####", "#....", "####.", "....#", "....#", "#...#", ".###.");
            G('6', ".##..", ".#...", "#....", "####.", "#...#", "#...#", ".###.");
            G('7', "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#...");
            G('8', ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###.");
            G('9', ".###.", "#...#", "#...#", ".####", "....#", "...#.", ".##..");
            G(':', ".....", "..#..", "..#..", ".....", "..#..", "..#..", ".....");
            G(';', ".....", "..#..", "..#..", ".....", "..#..", "..#..", ".#...");
            G('.', ".....", ".....", ".....", ".....", ".....", "..#..", "..#..");
            G(',', ".....", ".....", ".....", ".....", "..#..", "..#..", ".#...");
            G('!', "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#..");
            G('?', ".###.", "#...#", "....#", "..##.", "..#..", ".....", "..#..");
            G('/', "....#", "....#", "...#.", "..#..", ".#...", "#....", "#....");
            G('-', ".....", ".....", ".....", "#####", ".....", ".....", ".....");
            G('+', ".....", "..#..", "..#..", "#####", "..#..", "..#..", ".....");
            G('(', "..##.", ".#...", "#....", "#....", "#....", ".#...", "..##.");
            G(')', ".##..", "...#.", "....#", "....#", "....#", "...#.", ".##..");
            G('\'', "..#..", "..#..", "..#..", ".....", ".....", ".....", ".....");
            G('>', "#....", ".#...", "..#..", "...#.", "..#..", ".#...", "#....");
            G('<', "....#", "...#.", "..#..", ".#...", "..#..", "...#.", "....#");
            G('%', "#...#", "#..#.", "...#.", "..#..", ".#...", ".#..#", "#...#");
            G('*', ".....", "#.#.#", ".###.", "#####", ".###.", "#.#.#", ".....");
            G('=', ".....", ".....", "#####", ".....", "#####", ".....", ".....");
        }

        public static int Measure(string text, int scale, int spacing = 1)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Length * (GlyphW + spacing) * scale - spacing * scale;
        }

        public static void Draw(SpriteBatch sb, Texture2D pixel, string text, Vector2 pos, Color color, int scale, int spacing = 1)
        {
            if (string.IsNullOrEmpty(text)) return;
            text = text.ToUpperInvariant();
            int startX = (int)pos.X;
            int cx = startX;
            int cy = (int)pos.Y;
            foreach (char ch in text)
            {
                if (ch == '\n')
                {
                    cy += (GlyphH + 2) * scale;
                    cx = startX;
                    continue;
                }
                string[] g;
                if (Glyphs.TryGetValue(ch, out g))
                {
                    for (int y = 0; y < GlyphH; y++)
                    {
                        string row = g[y];
                        for (int x = 0; x < GlyphW; x++)
                        {
                            if (row[x] == '#')
                                sb.Draw(pixel, new Rectangle(cx + x * scale, cy + y * scale, scale, scale), color);
                        }
                    }
                }
                cx += (GlyphW + spacing) * scale;
            }
        }

        public static void DrawCentered(SpriteBatch sb, Texture2D pixel, string text, float centerX, float y, Color color, int scale)
        {
            int w = Measure(text, scale);
            Draw(sb, pixel, text, new Vector2(centerX - w / 2f, y), color, scale);
        }

        /// <summary>Draws text with a 1px (scaled) dark shadow for readability.</summary>
        public static void DrawShadowed(SpriteBatch sb, Texture2D pixel, string text, Vector2 pos, Color color, int scale)
        {
            Draw(sb, pixel, text, pos + new Vector2(scale, scale), new Color(0, 0, 0, 180), scale);
            Draw(sb, pixel, text, pos, color, scale);
        }

        public static void DrawCenteredShadowed(SpriteBatch sb, Texture2D pixel, string text, float centerX, float y, Color color, int scale)
        {
            int w = Measure(text, scale);
            DrawShadowed(sb, pixel, text, new Vector2(centerX - w / 2f, y), color, scale);
        }
    }
}
