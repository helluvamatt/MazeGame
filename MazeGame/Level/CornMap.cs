using MazeGame.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MazeGame.Level
{
    internal class CornMap : Map
    {
        public CornMap(int vPathCount, int hPathCount) : base(vPathCount, hPathCount) { }

        public override Point GetPlayerStart() => new Point(2, 2);

        public override string Texture => TEXTURE;
        public override int TileWidth => TILE_SIZE;
        public override int TileHeight => TILE_SIZE;

        protected override bool CheckTile(Point pt) => TryGetSpace(pt.X, pt.Y, out Space space) && space.CanWalk;

        protected override void MapTiles()
        {
            int id = 0;
            for (int y = 0; y < TILE_SIZE; y++)
            {
                for (int x = 0; x < TILE_SIZE; x++)
                {
                    AddTile(ref id, new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
                }
            }

            // Special tiles for multi-sprites
            AddTile(ref id, new Rectangle(960, 0, 64, 160)); // Tree, conifer
            AddTile(ref id, new Rectangle(768, 384, 96, 96)); // Dome bush, north
            AddTile(ref id, new Rectangle(864, 284, 96, 96)); // Dome bush, south
            AddTile(ref id, new Rectangle(512, 512, 96, 96)); // Bridge, with railings
            AddTile(ref id, new Rectangle(480, 384, 64, 96)); // Easter Island head
            AddTile(ref id, new Rectangle(448, 384, TILE_SIZE, 96)); // Column
            AddTile(ref id, new Rectangle(608, 480, TILE_SIZE, 96)); // Column, ruin
            AddTile(ref id, new Rectangle(640, 480, TILE_SIZE, 96)); // Column, cracked
            AddTile(ref id, new Rectangle(864, 928, 64, 96)); // Tree, small
            AddTile(ref id, new Rectangle(928, 896, 96, 128)); // Tree, large
            AddTile(ref id, new Rectangle(544, 284, TILE_SIZE, 64)); // Birdbath
        }

        protected override void Layout(Random rng, MazeCell[,] maze)
        {
            int rowCount = _VPathCount * 4 + 2;
            int cellCount = _HPathCount * 4 + 1;

            // Special handling for the first row
            var firstRow = new List<Space>();
            for (int baseX = 0; baseX < cellCount; baseX++) firstRow.Add(Space.Create(baseX, 0, DIRT_PLOWED, false).WithOverlays(0, CORN_TOP_NEAR_E_W, null));
            _Rows.Add(new Row(firstRow));

            // Project generated maze onto grid of tiles
            for (int y = 0; y < _VPathCount; y++)
            {
                var row1 = new List<Space>();
                var row2 = new List<Space>();
                var row3 = new List<Space>();
                var row4 = new List<Space>();

                int baseY = 1 + y * 4;
                for (int x = 0; x < _HPathCount; x++)
                {
                    var cell = maze[y, x];
                    int baseX = x * 4;

                    bool hasPathNorth = y > 0 && !cell.North;
                    bool hasPathSouth = y < _VPathCount - 1 && !cell.South;
                    bool hasPathWest = x > 0 && !cell.West;
                    bool hasPathEast = x < _HPathCount - 1 && !cell.East;

                    int nwCorner = DIRT_TRANSITION_W_N;
                    if (hasPathNorth && hasPathWest) nwCorner = DIRT_TRANSITION_NW;
                    else if (hasPathNorth) nwCorner = DIRT_TRANSITION_W;
                    else if (hasPathWest) nwCorner = DIRT_TRANSITION_N;

                    int neCorner = DIRT_TRANSITION_E_N;
                    if (hasPathNorth && hasPathEast) neCorner = DIRT_TRANSITION_NE;
                    else if (hasPathNorth) neCorner = DIRT_TRANSITION_E;
                    else if (hasPathEast) neCorner = DIRT_TRANSITION_N;

                    int swCorner = DIRT_TRANSITION_W_S;
                    if (hasPathSouth && hasPathWest) swCorner = DIRT_TRANSITION_SW;
                    else if (hasPathSouth) swCorner = DIRT_TRANSITION_W;
                    else if (hasPathWest) swCorner = DIRT_TRANSITION_S;

                    int seCorner = DIRT_TRANSITION_E_S;
                    if (hasPathSouth && hasPathEast) seCorner = DIRT_TRANSITION_SE;
                    else if (hasPathSouth) seCorner = DIRT_TRANSITION_E;
                    else if (hasPathEast) seCorner = DIRT_TRANSITION_S;

                    
                    row1.Add(Space.Create(baseX + 0, baseY + 0, DIRT_PLOWED, false));
                    row1.Add(Space.Create(baseX + 1, baseY + 0, hasPathNorth ? DIRT_TRANSITION_W : DIRT_PLOWED, hasPathNorth));
                    row1.Add(Space.Create(baseX + 2, baseY + 0, hasPathNorth ? GetRandomGrass(rng) : DIRT_PLOWED, hasPathNorth));
                    row1.Add(Space.Create(baseX + 3, baseY + 0, hasPathNorth ? DIRT_TRANSITION_E : DIRT_PLOWED, hasPathNorth));
                    row2.Add(Space.Create(baseX + 0, baseY + 1, hasPathWest ? DIRT_TRANSITION_N : DIRT_PLOWED, hasPathWest));
                    row2.Add(Space.Create(baseX + 1, baseY + 1, nwCorner, true));
                    row2.Add(Space.Create(baseX + 2, baseY + 1, hasPathNorth ? GetRandomGrass(rng) : DIRT_TRANSITION_N, true));
                    row2.Add(Space.Create(baseX + 3, baseY + 1, neCorner, true));
                    row3.Add(Space.Create(baseX + 0, baseY + 2, hasPathWest ? GetRandomGrass(rng) : DIRT_PLOWED, hasPathWest));
                    row3.Add(Space.Create(baseX + 1, baseY + 2, hasPathWest ? GetRandomGrass(rng) : DIRT_TRANSITION_W, true));
                    row3.Add(Space.Create(baseX + 2, baseY + 2, GetRandomGrass(rng), true));
                    row3.Add(Space.Create(baseX + 3, baseY + 2, hasPathEast ? GetRandomGrass(rng) : DIRT_TRANSITION_E, true));
                    row4.Add(Space.Create(baseX + 0, baseY + 3, hasPathWest ? DIRT_TRANSITION_S : DIRT_PLOWED, hasPathWest));
                    row4.Add(Space.Create(baseX + 1, baseY + 3, swCorner, true));
                    row4.Add(Space.Create(baseX + 2, baseY + 3, hasPathSouth ? GetRandomGrass(rng) : DIRT_TRANSITION_S, true));
                    row4.Add(Space.Create(baseX + 3, baseY + 3, seCorner, true));
                }

                // Special handling for the last column
                row1.Add(Space.Create(cellCount - 1, baseY + 0, DIRT_PLOWED, false));
                row2.Add(Space.Create(cellCount - 1, baseY + 1, DIRT_PLOWED, false));
                row3.Add(Space.Create(cellCount - 1, baseY + 2, DIRT_PLOWED, false));
                row4.Add(Space.Create(cellCount - 1, baseY + 3, DIRT_PLOWED, false));

                _Rows.Add(new Row(row1));
                _Rows.Add(new Row(row2));
                _Rows.Add(new Row(row3));
                _Rows.Add(new Row(row4));
            }

            // Special handling for first and last row (always dirt)
            var lastRow = new List<Space>();
            for (int baseX = 0; baseX < cellCount; baseX++) lastRow.Add(Space.Create(baseX, rowCount - 1, DIRT_PLOWED, false).WithOverlays(rowCount - 1, CORN_TOP_FILL_E_W, null));
            _Rows.Add(new Row(lastRow));

            const int CORN_STATE_NONE = 0;
            const int CORN_STATE_PLANTED = 1;
            const int CORN_STATE_TOPPED = 2;
            const int CORN_STATE_ENDING = 3;

            const int CORN_TILE_NONE = 0;
            const int CORN_TILE_BASE = 1;
            const int CORN_TILE_TOP_NEAR = 2;
            const int CORN_TILE_TOP_FILL = 3;
            const int CORN_TILE_TOP_FAR = 4;

            // Compute corn tiles
            var cornTiles = new int[rowCount, cellCount];
            for (int baseX = 0; baseX < cellCount; baseX++)
            {
                int cornState = CORN_STATE_TOPPED;
                for (int baseY = rowCount - 1; baseY > -1; baseY--)
                {
                    var space = GetSpace(baseX, baseY);
                    if (cornState == CORN_STATE_TOPPED)
                    {
                        cornState = space.BaseTile == DIRT_PLOWED ? CORN_STATE_TOPPED : CORN_STATE_ENDING;
                        cornTiles[baseY, baseX] = CORN_TILE_TOP_FILL;
                    }
                    else if (cornState == CORN_STATE_ENDING)
                    {
                        cornState = CORN_STATE_NONE;
                        cornTiles[baseY, baseX] = CORN_TILE_TOP_FAR;
                    }
                    else if (cornState == CORN_STATE_PLANTED)
                    {
                        cornState = space.BaseTile == DIRT_PLOWED ? CORN_STATE_TOPPED : CORN_STATE_ENDING;
                        cornTiles[baseY, baseX] = CORN_TILE_TOP_NEAR;
                    }
                    else if (space.BaseTile == DIRT_PLOWED)
                    {
                        cornState = CORN_STATE_PLANTED;
                        cornTiles[baseY, baseX] = CORN_TILE_BASE;
                    }
                    else
                    {
                        cornTiles[baseY, baseX] = CORN_TILE_NONE;
                    }
                }
            }

            // Finally, set corn overlays
            for (int baseY = 0; baseY < rowCount; baseY++)
            {
                for (int baseX = 0; baseX < cellCount; baseX++)
                {
                    var space = GetSpace(baseX, baseY);
                    var tile = cornTiles[baseY, baseX];
                    var tileW = baseX > 0 ? cornTiles[baseY, baseX - 1] : CORN_TILE_TOP_FILL;
                    var tileE = baseX < cellCount - 1 ? cornTiles[baseY, baseX + 1] : CORN_TILE_TOP_FILL;

                    // Overlay 1: Neighbors affect the corn "behind" this tile
                    if (tile != CORN_TILE_NONE)
                    {
                        if (tileW == CORN_TILE_BASE && tileE == CORN_TILE_BASE) space.OverlayTile1 = CORN_BOTTOM_E_W;
                        else if (tileW == CORN_TILE_BASE) space.OverlayTile1 = CORN_BOTTOM_W;
                        else if (tileE == CORN_TILE_BASE) space.OverlayTile1 = CORN_BOTTOM_E;

                        else if (tileW == CORN_TILE_TOP_NEAR && tileE == CORN_TILE_TOP_NEAR) space.OverlayTile1 = CORN_TOP_NEAR_E_W;
                        else if (tileW == CORN_TILE_TOP_NEAR) space.OverlayTile1 = CORN_TOP_NEAR_W;
                        else if (tileE == CORN_TILE_TOP_NEAR) space.OverlayTile1 = CORN_TOP_NEAR_E;

                        else if (tileW == CORN_TILE_TOP_FAR && tileE == CORN_TILE_TOP_FAR) space.OverlayTile1 = CORN_TOP_FAR_E_W;
                        else if (tileW == CORN_TILE_TOP_FAR) space.OverlayTile1 = CORN_TOP_FAR_W;
                        else if (tileE == CORN_TILE_TOP_FAR) space.OverlayTile1 = CORN_TOP_FAR_E;
                    }

                    // Overlay 2: The current space's actual growth
                    if (tile == CORN_TILE_BASE && tileW != CORN_TILE_NONE && tileE != CORN_TILE_NONE) space.OverlayTile2 = CORN_BOTTOM_E_W;
                    else if (tile == CORN_TILE_BASE && tileW != CORN_TILE_NONE) space.OverlayTile2 = CORN_BOTTOM_W;
                    else if (tile == CORN_TILE_BASE && tileE != CORN_TILE_NONE) space.OverlayTile2 = CORN_BOTTOM_E;
                    else if (tile == CORN_TILE_BASE) space.OverlayTile2 = CORN_BOTTOM_NONE;
                    
                    else if (tile == CORN_TILE_TOP_NEAR && tileW != CORN_TILE_TOP_FILL && tileE != CORN_TILE_TOP_FILL && tileW != CORN_TILE_TOP_NEAR && tileE != CORN_TILE_TOP_NEAR) space.OverlayTile2 = CORN_TOP_NEAR_NONE;
                    else if (tile == CORN_TILE_TOP_NEAR && tileW != CORN_TILE_TOP_FILL && tileW != CORN_TILE_TOP_NEAR) space.OverlayTile2 = CORN_TOP_NEAR_E;
                    else if (tile == CORN_TILE_TOP_NEAR && tileE != CORN_TILE_TOP_FILL && tileE != CORN_TILE_TOP_NEAR) space.OverlayTile2 = CORN_TOP_NEAR_W;
                    else if (tile == CORN_TILE_TOP_NEAR) space.OverlayTile2 = CORN_TOP_NEAR_E_W;

                    else if (tile == CORN_TILE_TOP_FILL && tileW == CORN_TILE_TOP_FILL && tileE == CORN_TILE_TOP_FILL) space.OverlayTile2 = CORN_TOP_FILL_E_W;
                    else if (tile == CORN_TILE_TOP_FILL && tileW == CORN_TILE_TOP_FILL) space.OverlayTile2 = CORN_TOP_FILL_W;
                    else if (tile == CORN_TILE_TOP_FILL && tileE == CORN_TILE_TOP_FILL) space.OverlayTile2 = CORN_TOP_FILL_E;
                    else if (tile == CORN_TILE_TOP_FILL) space.OverlayTile2 = CORN_TOP_FILL_NONE;
                    
                    else if (tile == CORN_TILE_TOP_FAR && tileW == CORN_TILE_NONE && tileE == CORN_TILE_NONE) space.OverlayTile2 = CORN_TOP_FAR_NONE;
                    else if (tile == CORN_TILE_TOP_FAR && tileW == CORN_TILE_NONE) space.OverlayTile2 = CORN_TOP_FAR_E;
                    else if (tile == CORN_TILE_TOP_FAR && tileE == CORN_TILE_NONE) space.OverlayTile2 = CORN_TOP_FAR_W;
                    else if (tile == CORN_TILE_TOP_FAR) space.OverlayTile2 = CORN_TOP_FAR_E_W;

                    if (tile == CORN_TILE_TOP_FAR) space.OverlayY = space.Y + 2;
                    else if (tile == CORN_TILE_TOP_FILL || tile == CORN_TILE_TOP_NEAR) space.OverlayY = space.Y + 1;
                }
            }
        }

        private int GetRandomGrass(Random rng) => GRASS_TILES[rng.Next(GRASS_TILES.Length)];

        private const int TILE_SIZE = 32;
        private const string TEXTURE = "gfx/terrain_atlas";

        // Tile ID constants
        private const int GRASS = 3 * TILE_SIZE + 22;
        private const int GRASS_2 = 5 * TILE_SIZE + 23;
        private const int GRASS_3 = 5 * TILE_SIZE + 22;
        private const int GRASS_4 = 5 * TILE_SIZE + 21;

        private const int DIRT_TRANSITION_W_N = 15 * TILE_SIZE + 6;
        private const int DIRT_TRANSITION_E_N = 15 * TILE_SIZE + 7;
        private const int DIRT_TRANSITION_W_S = 16 * TILE_SIZE + 6;
        private const int DIRT_TRANSITION_E_S = 16 * TILE_SIZE + 7;

        private const int DIRT_TRANSITION_SE = 17 * TILE_SIZE + 5;
        private const int DIRT_TRANSITION_S = 17 * TILE_SIZE + 6;
        private const int DIRT_TRANSITION_SW = 17 * TILE_SIZE + 7;
        private const int DIRT_TRANSITION_E = 18 * TILE_SIZE + 5;
        private const int DIRT_PLOWED = 18 * TILE_SIZE + 6;
        private const int DIRT_TRANSITION_W = 18 * TILE_SIZE + 7;
        private const int DIRT_TRANSITION_NE = 19 * TILE_SIZE + 5;
        private const int DIRT_TRANSITION_N = 19 * TILE_SIZE + 6;
        private const int DIRT_TRANSITION_NW = 19 * TILE_SIZE + 7;

        private const int CORN_BOTTOM_NONE = 19 * TILE_SIZE + 8;
        private const int CORN_BOTTOM_E = 19 * TILE_SIZE + 9;
        private const int CORN_BOTTOM_E_W = 19 * TILE_SIZE + 10;
        private const int CORN_BOTTOM_W = 19 * TILE_SIZE + 11;
        private const int CORN_TOP_NEAR_NONE = 18 * TILE_SIZE + 8;
        private const int CORN_TOP_NEAR_E = 18 * TILE_SIZE + 9;
        private const int CORN_TOP_NEAR_E_W = 18 * TILE_SIZE + 10;
        private const int CORN_TOP_NEAR_W = 18 * TILE_SIZE + 11;
        private const int CORN_TOP_FILL_NONE = 17 * TILE_SIZE + 8;
        private const int CORN_TOP_FILL_E = 17 * TILE_SIZE + 9;
        private const int CORN_TOP_FILL_E_W = 17 * TILE_SIZE + 10;
        private const int CORN_TOP_FILL_W = 17 * TILE_SIZE + 11;
        private const int CORN_TOP_FAR_NONE = 16 * TILE_SIZE + 8;
        private const int CORN_TOP_FAR_E = 16 * TILE_SIZE + 9;
        private const int CORN_TOP_FAR_E_W = 16 * TILE_SIZE + 10;
        private const int CORN_TOP_FAR_W = 16 * TILE_SIZE + 11;
        private const int CORN_TOP_FILL_NW = 21 * TILE_SIZE + 6;
        private const int CORN_TOP_FILL_NE = 21 * TILE_SIZE + 7;
        private const int CORN_TOP_FILL_SW = 22 * TILE_SIZE + 6;
        private const int CORN_TOP_FILL_SE = 22 * TILE_SIZE + 7;

        private static readonly int[] GRASS_TILES = { GRASS, GRASS_2, GRASS_3, GRASS_4 };
    }
}
