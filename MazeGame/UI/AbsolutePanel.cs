using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame.UI
{
    internal class AbsolutePanel : ContainerControl
    {
        private readonly Point _Size;
        private readonly List<PositionedControl> _Controls;

        public override int ControlCount => _Controls.Count;

        public AbsolutePanel(string name, Point size) : base(name)
        {
            _Size = size;
            _Controls = new List<PositionedControl>();
        }

        public void AddControl(Control control, Point location)
        {
            _Controls.Add(new PositionedControl(control, location));
        }

        #region Control overrides

        public override Point GetPreferredSize(Point availableSpace) => _Size;

        protected override void OnLayout(WindowManager uIRenderer)
        {
            foreach (var ctrl in _Controls)
            {
                var ctrlSize = ctrl.Control.GetPreferredSize(Point.Zero);
                var ctrlBounds = new Rectangle(ctrl.Location + Bounds.Location, ctrlSize);
                ctrl.Control.PerformLayout(uIRenderer, ctrlBounds);
            }
        }

        protected override void OnRender(WindowManager uIRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            foreach (var ctrl in _Controls)
            {
                ctrl.Control.Render(uIRenderer, sb, uiTexture);
            }
        }

        protected override Control GetControl(int index)
        {
            if (index < 0) return null;
            if (index >= ControlCount) return null;
            return _Controls[index].Control;
        }

        protected override int GetControlIndex(Control control) => _Controls.FindIndex(pc => pc.Control == control);

        #endregion

        private class PositionedControl
        {
            public PositionedControl(Control control, Point location)
            {
                Control = control;
                Location = location;
            }

            public Control Control { get; }

            public Point Location { get; }
        }
    }
}
