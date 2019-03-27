using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MazeGame.Primitives
{
    internal abstract class Entity
    {
        protected readonly int _AnimStepCount;

        public Entity(int animStepCount, string[] spriteKeys, Point initialLocation, Point initialLocationTile, Direction initialFacing = Direction.South)
        {
            _AnimStepCount = animStepCount;
            SpriteKeys = spriteKeys ?? throw new ArgumentNullException(nameof(spriteKeys));
            Location = initialLocation;
            LocationTile = initialLocationTile;
            Facing = initialFacing;
        }

        public Point LocationTile { get; set; }
        public Point Location { get; set; }

        public Direction Facing { get; set; }

        public int AnimationStep { get; protected set; }

        public string[] SpriteKeys { get; }

        public virtual void AdvanceAnimation()
        {
            var nextStep = AnimationStep + 1;
            if (nextStep >= _AnimStepCount) nextStep = 0;
            AnimationStep = nextStep;
        }

        public virtual void ResetAnimation()
        {
            AnimationStep = 0;
        }

        public abstract Rectangle SpriteTile { get; }

        public abstract Point SpriteSize { get; }
    }
}
