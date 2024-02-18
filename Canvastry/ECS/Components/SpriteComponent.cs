using Canvastry.Internals.Assets;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    public class SpriteComponent : Component
    {
        public Asset Texture;
        public Color Tint = Color.White;

        public SpriteComponent()
        {
        }

        public SpriteComponent(Asset texture, Color tint)
        {
            Texture = texture;
            Tint = tint;
        }
    }
}
