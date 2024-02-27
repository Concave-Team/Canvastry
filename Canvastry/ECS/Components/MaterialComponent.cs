using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [MoonSharpUserData]
    [Serializable]
    public class MaterialComponent : Component
    {
        public Color MaterialColor = Color.White;
        public Texture2D MaterialTexture;

        public MaterialComponent()
        {
        }

        public MaterialComponent(Color materialColor, Texture2D materialTexture)
        {
            MaterialColor = materialColor;
            MaterialTexture = materialTexture;
        }
    }
}
