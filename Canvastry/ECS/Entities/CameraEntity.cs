using Canvastry.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Entities
{
    [Serializable]
    public class CameraEntity : Entity
    {
        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();

            var transform = GetComponent<TransformComponent>();
            var camera = GetComponent<CameraComponent>();

            camera.Camera.Target = transform.Position;
            camera.Camera.Rotation = transform.Rotation;
            camera.Camera.Zoom = camera.Zoom;
        }

        public CameraEntity(bool createCmp = false) 
        {
            if (createCmp)
            {
                AddComponent<TransformComponent>(new TransformComponent());

                var transform = GetComponent<TransformComponent>();
                AddComponent<CameraComponent>(new CameraComponent(new Raylib_cs.Camera2D(new System.Numerics.Vector2(0, 0), transform.Position, transform.Rotation, 60f)));
            }
        }
    }
}
