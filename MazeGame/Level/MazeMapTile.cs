using Microsoft.Xna.Framework;

namespace MazeGame.Level
{
    internal class MazeMapTile
    {
        public MazeMapTile(int id, Rectangle bounds)
        {
            ID = id;
            Bounds = bounds;
        }

        public int ID { get; }

        public Rectangle Bounds { get; }
    }
}
