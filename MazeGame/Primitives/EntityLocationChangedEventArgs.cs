using System;

namespace MazeGame.Primitives
{
    internal class EntityLocationChangedEventArgs : EventArgs
    {
        public EntityLocationChangedEventArgs(Entity entity, bool isPlayer)
        {
            Entity = entity;
            IsPlayer = isPlayer;
        }

        public Entity Entity { get; }

        public bool IsPlayer { get; }
    }
}
