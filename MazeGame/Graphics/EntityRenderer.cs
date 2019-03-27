using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeGame.Primitives;

namespace MazeGame.Graphics
{
    internal class EntityRenderer : IDisposable
    {
        private readonly ContentManager _ContentManager;
        private readonly Dictionary<string, Texture2D> _LoadedTextures;

        public EntityRenderer(ContentManager contentManager)
        {
            _ContentManager = contentManager;
            _LoadedTextures = new Dictionary<string, Texture2D>();
        }

        public void Dispose()
        {
            foreach (var kvp in _LoadedTextures)
            {
                kvp.Value.Dispose();
            }
            _LoadedTextures.Clear();
        }

        public PlayerEntity CreateDefaultPlayer(Point startLoc, Point startLocTile)
        {
            var sprites = new string[] { PlayerEntity.SPRITE_BODY_MALE, PlayerEntity.SPRITE_LEGS_PANTS_GREENISH, PlayerEntity.SPRITE_TORSO_CHAIN_ARMOR, PlayerEntity.SPRITE_HEAD_HAIR_BLONDE, PlayerEntity.SPRITE_FEET_SHOES_BROWN };
            foreach (var sprite in sprites)
            {
                if (!_LoadedTextures.ContainsKey(sprite))
                {
                    _LoadedTextures.Add(sprite, _ContentManager.Load<Texture2D>(sprite));
                }
            }
            return new PlayerEntity(sprites, startLoc, startLocTile);
        }

        public void RenderEntity(SpriteBatch sb, Entity entity, Point offset)
        {
            var srcRect = entity.SpriteTile;
            var destRect = new Rectangle(entity.Location.X - entity.SpriteSize.X / 2 - offset.X, entity.Location.Y - entity.SpriteSize.Y / 2 - offset.Y, entity.SpriteSize.X, entity.SpriteSize.Y);
            foreach (var key in entity.SpriteKeys)
            {
                if (_LoadedTextures.TryGetValue(key, out Texture2D texture))
                {
                    sb.Draw(texture, destRect, srcRect, Color.White);
                }
            }
        }
    }
}
