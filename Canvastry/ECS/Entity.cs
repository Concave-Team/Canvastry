using Canvastry.ECS.Components;
using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Canvastry.Scripting;
using Canvastry.Internals.Assets;
using System.Runtime.CompilerServices;

namespace Canvastry.ECS
{
    [MoonSharpUserData]
    [Serializable]
    public class Entity
    {
        public ulong id { get; internal set; } = 0; // Effectively allows for up to 18,446,744,073,709,551,615 entities, which I think is enough(probably).

        public string Name;
        public string Tag;
        public string TypeName;
        public Entity Parent;
        [JsonIgnore]
        public Scene scene;
        public List<Component> Components = new List<Component>();

        public virtual void Update()
        {
            if (this.HasComponent<ScriptBehaviourComponent>())
            {
                var scriptComp = this.GetComponent<ScriptBehaviourComponent>();
                if (scriptComp._Script != null)
                {
                    var script = scriptComp._Script;

                    script.Globals["GameObject"] = this;

                    if(script.Globals["Update"] != null)
                        script.Call(script.Globals["Update"], Raylib_cs.Raylib.GetFrameTime());
                }
            }    
        }

        // Happens every 20ms
        public virtual void PhysicsUpdate()
        {
            if (this.HasComponent<BoxColliderComponent>())
            {
                var boxColliderComp = this.GetComponent<BoxColliderComponent>();

                boxColliderComp.Position = this.GetComponent<TransformComponent>().Position - this.GetComponent<TransformComponent>().Size / 2 + boxColliderComp.Offset;
                boxColliderComp.Size = this.GetComponent<TransformComponent>().Size;
            }
            if (this.HasComponent<ScriptBehaviourComponent>())
            {
                var scriptComp = this.GetComponent<ScriptBehaviourComponent>();
                if (scriptComp._Script != null)
                {
                    var script = scriptComp._Script;

                    script.Globals["GameObject"] = this;

                    if (script.Globals["PhysicsUpdate"] != null)
                        script.Call(script.Globals["PhysicsUpdate"]);

                    if (this.HasComponent<BoxColliderComponent>())
                    {
                        foreach (var entity in scene.SceneEntities)
                        {
                            if (entity.id != this.id)
                            {
                                if (entity.HasComponent<BoxColliderComponent>())
                                {
                                    var colliderA = this.GetComponent<BoxColliderComponent>();
                                    var colliderB = entity.GetComponent<BoxColliderComponent>();

                                    if (Raylib.CheckCollisionRecs(new Rectangle(colliderA.Position, colliderA.Size), new Rectangle(colliderB.Position, colliderB.Size)))
                                    {
                                        colliderA.IsColliding = true;

                                        Raylib.DrawRectangleRec(Raylib.GetCollisionRec(new Rectangle(colliderA.Position, colliderA.Size), new Rectangle(colliderB.Position, colliderB.Size)), Color.Red);
                                        if (script.Globals["OnCollisionEnter"] != null)
                                            script.Call(script.Globals["OnCollisionEnter"], new BoxCollision(entity, colliderB, Raylib.GetCollisionRec(new Rectangle(colliderA.Position, colliderA.Size), new Rectangle(colliderB.Position, colliderB.Size))));
                                    }
                                    else
                                    {
                                        if (colliderA.IsColliding == true)
                                        {
                                            colliderA.IsColliding = false;
                                            if (script.Globals["OnCollisionExited"] != null)
                                                script.Call(script.Globals["OnCollisionExited"]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Init()
        {
            if (this.HasComponent<ScriptBehaviourComponent>())
            {
                var scriptComp = this.GetComponent<ScriptBehaviourComponent>();
                if (scriptComp._Script != null)
                {
                    var script = scriptComp._Script;

                    script.Globals["GameObject"] = this;
                    if (script.Globals["Start"] != null)
                        script.Call(script.Globals["Start"]);
                }
                else
                {
                    Console.WriteLine("HEYYY");
                    scriptComp._Script = CVLuaExecutor.CreateScript(((CodeAssetRef)scriptComp.ScriptData.Data).Code);
                    var script = scriptComp._Script;

                    script.Globals["GameObject"] = this;

                    if (script.Globals["Start"] != null)
                        script.Call(script.Globals["Start"]);
                }
            }
        }

        public T? AddComponent<T> (T component) where T : Component
        {
            if(!HasComponent<T>())
            {
                Components.Add(component);

                return component;
            }

            return null; // Returns null incase it failed, so you can know if it was successful or not.
        }

        public bool HasComponent<T>() where T : Component
        {
            return Components.Any(cmp => cmp.GetType() == typeof(T));
        }

        public bool HasComponent(string type)
        {
            return Components.Any(cmp => cmp.GetType().Name == type);
        }

        public T? GetComponent<T>() where T : Component
        {
            return Components.Find(cmp => cmp.GetType() == typeof(T)) as T;
        }

        public Component? GetComponent(string type)
        {
            return Components.Find(cmp => cmp.GetType().Name == type);
        }

        public bool RemoveComponent<T>() where T : Component
        {
            if (HasComponent<T>())
            {
                Components.Remove(Components.Find(e => e.GetType() == typeof(T))!);
                return true;
            }

            return false;
        }

        public bool RemoveComponent(string component)
        {
            if (HasComponent(component))
            {
                Components.Remove(Components.Find(e => e.GetType().Name == component)!);
                return true;
            }

            return false;
        }

        public Entity Clone()
        {
            Console.WriteLine("EEE");
            var clone = (Entity)this.MemberwiseClone();

            Scene.LoadedScene.AddEntity(clone);

            return clone;
        }

        public Entity() 
        {
            Name = "Untitled";
        }
    }
}
