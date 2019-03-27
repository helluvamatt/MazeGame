using MazeGame.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGame.Level
{
    internal abstract class Map
    {
        private readonly Dictionary<int, Tile> _Tiles;

        protected readonly int _VPathCount;
        protected readonly int _HPathCount;
        protected readonly List<Row> _Rows;
        
        protected Map(int vPathCount, int hPathCount)
        {
            _VPathCount = vPathCount;
            _HPathCount = hPathCount;
            _Rows = new List<Row>();
            _Tiles = new Dictionary<int, Tile>();
            MapTiles();
        }

        public Tile GetTile(int id) => _Tiles.ContainsKey(id) ? _Tiles[id] : null;

        public bool TryGetTile(int id, out Rectangle bounds)
        {
            var tile = GetTile(id);
            if (tile != null)
            {
                bounds = tile.Bounds;
                return true;
            }
            bounds = Rectangle.Empty;
            return false;
        }

        public Space GetSpace(int x, int y) => y > -1 && y < _Rows.Count ? _Rows[y].GetCell(x) : null;

        public bool TryGetSpace(int x, int y, out Space space)
        {
            space = GetSpace(x, y);
            return space != null;
        }

        public IEnumerable<Space> GetOverlays(int x, int y)
        {
            var yCount = _Rows.Count;
            for (int y2 = 0; y2 < yCount; y2++)
            {
                if (TryGetSpace(x, y2, out Space space) && space.OverlayY == y)
                {
                    yield return space;
                }
            }
        }

        public IEnumerable<int> GetOverlayRows(int startY, int endY)
        {
            var window = _Rows.Skip(startY).Take(endY - startY + 1).ToList();
            int min = window.Min(r => r.MinOverlay);
            int max = window.Max(r => r.MaxOverlay);
            return Enumerable.Range(min, max - min + 1);
        }

        public int Height => _Rows.Count;
        public int Width => _Rows.Count > 0 ? _Rows[0].CellCount : 0;
        public Point Size => new Point(Width, Height);

        public abstract string Texture { get; }
        public abstract int TileWidth { get; }
        public abstract int TileHeight { get; }
        public Point MapPixelSize => new Point(Width * TileWidth, Height * TileHeight);

        protected abstract void MapTiles();

        protected abstract void Layout(Random rng, MazeCell[,] maze);
        
        public abstract Point GetPlayerStart();

        public Point GetEntityLocation(Point pt) => new Point(pt.X * TileWidth + TileWidth / 2, pt.Y * TileHeight - TileHeight / 2);

        protected abstract bool CheckTile(Point pt);

        public bool CanMoveTo(Point pt)
        {
            if (pt.X < 0 || pt.Y < 0 || pt.X >= Width || pt.Y >= Height) return false;
            return CheckTile(pt);
        }

        public void Generate()
        {
            var rng = new Random();

            // Randomized iterative depth-first traversal used to create labyrinths
            var cells = new MazeCell[_VPathCount, _HPathCount];
            for (int y = 0; y < _VPathCount; y++)
            {
                for (int x = 0; x < _HPathCount; x++)
                {
                    cells[y, x] = new MazeCell(x, y);
                }
            }
            var stack = new Stack<MazeCell>();
            stack.Push(cells[0, 0]);

            while (stack.Any())
            {
                var pathstart = stack.Pop();
                var pathstack = new Stack<MazeCell>();
                pathstack.Push(pathstart);

                while (pathstack.Any())
                {
                    var cell = pathstack.Pop();
                    cell.Visited = true;

                    var neighbors = new List<MazeCell>();
                    if (cell.X > 0 && !cells[cell.Y, cell.X - 1].Visited) neighbors.Add(cells[cell.Y, cell.X - 1]);
                    if (cell.Y > 0 && !cells[cell.Y - 1, cell.X].Visited) neighbors.Add(cells[cell.Y - 1, cell.X]);
                    if (cell.X < _HPathCount - 1 && !cells[cell.Y, cell.X + 1].Visited) neighbors.Add(cells[cell.Y, cell.X + 1]);
                    if (cell.Y < _VPathCount - 1 && !cells[cell.Y + 1, cell.X].Visited) neighbors.Add(cells[cell.Y + 1, cell.X]);

                    if (neighbors.Any())
                    {
                        var neighbor = neighbors[rng.Next(neighbors.Count)];
                        if (neighbor.X == cell.X && neighbor.Y == cell.Y - 1) { neighbor.South = false; cell.North = false; }
                        else if (neighbor.X == cell.X && neighbor.Y == cell.Y + 1) { neighbor.North = false; cell.South = false; }
                        else if (neighbor.X == cell.X - 1 && neighbor.Y == cell.Y) { neighbor.East = false; cell.West = false; }
                        else if (neighbor.X == cell.X + 1 && neighbor.Y == cell.Y) { neighbor.West = false; cell.East = false; }

                        stack.Push(neighbor);
                        pathstack.Push(neighbor);
                    }
                }
            }

            Layout(rng, cells);
        }

        protected void AddTile(ref int id, Rectangle bounds)
        {
            _Tiles.Add(id, new Tile(id, bounds));
            id++;
        }

        protected class Row
        {
            private readonly List<Space> _Cells;

            public Row(IEnumerable<Space> cells)
            {
                _Cells = new List<Space>();
                _Cells.AddRange(cells);

                CellCount = _Cells.Count;
                MinOverlay = _Cells.Min(c => c.OverlayY);
                MaxOverlay = _Cells.Max(c => c.OverlayY);
            }

            public int CellCount { get; }
            public int MinOverlay { get; }
            public int MaxOverlay { get; }

            public IEnumerable<Space> Cells => _Cells;

            public Space GetCell(int x) => x > -1 && x < _Cells.Count ? _Cells[x] : null;
        }

        protected class MazeCell
        {
            public MazeCell(int x, int y)
            {
                X = x;
                Y = y;
                North = true;
                South = true;
                East = true;
                West = true;
            }

            public int X { get; }
            public int Y { get; }

            // Visited?
            public bool Visited { get; set; }

            // Wall states
            public bool North { get; set; }
            public bool South { get; set; }
            public bool East { get; set; }
            public bool West { get; set; }
        }
    }
}
