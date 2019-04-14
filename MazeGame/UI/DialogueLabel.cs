using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MazeGame.UI
{
    internal class DialogueLabel : Control
    {
        private const int CURSOR_X = 128;
        private const int CURSOR_Y = 256;
        private const int CURSOR_SIZE = 16;
        private static readonly Rectangle CURSOR = new Rectangle(CURSOR_X, CURSOR_Y, CURSOR_SIZE, CURSOR_SIZE);

        private string _Text;
        private TimeSpan _NextAnimTick;
        private int _SubstringLength;
        private TimeSpan _NextBlinkTick;
        private bool _CursorVisible;

        private Rectangle _CursorDestRect;
        private Vector2 _TextLocation;

        public DialogueLabel(string name, SpriteFont font) : base(name)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            TextRate = TimeSpan.Zero;
            BlinkRate = TimeSpan.FromMilliseconds(400);
        }

        public event EventHandler Accept;

        public string Text
        {
            get => _Text;
            set
            {
                _CursorVisible = false;
                _SubstringLength = 0;
                _Text = value;
            }
        }

        public SpriteFont Font { get; }
        public Color Color { get; set; }
        public Padding Padding { get; set; }
        public TimeSpan TextRate { get; set; }
        public TimeSpan BlinkRate { get; set; }

        public override Point GetPreferredSize(Point availableSpace)
        {
            var textSize = Vector2.Ceiling(Font.MeasureString(Text ?? string.Empty)).ToPoint();
            return new Point(textSize.X + Padding.Horizontal, textSize.Y + Padding.Vertical + CURSOR_SIZE);
        }

        protected override void OnLayout(WindowManager windowManager)
        {
            _TextLocation = new Vector2(Bounds.X + Padding.Left, Bounds.Y + Padding.Top);
            _CursorDestRect = new Rectangle(Bounds.Right - (Padding.Right + CURSOR_SIZE), Bounds.Bottom - (Padding.Bottom + CURSOR_SIZE), CURSOR_SIZE, CURSOR_SIZE);
        }

        protected override void OnRender(WindowManager windowManager, SpriteBatch sb, Texture2D uiTexture)
        {
            sb.Begin();
            if (!string.IsNullOrEmpty(Text)) sb.DrawString(Font, Text.Substring(0, _SubstringLength), _TextLocation, Color);
            if (_CursorVisible) sb.Draw(uiTexture, _CursorDestRect, CURSOR, Color.White);
            sb.End();
        }

        public override void OnKeyPress(Keys key)
        {
            if (key == Keys.Space)
            {
                if (Text != null && _SubstringLength < Text.Length) _SubstringLength = Text.Length;
                else Accept?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void OnTick(GameTime gameTime)
        {
            var text = Text ?? string.Empty;
            if (_SubstringLength < text.Length)
            {
                if (TextRate > TimeSpan.Zero)
                {
                    if (gameTime.TotalGameTime > _NextAnimTick)
                    {
                        _NextAnimTick = gameTime.TotalGameTime + TextRate;
                        _SubstringLength++;
                    }
                }
                else _SubstringLength = text.Length;
            }
            else if (gameTime.TotalGameTime > _NextBlinkTick)
            {
                _CursorVisible = !_CursorVisible;
                _NextBlinkTick = gameTime.TotalGameTime + BlinkRate;
            }
        }
    }
}
