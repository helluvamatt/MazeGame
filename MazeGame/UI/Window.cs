using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using System;

namespace MazeGame.UI
{
    internal class Window : Control, INotifyClose
    {
        private Vector2 _TitlePosition;
        private Rectangle _ControlBounds;
        private bool _MouseOverControl;
        private Control _Control;
        private bool _LayoutComplete;

        public Window(string name, FrameType frameType, SpriteFont titleFont) : this(name, frameType, titleFont, null) { }

        public Window(string name, FrameType frameType, SpriteFont titleFont, string title) : base(name)
        {
            if (frameType != FrameType.None && frameType != FrameType.LargeScroll && frameType != FrameType.MediumScroll && frameType != FrameType.SmallScroll) throw new ArgumentOutOfRangeException(nameof(frameType));

            Title = title;
            TitleFont = titleFont;
            Type = frameType;
            Dock = DockMode.Center;
            TitleAlignment = Alignment.Middle;
        }

        public event EventHandler Close;

        public Control Control
        {
            get => _Control;
            set
            {
                if (_Control != value)
                {
                    if (_Control is INotifyClose notifyClose) notifyClose.Close -= OnClose;
                    _Control = value;
                    if (_Control is INotifyClose notifyCloseNew) notifyCloseNew.Close += OnClose;
                }
            }
        }

        public FrameType Type { get; }
        public Padding Padding { get; set; }

        public string Title { get; set; }
        public SpriteFont TitleFont { get; }
        public Padding TitlePadding { get; set; }
        public Alignment TitleAlignment { get; set; }

        public Padding Margin { get; set; }
        public DockMode Dock { get; set; }

        public bool Visible { get; set; }

        public bool CloseOnClickOutside { get; set; }
        public bool DimBackground { get; set; }

        public override Point GetPreferredSize(Point availableSpace)
        {
            var padding = GetPadding(out Rectangle titleArea);
            if (availableSpace.X > 0 && availableSpace.Y > 0)
            {
                availableSpace.X -= padding.Horizontal + Padding.Horizontal;
                availableSpace.Y -= padding.Vertical + Padding.Vertical;
            }
            var controlSize = Control?.GetPreferredSize(availableSpace) ?? Point.Zero;
            return new Point(Math.Max(titleArea.Width, controlSize.X) + padding.Horizontal, controlSize.Y + padding.Vertical);
        }

        protected override void OnLayout(WindowManager uiRenderer)
        {
            var framePadding = GetPadding(out Rectangle titleArea);
            float titleX;
            switch (TitleAlignment)
            {
                case Alignment.Near:
                    titleX = Bounds.X + framePadding.Left + TitlePadding.Left;
                    break;
                case Alignment.Middle:
                    titleX = Bounds.X + TitlePadding.Left + (Bounds.Width - titleArea.Width) / 2;
                    break;
                case Alignment.Far:
                    titleX = Bounds.Right - (framePadding.Right + TitlePadding.Right + titleArea.Width);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(TitleAlignment));
            }
                
            float titleY = Bounds.Y + titleArea.Y + TitlePadding.Top;
            _TitlePosition = new Vector2(titleX, titleY);
            if (Control != null)
            {
                var controlSize = new Point(Bounds.Width - framePadding.Horizontal, Bounds.Height - framePadding.Vertical);
                _ControlBounds = new Rectangle(new Point(Bounds.X + framePadding.Left, Bounds.Y + framePadding.Top), controlSize);
                Control.PerformLayout(uiRenderer, _ControlBounds);
            }
            _LayoutComplete = true;
        }

        protected override void OnRender(WindowManager uiRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            if (!Visible || !_LayoutComplete) return;
            if (DimBackground)
            {
                sb.Begin();
                sb.FillRectangle(sb.GraphicsDevice.Viewport.Bounds.ToRectangleF(), new Color(Color.Black, 0.7f));
                sb.End();
            }
            if (Type != FrameType.None)
            {
                sb.Begin();
                var frameRegion = new NinePatchRegion2D(new TextureRegion2D(uiTexture, GetFrameTextureRegion()), GetFramePadding());
                sb.Draw(frameRegion, Bounds, Color.White);
                if (TitleFont != null && !string.IsNullOrEmpty(Title)) sb.DrawString(TitleFont, Title, _TitlePosition, Color.White);
                sb.End();
            }
            Control?.Render(uiRenderer, sb, uiTexture);
        }

        public override void OnKeyDown(Keys key, bool repeat)
        {
            if (!Visible) return;
            Control?.OnKeyDown(key, repeat);
        }

        public override void OnKeyUp(Keys key)
        {
            if (!Visible) return;
            Control?.OnKeyUp(key);
        }

        public override void OnKeyPress(Keys key)
        {
            if (!Visible) return;
            Control?.OnKeyPress(key);
        }

        public override void OnMouseDown(Point point, MouseButton button)
        {
            if (!Visible) return;
            if (_ControlBounds.Contains(point)) Control?.OnMouseDown(point, button);
        }

        public override void OnMouseUp(Point point, MouseButton button)
        {
            if (!Visible) return;
            if (_ControlBounds.Contains(point)) Control?.OnMouseUp(point, button);
            else if (CloseOnClickOutside && !Bounds.Contains(point)) OnClose(this, EventArgs.Empty);
        }

        public override void OnMouseMove(Point point)
        {
            if (!Visible) return;
            if (_ControlBounds.Contains(point))
            {
                if (!_MouseOverControl) Control?.OnMouseOver(point);
                _MouseOverControl = true;
                Control?.OnMouseMove(point);
            }
            else if (_MouseOverControl)
            {
                Control?.OnMouseOut(point);
                _MouseOverControl = false;
            }
        }

        public override void OnShown()
        {
            Control?.OnShown();
        }

        public override void OnClosed()
        {
            Control?.OnClosed();
        }

        public override void OnTick(GameTime gameTime)
        {
            Control?.OnTick(gameTime);
        }

        public T FindControl<T>(string name) where T : Control => (T)FindControl(name);

        public Control FindControl(string name)
        {
            if (Control?.Name == name) return Control;
            if (Control is ContainerControl container) return container.FindControl(name);
            return null;
        }

        private int GetFramePadding() => Type == FrameType.LargeScroll ? 64 : 32;

        private Rectangle GetFrameTextureRegion()
        {
            switch (Type)
            {
                case FrameType.LargeScroll:
                    return new Rectangle(0, 0, 160, 160);
                case FrameType.MediumScroll:
                    return new Rectangle(0, 160, 96, 96);
                case FrameType.SmallScroll:
                    return new Rectangle(32, 320, 96, 96);
            }
            return Rectangle.Empty;
        }

        private Padding GetPadding(out Rectangle titleArea)
        {
            titleArea = Rectangle.Empty;
            if (Type == FrameType.None) return Padding.Zero;
            var framePadding = GetFramePadding();
            if (TitleFont != null && !string.IsNullOrEmpty(Title))
            {
                var titleSize = Vector2.Ceiling(TitleFont.MeasureString(Title));
                var titleWidth = (int)titleSize.X + TitlePadding.Horizontal;
                var titleHeight = (int)titleSize.Y + TitlePadding.Vertical;
                titleArea = new Rectangle(framePadding, framePadding, titleWidth, titleHeight);
                return new Padding(16, framePadding + titleHeight, 24, framePadding);
            }
            else
            {
                return new Padding(16, framePadding, 24, framePadding);
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            Close?.Invoke(this, e);
        }
    }
}
