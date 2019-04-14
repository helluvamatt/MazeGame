using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGame.UI
{
    internal class WindowManager : IDisposable, IKeyListener
    {
        public const int TILE_SIZE = 32;

        private static readonly TimeSpan KEY_DELAY = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan KEY_REPEAT = TimeSpan.FromSeconds(1.0 / 20.0); // 20hz
        
        private readonly ContentManager _ContentManager;
        private readonly HashSet<Keys> _PressedKeys;
        private readonly List<Window> _Windows;

        private Texture2D _UITexture;
        private Point _ClientSize;

        public WindowManager(ContentManager contentManager)
        {
            _ContentManager = contentManager;
            _PressedKeys = new HashSet<Keys>();
            _Windows = new List<Window>();
        }

        public void LoadContent()
        {
            _UITexture = _ContentManager.Load<Texture2D>(Content.Assets.Gfx.ui);
        }

        public void OpenWindow(Window window)
        {
            window.Close += OnWindowClose;
            window.Visible = true;
            window.OnShown();
            _Windows.Add(window);
            Layout(_ClientSize);
        }

        public void CloseWindow()
        {
            CloseWindow(_Windows.LastOrDefault());
        }

        public void CloseWindow(Window window)
        {
            _PressedKeys.Clear();
            if (window != null)
            {
                window.Visible = false;
                window.OnClosed();
                _Windows.Remove(window);
            }
        }

        public bool Layout(Point clientSize)
        {
            _ClientSize = clientSize;
            var focusedWindow = _Windows.LastOrDefault();
            if (focusedWindow != null)
            {
                var preferredSize = focusedWindow.GetPreferredSize(clientSize);
                Rectangle bounds;
                switch (focusedWindow.Dock)
                {
                    case DockMode.Center:
                        bounds = GetCenteredRectangle(preferredSize, clientSize);
                        break;
                    case DockMode.LowerThird:
                        bounds = GetLowerThird(clientSize, focusedWindow.Margin);
                        break;
                    case DockMode.Fill:
                        bounds = new Rectangle(focusedWindow.Margin.Left, focusedWindow.Margin.Top, clientSize.X - focusedWindow.Margin.Horizontal, clientSize.Y - focusedWindow.Margin.Vertical);
                        break;
                    default:
                        throw new ArgumentException($"Invalid DockMode enum value: {focusedWindow.Dock}", nameof(focusedWindow.Dock));
                }

                return focusedWindow.PerformLayout(this, bounds);
            }
            return false;
        }

        public void Update(GameTime gameTime)
        {
            var focusedWindow = _Windows.LastOrDefault();
            if (focusedWindow != null) focusedWindow.OnTick(gameTime);
        }

        public void Render(SpriteBatch sb)
        {
            foreach (var window in _Windows) window.Render(this, sb, _UITexture);
        }

        public void HandleMouse(GameTime gameTime, MouseState oldMouseState, MouseState newMouseState)
        {
            var focusedWindow = _Windows.LastOrDefault();
            if (focusedWindow != null)
            {
                var point = newMouseState.Position;

                if (oldMouseState.Position != point)
                {
                    focusedWindow.OnMouseMove(point);
                }
                HandleMouseButton(focusedWindow, point, oldMouseState.LeftButton, newMouseState.LeftButton, MouseButton.Left);
                HandleMouseButton(focusedWindow, point, oldMouseState.RightButton, newMouseState.RightButton, MouseButton.Right);
                HandleMouseButton(focusedWindow, point, oldMouseState.MiddleButton, newMouseState.MiddleButton, MouseButton.Middle);
                HandleMouseButton(focusedWindow, point, oldMouseState.XButton1, newMouseState.XButton1, MouseButton.XButton1);
                HandleMouseButton(focusedWindow, point, oldMouseState.XButton2, newMouseState.XButton2, MouseButton.XButton2);
            }
        }

        #region IKeyListener

        public void KeyDown(Keys key, bool repeat)
        {
            _PressedKeys.Add(key);
            var focusedWindow = _Windows.LastOrDefault();
            if (focusedWindow != null)
            {
                focusedWindow.OnKeyDown(key, repeat);
            }
        }

        public void KeyUp(Keys key)
        {
            bool doPress = _PressedKeys.Remove(key);
            var focusedWindow = _Windows.LastOrDefault();
            if (focusedWindow != null)
            {
                focusedWindow.OnKeyUp(key);
                if (doPress) focusedWindow.OnKeyPress(key);
            }
        }

        public void KeyPress(Keys key)
        {
            // No-op, handled elsewhere
        }

        #endregion

        public void Dispose()
        {
            _UITexture.Dispose();
        }

        private void HandleMouseButton(Window window, Point point, ButtonState oldState, ButtonState newState, MouseButton button)
        {
            if (newState != oldState)
            {
                if (newState == ButtonState.Pressed) window.OnMouseDown(point, button);
                else window.OnMouseUp(point, button);
            }
        }

        public static Rectangle GetCenteredRectangle(Point size, Point available)
        {
            var x = (available.X - size.X) / 2;
            var y = (available.Y - size.Y) / 2;
            return new Rectangle(x, y, size.X, size.Y);
        }

        public static Rectangle GetLowerThird(Point clientSize, Padding margin)
        {
            return new Rectangle(margin.Left, clientSize.Y * 2 / 3 + margin.Top, clientSize.X - margin.Horizontal, clientSize.Y / 3 - margin.Vertical);
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            if (sender is Window window) CloseWindow(window);
        }
    }
}
