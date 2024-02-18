using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MoonSharp.Interpreter;

namespace Canvastry.ECS.Components
{
    [MoonSharpUserData]
    public class TransformComponent : Component
    {
        public Vector2 Position = new Vector2(0,0);
        public float Rotation = 0f;
        public Vector2 Size = new Vector2(150,150);
        public Vector2 Scale = new Vector2(1, 1);

        public TransformComponent()
        {
        }

        public TransformComponent(Vector2 position, float rotation, Vector2 size, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Size = size;
            Scale = scale;
        }
    }
}
