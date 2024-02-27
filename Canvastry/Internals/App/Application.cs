using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.Internals.Assets;
using Canvastry.Internals.Events;
using ImGuiNET;
using MoonSharp.Interpreter;
using Raylib_cs;
using rlImGui_cs;
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
    public class ImGuiRenderEvent : Event { }

    public class RaylibInitializedEvent : Event { }

    public class Application
    {
        public ApplicationSettings Settings { get; set; }
        public bool IsRunning { get; private set; } = false;
        public Timer PhysicsTimer = new Timer();
        public Action ImGuiRenderFunction, RenderFunction;
        public Camera2D RenderCamera = new Camera2D();
        public bool StandaloneRun = true;

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
                if (Scene.LoadedScene != null)
                    foreach (var entity in Scene.LoadedScene.SceneEntities)
                    {
                        entity.PhysicsUpdate();
                    }
            };

            PhysicsTimer.Start();

            rlImGui.Setup(true, true);

            EventHandler.Invoke<RaylibInitializedEvent>(new EventData(this));

            while (IsRunning && !Raylib.WindowShouldClose())
            {
                try
                {
                    AssetManager.LazyLoadAssets();
                    EventHandler.PollEvents();

                    if (Rendertexture.CurrentRTx != null)
                        Raylib.BeginTextureMode(Rendertexture.CurrentRTx.rTexture);
                    Raylib.ClearBackground(Settings.WindowClearColor);
                    if (Rendertexture.CurrentRTx != null)
                        Raylib.EndTextureMode();

                    Raylib.BeginDrawing();
                    if (Rendertexture.CurrentRTx != null)

                        Raylib.ClearBackground(Settings.WindowClearColor);
                    if (Scene.LoadedScene != null)
                    {

                        // UPDATE ENTITIES:
                        foreach (var entity in Scene.LoadedScene.SceneEntities)
                        {
                            entity.Update();
                        }

                        if (StandaloneRun)
                        {
                            var camera = Scene.LoadedScene.SceneCamera.GetComponent<CameraComponent>();
                            RenderCamera = camera.Camera;
                        }

                        Raylib.BeginMode2D(RenderCamera);
                        Raylib.BeginTextureMode(Rendertexture.CurrentRTx.rTexture);

                        // GRAPHICS:
                        foreach (Entity e in Scene.LoadedScene.SceneEntities)
                        {
                            if (e is IDrawableEntity drawableEntity)
                            {
                                drawableEntity.Draw();
                            }
                        }

                        if (RenderFunction != null)
                            RenderFunction();

                        if (Rendertexture.CurrentRTx != null)
                            Raylib.EndTextureMode();
                        Raylib.EndMode2D();
                    }

                    // Render ImGui
                    rlImGui.Begin();

                    if (ImGuiRenderFunction != null)
                    {
                        ImGuiRenderFunction();
                    }

                    rlImGui.End();

                    Raylib.EndDrawing();

                    // Delete flagged entities at the end of the frame.
                    if (Scene.LoadedScene != null)
                        Scene.LoadedScene.DeleteEntities();
                }
                catch (ScriptRuntimeException ex)
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
            rlImGui.Shutdown();
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
