using Canvastry.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Entities
{
    internal class AudioSourceEntity : Entity
    {
        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        public AudioSourceEntity(bool createCmp = false) : base()
        {
            if (createCmp)
            {
                this.AddComponent<TransformComponent>(new TransformComponent());
                var transform = this.GetComponent<TransformComponent>();
                this.AddComponent<SpriteComponent>(new SpriteComponent());
                this.AddComponent<BoxColliderComponent>(new BoxColliderComponent(transform.Position, transform.Size));
            }
        }
    }
}
