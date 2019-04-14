using Microsoft.Xna.Framework;

namespace MazeGame.Primitives
{
    internal class SignEntity : Entity
    {
        private readonly string _Text;
        private readonly int _Type;

        public SignEntity(Point initialLocation, string text, int type) : base(initialLocation, Direction.South)
        {
            _Text = text;
            _Type = type;
        }

        public override void Interact(IInteraction interaction, Entity actor)
        {
            interaction.ShowSignInterface(_Text, _Type);
        }

        public override Rectangle GetBoundingRectangle(Point location) => new Rectangle(location, new Point(32, 32));
    }
}
