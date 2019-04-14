using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGame.UI
{
    internal class Menu : Control
    {
        private static readonly Rectangle CARET_REGION = new Rectangle(96, 160, WindowManager.TILE_SIZE, WindowManager.TILE_SIZE);

        private readonly List<MenuItem> _Items;
        private readonly SpriteFont _ItemFont;

        public Menu(string name, SpriteFont itemFont) : base(name)
        {
            _ItemFont = itemFont ?? throw new ArgumentNullException(nameof(itemFont));
            _Items = new List<MenuItem>();
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

        public override bool CanFocus => true;

        public int ItemCount => _Items.Count;
        public IEnumerable<MenuItem> Items => _Items;
        public Padding Padding { get; set; }
        public int ItemSpacing { get; set; } = 16;

        public int SelectedIndex { get; private set; } = -1;
        public MenuItem SelectedItem => SelectedIndex > -1 ? _Items[SelectedIndex] : null;

        public override Point GetPreferredSize(Point availableSpace)
        {
            // TODO Support scrolling when selecting items and the available height is less than what we need
            var itemSizes = Items.Select(item => Vector2.Ceiling(_ItemFont.MeasureString(item.Label)).ToPoint()).ToList();
            int width = itemSizes.Max(sz => sz.X) + Padding.Horizontal + WindowManager.TILE_SIZE + ItemSpacing;
            int height = ItemCount * WindowManager.TILE_SIZE + Padding.Vertical;
            return new Point(width, height);
        }

        protected override void OnLayout(WindowManager uIRenderer)
        {
            int itemX = Bounds.X + Padding.Left;
            int itemY = Bounds.Y + Padding.Top;
            foreach (var menuItem in Items)
            {
                var pt = new Point(itemX, itemY);
                menuItem.SelectRegion = new Rectangle(pt, new Point(WindowManager.TILE_SIZE, WindowManager.TILE_SIZE));
                var textSize = _ItemFont.MeasureString(menuItem.Label);
                menuItem.LabelPosition = new Vector2(itemX + WindowManager.TILE_SIZE + ItemSpacing, itemY + (WindowManager.TILE_SIZE - textSize.Y) / 2);
                menuItem.Bounds = new Rectangle(pt, new Point(Bounds.Width - Padding.Horizontal, WindowManager.TILE_SIZE));
                itemY += WindowManager.TILE_SIZE;
            }
        }

        protected override void OnRender(WindowManager uIRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            sb.Begin();
            foreach (var menuItem in Items)
            {
                sb.DrawString(_ItemFont, menuItem.Label, menuItem.LabelPosition, menuItem.Enabled ? Color.White : Color.DimGray);
            }

            if (SelectedItem != null) sb.Draw(uiTexture, SelectedItem.SelectRegion, CARET_REGION, Color.White);
            sb.End();
        }

        public override void OnKeyDown(Keys key, bool repeat)
        {
            switch (key)
            {
                case Keys.Up:
                    {
                        int i = SelectedIndex;
                        do i--;
                        while (i > -1 && !_Items[i].Enabled);
                        if (i > -1 && _Items[i].Enabled) SelectedIndex = i;
                    }
                    break;
                case Keys.Down:
                    {
                        int i = SelectedIndex;
                        do i++;
                        while (i < _Items.Count && !_Items[i].Enabled);
                        if (i < _Items.Count && _Items[i].Enabled) SelectedIndex = i;
                    }
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (!repeat) SelectedItem?.Action?.Invoke();
                    break;
            }
        }

        public override void OnMouseMove(Point point)
        {
            var item = _Items.FirstOrDefault(mi => mi.Enabled && mi.Bounds.Contains(point));
            if (item != null) SelectedIndex = _Items.IndexOf(item);
        }

        public override void OnMouseUp(Point point, MouseButton button)
        {
            var item = _Items.FirstOrDefault(mi => mi.Enabled && mi.Bounds.Contains(point));
            if (item != null)
            {
                SelectedIndex = _Items.IndexOf(item);
                SelectedItem?.Action?.Invoke();
            }
        }

        public override void OnShown()
        {
            SelectFirstEnabledItem();
        }
    }

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
        public Rectangle Bounds { get; set; }
    }
}
