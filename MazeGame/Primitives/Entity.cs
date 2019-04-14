using MazeGame.Level;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MazeGame.Primitives
{
    internal abstract class Entity
    {
        private Point _Location;

        public Entity(Point initialLocation, Direction initialFacing)
        {
            Location = initialLocation;
            Facing = initialFacing;
        }
        
        public Point Location
        {
            get => _Location;
            set
            {
                if (_Location != value)
                {
                    _Location = value;
                    LocationChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler LocationChanged;

        public Direction Facing { get; protected set; }

        public int AnimationStep { get; protected set; }

        public abstract Rectangle GetBoundingRectangle(Point location);

        public Rectangle BoundingRectangle => GetBoundingRectangle(Location);

        public virtual void AdvanceAnimation() { }

        public virtual void ResetAnimation() { }

        public virtual void Update(GameTime gameTime, Map map)
        {
            // Update state of the entity
        }

        public virtual void Render(SpriteBatch sb, EntityManager entityManager, Point offset, int fade)
        {
            // Some entity types won't do any rendering
        }

        /// <summary>
        /// The given actor has requested to interact with this entity
        /// </summary>
        /// <param name="interaction">Interaction interface</param>
        /// <param name="actor">The actor requesting the interaction</param>
        public virtual void Interact(IInteraction interaction, Entity actor)
        {
            // The given actor (usually the player) has chosen to interact with this entity...
        }
    }
}
