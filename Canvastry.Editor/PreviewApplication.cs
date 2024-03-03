using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.Internals.App;
using Canvastry.Internals.Assets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Canvastry.Editor
{
    /// <summary>
    /// The class that encompasses the logic of the game live preview window.
    /// </summary>
    public class PreviewApplication
    {
        public Application gameWindow = new Application();

        public void Init()
        {
            gameWindow.Settings = new ApplicationSettings(1720, 920, "[PREVIEW] " + ProjectFile.LoadedProject.ProjectName);
            gameWindow.StandaloneRun = true;
        }

        public void Run()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            var jsonScene = JsonConvert.SerializeObject(new LiveAssembly.GameData(gameWindow, ProjectFile.LoadedProject.PrimaryScene, AssetManager.LoadedAssets.Keys.ToList()), settings);
            File.WriteAllText("passed-data.json", jsonScene);

            var procSt = new ProcessStartInfo("dotnet", "LiveAssembly.dll");
            procSt.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process.Start(procSt);
        }

        public PreviewApplication() { }
    }
}
