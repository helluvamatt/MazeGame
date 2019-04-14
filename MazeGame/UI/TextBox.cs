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
    internal class TextBox : Control
    {
        private const int SPRITE_Y = 160;
        private const int SPRITE_X = 128;
        private const int SPRITE_FOCUSED_X = 144;
        private const int SPRITE_DISABLED_Y = 176;
        private const int SPRITE_WIDTH = 15;
        private const int SPRITE_HEIGHT = 15;
        private const int SPRITE_PADDING = 7;

        private static readonly Rectangle CARET_TEXTURE_REGION = new Rectangle(SPRITE_FOCUSED_X, SPRITE_DISABLED_Y, 2, 2);

        private readonly Point _Size;
        private string _Text;

        // Text selection
        //private int _CaretIndex;
        private Vector2 _TextOffset;

        // TODO Support selecting text

        // Caret blink
        private bool _CaretVisible;
        private TimeSpan _NextCaretBlink;

        public TextBox(string name, Point size, SpriteFont font) : base(name)
        {
            _Size = size;
            _TextOffset = Vector2.Zero;
            TextFont = font ?? throw new ArgumentNullException(nameof(font));
            TextColor = Color.Black;
            Text = string.Empty;
        }

        public SpriteFont TextFont { get; }
        public Color TextColor { get; set; }

        #region Text property

        public string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    OnTextChanged();
                }
            }
        }

        public event EventHandler TextChanged;

        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public override bool CanFocus => true;

        public override Point GetPreferredSize(Point availableSpace) => _Size;

        protected override void OnLayout(WindowManager uiRenderer)
        {
            // No layout needed
        }

        protected override void OnRender(WindowManager uiRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            var rasterizerState = new RasterizerState() { ScissorTestEnable = true };
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState);
            int x, y;
            if (!IsEnabled)
            {
                x = SPRITE_X;
                y = SPRITE_DISABLED_Y;
            }
            else if (IsFocused)
            {
                x = SPRITE_FOCUSED_X;
                y = SPRITE_Y;
            }
            else
            {
                x = SPRITE_X;
                y = SPRITE_Y;
            }
            var texture = new NinePatchRegion2D(new TextureRegion2D(uiTexture, new Rectangle(x, y, SPRITE_WIDTH, SPRITE_HEIGHT)), SPRITE_PADDING);
            sb.Draw(texture, Bounds, Color.White);

            var originalClip = sb.GraphicsDevice.ScissorRectangle;
            var textRect = new Rectangle(Bounds.X + SPRITE_PADDING, Bounds.Y + SPRITE_PADDING, Bounds.Width - SPRITE_PADDING * 2, Bounds.Height - SPRITE_PADDING * 2);
            sb.GraphicsDevice.ScissorRectangle = textRect;

            var textOrigin = new Vector2(textRect.X + _TextOffset.X, textRect.Y + _TextOffset.Y);
            sb.DrawString(TextFont, Text, textOrigin, TextColor);

            if (_CaretVisible && IsFocused)
            {
                var caretRect = new Rectangle(0, textRect.Y, CARET_TEXTURE_REGION.Width, textRect.Height);
                sb.Draw(uiTexture, caretRect, CARET_TEXTURE_REGION, TextColor);
            }

            sb.GraphicsDevice.ScissorRectangle = originalClip;
            sb.End();
        }

        public override void OnTick(GameTime gameTime)
        {
            if (gameTime.TotalGameTime > _NextCaretBlink)
            {
                _CaretVisible = !_CaretVisible;
                _NextCaretBlink = gameTime.TotalGameTime + TimeSpan.FromMilliseconds(400);
            }
        }

        public override void OnKeyDown(Keys key, bool repeat)
        {

            // TODO Process key events
        }
    }
}
