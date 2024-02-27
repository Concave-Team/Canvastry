using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [Serializable]
    public class CameraComponent : Component
    {
        public CameraComponent()
        {
        }

        public Camera2D Camera;
        public float Zoom = 1f;

        public CameraComponent(Camera2D camera)
        {
            Camera = camera;
        }
    }
}
