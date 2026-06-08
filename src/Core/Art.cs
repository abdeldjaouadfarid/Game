using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hereticide
{
    /// <summary>
    /// Builds every texture procedurally at load time. Warhammer 40K-flavoured chunky pixel art,
    /// no external asset files required.
    /// </summary>
    public static class Art
    {
        public static Texture2D Pixel;     // 1x1 white, used for bars/text/particles
        public static Texture2D Ground;     // tiled battlefield floor
        public static Texture2D Marine;     // the player (Ultramarine)
        public static Texture2D Cultist;    // weak chaos cultist
        public static Texture2D Gaunt;      // fast tyranid hormagaunt
        public static Texture2D Ork;        // tanky ork
        public static Texture2D Chaos;      // chaos space marine elite
        public static Texture2D Gem;        // xp gem
        public static Texture2D Bolt;       // bolter round
        public static Texture2D Plasma;     // plasma shot
        public static Texture2D Grenade;    // frag grenade
        public static Texture2D Sister;     // Battle Sister companion (unlocks lvl 5)
        public static Texture2D SisterBoss; // the Fallen Sister (boss at lvl 10)
        public static Texture2D Slash;      // chainsword arc
        public static Texture2D Circle;     // soft filled circle (explosions/glow)
        public static Texture2D Ring;       // hollow ring (aoe telegraph)

        public static void Build(GraphicsDevice gd)
        {
            Pixel = new Texture2D(gd, 1, 1);
            Pixel.SetData(new[] { Color.White });

            Ground = MakeGround(gd, 48);
            Circle = MakeCircle(gd, 64);
            Ring = MakeRing(gd, 64);
            Slash = MakeSlash(gd, 40);

            // ---- Player: Ultramarine ----
            var marinePal = new Dictionary<char, Color>
            {
                ['B'] = new Color(40, 78, 150),   // armour blue
                ['A'] = new Color(78, 130, 210),  // pauldron highlight
                ['H'] = new Color(18, 28, 52),    // dark trim
                ['E'] = new Color(235, 60, 50),   // red eye lens
                ['G'] = new Color(120, 126, 138), // bolter metal
                ['L'] = new Color(30, 36, 54),    // legs
                ['S'] = new Color(230, 226, 200), // chest aquila
            };
            Marine = Make(gd, marinePal,
                "...HHHH.....",
                "..HEBBEH....",
                "..HBBBBH....",
                ".AAHBBHAA...",
                "AABBBBBBAA.G",
                "ABBBSBBBA.GG",
                "ABBBSBBBAGG.",
                "AABBBBBBAA..",
                ".AHBBBBHA...",
                "..HBBBBH....",
                "...LLLL.....",
                "..LL..LL....",
                "..LL..LL....");

            // ---- Cultist: hooded, red robe, glowing eyes ----
            var cultPal = new Dictionary<char, Color>
            {
                ['R'] = new Color(120, 28, 30),
                ['r'] = new Color(165, 48, 48),
                ['K'] = new Color(14, 12, 16),
                ['F'] = new Color(190, 160, 140),
                ['Y'] = new Color(235, 205, 70),
            };
            Cultist = Make(gd, cultPal,
                "....KKKK....",
                "...KKKKKK...",
                "...KKFFKK...",
                "...KFYYFK...",
                "...RRRRRR...",
                "..RRRRRRRR..",
                "..RrRRRRrR..",
                "..RRRRRRRR..",
                "..RRRRRRRR..",
                "..RRRRRRRR..",
                "..RR.RR.RR..",
                "...R....R...",
                "...R....R...");

            // ---- Hormagaunt: fast tyranid, purple carapace + bone claws ----
            var gauntPal = new Dictionary<char, Color>
            {
                ['P'] = new Color(96, 64, 138),
                ['p'] = new Color(70, 44, 104),
                ['W'] = new Color(222, 212, 182),
                ['E'] = new Color(220, 70, 60),
            };
            Gaunt = Make(gd, gauntPal,
                ".....WW.....",
                "W...WPPW...W",
                ".W..PPPP..W.",
                "..WPPEEPPW..",
                "...PPPPPP...",
                "..PPpppPPP..",
                "..PPPPPPPP..",
                "...PPPPPP...",
                "..W.PPPP.W..",
                ".W..p..p..W.",
                "W...W..W...W",
                "....W..W....",
                "....W..W....");

            // ---- Ork: big, green, tusks, scrap armour ----
            var orkPal = new Dictionary<char, Color>
            {
                ['N'] = new Color(86, 138, 54),
                ['n'] = new Color(54, 94, 38),
                ['T'] = new Color(232, 226, 198),
                ['A'] = new Color(92, 98, 108),
                ['E'] = new Color(230, 70, 55),
            };
            Ork = Make(gd, orkPal,
                "....NNNNNN....",
                "...NNNNNNNN...",
                "..NNEENNEENN..",
                "..NNNNNNNNNN..",
                "..NTNNNNNNTN..",
                "..NNTNNNNTNN..",
                ".AANNNNNNNNAA.",
                ".AANNNNNNNNAA.",
                "..NNNNNNNNNN..",
                "..nNNNNNNNNn..",
                "..nNN.NN.NNn..",
                "...N..NN..N...",
                "...NN.NN.NN...",
                "..NN..NN..NN..");

            // ---- Chaos Space Marine elite: dark red + brass + horns ----
            var chaosPal = new Dictionary<char, Color>
            {
                ['B'] = new Color(110, 26, 30),   // dark red armour
                ['A'] = new Color(150, 44, 46),   // highlight
                ['H'] = new Color(20, 14, 16),    // trim
                ['E'] = new Color(120, 235, 90),  // sickly green eye
                ['G'] = new Color(150, 120, 60),  // brass
                ['L'] = new Color(28, 18, 20),
                ['S'] = new Color(180, 150, 70),  // brass star
            };
            Chaos = Make(gd, chaosPal,
                "..G.HHHH.G..",
                "..GHEBBEHG..",
                "...HBBBBH...",
                ".AAHBBHAA...",
                "AABBBBBBAA.G",
                "ABBBSBBBA.GG",
                "ABBBSBBBAGG.",
                "AABBBBBBAA..",
                ".AHBBBBHA...",
                "..HBBBBH....",
                "...LLLL.....",
                "..LL..LL....",
                "..LL..LL....");

            // ---- XP gem ----
            var gemPal = new Dictionary<char, Color>
            {
                ['X'] = new Color(90, 235, 130),
                ['x'] = new Color(40, 170, 86),
                ['o'] = new Color(200, 255, 210),
            };
            Gem = Make(gd, gemPal,
                "...o...",
                "..XoX..",
                ".XXXXX.",
                "xXXXXXx",
                ".xXXXx.",
                "..xXx..",
                "...x...");

            // ---- Bolter round ----
            var boltPal = new Dictionary<char, Color>
            {
                ['Y'] = new Color(255, 226, 90),
                ['O'] = new Color(240, 140, 30),
                ['w'] = new Color(255, 250, 220),
            };
            Bolt = Make(gd, boltPal,
                ".OO.",
                "OYYO",
                "wYYw",
                ".OO.");

            // ---- Plasma shot ----
            var plasmaPal = new Dictionary<char, Color>
            {
                ['C'] = new Color(120, 200, 255),
                ['c'] = new Color(60, 130, 240),
                ['w'] = new Color(235, 250, 255),
            };
            Plasma = Make(gd, plasmaPal,
                ".cCc..",
                "cCwCc.",
                "CwwwC.",
                "cCwCc.",
                ".cCc..",
                "......");

            // ---- Frag grenade ----
            var nadePal = new Dictionary<char, Color>
            {
                ['M'] = new Color(70, 80, 70),
                ['m'] = new Color(45, 52, 45),
                ['Y'] = new Color(220, 180, 60),
            };
            Grenade = Make(gd, nadePal,
                "..Y..",
                ".mMm.",
                ".MMM.",
                ".mMm.",
                ".....");

            // ---- Battle Sister companion: black power armour, red trim, halo, curly hair ----
            var sisterPal = new Dictionary<char, Color>
            {
                ['K'] = new Color(32, 30, 40),    // black armour
                ['k'] = new Color(18, 18, 24),    // boots/shadow
                ['R'] = new Color(185, 45, 45),   // red trim
                ['H'] = new Color(46, 30, 34),    // dark curly hair
                ['F'] = new Color(216, 176, 150), // skin
                ['E'] = new Color(120, 175, 235), // eyes
                ['G'] = new Color(122, 128, 140), // bolt pistol
                ['W'] = new Color(240, 235, 210), // halo
            };
            Sister = Make(gd, sisterPal,
                "....WWWW....",
                "...HHHHHH...",
                "..HHHHHHHH..",
                "..HHFFFFHH..",
                "..HFFEEFFH..",
                "...RRKKRR...",
                "..KKKKKKKKG.",
                ".KKKRKKKKGG.",
                ".KKKRKKKK.G.",
                ".KKKKKKKKK..",
                "..KKKKKKKK..",
                "..RKK..KKR..",
                "..kk....kk..",
                "..kk....kk..");

            // ---- The Fallen Sister boss: corrupted armour, wings, horns, green eyes ----
            var fallenPal = new Dictionary<char, Color>
            {
                ['K'] = new Color(46, 26, 54),    // corrupted armour
                ['k'] = new Color(26, 16, 32),    // boots/shadow
                ['P'] = new Color(98, 56, 128),   // daemon wings
                ['R'] = new Color(168, 36, 46),   // chaos red
                ['H'] = new Color(34, 22, 28),    // wild hair
                ['F'] = new Color(178, 150, 172), // pallid skin
                ['E'] = new Color(122, 235, 96),  // sickly green glow
                ['G'] = new Color(152, 122, 62),  // brass
                ['W'] = new Color(212, 200, 182), // horns
            };
            SisterBoss = Make(gd, fallenPal,
                "..W..........W..",
                "..WH........HW..",
                "...HHHHHHHHHH...",
                "..HHHHHHHHHHHH..",
                "..HHFFFFFFFFHH..",
                "..HFFEEFFEEFFH..",
                ".PPRRKKKKKKRRPP.",
                "PPPKKKKKKKKKKPPP",
                "PP.KKKKRRKKKK.PP",
                "P..KKKKRRKKKK..P",
                "...KKKKKKKKKK...",
                "..GKKKKKKKKKKG..",
                "...KKKKKKKKKK...",
                "...KKKKKKKKKK...",
                "...RKKK..KKKR...",
                "...KKK....KKK...",
                "...kk......kk...",
                "..kk........kk..");
        }

        static Texture2D Make(GraphicsDevice gd, Dictionary<char, Color> pal, params string[] rows)
        {
            int h = rows.Length;
            int w = 0;
            for (int i = 0; i < h; i++) if (rows[i].Length > w) w = rows[i].Length;
            var data = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                string row = rows[y];
                for (int x = 0; x < w; x++)
                {
                    char c = x < row.Length ? row[x] : '.';
                    Color col;
                    if (c == '.' || !pal.TryGetValue(c, out col)) col = Color.Transparent;
                    data[y * w + x] = col;
                }
            }
            var t = new Texture2D(gd, w, h);
            t.SetData(data);
            return t;
        }

        static Texture2D MakeGround(GraphicsDevice gd, int size)
        {
            var rng = new Random(40000);
            var data = new Color[size * size];
            var baseA = new Color(26, 24, 30);
            var baseB = new Color(32, 30, 38);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // subtle two-tone flagstones + noise speckles of grime
                    bool tile = ((x / 12) + (y / 12)) % 2 == 0;
                    Color c = tile ? baseA : baseB;
                    int n = rng.Next(100);
                    if (n < 6) c = new Color(18, 16, 22);              // dark grime
                    else if (n < 9) c = new Color(44, 40, 34);         // dust highlight
                    else if (n == 9) c = new Color(70, 24, 24);        // dried blood fleck
                    // mortar lines
                    if (x % 12 == 0 || y % 12 == 0) c = new Color(16, 14, 18);
                    data[y * size + x] = c;
                }
            }
            var t = new Texture2D(gd, size, size);
            t.SetData(data);
            return t;
        }

        static Texture2D MakeCircle(GraphicsDevice gd, int d)
        {
            var data = new Color[d * d];
            float r = d / 2f;
            var c = new Vector2(r, r);
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    float t = dist / r;
                    if (t >= 1f) { data[y * d + x] = Color.Transparent; continue; }
                    float a = 1f - t;
                    a = a * a;
                    data[y * d + x] = new Color(1f, 1f, 1f, a);
                }
            var tex = new Texture2D(gd, d, d);
            tex.SetData(data);
            return tex;
        }

        static Texture2D MakeRing(GraphicsDevice gd, int d)
        {
            var data = new Color[d * d];
            float r = d / 2f;
            var c = new Vector2(r, r);
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) / r;
                    // bright band near the outer edge
                    float a = 0f;
                    if (dist < 1f) a = MathHelper.Clamp(1f - Math.Abs(dist - 0.85f) * 7f, 0f, 1f);
                    data[y * d + x] = new Color(1f, 1f, 1f, a);
                }
            var tex = new Texture2D(gd, d, d);
            tex.SetData(data);
            return tex;
        }

        static Texture2D MakeSlash(GraphicsDevice gd, int d)
        {
            // a crescent: filled disc minus an offset disc, used for the chainsword arc
            var data = new Color[d * d];
            float r = d / 2f;
            var c = new Vector2(r, r);
            var inner = new Vector2(r * 1.35f, r);
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float d1 = Vector2.Distance(p, c) / r;
                    float d2 = Vector2.Distance(p, inner) / r;
                    float a = 0f;
                    if (d1 < 1f && d2 > 0.75f)
                        a = MathHelper.Clamp((1f - d1) * 2.2f, 0f, 1f);
                    data[y * d + x] = new Color(1f, 1f, 1f, a);
                }
            var tex = new Texture2D(gd, d, d);
            tex.SetData(data);
            return tex;
        }
    }
}
