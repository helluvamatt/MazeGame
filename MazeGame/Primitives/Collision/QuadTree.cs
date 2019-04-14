using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Primitives.Collision
{
    internal class QuadTree
    {
        private const int MAX_OBJECTS = 10;
        private const int MAX_LEVELS = 5;

        private readonly QuadTree _Parent;
        private readonly int _Level;
        private readonly List<Entity> _Entities;
        private readonly Rectangle _Bounds;
        private readonly QuadTree[] _Children;

        public QuadTree(Rectangle bounds)
        {
            _Entities = new List<Entity>();
            _Bounds = bounds;
            _Children = new QuadTree[4];
        }

        private QuadTree(Rectangle bounds, int level, QuadTree parent) : this(bounds)
        {
            _Level = level;
            _Parent = parent;
        }

        public void Clear()
        {
            _Entities.Clear();
            for (int i = 0; i < 4; i++)
            {
                _Children[i].Clear();
                _Children[i] = null;
            }
        }

        public void Insert(Entity entity)
        {
            if (_Children[0] != null)
            {
                int index = GetIndex(entity.BoundingRectangle);
                if (index > -1)
                {
                    _Children[index].Insert(entity);
                    return;
                }
            }

            _Entities.Add(entity);
            entity.LocationChanged += OnEntityLocationChanged;

            if (_Entities.Count > MAX_OBJECTS && _Level < MAX_LEVELS)
            {
                if (_Children[0] == null) Split();

                for (int i = _Entities.Count - 1; i > -1; i--)
                {
                    int index = GetIndex(_Entities[i].BoundingRectangle);
                    if (index > -1) _Children[index].Insert(_Entities[i]);
                    _Entities.RemoveAt(i);
                }
            }
        }

        public IEnumerable<Entity> Search(Rectangle region)
        {
            // TODO
            yield break;
        }

        private void Split()
        {
            int w = _Bounds.Width / 2;
            int h = _Bounds.Height / 2;
            int x = _Bounds.X;
            int y = _Bounds.Y;
            _Children[0] = new QuadTree(new Rectangle(x + w, y, w, h), _Level + 1, this);
            _Children[1] = new QuadTree(new Rectangle(x, y, w, h), _Level + 1, this);
            _Children[2] = new QuadTree(new Rectangle(x, y + h, w, h), _Level + 1, this);
            _Children[3] = new QuadTree(new Rectangle(x + w, y + h, w, h), _Level + 1, this);
        }

        private int GetIndex(Rectangle region)
        {
            int verticalMidpoint = _Bounds.X + (_Bounds.Width / 2);
            int horizontalMidpoint = _Bounds.Y + (_Bounds.Height / 2);
            bool topQuadrant = region.Bottom < horizontalMidpoint;
            bool bottomQuadrant = region.Top > horizontalMidpoint;
            bool leftQuadrant = region.Right < verticalMidpoint;
            bool rightQuadrant = region.Left > verticalMidpoint;
            if (rightQuadrant && topQuadrant) return 0;
            if (leftQuadrant && topQuadrant) return 1;
            if (leftQuadrant && bottomQuadrant) return 2;
            if (rightQuadrant && bottomQuadrant) return 3;
            return -1;
        }

        private void OnEntityLocationChanged(object sender, EventArgs args)
        {
            var e = (Entity)sender;
            if (_Entities.Remove(e))
            {
                if (_Bounds.Contains(e.BoundingRectangle)) Insert(e);
                else if (_Parent != null) _Parent.Insert(e);
            }
        }

    }
}
