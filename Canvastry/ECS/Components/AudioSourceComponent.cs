using Canvastry.Internals.Assets;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Canvastry.ECS.Components
{
    [MoonSharpUserData]
    public class AudioSourceComponent : Component
    {
        [JsonIgnore]
        private Asset AudioAsset { get; set; }
        public string AudioPath { get; set; }
        public float Volume { get; set; } = 1.0f;

        public void Play()
        {
            AudioAsset = AssetManager.GetLoadedAsset(AudioPath);
            if (AudioAsset != null)
            {
                var AudioData = (AudioAssetRef)AudioAsset.Data;
                if(AudioData.soundOut != null)
                    AudioData.soundOut.Play();
            }
        }

        public void Pause()
        {
            if (AudioAsset != null)
            {
                var AudioData = (AudioAssetRef)AudioAsset.Data;
                if (AudioData.soundOut != null)
                    AudioData.soundOut.Pause();
            }
        }

        public void Stop()
        {
            if (AudioAsset != null)
            {
                var AudioData = (AudioAssetRef)AudioAsset.Data;
                if (AudioData.soundOut != null)
                    AudioData.soundOut.Stop();
            }
        }

        public AudioSourceComponent() { }
    }
}
