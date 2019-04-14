using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MazeGame.UI
{
    internal abstract class Control
    {
        public Control(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsEnabled = true;
        }

        public string Name { get; }

        public Rectangle Bounds { get; private set; }
        public Point Size => Bounds.Size;
        public Point Location => Bounds.Location;

        public bool IsFocused { get; set; }
        public bool IsEnabled { get; set; }
        public virtual bool CanFocus => false;

        public bool PerformLayout(WindowManager windowManager, Rectangle controlBounds)
        {
            if (Bounds != controlBounds)
            {
                Bounds = controlBounds;
                OnLayout(windowManager);
                return true;
            }
            return false;
        }

        public void Render(WindowManager windowManager, SpriteBatch sb, Texture2D uiTexture)
        {
            OnRender(windowManager, sb, uiTexture);
        }

        #region Virtual methods

        public virtual void OnShown() { }

        public virtual void OnClosed() { }

        public virtual void OnKeyDown(Keys key, bool repeat) { }

        public virtual void OnKeyUp(Keys key) { }

        public virtual void OnKeyPress(Keys key) { }

        public virtual void OnMouseMove(Point point) { }

        public virtual void OnMouseDown(Point point, MouseButton button) { }

        public virtual void OnMouseUp(Point point, MouseButton button) { }

        public virtual void OnMouseOver(Point point) { }

        public virtual void OnMouseOut(Point point) { }

        public virtual void OnTick(GameTime gameTime) { }

        #endregion

        #region Abstract methods

        public abstract Point GetPreferredSize(Point availableSpace);

        protected abstract void OnLayout(WindowManager windowManager);

        protected abstract void OnRender(WindowManager windowManager, SpriteBatch sb, Texture2D uiTexture);

        #endregion
    }
}
