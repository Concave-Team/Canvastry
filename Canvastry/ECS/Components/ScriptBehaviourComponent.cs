using Canvastry.Internals.Assets;
using Canvastry.Scripting;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [Serializable]
    public class ScriptBehaviourComponent : Component
    {
        [JsonIgnore]
        internal Asset ScriptData;
        public string ScriptPath;
        public Script _Script;

        public ScriptBehaviourComponent(string ScriptPath)
        {
            this.ScriptPath = ScriptPath;
            ScriptData = AssetManager.GetLoadedAsset(ScriptPath);
            if (ScriptData != null)
                if (ScriptData.Type == AssetType.CODE)
                {
                    _Script = CVLuaExecutor.CreateScript(((CodeAssetRef)ScriptData.Data).Code);
                }
        }

        public ScriptBehaviourComponent()
        {
        }
    }
}
