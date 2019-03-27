using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Primitives
{
    internal class PlayerEntity : Entity
    {
        private const int TILE_WIDTH = 32;
        private const int TILE_HEIGHT = 64;
        private const int TILE_PADDING_X = 16;

        public const string SPRITE_BODY_MALE = "gfx/walkcycle/BODY_male";
        public const string SPRITE_LEGS_PANTS_GREENISH = "gfx/walkcycle/LEGS_pants_greenish";
        public const string SPRITE_TORSO_CHAIN_ARMOR = "gfx/walkcycle/TORSO_chain_armor_torso";
        public const string SPRITE_HEAD_HAIR_BLONDE = "gfx/walkcycle/HEAD_hair_blonde";
        public const string SPRITE_FEET_SHOES_BROWN = "gfx/walkcycle/FEET_shoes_brown";

        private static readonly Point _SpriteSize = new Point(TILE_WIDTH, TILE_HEIGHT);

        public PlayerEntity(string[] spriteKeys, Point initialLocation, Point initialLocationTile, Direction initialFacing = Direction.South) : base(9, spriteKeys, initialLocation, initialLocationTile, initialFacing)
        {

        }

        public override Rectangle SpriteTile
        {
            get
            {
                int row;
                switch (Facing)
                {
                    case Direction.North: row = 0; break;
                    case Direction.West: row = 1; break;
                    case Direction.South: row = 2; break;
                    case Direction.East: row = 3; break;
                    default: throw new ArgumentOutOfRangeException(nameof(Facing));
                }
                return new Rectangle(AnimationStep * (TILE_WIDTH + TILE_PADDING_X * 2) + TILE_PADDING_X, row * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
            }
        }

        public override Point SpriteSize => _SpriteSize;

        public override void AdvanceAnimation()
        {
            var nextStep = AnimationStep + 1;
            if (nextStep >= _AnimStepCount) nextStep = 1;
            AnimationStep = nextStep;
        }
    }
}
