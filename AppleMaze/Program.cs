using Canvastry;
using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.ECS.Entities;
using Canvastry.Internals.App;
using Canvastry.Internals.Assets;
using System.Numerics;

namespace AppleMaze
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AppleMaze");

            Application gameApp = new Application(new ApplicationSettings(1200, 900, "AppleMaze"));

            gameApp.Initialize();

            Scene scene = Scene.CreateScene();
            Asset tex = AssetManager.LoadAsset("resources/player-front.png", AssetType.TEXTURE);

            var rectangleEntity = scene.CreateEntity<SpriteEntity>(null, "My Entity");
            var rectangleEntity2 = scene.CreateEntity<SpriteEntity>(null, "My Entity #2");
            var cameraEntity = scene.CreateEntity<CameraEntity>(null, "Main Camera");

            Asset luaFile = AssetManager.LoadAsset("resources/movement.lua", AssetType.CODE);
            rectangleEntity.AddComponent<ScriptBehaviourComponent>(new ScriptBehaviourComponent(luaFile));

            scene.SceneCamera = (CameraEntity)cameraEntity;

            Scene.LoadScene(scene);

            rectangleEntity.GetComponent<SpriteComponent>().Texture = tex;

            rectangleEntity.GetComponent<TransformComponent>().Rotation = 0f;
            rectangleEntity.GetComponent<TransformComponent>().Position = new System.Numerics.Vector2(gameApp.Settings.WindowWidth / 2, gameApp.Settings.WindowHeight / 2);
            rectangleEntity.AddComponent<BoxColliderComponent>(new BoxColliderComponent(rectangleEntity.GetComponent<TransformComponent>().Position, rectangleEntity.GetComponent<TransformComponent>().Size));

            rectangleEntity2.GetComponent<SpriteComponent>().Texture = tex;
            rectangleEntity2.GetComponent<TransformComponent>().Rotation = 0f;
            rectangleEntity2.GetComponent<TransformComponent>().Position = new System.Numerics.Vector2(gameApp.Settings.WindowWidth / 2, gameApp.Settings.WindowHeight / 2 - 350);
            rectangleEntity2.AddComponent<BoxColliderComponent>(new BoxColliderComponent(rectangleEntity2.GetComponent<TransformComponent>().Position, rectangleEntity2.GetComponent<TransformComponent>().Size));


            gameApp.Run();
        }
    }
}
