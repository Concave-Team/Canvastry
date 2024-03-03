using Canvastry.ECS.Entities;
using Canvastry.Internals.Events;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EventHandler = Canvastry.Internals.Events.EventHandler;

namespace Canvastry.ECS
{
    public class SceneEventData : EventData
    {
        public Scene Scene { get; private set; }

        public SceneEventData(object sender, Scene scene) : base(sender)
        {
            this.Sender = sender;
            this.Scene = scene;
        }
    }

    public class EntityEventData : EventData
    {
        public Entity Entity { get; private set; }
        public Scene Scene { get; private set; }

        public EntityEventData(object sender, Entity entity, Scene scene) : base(sender)
        {
            Entity = entity;
            Scene = scene;
        }
    }

    public class SceneLoadEvent : Event {}
    public class SceneCreateEvent : Event { }

    public class EntityCreatedEvent : Event { }

    [MoonSharpUserData]
    [Serializable]
    public class Scene
    {
        #region OBJECT_MEMBERS
        public List<Entity> SceneEntities { get; set; } = new List<Entity>();
        [JsonIgnore]
        public List<Entity> DeleteFlagList { get; set; } = new List<Entity>();
        public string SceneName { get; set; } = "Untitled";
        public CameraEntity SceneCamera { get; set; }

        /// <summary>
        /// Creates an entity and adds it to the scene. Invokes the EntityCreatedEvent.
        /// </summary>
        /// <typeparam name="T">The type of your entity class.</typeparam>
        /// <param name="parent">The parent of your entity. Leave null for it to be connected to the root.</param>
        /// <param name="name">The name of the entity. Default: Untitled</param>
        /// <returns></returns>
        public Entity CreateEntity<T>(Entity parent, string name = "Untitled") where T : Entity
        {
            Entity createdEntt = (T)Activator.CreateInstance(typeof(T), args: true)!;

            createdEntt.scene = this;
            createdEntt.id = (ulong)SceneEntities.Count + 1;
            createdEntt.Name = name;
            createdEntt.Parent = parent;
            SceneEntities.Add(createdEntt);

            EventHandler.Invoke<EntityCreatedEvent>(new EntityEventData(this, createdEntt, this));

            return createdEntt;
        }

        public void DestroyEntity(Entity e)
        {
            if(SceneEntities.Contains(e))
                DeleteFlagList.Add(e);
        }

        public void DeleteEntities()
        {
            foreach (Entity e in DeleteFlagList)
            {
                SceneEntities.Remove(e);
            }

            DeleteFlagList.Clear();
        }

        public void SaveToFile(string path)
        {
            path = Path.GetFullPath(path);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            var jsonScene = JsonConvert.SerializeObject(this, settings);

            File.WriteAllText(path, jsonScene);
        }
        #endregion

        #region STATIC_MEMBERS
        [JsonIgnore]
        public static Scene LoadedScene;

        public static Scene LoadSceneFromFile(string path)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            Scene? scene = JsonConvert.DeserializeObject<Scene>(File.ReadAllText(path),settings);

            foreach(var e in scene.SceneEntities)
                e.scene = scene;

            LoadedScene = scene;

            return scene;
        }

        public static Scene CreateSceneFromFile(string path)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            Scene? scene = JsonConvert.DeserializeObject<Scene>(File.ReadAllText(path), settings);

            foreach (var e in scene.SceneEntities)
                e.scene = scene;

            return scene;
        }

        public static void LoadScene(Scene scene)
        {
            LoadedScene = scene;

            foreach (var entity in Scene.LoadedScene.SceneEntities)
            {
                entity.Init();
            }
        }

        public static Scene CreateScene()
        {
            Scene scene = new Scene();

            EventHandler.Invoke<SceneCreateEvent>(new SceneEventData(scene, scene));

            return scene;
        }
        #endregion
    }
}
