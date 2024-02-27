using Canvastry.Internals.Assets;
using Canvastry.Scripting;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [Serializable]
    public class ScriptBehaviourComponent : Component
    {
        public Asset ScriptData;
        internal Script _Script;

        public ScriptBehaviourComponent(Asset scriptData)
        {
            if (scriptData.Type == AssetType.CODE)
            {
                ScriptData = scriptData;

                _Script = CVLuaExecutor.CreateScript(((CodeAssetRef)ScriptData.Data).Code);
            }
        }

        public ScriptBehaviourComponent() { }
    }
}
