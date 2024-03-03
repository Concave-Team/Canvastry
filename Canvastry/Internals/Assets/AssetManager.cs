using Canvastry.ECS;
using Canvastry.Internals.Events;
using Raylib_cs;
using CSCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using EventHandler = Canvastry.Internals.Events.EventHandler;
using CSCore.Codecs;
using CSCore.Streams;
using CSCore.SoundOut;

namespace Canvastry.Internals.Assets
{
    public class AssetEventData : EventData
    {
        public Asset Asset { get; private set; }

        public AssetEventData(object sender, Asset asset) : base(sender)
        {
            this.Sender = sender;
            this.Asset = asset;
        }
    }

    public class AssetLoadedEvent : Event { }
    public class AssetUnloadedEvent : Event { }

    /// <summary>
    /// LazyAsset class for deferred loading of some asset types like textures and audio.
    /// </summary>
    public class LazyAsset : Asset
    {
        public string FilePath;
        public AssetType Type;
        public Asset asset; // Remains null until the lazy-loader loads this part in.

        public LazyAsset(string filePath, AssetType type)
        {
            FilePath = filePath;
            Type = type;
        }
    }

    public static class AssetManager
    {
        public static Dictionary<string, Asset> LoadedAssets = new Dictionary<string, Asset>();
        public static int LoadedAssetCount = 0;
        public static List<LazyAsset> lazyAssets = new List<LazyAsset>(); // Since some assets can only be loaded after Raylib was initialized, I use this.
        public static bool AppRunning = false;

        public static Asset CreateAssetByType(string assetPath, AssetType type)
        {
            var assetName = Path.GetFileName(assetPath);
            Asset sAsset = new Asset("Error-Asset", AssetType.UNKNOWN, "Something went wrong, while loading your asset! Re-C:/heck your file path or if your file is existing/valid.");
            switch (type)
            {
                case AssetType.UNKNOWN:
                    sAsset = new Asset(assetName, type, assetPath, new TextAssetRef(File.ReadAllText(assetPath)));
                    break;
                case AssetType.TEXTURE:
                    if (!AppRunning)
                    {
                        var lAs = new LazyAsset(assetPath, AssetType.TEXTURE);
                        lazyAssets.Add(lAs);
                        return lAs;
                    }
                    sAsset = new Asset(assetName, type, assetPath, new TextureAssetRef(Raylib.LoadTexture(assetPath)));
                    break;
                case AssetType.AUDIO:
                    if (!AppRunning)
                    {
                        var lAs = new LazyAsset(assetPath, AssetType.AUDIO);
                        lazyAssets.Add(lAs);
                        return lAs;
                    }
                    //sAsset = new Asset(assetName, type, assetPath, new AudioAssetRef(Bass.CreateStream(assetPath)));
                    var audioFile = CodecFactory.Instance.GetCodec(assetPath)
                        .ToSampleSource()
                        .ToWaveSource();
                    var soundOut = new WasapiOut { Latency = 10, Device = SharedData.CurrentAudioDevice };
                    soundOut.Initialize(audioFile);

                    sAsset = new Asset(assetName, type, assetPath, new AudioAssetRef(audioFile, soundOut));
                    break;
                case AssetType.TEXT:
                    sAsset = new Asset(assetName, type, assetPath, new TextAssetRef(File.ReadAllText(assetPath)));
                    break;
                case AssetType.CODE:
                    sAsset = new Asset(assetName, type, assetPath, new CodeAssetRef(File.ReadAllText(assetPath)));
                    break;
                case AssetType.AUTO:
                    var ext = Path.GetExtension(assetPath);

                    switch (ext)
                    {
                        case ".txt":
                            sAsset = CreateAssetByType(assetPath, AssetType.TEXT);
                            break;
                        case ".png":
                            sAsset = CreateAssetByType(assetPath, AssetType.TEXTURE);
                            break;
                        case ".lua":
                            sAsset = CreateAssetByType(assetPath, AssetType.CODE);
                            break;
                        case ".mp3":
                            sAsset = CreateAssetByType(assetPath, AssetType.AUDIO);
                            break;
                        case ".wav":
                            sAsset = CreateAssetByType(assetPath, AssetType.AUDIO);
                            break;
                        case ".ogg":
                            sAsset = CreateAssetByType(assetPath, AssetType.AUDIO);
                            break;
                        case ".flac":
                            sAsset = CreateAssetByType(assetPath, AssetType.AUDIO);
                            break;
                    }

                    break;
            }
            return sAsset;
        }

        /// <summary>
        /// Lazy-loads assets during runtime, useful for assets that cannot be loaded before Raylib's initialization.
        /// </summary>
        public static void LazyLoadAssets()
        {
            for (int i = 0; i < lazyAssets.Count; i++)
            {
                var lAsset = lazyAssets[i];
                var asset = LoadAsset(lAsset.FilePath, lAsset.Type);
                lAsset.asset = asset;

                lazyAssets.RemoveAt(i);
            }
        }

        public static Asset LoadAsset(string assetPath, AssetType type = AssetType.UNKNOWN)
        {
            var asset = CreateAssetByType(assetPath, type);

            if (asset is not LazyAsset)
            {
                LoadedAssets.Add(assetPath, asset);
                LoadedAssetCount++;

                EventHandler.Invoke<AssetLoadedEvent>(new AssetEventData(asset, asset));
            }

            return asset;
        }

        public static bool UnloadAsset(string assetPath)
        {
            if (LoadedAssets.ContainsKey(assetPath))
            {
                if (LoadedAssets[assetPath].Type == AssetType.TEXTURE)
                {
                    var tex = LoadedAssets[assetPath].Data as TextureAssetRef;
                    Raylib.UnloadTexture(tex.Texture);
                }
                else if (LoadedAssets[assetPath].Type == AssetType.AUDIO)
                {
                    var tex = LoadedAssets[assetPath].Data as AudioAssetRef;

                }

                LoadedAssets.Remove(assetPath);
                LoadedAssetCount--;

                EventHandler.Invoke<AssetUnloadedEvent>(new EventData("AssetManager"));

                return true;
            }
            EventHandler.Invoke<ErrorEvent>(new ErrorEventData("AssetManager", "Couldn't find loaded asset '" + assetPath + "'. Is your asset path correctly written?", "WARN", "UnloadAsset"));

            return false;
        }

        public static Asset GetLoadedAsset(string assetPath)
        {
            if (LoadedAssets.ContainsKey(assetPath))
                return LoadedAssets[assetPath];
            EventHandler.Invoke<ErrorEvent>(new ErrorEventData("AssetManager", "Couldn't find loaded asset '" + assetPath + "'. Is your asset path correctly written?", "WARN", "GetLoadedAsset"));

            return new Asset("Error-Asset", AssetType.UNKNOWN, "Something went wrong, while loading your asset! Re-C:/heck your file path or if your file is existing/valid.");
        }
    }
}
