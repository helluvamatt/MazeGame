namespace MazeGame.Level
{
    internal class Space
    {
        public static Space Create(int x, int y, int baseTile, bool canWalk)
        {
            return new Space(x, y, baseTile, canWalk);
        }

        public Space WithOverlays(int overlayY, int? tileId1, int? tileId2)
        {
            OverlayY = overlayY;
            OverlayTile1 = tileId1;
            OverlayTile2 = tileId2;
            return this;
        }

        private Space(int x, int y, int baseTile, bool canWalk)
        {
            X = x;
            Y = y;
            OverlayY = y;
            BaseTile = baseTile;
            CanWalk = canWalk;
        }

        public int X { get; }
        public int Y { get; }
        public int OverlayY { get; set; }

        public bool CanWalk { get; }

        public int BaseTile { get; }
        public int? OverlayTile1 { get; set; }
        public int? OverlayTile2 { get; set; }
    }
}
