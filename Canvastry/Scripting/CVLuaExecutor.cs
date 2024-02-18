using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Canvastry.ECS.Components;
using Canvastry.ECS.Entities;
using Canvastry.Input;
using MoonSharp.Interpreter;
using Raylib_cs;

namespace Canvastry.Scripting
{
    public static class CVLuaExecutor
    {
        
        public static Script CreateScript(string code)
        {
            UserData.RegisterAssembly(Assembly.GetCallingAssembly());

            UserData.RegisterType<Vector2>(InteropAccessMode.Default, "Vector2");
            UserData.RegisterType<InputKey>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<Rectangle>();
            UserData.RegisterType<SpriteComponent>();
            UserData.RegisterType<BoxColliderComponent>();
            UserData.RegisterType<BoxCollision>();
            UserData.RegisterType<Input.MouseButton>();

            var inKey = UserData.CreateStatic<InputKey>();
            var msButton = UserData.CreateStatic<Input.MouseButton>();
            var colors = UserData.CreateStatic<Color>();

            Script script = new Script(CoreModules.Preset_SoftSandbox);

            script.Globals.RegisterConstants();
            script.Globals["Input"] = typeof(Input.Input);
            script.Globals["Scene"] = typeof(ECS.Scene);
            script.Globals["MaterialComponent"] = typeof(MaterialComponent);
            script.Globals["TransformComponent"] = typeof(TransformComponent);
            script.Globals["SpriteComponent"] = typeof(SpriteComponent);
            script.Globals["RectangleEntity"] = typeof(RectangleEntity);
            script.Globals["BoxColliderComponent"] = typeof(BoxColliderComponent);
            script.Globals["Rect"] = typeof(Rectangle);
            script.Globals["BoxCollision"] = typeof(BoxCollision);
            script.Globals["Key"] = inKey;
            script.Globals["MouseButton"] = msButton;
            script.Globals["Vector2"] = typeof(Vector2);
            script.Globals["Color"] = colors;

            var result = script.DoString(code);

            return script;
        }
    }
}
