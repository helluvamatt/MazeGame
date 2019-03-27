using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Primitives
{
    internal enum Direction : byte { North, South, East, West }

    internal static class DirectionExtensions
    {
        public static Direction Opposite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
            }
            throw new ArgumentOutOfRangeException(nameof(dir));
        }

        public static bool Move(this Point start, Direction dir, Point size, out Point end)
        {
            switch (dir)
            {
                case Direction.North: end = new Point(start.X, start.Y - 1); break;
                case Direction.South: end = new Point(start.X, start.Y + 1); break;
                case Direction.East: end = new Point(start.X + 1, start.Y); break;
                case Direction.West: end = new Point(start.X - 1, start.Y); break;
                default: throw new ArgumentOutOfRangeException(nameof(dir));
            }
            return end.X > -1 && end.Y > -1 && end.X < size.X && end.Y < size.Y;
        }
    }
}
