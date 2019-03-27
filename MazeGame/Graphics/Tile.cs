using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MazeGame.Graphics
{
    internal class Tile
    {
        public Tile(int id, Rectangle bounds)
        {
            ID = id;
            Bounds = bounds;
        }

        public int ID { get; }

        public Rectangle Bounds { get; }
    }
}
