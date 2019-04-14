using Microsoft.Xna.Framework;
using System;

namespace MazeGame.Primitives
{
    [Flags]
    internal enum Direction : byte
    {
        None = 0,
        North = 1,
        South = 2,
        East = 4,
        West = 8
    }

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
                default: return Direction.None;
            }
        }

        public static bool Move(this Point start, Direction dir, Point size, out Point end) => Move(start, dir, size, 1, out end);

        public static bool Move(this Point start, Direction dir, Point size, int multiplier, out Point end)
        {
            dir.Delta(out int dX, out int dY);
            end = new Point(start.X + dX, start.Y + dY);
            return end.X > -1 && end.Y > -1 && end.X < size.X && end.Y < size.Y;
        }

        public static void Delta(this Direction dir, out int dX, out int dY)
        {
            dX = 0;
            dY = 0;
            if (dir.HasFlag(Direction.North)) dY--;
            if (dir.HasFlag(Direction.South)) dY++;
            if (dir.HasFlag(Direction.West)) dX--;
            if (dir.HasFlag(Direction.East)) dX++;
        }
    }
}
