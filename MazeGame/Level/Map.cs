using MazeGame.Primitives;
using MazeGame.Primitives.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace MazeGame.Level
{
    internal abstract class Map
    {
        protected readonly ObservableList<Entity> _Entities;

        private PlayerEntity _Player;

        public Map(string name)
        {
            Name = name;
            _Entities = new ObservableList<Entity>();
            _Entities.ListClearing += OnEntitiesListClearing;
            _Entities.ListChanged += OnEntitiesListChanged;
        }

        #region Properties

        public PlayerEntity Player
        {
            get => _Player;
            set
            {
                if (_Player != value)
                {
                    if (_Player != null) _Entities.Remove(_Player);
                    _Player = value;
                    if (_Player != null) _Entities.Insert(0, _Player);
                }
            }
        }

        public string Name { get; }

        public abstract int TileWidth { get; }
        public abstract int TileHeight { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public int PixelWidth => Width * TileWidth;
        public int PixelHeight => Height * TileHeight;

        public Point Size => new Point(Width, Height);
        public Point MapPixelSize => new Point(Width * TileWidth, Height * TileHeight);

        #endregion

        public event EventHandler<EntityLocationChangedEventArgs> EntityLocationChanged;

        public abstract Point GetPlayerStart(string fromLevelName);
        public abstract bool CheckTeleport(Point worldLocation, out string newMapName);
        
        public void Update(GameTime gameTime)
        {
            foreach (var entity in _Entities) entity.Update(gameTime, this);
            OnUpdate(gameTime);
        }

        protected abstract bool CheckLocation(Point pt);

        protected virtual void OnUpdate(GameTime gameTime) { }

        public abstract void Render(SpriteBatch spriteBatch, EntityManager entityManager, Point scrollOffset, Point clientSize, int fade);

        public bool CanMoveTo(Entity entity, Point pt)
        {
            if (pt.X < 0 || pt.Y < 0 || pt.X >= PixelWidth || pt.Y >= PixelHeight) return false;
            if (FindEntityAt(GetEntityTileLocation(pt), out Entity e) && e != entity) return false;
            return CheckLocation(pt);
        }

        public bool FindEntityAt(Point tileLoc, out Entity entity)
        {
            entity = _Entities.FirstOrDefault(e => GetEntityTileLocation(e.Location) == tileLoc);
            return entity != null;
        }

        public Point GetEntityTileLocation(Point pt) => new Point(pt.X / TileWidth, pt.Y / TileHeight);

        public Point GetCenterOfTile(Point pt) => new Point(pt.X * TileWidth + TileWidth / 2, pt.Y * TileHeight - TileHeight / 2);

        private void OnEntitiesListClearing(object sender, EventArgs e)
        {
            foreach (var entity in _Entities) entity.LocationChanged -= OnEntityLocationChanged;
        }

        private void OnEntitiesListChanged(object sender, ListChangedEventArgs<Entity> args)
        {
            switch (args.Type)
            {
                case ListChangedType.Add:
                    args.NewItem.LocationChanged += OnEntityLocationChanged;
                    break;
                case ListChangedType.Remove:
                    args.OldItem.LocationChanged -= OnEntityLocationChanged;
                    break;
                case ListChangedType.Replace:
                    args.OldItem.LocationChanged -= OnEntityLocationChanged;
                    args.NewItem.LocationChanged += OnEntityLocationChanged;
                    break;
            }
        }

        private void OnEntityLocationChanged(object sender, EventArgs e)
        {
            var entity = (Entity)sender;
            EntityLocationChanged?.Invoke(this, new EntityLocationChangedEventArgs(entity, entity == Player));
        }
    }
}
