using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.TextureAtlases;

namespace MazeGame.UI
{
    internal class Button : Control
    {
        private const int SPRITE_X = 160;
        private const int SPRITE_Y_NORMAL = 0;
        private const int SPRITE_Y_PRESSED = 32;
        private const int SPRITE_Y_FOCUSED = 64;
        private const int SPRITE_Y_FOCUSED_PRESSED = 96;
        private const int SPRITE_Y_DISABLED = 128;

        private const int SPRITE_HEIGHT = 32;
        private const int SPRITE_WIDTH = 88;

        private const int SPRITE_PADDING_X = 15;
        private const int SPRITE_PADDING_Y = 11;

        //private const int SPRITE_9PATCH_X_START = 37;
        //private const int SPRITE_9PATCH_X_END = 51;
        //private const int SPRITE_9PATCH_Y_START = 15;
        //private const int SPRITE_9PATCH_Y_END = 17;
        private const int SPRITE_9P_PADDING_X = 37;
        private const int SPRITE_9P_PADDING_Y = 15;

        private const int HITBOX_PADDING = 6;

        private Vector2 _TextLocation;
        private bool _Pressed;

        public Button(string name, string text, SpriteFont font) : base(name)
        {
            Text = text;
            TextFont = font ?? throw new ArgumentNullException(nameof(font));
            AutoSize = true;
            TextColor = Color.Black;
        }

        public event EventHandler Click;

        public string Text { get; }
        public SpriteFont TextFont { get; }
        public Color TextColor { get; set; }
        public Padding Padding { get; set; }

        public bool AutoSize { get; set; }
        public Point ExplicitSize { get; set; }

        public override bool CanFocus => true;

        public override Point GetPreferredSize(Point availableSpace)
        {
            if (!AutoSize) return ExplicitSize;
            var size = Vector2.Ceiling(TextFont.MeasureString(Text)).ToPoint();
            size.X += SPRITE_PADDING_X * 2 + Padding.Horizontal;
            size.Y += SPRITE_PADDING_Y * 2 + Padding.Vertical;
            return size;
        }

        protected override void OnLayout(WindowManager uiRenderer)
        {
            var textSize = TextFont.MeasureString(Text);
            float x = Bounds.X + (Bounds.Width - textSize.X) / 2;
            float y = Bounds.Y + (Bounds.Height - textSize.Y) / 2;
            _TextLocation = new Vector2(x, y);
        }

        protected override void OnRender(WindowManager uiRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            int spriteRegionY;
            if (!IsEnabled) spriteRegionY = SPRITE_Y_DISABLED;
            else if (IsFocused && _Pressed) spriteRegionY = SPRITE_Y_FOCUSED_PRESSED;
            else if (IsFocused) spriteRegionY = SPRITE_Y_FOCUSED;
            else if (_Pressed) spriteRegionY = SPRITE_Y_PRESSED;
            else spriteRegionY = SPRITE_Y_NORMAL;
            var frameRegion = new NinePatchRegion2D(new TextureRegion2D(uiTexture, new Rectangle(SPRITE_X, spriteRegionY, SPRITE_WIDTH, SPRITE_HEIGHT)), new MonoGame.Extended.Thickness(SPRITE_9P_PADDING_X, SPRITE_9P_PADDING_Y));
            sb.Begin();
            sb.Draw(frameRegion, Bounds, Color.White);
            sb.DrawString(TextFont, Text, _TextLocation, TextColor);
            sb.End();
        }

        public override void OnMouseDown(Point point, MouseButton button)
        {
            if (GetHitbox().Contains(point)) _Pressed = true;
        }

        public override void OnMouseUp(Point point, MouseButton button)
        {
            if (_Pressed)
            {
                _Pressed = false;
                OnClick();
            }
        }

        public override void OnMouseMove(Point point)
        {
            if (_Pressed && !GetHitbox().Contains(point)) _Pressed = false;
        }

        public override void OnMouseOut(Point point)
        {
            _Pressed = false;
        }

        public override void OnKeyDown(Keys key, bool repeat)
        {
            if (key == Keys.Space || key == Keys.Enter) _Pressed = true;
        }

        public override void OnKeyUp(Keys key)
        {
            if (_Pressed && (key == Keys.Space || key == Keys.Enter))
            {
                _Pressed = false;
                OnClick();
            }
        }

        protected virtual void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        private Rectangle GetHitbox() => new Rectangle(Bounds.X + HITBOX_PADDING, Bounds.Y + HITBOX_PADDING, Bounds.Width - HITBOX_PADDING * 2, Bounds.Height - HITBOX_PADDING * 2);
    }
}
