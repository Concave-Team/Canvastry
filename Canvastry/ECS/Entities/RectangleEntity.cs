using Canvastry.ECS.Components;
using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Entities
{
    [MoonSharpUserData]
    [Serializable]
    public partial class RectangleEntity : Entity, IDrawableEntity
    {
        public void Draw()
        {
            var transform = this.GetComponent<TransformComponent>();
            var color = this.GetComponent<MaterialComponent>();

            Raylib.DrawRectanglePro(new Rectangle(transform.Position, transform.Size), new Vector2(transform.Size.X / 2, transform.Size.Y / 2), transform.Rotation, color.MaterialColor);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        public RectangleEntity(bool createCmp = false)
        {
            if (createCmp)
            {
                this.AddComponent<TransformComponent>(new TransformComponent());
                var transform = this.GetComponent<TransformComponent>();
                this.AddComponent<MaterialComponent>(new MaterialComponent());
                this.AddComponent<BoxColliderComponent>(new BoxColliderComponent(transform.Position, transform.Size));
            }
        }

        public RectangleEntity() : base()
        {
            
        }
    }
}
