using MazeGame.Level;
using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;

namespace MazeGame
{
    internal sealed class LevelManager
    {
        private static readonly Dictionary<string, Type> MazeMapTypes = new Dictionary<string, Type>()
        {
            { "CornMaze", typeof(CornMap) },
        };

        private static readonly Dictionary<string, string> TileMapTypes = new Dictionary<string, string>()
        {
            { "town", Content.Assets.Levels.town },
        };

        private readonly Dictionary<string, TileMap> _TileMaps;
        private readonly ContentManager _ContentManager;

        public LevelManager(ContentManager contentManager)
        {
            _TileMaps = new Dictionary<string, TileMap>();
            _ContentManager = contentManager;
        }

        public event EventHandler<EntityLocationChangedEventArgs> EntityLocationChanged;

        public Map CurrentMap { get; private set; }

        public void NavigateTo(string name, out Point startLocation)
        {
            PlayerEntity player = null;
            string oldName = null;
            if (CurrentMap != null)
            {
                player = CurrentMap.Player;
                oldName = CurrentMap.Name;
                CurrentMap.EntityLocationChanged -= OnEntityLocationChanged;
            }
            CurrentMap = FindOrCreateMap(name);
            if (CurrentMap == null) throw new ArgumentException($"Invalid map name: \"{name}\"");
            CurrentMap.EntityLocationChanged += OnEntityLocationChanged;
            startLocation = CurrentMap.GetPlayerStart(oldName);
            if (player != null)
            {
                CurrentMap.Player = player;
                CurrentMap.Player.Location = startLocation;
            }
        }

        private void OnEntityLocationChanged(object sender, EntityLocationChangedEventArgs args)
        {
            EntityLocationChanged?.Invoke(this, args);
        }

        private Map FindOrCreateMap(string name)
        {
            if (_TileMaps.TryGetValue(name, out TileMap tileMap)) return tileMap;
            if (TileMapTypes.TryGetValue(name, out string resource)) return CreateTileMap(name, resource);
            return CreateMazeMap(name, 16, 12);
        }

        private Map CreateTileMap(string name, string resource)
        {
            var tiledMap = _ContentManager.Load<TiledMap>(resource);
            var map = new TileMap(name, tiledMap, _ContentManager);
            _TileMaps.Add(name, map);
            return map;
        }

        private Map CreateMazeMap(string name, int width, int height)
        {
            if (MazeMapTypes.TryGetValue(name, out Type type))
            {
                var ctor = type.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(ContentManager) });
                var mazeMap = (MazeMap)ctor.Invoke(new object[] { width, height, _ContentManager });
                mazeMap.Generate();
                return mazeMap;
            }
            throw new ArgumentException($"Unknown map name: {name}");
        }
    }
}
