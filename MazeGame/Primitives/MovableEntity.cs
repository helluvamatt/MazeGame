using MazeGame.Level;
using Microsoft.Xna.Framework;
using System;

namespace MazeGame.Primitives
{
    internal abstract class MovableEntity : Entity
    {
        private const int MOVEMENT_MULTIPLIER = 4;

        private static readonly TimeSpan MOVE_DELAY = TimeSpan.FromSeconds(1.0 / 25.0); // 25hz

        private Direction _CurrentMovement;

        private TimeSpan _NextMoveTime;

        public MovableEntity(Point initialLocation, Direction initialFacing) : base(initialLocation, initialFacing)
        {
            _CurrentMovement = Direction.None;
        }

        public override void Update(GameTime gameTime, Map map)
        {
            if (gameTime.TotalGameTime > _NextMoveTime && _CurrentMovement != Direction.None)
            {
                if (ComputeAllowedMovement(map, out Point newLoc))
                {
                    AdvanceAnimation();
                    Location = newLoc;
                }
                else ResetAnimation();
                _NextMoveTime = gameTime.TotalGameTime + MOVE_DELAY;
            }

            base.Update(gameTime, map);
        }

        public void ClearMovement()
        {
            if (_CurrentMovement != Direction.None) ResetAnimation();
            _CurrentMovement = Direction.None;
        }

        public void BeginMovement(Direction direction)
        {
            _CurrentMovement |= direction;
        }

        public void EndMovement(Direction direction)
        {
            _CurrentMovement &= ~direction;
            if (_CurrentMovement == Direction.None) ResetAnimation();
        }

        // TODO Simple path-finding
        public void MoveTo(Point location)
        {

        }

        private bool ComputeAllowedMovement(Map map, out Point newLoc)
        {
            _CurrentMovement.Delta(out int dX, out int dY);
            var targetLoc = new Point(Location.X + dX * MOVEMENT_MULTIPLIER, Location.Y + dY * MOVEMENT_MULTIPLIER);
            newLoc = Location;
            bool result = false;
            if (dX != 0 && map.CanMoveTo(this, new Point(targetLoc.X, Location.Y)))
            {
                result = true;
                newLoc.X = targetLoc.X;
            }
            if (dY != 0 && map.CanMoveTo(this, new Point(Location.X, targetLoc.Y)))
            {
                result = true;
                newLoc.Y = targetLoc.Y;
            }

            // Compute Facing
            // Checking X first, only checking Y if we are not moving horizontally, we don't have full diagonal sprites, so we will move diagonally with the horizontal sprites
            if (newLoc.X < Location.X) Facing = Direction.West;
            else if (newLoc.X > Location.X) Facing = Direction.East;
            else
            {
                if (newLoc.Y < Location.Y) Facing = Direction.North;
                else if (newLoc.Y > Location.Y) Facing = Direction.South;
            }

            return result;
        }
    }
}
