using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.Internals.App
{
    public class Rendertexture
    {
        public RenderTexture2D rTexture;
        public static Rendertexture CurrentRTx;

        public static Rendertexture RequestRTx(int width, int height)
        {
            Rendertexture rt = new Rendertexture();
            rt.rTexture = Raylib.LoadRenderTexture(width, height);
            return rt;
        }
    }
}
