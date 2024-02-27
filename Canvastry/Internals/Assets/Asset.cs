using Canvastry.ECS;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Canvastry.Internals.Assets
{
    public enum AssetType
    {
        /// <summary>
        /// Unknown or unidentified asset type(default).
        /// If you got this while loading an asset, check if you specified the asset type or if your file extension is correct, if you're using auto.
        /// However, if you load with AssetType.Unknown, it isn't a sign of failure and you can of course use it.
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// You can only use this type with: <code>AssetManager.LoadShaderAsset();</code>
        /// 
        /// File extensions: *.fs, .glsl, .frag
        /// </summary>
        SHADER_FS,
        /// <summary>
        /// You can only use this type with: <code>AssetManager.LoadShaderAsset();</code>
        /// 
        /// File extensions: *.vs, .glsl, .vert
        /// </summary>
        SHADER_VS,
        /// <summary>
        /// Defines a texture asset type.
        /// 
        /// (NOTE: *.jpg and *.bmp files are currently not supported.)
        /// File extensions: *.png, *.svg
        /// </summary>
        TEXTURE,
        /// <summary>
        /// Defines a sound/audio asset type.
        /// 
        /// File extensions: *.mp3, *.ogg, *.wav
        /// </summary>
        AUDIO,
        /// <summary>
        /// Defines a code(Lua) asset type.
        /// 
        /// File extensions: *.lua, *.cvc
        /// </summary>
        CODE,
        /// <summary>
        /// Defines a text asset type:
        /// File extension: *.txt
        /// </summary>
        TEXT,
        /// <summary>
        /// Defines a scene asset type:
        /// File extension: *.scene
        /// </summary>
        SCENE,
        /// <summary>
        /// Automatically decides the type based on the file extension.
        /// </summary>
        AUTO
    }

    public class Asset
    {
        public string Name { get; set; } = "Untitled";
        public AssetType Type { get; set; }
        public string Path { get; set; }

        public AssetRef Data { get; set; }

        public Asset(string name, AssetType type, string path)
        {
            Name = name;
            Type = type;
            Path = path;
        }

        public Asset(string name, AssetType type, string path, AssetRef data) : this(name, type, path)
        {
            Data = data;
        }

        public Asset()
        {
        }
    }

    public class AssetRef
    {}

    public class TextureAssetRef : AssetRef
    {
        public Texture2D Texture { get; set; }

        public TextureAssetRef(Texture2D texture)
        {
            Texture = texture;
        }
    }

    public class ShaderAssetRef : AssetRef
    {
        public Shader Shader { get; set; }

        public ShaderAssetRef(Shader shader)
        {
            Shader = shader;
        }
    }

    public class SceneAssetRef : AssetRef
    {
        public Scene Scene { get; set; }

        public SceneAssetRef(Scene scene)
        {
            Scene = scene;
        }
    }

    public class CodeAssetRef : AssetRef
    {
        public string Code { get; set; }

        public CodeAssetRef(string code)
        {
            Code = code;
        }
    }

    public class AudioAssetRef : AssetRef
    {
        public Sound Sound { get; set; }

        public AudioAssetRef(Sound sound)
        {
            Sound = sound;
        }
    }

    public class TextAssetRef : AssetRef
    { 
        public string Text { get; set; }

        public TextAssetRef(string text)
        {
            Text = text;
        }
    }
}
