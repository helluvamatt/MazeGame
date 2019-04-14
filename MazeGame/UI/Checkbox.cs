using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MazeGame.UI
{
    internal class Checkbox : Control
    {
        private const int SPRITE_SIZE = WindowManager.TILE_SIZE;

        private static readonly Rectangle CHECKED_REGION =   new Rectangle(96, 224, SPRITE_SIZE, SPRITE_SIZE);
        private static readonly Rectangle UNCHECKED_REGION = new Rectangle(96, 192, SPRITE_SIZE, SPRITE_SIZE);
        private static readonly Rectangle CHECKED_FOCUSED_REGION = new Rectangle(128, 224, SPRITE_SIZE, SPRITE_SIZE);
        private static readonly Rectangle UNCHECKED_FOCUSED_REGION = new Rectangle(128, 192, SPRITE_SIZE, SPRITE_SIZE);
        
        private static readonly Rectangle HIT_REGION = new Rectangle(9, 9, 14, 14);

        private int _GlyphOffsetY;
        private Rectangle _HitRegion;
        private Rectangle _LabelHitRegion;
        private Vector2 _LabelTextPosition;
        private bool _Checked;

        private bool HasLabel => LabelFont != null && !string.IsNullOrEmpty(LabelText);

        public Checkbox(string name) : base(name) { }

        public string LabelText { get; set; }
        public SpriteFont LabelFont { get; set; }
        public Color LabelColor { get; set; }
        public int LabelSpacing { get; set; } = 6;

        #region Checked property

        public bool Checked
        {
            get => _Checked;
            set
            {
                if (_Checked != value)
                {
                    _Checked = value;
                    OnCheckedChanged();
                }
            }
        }

        public event EventHandler CheckedChanged;

        protected virtual void OnCheckedChanged()
        {
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public override bool CanFocus => true;

        public override Point GetPreferredSize(Point availableSpace)
        {
            int width = SPRITE_SIZE;
            int height = SPRITE_SIZE;
            if (HasLabel)
            {
                var textSize = Vector2.Ceiling(LabelFont.MeasureString(LabelText)).ToPoint();
                width += LabelSpacing + textSize.X;
                height = Math.Max(height, textSize.Y);
            }
            return new Point(width, height);
        }

        protected override void OnLayout(WindowManager uIRenderer)
        {
            _HitRegion = new Rectangle(Location.X + HIT_REGION.X, Location.Y + HIT_REGION.Y, HIT_REGION.Width, HIT_REGION.Height);
            if (HasLabel)
            {
                _GlyphOffsetY = (Bounds.Height - SPRITE_SIZE) / 2;

                var textSize = LabelFont.MeasureString(LabelText);

                float textX = Bounds.X + SPRITE_SIZE + LabelSpacing;
                float textY = Bounds.Y + (Bounds.Height - textSize.Y) / 2;
                _LabelTextPosition = new Vector2(textX, textY);

                var textSizeInt = Vector2.Ceiling(textSize).ToPoint();
                _LabelHitRegion = new Rectangle(Bounds.X + SPRITE_SIZE + LabelSpacing, Bounds.Y + (Bounds.Height - textSizeInt.Y) / 2, textSizeInt.X, textSizeInt.Y);
            }
        }

        protected override void OnRender(WindowManager uIRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            sb.Begin();
            var srcRect = IsFocused ? (Checked ? CHECKED_FOCUSED_REGION : UNCHECKED_FOCUSED_REGION) : (Checked ? CHECKED_REGION : UNCHECKED_REGION);
            sb.Draw(uiTexture, new Rectangle(Bounds.X, Bounds.Y + _GlyphOffsetY, SPRITE_SIZE, SPRITE_SIZE), srcRect, Color.White);
            if (HasLabel) sb.DrawString(LabelFont, LabelText, _LabelTextPosition, LabelColor);
            sb.End();
        }

        public override void OnMouseUp(Point point, MouseButton button)
        {
            if (_HitRegion.Contains(point) || (HasLabel && _LabelHitRegion.Contains(point))) Checked = !Checked;
        }

        public override void OnKeyUp(Keys key)
        {
            if (key == Keys.Space) Checked = !Checked;
        }
    }
}
