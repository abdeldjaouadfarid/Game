using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hereticide
{
    public enum GameState { Title, Playing, LevelUp, GameOver }

    public class Game1 : Game
    {
        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _sb;

        World _world;
        Camera2D _camera;
        Input _input;
        VirtualJoystick _joy;

        GameState _state = GameState.Title;
        List<UpgradeOption> _options = new List<UpgradeOption>();
        float _gameOverTimer;
        bool _started;

        // Set HERETICIDE_AUTOPLAY=1 to make the game self-drive (attract / smoke-test mode):
        // auto-deploys, wanders the marine around, and auto-picks the first level-up blessing.
        readonly bool _autoplay = Environment.GetEnvironmentVariable("HERETICIDE_AUTOPLAY") == "1";
        float _autoTime;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = "HERETICIDE: Imperium Survivors";
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = true;
        }

        protected override void Initialize()
        {
            _input = new Input();
            _joy = new VirtualJoystick();
            _world = new World();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            Art.Build(GraphicsDevice);
            _camera = new Camera2D { Viewport = GraphicsDevice.Viewport, Zoom = 3f };
        }

        void StartRun()
        {
            _world.Start(_camera);
            _camera.Position = _world.Player.Position;
            _state = GameState.Playing;
            _started = true;
        }

        void OpenLevelUp()
        {
            _options = Upgrades.Roll(_world, 3);
            _state = GameState.LevelUp;
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt > 0.05f) dt = 0.05f; // avoid huge steps after a hitch

            _input.Update();
            _camera.Viewport = GraphicsDevice.Viewport;
            int sw = GraphicsDevice.Viewport.Width;
            int sh = GraphicsDevice.Viewport.Height;
            if (_autoplay) _autoTime += dt;

            if (_input.KeyPressed(Keys.Escape))
            {
                if (_state == GameState.Playing || _state == GameState.LevelUp) { _state = GameState.Title; }
                else Exit();
            }

            switch (_state)
            {
                case GameState.Title:
                    if (_autoplay || _input.AnyPressed() || _input.KeyPressed(Keys.Enter) || _input.KeyPressed(Keys.Space))
                        StartRun();
                    break;

                case GameState.Playing:
                    _joy.Update(_input, sw, sh);
                    Vector2 move = _joy.Direction + _input.KeyboardMove();
                    if (_autoplay)
                    {
                        // kite: flee the nearest enemy with a little circular wander
                        Vector2 flee = Vector2.Zero;
                        var ne = _world.NearestEnemy(_world.Player.Position);
                        if (ne != null)
                        {
                            flee = _world.Player.Position - ne.Position;
                            if (flee != Vector2.Zero) flee.Normalize();
                        }
                        Vector2 wander = new Vector2((float)Math.Cos(_autoTime * 0.9), (float)Math.Sin(_autoTime * 0.7)) * 0.5f;
                        move = flee + wander;
                    }
                    _world.Update(dt, move);
                    _camera.Follow(_world.Player.Position, dt);
                    if (!_world.Player.Alive) { _state = GameState.GameOver; _gameOverTimer = 1.0f; }
                    else if (_world.PendingLevelUps > 0) OpenLevelUp();
                    break;

                case GameState.LevelUp:
                    HandleLevelUpInput(sw, sh);
                    break;

                case GameState.GameOver:
                    if (_autoplay) { _state = GameState.Title; break; }
                    if (_gameOverTimer > 0f) _gameOverTimer -= dt;
                    else if (_input.AnyPressed() || _input.KeyPressed(Keys.Enter) || _input.KeyPressed(Keys.Space))
                        _state = GameState.Title;
                    break;
            }

            base.Update(gameTime);
        }

        void HandleLevelUpInput(int sw, int sh)
        {
            Rectangle[] rects = CardRects(sw, sh);
            int pick = -1;
            if (_autoplay) pick = 0;
            if (_input.KeyPressed(Keys.D1) || _input.KeyPressed(Keys.NumPad1)) pick = 0;
            if (_input.KeyPressed(Keys.D2) || _input.KeyPressed(Keys.NumPad2)) pick = 1;
            if (_input.KeyPressed(Keys.D3) || _input.KeyPressed(Keys.NumPad3)) pick = 2;
            for (int i = 0; i < _options.Count; i++)
                if (_input.TryGetPressInside(rects[i], out _)) pick = i;

            if (pick >= 0 && pick < _options.Count)
            {
                _options[pick].Apply();
                _world.PendingLevelUps--;
                if (_world.PendingLevelUps > 0) _options = Upgrades.Roll(_world, 3);
                else _state = GameState.Playing;
            }
        }

        Rectangle[] CardRects(int sw, int sh)
        {
            int cardW = (int)Math.Min(sw * 0.82f, 620);
            int cardH = (int)Math.Min(sh * 0.19f, 120);
            int gap = (int)(cardH * 0.22f);
            int totalH = cardH * 3 + gap * 2;
            int startY = (sh - totalH) / 2 + (int)(sh * 0.05f);
            int x = (sw - cardW) / 2;
            var r = new Rectangle[3];
            for (int i = 0; i < 3; i++)
                r[i] = new Rectangle(x, startY + i * (cardH + gap), cardW, cardH);
            return r;
        }

        // -------------------------------------------------------------- drawing
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(12, 10, 14));
            int sw = GraphicsDevice.Viewport.Width;
            int sh = GraphicsDevice.Viewport.Height;

            if (_state == GameState.Title && !_started)
            {
                _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                DrawTitle(sw, sh);
                _sb.End();
                base.Draw(gameTime);
                return;
            }

            // world (camera space)
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.Transform);
            _world.Draw(_sb);
            _sb.End();

            // ui (screen space)
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawHud(sw, sh);
            DrawBossBar(sw, sh);
            DrawBanner(sw, sh);
            if (_state == GameState.Playing) _joy.Draw(_sb, Art.Ring, Art.Circle);
            if (_state == GameState.LevelUp) DrawLevelUp(sw, sh);
            if (_state == GameState.Title && _started) DrawTitle(sw, sh);
            if (_state == GameState.GameOver) DrawGameOver(sw, sh);
            _sb.End();

            base.Draw(gameTime);
        }

        void Rect(int x, int y, int w, int h, Color c) => _sb.Draw(Art.Pixel, new Rectangle(x, y, w, h), c);

        void Outline(Rectangle r, int t, Color c)
        {
            Rect(r.X, r.Y, r.Width, t, c);
            Rect(r.X, r.Y + r.Height - t, r.Width, t, c);
            Rect(r.X, r.Y, t, r.Height, c);
            Rect(r.X + r.Width - t, r.Y, t, r.Height, c);
        }

        void DrawHud(int sw, int sh)
        {
            var p = _world.Player;

            // XP bar (full width, top)
            int barH = 10;
            Rect(0, 0, sw, barH, new Color(22, 20, 28));
            float xpPct = p.XpToNext > 0 ? MathHelper.Clamp(p.Xp / p.XpToNext, 0f, 1f) : 0f;
            Rect(0, 0, (int)(sw * xpPct), barH, new Color(130, 95, 230));

            PixelFont.DrawShadowed(_sb, Art.Pixel, "LV " + p.Level, new Vector2(6, barH + 6), Color.White, 2);

            string t = FormatTime(_world.Time);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, t, sw / 2f, barH + 4, new Color(235, 225, 200), 3);

            string k = "SKULLS " + _world.Kills;
            int kw = PixelFont.Measure(k, 2);
            PixelFont.DrawShadowed(_sb, Art.Pixel, k, new Vector2(sw - kw - 8, barH + 6), new Color(220, 210, 190), 2);

            // HP bar
            int hpW = (int)Math.Min(240, sw * 0.3f);
            int hpH = 16;
            int hx = 6, hy = barH + 26;
            Rect(hx, hy, hpW, hpH, new Color(45, 14, 14));
            float hpPct = p.MaxHp > 0 ? MathHelper.Clamp(p.Hp / p.MaxHp, 0f, 1f) : 0f;
            Rect(hx, hy, (int)(hpW * hpPct), hpH, new Color(205, 45, 45));
            Outline(new Rectangle(hx, hy, hpW, hpH), 1, new Color(20, 8, 8));
            string hp = (int)Math.Ceiling(p.Hp) + " / " + (int)p.MaxHp;
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, hp, hx + hpW / 2f, hy + 3, Color.White, 1);

            // weapon list (bottom-left)
            int wy = sh - 6 - p.Weapons.Count * 14;
            for (int i = 0; i < p.Weapons.Count; i++)
            {
                var w = p.Weapons[i];
                string line = w.Name + "  " + w.Level + (w.IsMaxed ? " MAX" : "");
                PixelFont.DrawShadowed(_sb, Art.Pixel, line, new Vector2(8, wy + i * 14), new Color(200, 200, 210), 1);
            }
        }

        void DrawBossBar(int sw, int sh)
        {
            var b = _world.Boss;
            if (b == null || !b.Alive) return;
            int bw = (int)Math.Min(sw * 0.6f, 520);
            int bh = 14;
            int bx = (sw - bw) / 2;
            int by = 64;
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, b.Name, sw / 2f, by - 18, new Color(235, 150, 235), 2);
            Rect(bx, by, bw, bh, new Color(30, 10, 30));
            float pct = b.MaxHp > 0 ? MathHelper.Clamp(b.Hp / b.MaxHp, 0f, 1f) : 0f;
            Rect(bx, by, (int)(bw * pct), bh, new Color(180, 40, 170));
            Outline(new Rectangle(bx, by, bw, bh), 1, new Color(220, 120, 220));
        }

        void DrawBanner(int sw, int sh)
        {
            if (_world.BannerTimer <= 0f) return;
            float a = MathHelper.Clamp(_world.BannerTimer, 0f, 1f);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, _world.BannerText, sw / 2f, sh * 0.3f, new Color(255, 90, 70) * a, 4);
        }

        void DrawTitle(int sw, int sh)
        {
            // backdrop
            Rect(0, 0, sw, sh, new Color(14, 12, 16));
            // big marine
            var origin = new Vector2(Art.Marine.Width / 2f, Art.Marine.Height / 2f);
            _sb.Draw(Art.Marine, new Vector2(sw / 2f, sh * 0.4f), null, Color.White, 0f, origin, 9f, SpriteEffects.None, 0f);

            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "HERETICIDE", sw / 2f, sh * 0.56f, new Color(210, 40, 40), 8);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "IMPERIUM SURVIVORS", sw / 2f, sh * 0.56f + 70, new Color(220, 200, 150), 3);

            float blink = (float)(Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 3.0) * 0.5 + 0.5);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "TAP OR PRESS ENTER TO DEPLOY", sw / 2f, sh * 0.8f,
                new Color(255, 255, 255) * (0.4f + 0.6f * blink), 3);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "MOVE: DRAG LEFT SIDE  OR  WASD     WEAPONS FIRE AUTOMATICALLY", sw / 2f, sh * 0.88f,
                new Color(150, 150, 160), 2);
        }

        void DrawLevelUp(int sw, int sh)
        {
            Rect(0, 0, sw, sh, new Color(0, 0, 0, 170));
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "LEVEL UP", sw / 2f, sh * 0.07f, new Color(255, 210, 90), 6);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "CHOOSE A BLESSING", sw / 2f, sh * 0.07f + 50, new Color(200, 190, 170), 2);

            Rectangle[] rects = CardRects(sw, sh);
            for (int i = 0; i < _options.Count; i++)
            {
                var o = _options[i];
                var r = rects[i];
                Rect(r.X, r.Y, r.Width, r.Height, new Color(28, 26, 34));
                Rect(r.X, r.Y, 7, r.Height, o.Accent);
                Outline(r, 2, o.Accent * 0.6f);

                int tx = r.X + 22;
                int ty = r.Y + (int)(r.Height * 0.18f);
                PixelFont.Draw(_sb, Art.Pixel, o.Title, new Vector2(tx, ty), Color.White, 3);
                PixelFont.Draw(_sb, Art.Pixel, o.Desc, new Vector2(tx, ty + 34), new Color(190, 190, 200), 2);

                int tagW = PixelFont.Measure(o.Tag, 2);
                PixelFont.Draw(_sb, Art.Pixel, o.Tag, new Vector2(r.Right - tagW - 14, r.Y + 12), o.Accent, 2);

                PixelFont.Draw(_sb, Art.Pixel, (i + 1).ToString(), new Vector2(r.Right - 22, r.Bottom - 24), new Color(120, 120, 130), 2);
            }
        }

        void DrawGameOver(int sw, int sh)
        {
            Rect(0, 0, sw, sh, new Color(40, 0, 0, 150));
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "YOU DIED", sw / 2f, sh * 0.26f, new Color(220, 40, 40), 9);

            string s1 = "SURVIVED  " + FormatTime(_world.Time);
            string s2 = "LEVEL  " + _world.Player.Level;
            string s3 = "SKULLS TAKEN  " + _world.Kills;
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, s1, sw / 2f, sh * 0.46f, Color.White, 3);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, s2, sw / 2f, sh * 0.46f + 34, Color.White, 3);
            PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, s3, sw / 2f, sh * 0.46f + 68, Color.White, 3);

            if (_gameOverTimer <= 0f)
                PixelFont.DrawCenteredShadowed(_sb, Art.Pixel, "TAP OR PRESS ENTER", sw / 2f, sh * 0.74f, new Color(220, 210, 190), 3);
        }

        static string FormatTime(float seconds)
        {
            int total = (int)seconds;
            int m = total / 60;
            int s = total % 60;
            return m.ToString() + ":" + (s < 10 ? "0" + s : s.ToString());
        }
    }
}
