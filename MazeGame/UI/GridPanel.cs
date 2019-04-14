using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame.UI
{
    // TODO Support individual cell padding
    internal class GridPanel : ContainerControl
    {
        private readonly SortedList<CellLocation, Control> _Cells;
        
        public GridPanel(string name) : base(name)
        {
            _Cells = new SortedList<CellLocation, Control>();
        }

        public int CellSpacing { get; set; }

        public void AddControl(Control control, int row, int col)
        {
            AddControl(control, row, col, 1, 1);
            if (control.CanFocus && FocusedControl == null) MoveFocus(false);
        }

        public void AddControl(Control control, int row, int col, int rowSpan, int colSpan)
        {
            if (rowSpan < 1) throw new ArgumentOutOfRangeException(nameof(rowSpan));
            if (colSpan < 1) throw new ArgumentOutOfRangeException(nameof(colSpan));
            _Cells.Add(new CellLocation(row, col, rowSpan, colSpan), control);
        }

        public void SetCellAlign(Control control, Alignment hAlign, Alignment vAlign)
        {
            var i = _Cells.IndexOfValue(control);
            if (i > -1)
            {
                _Cells.Keys[i].HAlign = hAlign;
                _Cells.Keys[i].VAlign = vAlign;
            }
        }

        #region Control overrides

        public override Point GetPreferredSize(Point availableSpace)
        {
            if (!_Cells.Any()) return Point.Zero;

            var columnWidths = new Dictionary<int, int>();
            var rowHeights = new Dictionary<int, int>();
            
            foreach (var kvp in _Cells)
            {
                var ctrlSize = kvp.Value.GetPreferredSize(Point.Zero);
                int r = kvp.Key.Row;
                int availableY = 0;
                for (int rOffset = 0; rOffset < kvp.Key.RowSpan; rOffset++)
                {
                    availableY += rowHeights.ContainsKey(r + rOffset) ? rowHeights[r + rOffset] : 0;
                }
                if (availableY < ctrlSize.Y)
                {
                    int yToAdd = (ctrlSize.Y - availableY) / kvp.Key.RowSpan;
                    for (int rOffset = 0; rOffset < kvp.Key.RowSpan; rOffset++)
                    {
                        int existingHeight = rowHeights.ContainsKey(r + rOffset) ? rowHeights[r + rOffset] : 0;
                        rowHeights[r + rOffset] = existingHeight + yToAdd;
                    }
                }

                int c = kvp.Key.Col;
                int availableX = 0;
                for (int cOffset = 0; cOffset < kvp.Key.ColSpan; cOffset++)
                {
                    availableX += columnWidths.ContainsKey(c + cOffset) ? columnWidths[c + cOffset] : 0;
                }
                if (availableX < ctrlSize.X)
                {
                    int xToAdd = (ctrlSize.X - availableX) / kvp.Key.ColSpan;
                    for (int cOffset = 0; cOffset < kvp.Key.ColSpan; cOffset++)
                    {
                        int existingWidth = columnWidths.ContainsKey(c + cOffset) ? columnWidths[c + cOffset] : 0;
                        columnWidths[c + cOffset] = existingWidth + xToAdd;
                    }
                }
            }

            var width = columnWidths.Values.Sum() + (columnWidths.Count - 1) * CellSpacing;
            var height = rowHeights.Values.Sum() + (rowHeights.Count - 1) * CellSpacing;
            return new Point(width, height);
        }

        protected override void OnLayout(WindowManager uIRenderer)
        {
            var columnWidths = new Dictionary<int, int>();
            var rowHeights = new Dictionary<int, int>();

            // First pass: Measure cells
            foreach (var kvp in _Cells)
            {
                kvp.Key.Size = kvp.Value.GetPreferredSize(Point.Zero);
                int r = kvp.Key.Row;
                int availableY = 0;
                for (int rOffset = 0; rOffset < kvp.Key.RowSpan; rOffset++)
                {
                    availableY += rowHeights.ContainsKey(r + rOffset) ? rowHeights[r + rOffset] : 0;
                }
                if (availableY < kvp.Key.Size.Y)
                {
                    int yToAdd = (kvp.Key.Size.Y - availableY) / kvp.Key.RowSpan;
                    for (int rOffset = 0; rOffset < kvp.Key.RowSpan; rOffset++)
                    {
                        int existingHeight = rowHeights.ContainsKey(r + rOffset) ? rowHeights[r + rOffset] : 0;
                        rowHeights[r + rOffset] = existingHeight + yToAdd;
                    }
                }

                int c = kvp.Key.Col;
                int availableX = 0;
                for (int cOffset = 0; cOffset < kvp.Key.ColSpan; cOffset++)
                {
                    availableX += columnWidths.ContainsKey(c + cOffset) ? columnWidths[c + cOffset] : 0;
                }
                if (availableX < kvp.Key.Size.X)
                {
                    int xToAdd = (kvp.Key.Size.X - availableX) / kvp.Key.ColSpan;
                    for (int cOffset = 0; cOffset < kvp.Key.ColSpan; cOffset++)
                    {
                        int existingWidth = columnWidths.ContainsKey(c + cOffset) ? columnWidths[c + cOffset] : 0;
                        columnWidths[c + cOffset] = existingWidth + xToAdd;
                    }
                }
            }

            // Second pass: layout cells
            foreach (var kvp in _Cells)
            {
                int x = Bounds.X + kvp.Key.Col * CellSpacing + ComputeOffset(columnWidths, kvp.Key.Col);
                int y = Bounds.Y + kvp.Key.Row * CellSpacing + ComputeOffset(rowHeights, kvp.Key.Row);

                int availableWidth = columnWidths[kvp.Key.Col];
                if (kvp.Key.Size.X < availableWidth)
                {
                    int xExtra = availableWidth - kvp.Key.Size.X;
                    if (kvp.Key.HAlign == Alignment.Far) x += xExtra;
                    else if (kvp.Key.HAlign == Alignment.Middle) x += xExtra / 2;
                }

                int availableHeight = rowHeights[kvp.Key.Row];
                if (kvp.Key.Size.Y < availableHeight)
                {
                    int yExtra = availableHeight - kvp.Key.Size.Y;
                    if (kvp.Key.VAlign == Alignment.Far) y += yExtra;
                    else if (kvp.Key.VAlign == Alignment.Middle) y += yExtra / 2;
                }

                kvp.Value.PerformLayout(uIRenderer, new Rectangle(new Point(x, y), kvp.Key.Size));
            }
        }

        protected override void OnRender(WindowManager uIRenderer, SpriteBatch sb, Texture2D uiTexture)
        {
            foreach (var ctrl in _Cells.Values)
            {
                ctrl.Render(uIRenderer, sb, uiTexture);
            }
        }

        public override int ControlCount => _Cells.Count;

        protected override Control GetControl(int index)
        {
            if (index < 0) return null;
            if (index >= ControlCount) return null;
            return _Cells.Values[index];
        }

        protected override int GetControlIndex(Control control) => _Cells.Values.IndexOf(control);

        #endregion

        private int ComputeOffset(IDictionary<int, int> measurements, int index)
        {
            int result = 0;
            for (int i = 0; i < index; i++) if (measurements.TryGetValue(i, out int m)) result += m;
            return result;
        }

        private class CellLocation : IComparable<CellLocation>
        {
            public CellLocation(int row, int col, int rowSpan, int colSpan)
            {
                Row = row;
                Col = col;
                RowSpan = rowSpan;
                ColSpan = colSpan;
                HAlign = Alignment.Near;
                VAlign = Alignment.Near;
            }

            public int Row { get; }

            public int Col { get; }

            public Point Size { get; set; }

            public int RowSpan { get; }

            public int ColSpan { get; }

            public Alignment HAlign { get; set; }

            public Alignment VAlign { get; set; }

            public override bool Equals(object obj) => obj is CellLocation loc && loc.Row == Row && loc.Col == Col;

            public override int GetHashCode() => ((Row << 5) + Row) ^ Col;

            public int CompareTo(CellLocation other)
            {
                int rowCompare = Row.CompareTo(other.Row);
                if (rowCompare == 0) return Col.CompareTo(other.Col);
                return rowCompare;
            }
        }
    }
}
