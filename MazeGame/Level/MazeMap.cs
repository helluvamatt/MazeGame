using System;
using System.Collections.Generic;
using System.Linq;
using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame.Level
{
    internal abstract class MazeMap : Map
    {
        private readonly Dictionary<int, MazeMapTile> _Tiles;
        private readonly Texture2D _Texture;

        protected readonly int _VPathCount;
        protected readonly int _HPathCount;
        protected readonly List<Row> _Rows;

        protected MazeMap(string name, int vPathCount, int hPathCount, ContentManager contentManager) : base(name)
        {
            _VPathCount = vPathCount;
            _HPathCount = hPathCount;
            _Texture = contentManager.Load<Texture2D>(Texture);
            _Rows = new List<Row>();
            _Tiles = new Dictionary<int, MazeMapTile>();
            MapTiles();
        }

        public override int Height => _Rows.Count;
        public override int Width => _Rows.Count > 0 ? _Rows[0].CellCount : 0;

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

        public override void Render(SpriteBatch sb, EntityManager entityManager, Point offset, Point size, int fade)
        {
            var entityRows = new Dictionary<int, List<Entity>>();
            foreach (var entity in _Entities)
            {
                var tileLoc = GetEntityTileLocation(entity.Location);
                if (!entityRows.ContainsKey(tileLoc.Y)) entityRows.Add(tileLoc.Y, new List<Entity>());
                entityRows[tileLoc.Y].Add(entity);
            }
            var entities = new Dictionary<int, IEnumerable<Entity>>();
            foreach (var kvp in entityRows)
            {
                entities.Add(kvp.Key, kvp.Value);
            }

            var filter = new Color(fade, fade, fade, 255);

            // Draw tiles, culling as necessary
            int startX = offset.X / TileWidth;
            int startY = offset.Y / TileHeight;
            if (startX < 1) startX--;
            if (startY < 1) startY--;
            int endX = Math.Min(Width, (offset.X + size.X) / TileWidth + 1);
            int endY = Math.Min(Height, (offset.Y + size.Y) / TileHeight + 1);
            int scX, scY;
            for (int y = startY; y < endY; y++)
            {
                scY = (y * TileHeight) - offset.Y;
                for (int x = startX; x < endX; x++)
                {
                    scX = (x * TileWidth) - offset.X;
                    var destRect = new Rectangle(scX, scY, TileWidth, TileHeight);
                    if (TryGetSpace(x, y, out Space space) && TryGetTile(space.BaseTile, out Rectangle srcRect))
                    {
                        sb.Draw(_Texture, destRect, srcRect, filter);
                    }
                }
            }

            foreach (int y in GetVisibleOverlayRows(offset, size, out startY, out endY))
            {
                if (entities.TryGetValue(y, out IEnumerable<Entity> entitiesInRow))
                {
                    foreach (var entity in entitiesInRow)
                    {
                        entity.Render(sb, entityManager, offset, fade);
                    }
                }

                for (int x = startX; x < endX; x++)
                {

                    var yCount = Height;
                    for (int y2 = 0; y2 < yCount; y2++)
                    {
                        if (TryGetSpace(x, y2, out Space overlaySpace) && overlaySpace.OverlayY == y)
                        {
                            if (overlaySpace.Y < startY || overlaySpace.Y > endY) continue;

                            scX = overlaySpace.X * TileWidth - offset.X;
                            scY = overlaySpace.Y * TileHeight - offset.Y;
                            var destRect = new Rectangle(scX, scY, TileWidth, TileHeight);

                            if (overlaySpace.OverlayTile1.HasValue && TryGetTile(overlaySpace.OverlayTile1.Value, out Rectangle srcRect))
                            {
                                sb.Draw(_Texture, destRect, srcRect, filter);
                            }
                            if (overlaySpace.OverlayTile2.HasValue && TryGetTile(overlaySpace.OverlayTile2.Value, out Rectangle srcRect2) && (!overlaySpace.OverlayTile1.HasValue || overlaySpace.OverlayTile1.Value != overlaySpace.OverlayTile2.Value))
                            {
                                sb.Draw(_Texture, destRect, srcRect2, filter);
                            }
                        }
                    }
                }
            }
        }

        protected abstract void MapTiles();
        protected abstract void Layout(Random rng, MazeCell[,] maze);
        protected abstract string Texture { get; }

        protected void AddTile(ref int id, Rectangle bounds)
        {
            _Tiles.Add(id, new MazeMapTile(id, bounds));
            id++;
        }

        private MazeMapTile GetTile(int id) => _Tiles.ContainsKey(id) ? _Tiles[id] : null;

        protected bool TryGetTile(int id, out Rectangle bounds)
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

        protected Space GetSpace(int x, int y) => y > -1 && y < Height ? _Rows[y].GetCell(x) : null;

        protected bool TryGetSpace(int x, int y, out Space space)
        {
            space = GetSpace(x, y);
            return space != null;
        }

        private IEnumerable<int> GetVisibleOverlayRows(Point offset, Point size, out int startY, out int endY)
        {
            startY = offset.Y / TileHeight;
            endY = Math.Min(Height, (offset.Y + size.Y) / TileHeight + 1);
            var window = _Rows.Skip(startY).Take(endY - startY + 1).ToList();
            int min = window.Min(r => r.MinOverlay);
            int max = window.Max(r => r.MaxOverlay);
            return Enumerable.Range(min, max - min + 1);
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
