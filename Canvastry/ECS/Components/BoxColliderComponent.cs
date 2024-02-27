using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [Serializable]
    public class BoxColliderComponent : Component
    {
        public Vector2 Position { get; set; }
        public Vector2 Offset { get; set; } = new Vector2(0, 0);
        public Vector2 Size { get; set; }
        public bool Collides = true;
        public bool IsColliding = false;

        public BoxColliderComponent(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public BoxColliderComponent()
        {
        }
    }

    /// <summary>
    /// The data of a collision between two BoxColliderComponents.
    /// </summary>
    public class BoxCollision
    {
        /// <summary>
        /// The entity the GameObject collided with.
        /// </summary>
        public Entity CollidedWith;
        /// <summary>
        /// The BoxColliderComponent of the entity the GameObject collided with.
        /// </summary>
        public BoxColliderComponent Collider;
        /// <summary>
        /// The area of the collision space.
        /// </summary>
        public Rectangle Area;

        public BoxCollision(Entity collidedWith, BoxColliderComponent collider, Rectangle area)
        {
            CollidedWith = collidedWith;
            Collider = collider;
            Area = area;
        }
    }
}
