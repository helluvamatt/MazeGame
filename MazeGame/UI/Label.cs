using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame.UI
{
    internal class Label : Control
    {
        private Vector2 _TextLocation;

        public Label(string name, string text, SpriteFont textFont, Color color) : base(name)
        {
            Text = text;
            TextFont = textFont ?? throw new ArgumentNullException(nameof(textFont));
            TextColor = color;
        }

        public SpriteFont TextFont { get; }
        public string Text { get; }
        public Color TextColor { get; }
        public Padding Padding { get; set; }

        public override Point GetPreferredSize(Point availableSpace)
        {
            var textSize = Vector2.Ceiling(TextFont.MeasureString(Text)).ToPoint();
            return new Point(textSize.X + Padding.Horizontal, textSize.Y + Padding.Vertical);
        }

        protected override void OnLayout(WindowManager uIRenderer)
        {
            _TextLocation = new Vector2(Bounds.X + Padding.Left, Bounds.Y + Padding.Top);
        }

        protected override void OnRender(WindowManager uIRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            sb.Begin();
            sb.DrawString(TextFont, Text, _TextLocation, TextColor);
            sb.End();
        }
    }
}
