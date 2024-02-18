using Canvastry.ECS;
using Canvastry.Internals.Assets;
using Canvastry.Internals.Events;
using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using EventHandler = Canvastry.Internals.Events.EventHandler;
using Timer = System.Timers.Timer;

namespace Canvastry.Internals.App
{
    public class ApplicationLoopEvent : Event { }

    public class Application
    {
        public ApplicationSettings Settings { get; set; }
        public bool IsRunning { get; private set; } = false;
        public Timer PhysicsTimer = new Timer();

        public void Run()
        {
            Raylib.InitWindow(Settings.WindowWidth, Settings.WindowHeight, Settings.WindowTitle);
            Raylib.SetTargetFPS(Settings.TargetFPS);

            IsRunning = true;
            AssetManager.AppRunning = true;

            Console.WriteLine("Canvastry v0.01 -- Application Running -- ######");
            Console.WriteLine("Window Title: " + Settings.WindowTitle);

            PhysicsTimer.Interval = Settings.PhysicsTickRate;
            PhysicsTimer.AutoReset = true;
            PhysicsTimer.Enabled = true;

            PhysicsTimer.Elapsed += (Object source, ElapsedEventArgs e) =>
            {
                foreach (var entity in Scene.LoadedScene.SceneEntities)
                {
                    entity.PhysicsUpdate();
                }
            };

            PhysicsTimer.Start();

            while (IsRunning && !Raylib.WindowShouldClose())
            {
                try
                { 
                    AssetManager.LazyLoadAssets();
                    EventHandler.PollEvents();

                    // UPDATE ENTITIES:
                    foreach (var entity in Scene.LoadedScene.SceneEntities)
                    {
                        entity.Update();
                    }

                    // GRAPHICS:
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Settings.WindowClearColor);

                    foreach (Entity e in Scene.LoadedScene.SceneEntities)
                    {
                        if (e is IDrawableEntity drawableEntity)
                        {
                            drawableEntity.Draw();
                        }
                    }

                    Raylib.EndDrawing();

                    Scene.LoadedScene.DeleteEntities();
                }
                catch(ScriptRuntimeException ex)
                {
                    Console.WriteLine("ScriptRuntimeException Caught!");
                    Console.WriteLine(ex.DecoratedMessage);
                }
            }

            Shutdown();
        }

        public void Shutdown()
        {
            IsRunning = false;
            PhysicsTimer.Stop();
            Raylib.CloseWindow();
        }

        public void Initialize()
        {
            Console.WriteLine("Canvastry v0.01 -- Starting Application -- ######");
            Console.WriteLine("Window Title: " + Settings.WindowTitle);
        }

        public Application()
        {
            Settings = new ApplicationSettings();
        }

        public Application(ApplicationSettings settings)
        {
            Settings = settings;
        }
    }
}
