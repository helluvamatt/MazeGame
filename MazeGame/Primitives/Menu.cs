using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Primitives
{
    internal class Menu
    {
        private readonly List<MenuItem> _Items;

        public Menu(string title, MenuType type)
        {
            _Items = new List<MenuItem>();
            Title = title;
            Type = type;
        }

        public Menu WithItem(string label, Action action, bool enabled)
        {
            _Items.Add(new MenuItem(label, action, enabled));
            return this;
        }

        public Menu SelectFirstEnabledItem()
        {
            SelectedIndex = _Items.FindIndex(mi => mi.Enabled);
            return this;
        }

        public string Title { get; }
        public MenuType Type { get; }

        public int ItemCount => _Items.Count;
        public IEnumerable<MenuItem> Items => _Items;

        public int SelectedIndex { get; private set; } = -1;
        public MenuItem SelectedItem => SelectedIndex > -1 ? _Items[SelectedIndex] : null;

        #region Layout parameters
        public Point ClientSize { get; set; }
        public int RowCount { get; set; }
        public int CellCount { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 TitlePosition { get; set; }
        #endregion

        public virtual void Move(Direction direction)
        {
            if (direction == Direction.North)
            {
                int i = SelectedIndex;
                do i--;
                while (i > -1 && !_Items[i].Enabled);
                if (i > -1 && _Items[i].Enabled) SelectedIndex = i;
            }
            else if (direction == Direction.South)
            {
                int i = SelectedIndex;
                do i++;
                while (i < _Items.Count && !_Items[i].Enabled);
                if (i < _Items.Count && _Items[i].Enabled) SelectedIndex = i;
            }
        }

        public virtual void ResetSelection() => SelectFirstEnabledItem();
    }

    internal enum MenuType { LargeScroll, MediumScroll, SmallScroll }

    internal class MenuItem
    {
        public MenuItem(string label, Action action) : this(label, action, true) { }

        public MenuItem(string label, Action action, bool enabled)
        {
            Label = label;
            Action = action;
            Enabled = enabled;
        }

        public string Label { get; }
        public Action Action { get; }
        public bool Enabled { get; set; }

        public Vector2 LabelPosition { get; set; }
        public Rectangle SelectRegion { get; set; }
    }
}
