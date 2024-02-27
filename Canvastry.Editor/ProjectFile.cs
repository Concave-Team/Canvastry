using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.Editor
{
    public class ProjectFile
    {
        public static ProjectFile LoadedProject;
        public List<string> SceneFiles = new List<string>();
        public string ProjectName = "Untitled Project";
        public string EditorVersion = "2024-1.0a";
        public string LastOpened = DateTime.Now.ToString();
        public string PrimaryScene = "";
        public string Path = "";

        public void SaveToFile(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public static ProjectFile FromFile(string path)
        {
            return JsonConvert.DeserializeObject<ProjectFile>(File.ReadAllText(path));
        }

        public ProjectFile(List<string> sceneFiles, string projectName, string editorVersion, string path)
        {
            SceneFiles = sceneFiles;
            ProjectName = projectName;
            EditorVersion = editorVersion;
            Path = path;
        }
    }
}
