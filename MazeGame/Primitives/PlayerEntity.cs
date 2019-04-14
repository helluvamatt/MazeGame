using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MazeGame.Primitives
{
    internal class PlayerEntity : MovableEntity
    {
        private const int TILE_WIDTH = 32;
        private const int TILE_HEIGHT = 64;
        private const int TILE_PADDING_X = 16;
        private const int ANIM_STEP_COUNT = 9;

        public const string SPRITE_BODY_MALE = "gfx/walkcycle/BODY_male";
        public const string SPRITE_LEGS_PANTS_GREENISH = "gfx/walkcycle/LEGS_pants_greenish";
        public const string SPRITE_TORSO_CHAIN_ARMOR = "gfx/walkcycle/TORSO_chain_armor_torso";
        public const string SPRITE_HEAD_HAIR_BLONDE = "gfx/walkcycle/HEAD_hair_blonde";
        public const string SPRITE_FEET_SHOES_BROWN = "gfx/walkcycle/FEET_shoes_brown";

        private static readonly Point _SpriteSize = new Point(TILE_WIDTH, TILE_HEIGHT);

        private readonly string[] _SpriteKeys;

        public PlayerEntity(string[] spriteKeys, Point initialLocation, Direction initialFacing = Direction.South) : base(initialLocation, initialFacing)
        {
            _SpriteKeys = spriteKeys ?? throw new ArgumentNullException(nameof(spriteKeys));
        }

        public override void AdvanceAnimation()
        {
            var nextStep = AnimationStep + 1;
            if (nextStep >= ANIM_STEP_COUNT) nextStep = 1;
            AnimationStep = nextStep;
        }

        public override void ResetAnimation()
        {
            AnimationStep = 0;
        }

        public override void Render(SpriteBatch sb, EntityManager entityManager, Point offset, int fade)
        {
            var filter = new Color(fade, fade, fade);
            int row;
            switch (Facing) // TODO Diagonal sprites?
            {
                case Direction.North: row = 0; break;
                case Direction.West: row = 1; break;
                case Direction.South: row = 2; break;
                case Direction.East: row = 3; break;
                default: throw new ArgumentOutOfRangeException(nameof(Facing));
            }
            var srcRect = new Rectangle(AnimationStep * (TILE_WIDTH + TILE_PADDING_X * 2) + TILE_PADDING_X, row * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
            var destRect = GetBoundingRectangle(Location - offset);
            foreach (var key in _SpriteKeys)
            {
                var texture = entityManager.GetEntitySprite(key);
                sb.Draw(texture, destRect, srcRect, filter);
            }
        }

        public override Rectangle GetBoundingRectangle(Point pt) => new Rectangle(pt.X - _SpriteSize.X / 2, pt.Y - _SpriteSize.Y, _SpriteSize.X, _SpriteSize.Y);
    }
}
