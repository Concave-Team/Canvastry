using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.Internals.Assets;
using Canvastry.Internals.Events;
using Canvastry.Scripting;
using CSCore.CoreAudioAPI;
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
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
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

            if (!StandaloneRun)
                rlImGui.Setup(true, true);

            EventHandler.Invoke<RaylibInitializedEvent>(new EventData(this));

            if (StandaloneRun)
                foreach (var e in Scene.LoadedScene.SceneEntities)
                {
                    if (e.HasComponent<ScriptBehaviourComponent>())
                    {
                        var sCmp = e.GetComponent<ScriptBehaviourComponent>();
                        if (sCmp.ScriptPath != null)
                        {
                            sCmp.ScriptData = AssetManager.GetLoadedAsset(sCmp.ScriptPath);
                            sCmp._Script = CVLuaExecutor.CreateScript(((CodeAssetRef)sCmp.ScriptData.Data).Code);
                            e.Init();
                        }
                    }
                }

            while (IsRunning && !Raylib.WindowShouldClose())
            {
                if (StandaloneRun)
                    Rendertexture.CurrentRTx = null;
                try
                {
                    AssetManager.LazyLoadAssets();
                    EventHandler.PollEvents();

                    // Delete flagged entities
                    if (Scene.LoadedScene != null)
                        Scene.LoadedScene.DeleteEntities();

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
                        for(int l = 0; l < Scene.LoadedScene.SceneEntities.Count; l++)
                        {
                            var entity = Scene.LoadedScene.SceneEntities[l];
                            entity.Update();
                        }

                        if (StandaloneRun)
                        {
                            var camera = Scene.LoadedScene.SceneCamera.GetComponent<CameraComponent>();
                            var cameraT = Scene.LoadedScene.SceneCamera.GetComponent<TransformComponent>();

                            RenderCamera = new Camera2D(cameraT.Position, new System.Numerics.Vector2(0, 0), cameraT.Rotation, camera.Zoom);
                        }

                        Raylib.BeginMode2D(RenderCamera);
                        if (!StandaloneRun)
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

                        if (!StandaloneRun)
                            if (Rendertexture.CurrentRTx != null)
                                Raylib.EndTextureMode();
                        Raylib.EndMode2D();
                    }

                    if (!StandaloneRun)
                    {
                        // Render ImGui
                        rlImGui.Begin();

                        if (ImGuiRenderFunction != null)
                        {
                            ImGuiRenderFunction();
                        }

                        rlImGui.End();
                    }

                    Raylib.EndDrawing();
                }
                catch (ScriptRuntimeException ex)
                {
                    Console.WriteLine("ScriptRuntimeException Caught!");
                    Console.WriteLine(ex.DecoratedMessage);
                }
            }
        }

        public void Shutdown()
        {
            IsRunning = false;
            PhysicsTimer.Stop();
            if (!StandaloneRun)
                rlImGui.Shutdown();

            for(int i = 0; i < AssetManager.LoadedAssets.Count; i++)
                AssetManager.UnloadAsset(AssetManager.LoadedAssets.Keys.ToList()[i]);

            Raylib.CloseWindow();
        }

        public void Initialize()
        {
            Console.WriteLine("Canvastry v0.01 -- Starting Application -- ######");
            Console.WriteLine("Window Title: " + Settings.WindowTitle);

            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (
                    var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    SharedData.CurrentAudioDevice = mmdeviceCollection[0];

                    Console.WriteLine("[ENGINE][AUDIO]: Using Audio Device " + SharedData.CurrentAudioDevice.FriendlyName);
                }
            }
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
