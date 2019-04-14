using System;
using System.Collections.Generic;
using System.Linq;
using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace MazeGame.Level
{
    internal class TileMap : Map
    {
        private const string LAYER_META = "Meta";
        private const string LAYER_BASE = "Base";
        private const string LAYER_PROP_ELEVATION = "Elevation";
        private const string OBJECT_TYPE_SPAWN = "Spawn";
        private const string OBJECT_TYPE_TELEPORT = "LevelTeleport";
        private const string OBJECT_TYPE_WALL = "Wall";
        private const string OBJECT_TYPE_SIGN = "Sign";
        private const string OBJECT_SPAWN_PROP_LEVELFROMNAME = "LevelFromName";
        private const string OBJECT_TELEPORT_PROP_LEVELNAME = "LevelName";
        private const string OBJECT_SIGN_PROP_TEXT = "Text";
        private const string OBJECT_SIGN_PROP_TYPE = "Type";

        private static readonly string[] BASE_LAYER_NAMES = { "Base", "Terrain Overlay 1", "Terrain Overlay 2" };
        private static readonly string[] OVERLAY_LAYER_NAMES = { "Items 1", "Items 2", "Structures 1", "Structures 2", "Structure Shadows", "Structures 3", "Structures 4", "Foliage 1", "Foliage 2", "Foliage 3", "Foliage Top", "Items Top" };

        private readonly ContentManager _ContentManager;
        private readonly TiledMap _TiledMap;
        private readonly Point _SpawnPoint;
        private readonly Dictionary<string, Point> _SpawnPoints;
        private readonly List<Teleport> _Teleports;

        public TileMap(string name, TiledMap tiledMap, ContentManager contentManager) : base(name)
        {
            _TiledMap = tiledMap ?? throw new ArgumentNullException(nameof(tiledMap));
            _ContentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            _Teleports = new List<Teleport>();
            _SpawnPoints = new Dictionary<string, Point>();

            var metaLayer = _TiledMap.GetLayer<TiledMapObjectLayer>(LAYER_META);

            foreach (var spawn in metaLayer.Objects.Where(obj => obj.Type == OBJECT_TYPE_SPAWN))
            {
                var rawLocation = spawn.Position.ToPoint();
                if (spawn.Properties.TryGetValue(OBJECT_SPAWN_PROP_LEVELFROMNAME, out string levelFromName) && !string.IsNullOrEmpty(levelFromName)) _SpawnPoints.Add(levelFromName, rawLocation);
                else _SpawnPoint = rawLocation;
            }

            foreach (var signObj in metaLayer.Objects.Where(obj => obj.Type == OBJECT_TYPE_SIGN))
            {
                var rawLocation = signObj.Position.ToPoint();
                var text = Content.Strings.ResourceManager.GetString(signObj.Properties[OBJECT_SIGN_PROP_TEXT]);
                var type = signObj.Properties.ContainsKey(OBJECT_SIGN_PROP_TYPE) ? int.Parse(signObj.Properties[OBJECT_SIGN_PROP_TYPE]) : 0;
                _Entities.Add(new SignEntity(rawLocation, text, type));
            }

            foreach (var teleportObj in metaLayer.Objects.Where(obj => obj.Type == OBJECT_TYPE_TELEPORT))
            {
                if (teleportObj.Properties.ContainsKey(OBJECT_TELEPORT_PROP_LEVELNAME))
                {
                    var rawLocation = teleportObj.Position.ToPoint();
                    var rawSize = new Point((int)teleportObj.Size.Width, (int)teleportObj.Size.Height);
                    var toWorld = teleportObj.Properties[OBJECT_TELEPORT_PROP_LEVELNAME];
                    _Teleports.Add(new Teleport(new Rectangle(rawLocation, rawSize), toWorld));
                }
            }
        }

        public override int TileWidth => _TiledMap.TileWidth;
        public override int TileHeight => _TiledMap.TileHeight;
        public override int Width => _TiledMap.Width;
        public override int Height => _TiledMap.Height;

        public override Point GetPlayerStart(string fromLevelName)
        {
            if (!string.IsNullOrEmpty(fromLevelName) && _SpawnPoints.TryGetValue(fromLevelName, out Point spawnLoc)) return spawnLoc;
            return _SpawnPoint;
        }

        public override bool CheckTeleport(Point worldLocation, out string newMapName)
        {
            var found = _Teleports.FirstOrDefault(teleport => teleport.Region.Contains(worldLocation));
            newMapName = found?.ToLevel;
            return found != null;
        }

        public override void Render(SpriteBatch sb, EntityManager entityManager, Point offset, Point size, int maxLevel)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            var filter = new Color(maxLevel, maxLevel, maxLevel, 255);

            var entities = _Entities.ToDictionary(e => GetEntityTileLocation(e.Location));

            // Draw tiles, culling as necessary
            int startX = Math.Max(offset.X / TileWidth, 0);
            int startY = Math.Max(offset.Y / TileHeight, 0);
            int endX = Math.Min(Width, (offset.X + size.X) / TileWidth + 1);
            int endY = Math.Min(Height, (offset.Y + size.Y) / TileHeight + 1);
            int scX, scY;

            // Draw base terrain layers
            var baseLayers = BASE_LAYER_NAMES.Select(layerName => _TiledMap.GetLayer<TiledMapTileLayer>(layerName)).ToArray();
            for (int y = startY; y < endY && y < Height; y++)
            {
                scY = (y * TileHeight) - offset.Y;
                for (int x = startX; x < endX; x++)
                {
                    scX = (x * TileWidth) - offset.X;
                    var destRect = new Rectangle(scX, scY, TileWidth, TileHeight);
                    foreach (var layer in baseLayers)
                    {
                        if (layer.TryGetTile(x, y, out TiledMapTile? tile) && tile.HasValue && !tile.Value.IsBlank)
                        {
                            var tileset = _TiledMap.GetTilesetByTileGlobalIdentifier(tile.Value.GlobalIdentifier);
                            var srcRect = tileset.GetTileRegion(tile.Value.GlobalIdentifier - tileset.FirstGlobalIdentifier);
                            sb.Draw(tileset.Texture, destRect, srcRect, filter);
                        }
                    }
                }
            }

            // Draw entities and elevated tiles
            var overlayLayers = OVERLAY_LAYER_NAMES.Select(layerName => _TiledMap.GetLayer<TiledMapTileLayer>(layerName)).ToArray();
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (entities.TryGetValue(new Point(x, y), out Entity entity))
                    {
                        entity.Render(sb, entityManager, offset, maxLevel);
                    }

                    scX = (x * TileWidth) - offset.X;
                    foreach(var layer in overlayLayers)
                    {
                        int yOffset = int.Parse(layer.Properties[LAYER_PROP_ELEVATION]);
                        for (int row = y - yOffset; row < endY; row++)
                        {
                            scY = (row * TileHeight) - offset.Y;
                            var destRect = new Rectangle(scX, scY, TileWidth, TileHeight);
                            if (layer.TryGetTile(x, row, out TiledMapTile? tile) && tile.HasValue && !tile.Value.IsBlank)
                            {
                                var tileset = _TiledMap.GetTilesetByTileGlobalIdentifier(tile.Value.GlobalIdentifier);
                                var srcRect = tileset.GetTileRegion(tile.Value.GlobalIdentifier - tileset.FirstGlobalIdentifier);
                                sb.Draw(tileset.Texture, destRect, srcRect, filter);
                            }
                        }
                    }
                }
            }
        }

        protected override bool CheckLocation(Point location)
        {
            var layer = _TiledMap.GetLayer<TiledMapObjectLayer>(LAYER_META);
            var walls = layer.Objects.Where(obj => obj.Type == OBJECT_TYPE_WALL).ToList();
            return walls.OfType<TiledMapRectangleObject>().All(obj => RectangleHitTest(location, obj)) && walls.OfType<TiledMapPolygonObject>().All(obj => PolygonHitTest(location, obj));
        }

        private bool RectangleHitTest(Point location, TiledMapRectangleObject obj)
        {
            var shape = new Rectangle(obj.Position.ToPoint(), new Point((int)obj.Size.Width, (int)obj.Size.Height));
            return !shape.Contains(location);
        }

        private bool PolygonHitTest(Point location, TiledMapPolygonObject obj)
        {
            var shape = new MonoGame.Extended.Shapes.Polygon(obj.Points.Select(pt => new Vector2(obj.Position.X + pt.X, obj.Position.Y + pt.Y)));
            return !shape.Contains(location.ToVector2());
        }

        private class Teleport
        {
            public Teleport(Rectangle region, string toLevel)
            {
                Region = region;
                ToLevel = toLevel;
            }

            public Rectangle Region { get; }

            public string ToLevel { get; }
        }
    }
}
