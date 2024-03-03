using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.Internals.App;
using Canvastry.Internals.Assets;
using Canvastry.Scripting;
using Newtonsoft.Json;
using static System.Formats.Asn1.AsnWriter;

namespace LiveAssembly
{
    public class GameData
    {
        public Application runningApplication;
        public string FirstScene;
        public List<string> assetsToLoad;

        public GameData(Application runningApplication, string firstScene, List<string> assetsToLoad)
        {
            this.runningApplication = runningApplication;
            FirstScene = firstScene;
            this.assetsToLoad = assetsToLoad;
        }
    }

    public class Program
    {
        GameData data;

        public void Run()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            data = JsonConvert.DeserializeObject<GameData>(File.ReadAllText("passed-data.json"));
            
            if (data != null)
            {
                data.runningApplication.Initialize();
                Scene.LoadSceneFromFile(data.FirstScene);
                foreach (var asset in data.assetsToLoad)
                    AssetManager.LoadAsset(asset, AssetType.AUTO);
                data.runningApplication.Run();
                data.runningApplication.Shutdown();
            }
        }

        public static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
