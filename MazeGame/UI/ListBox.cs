using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame.UI
{
    internal class ListBox : ContainerControl
    {
        private readonly Point _Size;
        private readonly List<Control> _Children;

        //private Point _ScrollOffset;

        public ListBox(string name) : this(name, Point.Zero) { }

        public ListBox(string name, Point size) : base(name)
        {
            _Size = size;
            _Children = new List<Control>();
        }

        public void Add(Control control)
        {
            _Children.Add(control);
        }

        public override int ControlCount => _Children.Count;
        protected override Control GetControl(int index) => index > -1 && index < _Children.Count ? _Children[index] : null;
        protected override int GetControlIndex(Control control) => _Children.IndexOf(control);

        public override Point GetPreferredSize(Point availableSpace)
        {
            // TODO Better measuring, account for availableSpace == Point.Zero
            return _Size != Point.Zero ? _Size : availableSpace;
        }
        
        protected override void OnLayout(WindowManager windowManager)
        {
            // TODO Layout items
        }

        protected override void OnRender(WindowManager windowManager, SpriteBatch sb, Texture2D uiTexture)
        {
            // TODO Render visible items
        }
    }
}
