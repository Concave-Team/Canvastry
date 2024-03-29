﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.ECS.Entities;
using Canvastry.InputLib;
using MoonSharp.Interpreter;
using Raylib_cs;

namespace Canvastry.Scripting
{
    public static class CVLuaExecutor
    {
        public static List<string> ExtensiveAssemblies = new List<string>();

        public static void LoadCSExtension(string assemblyPath)
        {

        }

        public static Script CreateScript(string code)
        {
            try
            {
                UserData.RegisterAssembly(Assembly.GetCallingAssembly());

                UserData.RegisterType<Vector2>(InteropAccessMode.Default, "Vector2");
                UserData.RegisterType<InputLib.InputKey>();
                UserData.RegisterType<Color>();
                UserData.RegisterType<Rectangle>();
                UserData.RegisterType<SpriteComponent>();
                UserData.RegisterType<BoxColliderComponent>();
                UserData.RegisterType<BoxCollision>();
                UserData.RegisterType<InputLib.MouseButton>();

                var inKey = UserData.CreateStatic<InputLib.InputKey>();
                var msButton = UserData.CreateStatic<InputLib.MouseButton>();
                var colors = UserData.CreateStatic<Color>();

                Script script = new Script(CoreModules.Preset_SoftSandbox);

                script.Globals.RegisterConstants();
                script.Globals["Input"] = typeof(InputLib.Input);
                script.Globals["Scene"] = typeof(ECS.Scene);
                script.Globals["MaterialComponent"] = typeof(MaterialComponent);
                script.Globals["TransformComponent"] = typeof(TransformComponent);
                script.Globals["SpriteComponent"] = typeof(SpriteComponent);
                script.Globals["RectangleEntity"] = typeof(RectangleEntity);
                script.Globals["CameraEntity"] = typeof(CameraEntity);
                script.Globals["Entity"] = typeof(Entity);
                script.Globals["BoxColliderComponent"] = typeof(BoxColliderComponent);
                script.Globals["AudioSourceComponent"] = typeof(AudioSourceComponent);
                script.Globals["Rect"] = typeof(Rectangle);
                script.Globals["BoxCollision"] = typeof(BoxCollision);
                script.Globals["Key"] = inKey;
                script.Globals["MouseButton"] = msButton;
                script.Globals["Vector2"] = typeof(Vector2);
                script.Globals["Color"] = colors;
                script.Globals["getfenv"] = () => { return script.Globals; };


                var result = script.DoString(code);
                return script;
            }
            catch (ScriptRuntimeException e)
            {
                Console.WriteLine("[GAME]: Exception caught " + e.GetType() + " | " + e.Message + " Stack Trace: " + e.StackTrace);
                throw e;
            }
        }
    }
}
