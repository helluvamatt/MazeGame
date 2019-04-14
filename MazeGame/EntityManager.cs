using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using MazeGame.Primitives;

namespace MazeGame
{
    internal class EntityManager : IDisposable
    {
        private readonly ContentManager _ContentManager;
        private readonly Dictionary<string, Texture2D> _LoadedTextures;

        public EntityManager(ContentManager contentManager)
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

        public PlayerEntity CreateDefaultPlayer(Point startLoc)
        {
            var sprites = new string[] { PlayerEntity.SPRITE_BODY_MALE, PlayerEntity.SPRITE_LEGS_PANTS_GREENISH, PlayerEntity.SPRITE_TORSO_CHAIN_ARMOR, PlayerEntity.SPRITE_HEAD_HAIR_BLONDE, PlayerEntity.SPRITE_FEET_SHOES_BROWN };
            foreach (var sprite in sprites)
            {
                if (!_LoadedTextures.ContainsKey(sprite))
                {
                    _LoadedTextures.Add(sprite, _ContentManager.Load<Texture2D>(sprite));
                }
            }
            return new PlayerEntity(sprites, startLoc);
        }

        public Texture2D GetEntitySprite(string name)
        {
            if (!_LoadedTextures.TryGetValue(name, out Texture2D texture))
            {
                texture = _ContentManager.Load<Texture2D>(name);
                _LoadedTextures.Add(name, texture);
            }
            return texture;
        }
    }
}
