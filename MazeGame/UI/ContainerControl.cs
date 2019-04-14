using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MazeGame.UI
{
    internal abstract class ContainerControl : Control
    {
        protected ContainerControl(string name) : base(name) { }

        public abstract int ControlCount { get; }
        public int FocusedIndex { get; private set; } = -1;
        public Control FocusedControl => GetControl(FocusedIndex);

        public override bool CanFocus => GetFocusableControls().Any();

        private bool _LShiftPressed;
        private bool _RShiftPressed;

        private Control _MouseOverControl;

        public override void OnKeyUp(Keys key)
        {
            switch (key)
            {
                case Keys.LeftShift:
                    _LShiftPressed = false;
                    FocusedControl?.OnKeyUp(key);
                    break;
                case Keys.RightShift:
                    _RShiftPressed = false;
                    FocusedControl?.OnKeyUp(key);
                    break;
                default:
                    FocusedControl?.OnKeyUp(key);
                    break;
            }
        }

        public override void OnKeyDown(Keys key, bool repeat)
        {
            switch (key)
            {
                case Keys.LeftShift:
                    _LShiftPressed = true;
                    FocusedControl?.OnKeyDown(key, repeat);
                    break;
                case Keys.RightShift:
                    _RShiftPressed = true;
                    FocusedControl?.OnKeyDown(key, repeat);
                    break;
                default:
                    FocusedControl?.OnKeyDown(key, repeat);
                    break;
            }
        }

        public override void OnKeyPress(Keys key)
        {
            if (key == Keys.Tab) MoveFocus(_LShiftPressed || _RShiftPressed);
            else FocusedControl?.OnKeyPress(key);
        }

        public override void OnMouseDown(Point point, MouseButton button)
        {
            GetControlUnderPoint(point)?.OnMouseDown(point, button);
        }

        public override void OnMouseMove(Point point)
        {
            var control = GetControlUnderPoint(point);
            if (_MouseOverControl != control)
            {
                if (_MouseOverControl != null) _MouseOverControl.OnMouseOut(point);
                _MouseOverControl = control;
                if (_MouseOverControl != null) _MouseOverControl.OnMouseOver(point);
            }
            control?.OnMouseMove(point);
        }

        public override void OnMouseUp(Point point, MouseButton button)
        {
            var ctrl = GetControlUnderPoint(point);
            if (ctrl != null)
            {
                if (ctrl.CanFocus && !ctrl.IsFocused) FocusedIndex = GetControlIndex(ctrl);
                ctrl.OnMouseUp(point, button);
            }
        }

        public override void OnShown()
        {
            foreach (var ctrl in EnumerateControls()) ctrl.OnShown();
        }

        public override void OnClosed()
        {
            foreach (var ctrl in EnumerateControls()) ctrl.OnClosed();
        }

        public override void OnTick(GameTime gameTime)
        {
            foreach (var ctrl in EnumerateControls()) ctrl.OnTick(gameTime);
        }

        public Control FindControl(string name)
        {
            for (int i = 0; i < ControlCount; i++)
            {
                var ctrl = GetControl(i);
                if (ctrl.Name == name) return ctrl;
                if (ctrl is ContainerControl container)
                {
                    var match = container.FindControl(name);
                    if (match != null) return match;
                }
            }
            return null;
        }

        protected abstract Control GetControl(int index);
        protected abstract int GetControlIndex(Control control);

        protected virtual Control GetControlUnderPoint(Point point)
        {
            for (int i = 0; i < ControlCount; i++)
            {
                var ctrl = GetControl(i);
                if (ctrl.Bounds.Contains(point)) return ctrl;
            }
            return null;
        }

        protected virtual void MoveFocus(bool reverse)
        {
            if (FocusedControl is ContainerControl container && container.CanMoveFocus(reverse))
            {
                container.MoveFocus(reverse);
            }
            else
            {
                if (FocusedControl != null)
                {
                    FocusedControl.IsFocused = false;
                    if (FocusedControl is ContainerControl oldContainer) oldContainer.ClearFocus();
                }
                FocusedIndex = FindNextFocus(FocusedIndex, reverse);
                if (FocusedControl != null)
                {
                    FocusedControl.IsFocused = true;
                    if (FocusedControl is ContainerControl newContainer) newContainer.MoveFocus(reverse);
                }
            }
        }

        protected virtual bool CanMoveFocus(bool reverse) => reverse ? FocusedIndex > 0 : FocusedIndex < ControlCount - 1;

        protected virtual void ClearFocus()
        {
            if (FocusedControl != null) FocusedControl.IsFocused = false;
            FocusedIndex = -1;
        }

        private int FindNextFocus(int start, bool reverse)
        {
            var focusables = GetFocusableControls().ToList();
            if (!focusables.Any()) return start;
            int i = start > -1 ? focusables.IndexOf(start) : 0;
            if (reverse)
            {
                i--;
                if (i < 0) i = focusables.Count - 1;
            }
            else
            {
                i++;
                if (i >= focusables.Count) i = 0;
            }
            return focusables[i];
        }

        private IEnumerable<int> GetFocusableControls()
        {
            for (int i = 0; i < ControlCount; i++)
            {
                var ctrl = GetControl(i);
                if (ctrl.CanFocus) yield return i;
            }
        }

        private IEnumerable<Control> EnumerateControls()
        {
            for (int i = 0; i < ControlCount; i++) yield return GetControl(i);
        }
    }
}
