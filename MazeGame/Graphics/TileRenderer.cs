using MazeGame.Level;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Graphics
{
    internal class TileRenderer
    {
        private readonly ContentManager _ContentManager;
        private readonly Dictionary<string, Texture2D> _TextureCache;

        public TileRenderer(ContentManager contentManager)
        {
            _ContentManager = contentManager;
            _TextureCache = new Dictionary<string, Texture2D>();
        }

        public void RenderBase(SpriteBatch sb, Map map, Point offset, Point size, Color filter)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (map == null) throw new ArgumentNullException(nameof(map));

            var texture = LoadTexture(map.Texture);

            // Draw tiles, culling as necessary
            int startX = offset.X / map.TileWidth;
            int startY = offset.Y / map.TileHeight;
            int endX = Math.Min(map.Width, (offset.X + size.X) / map.TileWidth + 1);
            int endY = Math.Min(map.Height, (offset.Y + size.Y) / map.TileHeight + 1);
            int scX, scY;
            for (int y = startY; y < endY; y++)
            {
                scY = (y * map.TileHeight) - offset.Y;
                for (int x = startX; x < endX; x++)
                {
                    scX = (x * map.TileWidth) - offset.X;
                    var destRect = new Rectangle(scX, scY, map.TileWidth, map.TileHeight);
                    if (map.TryGetSpace(x, y, out Space space) && map.TryGetTile(space.BaseTile, out Rectangle srcRect))
                    {
                        sb.Draw(texture, destRect, srcRect, filter);
                    }
                }
            }
        }

        public void RenderOverlaysRow(SpriteBatch sb, Map map, Point offset, Point size, int y, int startY, int endY, Color filter)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (map == null) throw new ArgumentNullException(nameof(map));

            var texture = LoadTexture(map.Texture);

            // Draw tiles, culling as necessary
            int startX = offset.X / map.TileWidth;
            int endX = Math.Min(map.Width, (offset.X + size.X) / map.TileWidth + 1);
            
            for (int x = startX; x < endX; x++)
            {
                foreach (var overlaySpace in map.GetOverlays(x, y))
                {
                    if (overlaySpace.Y < startY || overlaySpace.Y > endY) continue;

                    int scX = overlaySpace.X * map.TileWidth - offset.X;
                    int scY = overlaySpace.Y * map.TileHeight - offset.Y;
                    var destRect = new Rectangle(scX, scY, map.TileWidth, map.TileHeight);

                    if (overlaySpace.OverlayTile1.HasValue && map.TryGetTile(overlaySpace.OverlayTile1.Value, out Rectangle srcRect))
                    {
                        sb.Draw(texture, destRect, srcRect, filter);
                    }
                    if (overlaySpace.OverlayTile2.HasValue && map.TryGetTile(overlaySpace.OverlayTile2.Value, out Rectangle srcRect2) && (!overlaySpace.OverlayTile1.HasValue || overlaySpace.OverlayTile1.Value != overlaySpace.OverlayTile2.Value))
                    {
                        sb.Draw(texture, destRect, srcRect2, filter);
                    }
                }
            }
        }

        public IEnumerable<int> GetVisibleOverlayRows(Map map, Point offset, Point size, out int startY, out int endY)
        {
            startY = offset.Y / map.TileHeight;
            endY = Math.Min(map.Height, (offset.Y + size.Y) / map.TileHeight + 1);
            return map.GetOverlayRows(startY, endY);
        }

        private Texture2D LoadTexture(string textureKey)
        {
            if (!_TextureCache.TryGetValue(textureKey, out Texture2D texture))
            {
                texture = _ContentManager.Load<Texture2D>(textureKey);
                _TextureCache.Add(textureKey, texture);
            }
            return texture;
        }
    }
}
