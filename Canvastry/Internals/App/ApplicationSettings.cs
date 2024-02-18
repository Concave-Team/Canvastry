using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.Internals.App
{
    public class ApplicationSettings
    {
        /// <summary>
        /// The width size (in pixels) of the application(game) window. Default: 800px.
        /// </summary>
        public int WindowWidth { get; set; }
        /// <summary>
        /// The height size (in pixels) of the application(game) window. Default: 600px.
        /// </summary>
        public int WindowHeight { get; set; }
        /// <summary>
        /// The title of the window. You can set this to whatever you like.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// FPS Limit for the application
        /// </summary>
        public int TargetFPS { get; set; } = 60;

        /// <summary>
        /// How often the physics ticks occur(in milliseconds)
        /// </summary>
        public int PhysicsTickRate { get; set; } = 33;

        /// <summary>
        /// The color the window is cleared.
        /// </summary>
        public Color WindowClearColor { get; set; } = Color.RayWhite;

        public ApplicationSettings() 
        {
            WindowWidth = 800;
            WindowHeight = 600;
            WindowTitle = "Game Window";
        }
        public ApplicationSettings(int windowWidth, int windowHeight, string windowTitle = "Game Window") 
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            WindowTitle = windowTitle;
        }
    }
}
