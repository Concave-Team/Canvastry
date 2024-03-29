﻿using Canvastry.ECS;
using Canvastry.ECS.Components;
using Canvastry.ECS.Entities;
using Canvastry.Internals.App;
using Canvastry.Internals.Assets;
using Canvastry.Internals.Events;
using ImGuiNET;
using Newtonsoft.Json;
using Raylib_cs;
using rlImGui_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Canvastry.InputLib;
using EventHandler = Canvastry.Internals.Events.EventHandler;
using CSCore.XAudio2;
using System.Collections.Immutable;
using MoonSharp.Interpreter;
using System.Diagnostics;

namespace Canvastry.Editor
{
    public class ProjectLoadedEvent : Event { }

    public class EditorApplication
    {
        public Application engineApp;
        public EditorData editorData = new EditorData();
        public Rendertexture editorViewRTx;
        public Dictionary<string, ImFontPtr> Fonts = new Dictionary<string, ImFontPtr>();

        public bool ShowEntityCreationWindow = false;
        public string SelectedEntityType = "";
        public Type tySelectedEntityType;
        public string EntityCName = "New Entity";

        public bool Focused = false;

        public Entity explorerSelEntity;

        public string pEntityName = "";

        float vol = 0.5f;
        public ImGuiStyle _propStyle;
        public Vector4 matCompColor = new Vector4(0, 0, 0, 1);

        public bool EditorJustOpened = true;
        public int ProjSelIndex = 0;
        public bool SCreateProjectWindow = false;

        public string CreateProjectName = "My Game Project";
        public string CreateProjectPath = "";

        public bool IsFilePickerOpen = false;
        public string SelectedFileFP = "";

        public string SelectedSceneFile = "";

        public bool progressBarShow = false;
        public float progressBarAM = 0.0f;
        public bool ShSceneOpen = false;

        public string SelectedPName = "";
        public string SelectedPLDate = "";

        public bool OpenInputModal = false;
        public string InputModalText = "Enter Data:";
        public string InputtedText = "";

        public int MainSceneIdx = 0;

        public bool WaitOnSceneCreation = false;

        public bool SelectMainScene = false;

        public bool DrawCNFPopupOK = false;

        public Texture2D FileIconImage, FolderIconImage, TextureIconImage, SceneIconImage;

        private static StringWriter consoleOutput = new StringWriter();

        public Vector2 RelativeMousePosition = new Vector2();

        public bool AssetSelectionWindow = false;
        public string AssetSelFilter = "all";
        public Asset AssetSelected;

        public bool ComponentAdditionWindow = false;
        public bool RunningLive = false;
        public Type tyComponentSelection;

        public Asset SelAssetBrowser;
        public string SelAssetText;

        public bool ShowAssetTextViewer = false;
        int rT = 0;
        public bool PopupBlock = false;
        Entity entity;

        void DisplayEntityTree(Entity _entity)
        {
            entity = _entity;
            //ImGui.PushID((int)entity.id);
            bool open = ImGui.TreeNodeEx(entity.Name);
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                explorerSelEntity = entity;
                if (entity.HasComponent<BoxColliderComponent>())
                {
                    selectedEntity = entity;
                }
                pEntityName = entity.Name;
            }

            ImGui.OpenPopupOnItemClick("Menu", ImGuiPopupFlags.MouseButtonRight);

            if (open)
            {

                foreach (var childEntity in Scene.LoadedScene.SceneEntities.Where(e => e.Parent == entity))
                {
                    DisplayEntityTree(childEntity);
                }

                ImGui.TreePop();
            }

            //ImGui.PopID();
        }

        public void ConfirmPopupOK(string text, Action onConfirm)
        {
            if (DrawCNFPopupOK)
            {
                ImGui.Begin("Confirmation");
                ImGui.TextWrapped(text);
                if (ImGui.Button("Ok"))
                {
                    DrawCNFPopupOK = false;
                    onConfirm();
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    DrawCNFPopupOK = false;
                }
                ImGui.End();
            }
        }

        Action RunEveryFrame;
        Action RunOnAssetSelected;

        bool pOpenPP = true;

        int igFrames = 0;
        public void DrawImGui()
        {
            try
            {
                ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
                ImGui.StyleColorsDark();

                #region Scene Editor Menu
                if (ImGui.Begin("SceneEditorMenu", ImGuiWindowFlags.NoTitleBar))
                {
                    if (ImGui.Button(IconFonts.FontAwesome6.Play))
                    {
                        new Thread(() =>
                        {
                            PreviewApplication app = new PreviewApplication();
                            RunningLive = true;
                            app.Init();
                            app.Run();
                        }).Start();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button(IconFonts.FontAwesome6.Stop))
                    {

                    }
                    ImGui.End();
                }
                #endregion
                ImGui.PushFont(Fonts["MontserratSB14p"]);
                #region Asset Text Viewer
                if (ShowAssetTextViewer)
                    if (ImGui.Begin("Asset Text Viewer"))
                    {
                        ImGui.PushFont(ImGui.GetIO().FontDefault);
                        ImGui.PopFont();
                        ImGui.PushFont(Fonts["MontserratSB16p"]);
                        ImGui.Separator();
                        ImGui.InputTextMultiline("", ref SelAssetText, 12000, new Vector2(ImGui.GetWindowSize().X - 14, 350), ImGuiInputTextFlags.ReadOnly);
                        ImGui.Separator();
                        if (ImGui.Button("Exit"))
                            ShowAssetTextViewer = false;
                        ImGui.End();
                    }
                #endregion
                #region Editor Loaded Window
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
                if (EditorJustOpened)
                    if (ImGui.Begin("Project Initialization"))
                    {
                        ImGui.TextWrapped("Welcome back to Canvastry!\n\nWould you like to select an existing project and continue where you left off or create a new project?");
                        ImGui.Separator();
                        ImGui.TextWrapped("Select an existing project: ");
                        if (editorData.ProjectPaths.Count > 0)
                        {
                            ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(15, 15));
                            if (ImGui.ListBox("", ref ProjSelIndex, editorData.ProjectPaths.ToArray(), editorData.ProjectPaths.Count))
                            {
                                var sel = editorData.ProjectPaths[ProjSelIndex];

                                var s = Directory.GetFiles(sel).First(e => Path.GetExtension(e) == ".json");

                                var pF = ProjectFile.FromFile(s);

                                SelectedPLDate = pF.LastOpened;
                                SelectedPName = pF.ProjectName;
                            }
                        }
                        else
                        {
                            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X / 2 - ImGui.CalcTextSize("no previous projects found").X / 2, ImGui.GetWindowSize().Y / 2 - (ImGui.CalcTextSize("no previous projects found").Y / 2)));
                            ImGui.TextWrapped("no previous projects found");
                        }
                        ImGui.SeparatorText("DETAILS");

                        ImGui.TextWrapped("Project Name\n" + SelectedPName);
                        ImGui.SameLine();
                        ImGui.TextWrapped("Last Opened\n" + SelectedPLDate);

                        ImGui.Separator();
                        ImGui.SetCursorPosX(ImGui.GetWindowSize().X / 4 - ImGui.CalcTextSize("Create a New Project").X / 4);
                        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - 25);
                        if (ImGui.Button("Create a New Project"))
                        {
                            SCreateProjectWindow = true;
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("Load Selected"))
                        {
                            progressBarShow = true;
                            progressBarAM = 0f;

                            var sel = editorData.ProjectPaths[ProjSelIndex];

                            var s = Directory.GetFiles(sel).First(e => Path.GetExtension(e) == ".json");

                            var pF = ProjectFile.FromFile(s);

                            if (!File.Exists(pF.PrimaryScene) || pF.PrimaryScene == null)
                                pF.PrimaryScene = "";

                            ProjectFile.LoadedProject = pF;

                            if (ProjectFile.LoadedProject != null)
                            {
                                for (int m = 0; m < ProjectFile.LoadedProject.SceneFiles.Count; m++)
                                {
                                    var scFile = ProjectFile.LoadedProject.SceneFiles[m];
                                    Console.WriteLine(scFile + " " + File.Exists(scFile));
                                    if (!File.Exists(scFile))
                                        ProjectFile.LoadedProject.SceneFiles.RemoveAt(m);
                                }

                                ProjectFile.LoadedProject.SceneFiles = ProjectFile.LoadedProject.SceneFiles.Distinct().ToList();

                                if (ProjectFile.LoadedProject.PrimaryScene == "")
                                    SelectMainScene = true;
                                else
                                    Scene.LoadSceneFromFile(ProjectFile.LoadedProject.PrimaryScene);

                                EventHandler.Invoke<ProjectLoadedEvent>(new EventData(this));
                            }

                            EditorJustOpened = false;
                        }




                        ImGui.End();
                    }
                ImGui.PopStyleVar();
                #endregion
                #region New Project Window
                ImGui.SetNextWindowSize(new Vector2(330, 160));
                if (SCreateProjectWindow)
                    if (ImGui.Begin("Create a New Project"))
                    {
                        ImGui.TextWrapped("Write the Name of your Project: ");
                        ImGui.PushID(16);
                        ImGui.InputText("", ref CreateProjectName, 5000);
                        ImGui.PopID();
                        ImGui.TextWrapped("Select the Location for your Project: ");
                        ImGui.InputText("", ref CreateProjectPath, 5000);
                        ImGui.NewLine();
                        if (ImGui.Button("Create"))
                        {
                            Directory.CreateDirectory(Path.Combine(CreateProjectPath, CreateProjectName));
                            ProjectFile proj = new ProjectFile(new List<string>(), CreateProjectName, "2024-1.0a", Path.Combine(CreateProjectPath, CreateProjectName));
                            proj.LastOpened = DateTime.Now.ToString();
                            proj.SaveToFile(Path.Combine(CreateProjectPath, CreateProjectName, CreateProjectName + ".json"));
                            Directory.CreateDirectory(Path.Combine(CreateProjectPath, CreateProjectName, "Assets"));
                            Directory.CreateDirectory(Path.Combine(CreateProjectPath, CreateProjectName, "Scenes"));
                            editorData.ProjectPaths.Add(Path.GetFullPath(Path.Combine(CreateProjectPath, CreateProjectName)));
                            ProjectFile.LoadedProject = proj;
                            EventHandler.Invoke<ProjectLoadedEvent>(new EventData(this));
                            SCreateProjectWindow = false;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            SCreateProjectWindow = false;
                        }
                        ImGui.End();
                    }
                #endregion
                #region Scene Editor Window
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
                ImGui.SetNextWindowSizeConstraints(new Vector2(400, 400), new Vector2((float)Raylib.GetScreenWidth(), (float)Raylib.GetScreenHeight()));

                // Scene View Window:
                if (ImGui.Begin("Scene Editor", ImGuiWindowFlags.NoScrollbar))
                {
                    Focused = ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows);
                    Vector2 imguiWindowPos = ImGui.GetWindowContentRegionMin();
                    Vector2 imguiWindowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
                    rlImGui.ImageRenderTextureFit(editorViewRTx.rTexture);
                    ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ImGui.GetColorU32(ImGuiCol.Border));

                    var MousePos = Raylib.GetMousePosition();
                    var RectMin = ImGui.GetItemRectMin();
                    var ScalingFactor = ImGui.GetWindowSize() / new Vector2(editorViewRTx.rTexture.Texture.Width, editorViewRTx.rTexture.Texture.Height);

                    if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(ImGui.GetItemRectMin(), ImGui.GetItemRectSize())))
                        RelativeMousePosition = (MousePos - ImGui.GetWindowPos()) / ScalingFactor;



                    ImGui.End();
                }
                else
                    Console.WriteLine("[EDITOR]: Couldn't create Scene Editor window.");

                ImGui.PopStyleVar();
                #endregion
                #region Window Bar
                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Create New Scene"))
                        {
                            if (ProjectFile.LoadedProject != null)
                            {
                                OpenInputModal = true;
                                InputModalText = "Enter Your Scene Name:";
                                WaitOnSceneCreation = false;
                            }
                        }
                        if (ImGui.MenuItem("Create New Project"))
                        {
                            SCreateProjectWindow = true;
                        }
                        if (ImGui.MenuItem("Open Scene"))
                        {
                            ShSceneOpen = true;
                        }
                        if (ImGui.MenuItem("Save Scene"))
                        {
                            if (Scene.LoadedScene != null)
                            {
                                Scene.LoadedScene.SaveToFile(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes", Scene.LoadedScene.SceneName + ".json"));
                            }
                        }
                        ImGui.EndMenu();
                    }

                    if (Scene.LoadedScene != null)
                    {
                        if (ImGui.BeginMenu("Entities"))
                        {
                            if (ImGui.MenuItem("New Entity"))
                            {
                                ShowEntityCreationWindow = true;
                            }
                            ImGui.EndMenu();
                        }
                    }

                    if(ProjectFile.LoadedProject != null)
                    {
                        if (ImGui.BeginMenu("Project"))
                        {
                            if (ImGui.MenuItem("Change Main Scene"))
                            {
                                SelectMainScene = true;
                            }
                            ImGui.EndMenu();
                        }
                    }
                    ImGui.EndMainMenuBar();
                }
                #endregion
                #region Scene Explorer Window
                if (ImGui.Begin("Explorer"))
                {
                    ImGui.PushFont(Fonts["MontserratSB14p"]);
                    if (Scene.LoadedScene != null)
                    {
                        if (ImGui.TreeNodeEx("Scene"))
                        {
                            if (ImGui.IsItemClicked())
                            {
                                explorerSelEntity = null;
                                pEntityName = Scene.LoadedScene.SceneName;
                            }

                            rT = 0;

                            foreach (var entity in Scene.LoadedScene.SceneEntities.Where(e => e.Parent == null && e != null))
                            {
                                DisplayEntityTree(entity);
                            }

                            if (ImGui.BeginPopupModal("Menu"))
                            {
                                rT++;
                                if (ImGui.MenuItem("New Child"))
                                {
                                    explorerSelEntity = entity;
                                    ShowEntityCreationWindow = true;
                                }
                                if (ImGui.MenuItem("Remove"))
                                {
                                    entity.scene.DestroyEntity(entity);
                                }

                                ImGui.EndPopup();
                            }
                            ImGui.TreePop();
                        }
                    }



                    ImGui.PopFont();
                    ImGui.End();
                }
                #endregion
                #region Entity Creation Window
                if (ShowEntityCreationWindow)
                {
                    if (ImGui.Begin("Create a New Entity"))
                    {
                        ImGui.PushFont(Fonts["MontserratSB16p"]);
                        ImGui.Text("Select an Entity Type");
                        ImGui.PopFont();
                        if (ImGui.BeginListBox(""))
                        {
                            ImGui.PushItemWidth(125);
                            if (ImGui.Button("Camera"))
                            {
                                SelectedEntityType = "Camera";
                                tySelectedEntityType = typeof(CameraEntity);
                            }

                            if (ImGui.Button("Sprite"))
                            {
                                SelectedEntityType = "Sprite";
                                tySelectedEntityType = typeof(SpriteEntity);
                            }

                            if (ImGui.Button("Rectangle"))
                            {
                                SelectedEntityType = "Rectangle";
                                tySelectedEntityType = typeof(RectangleEntity);
                            }
                            ImGui.PopItemWidth();
                            ImGui.EndListBox();
                        }

                        ImGui.PushID(0);
                        ImGui.PushFont(Fonts["MontserratSB16p"]);
                        ImGui.Text("Entity Type");
                        ImGui.InputText("", ref SelectedEntityType, 50, ImGuiInputTextFlags.ReadOnly);
                        ImGui.PopFont();
                        ImGui.PopID();

                        ImGui.PushFont(Fonts["MontserratSB16p"]);
                        ImGui.Text("Entity Name");
                        ImGui.InputText("", ref EntityCName, 35, ImGuiInputTextFlags.AutoSelectAll);


                        ImGui.Spacing();
                        ImGui.NewLine();

                        System.Numerics.Vector2 contentMax = ImGui.GetWindowContentRegionMax();
                        float padding = 15; // Adjust the padding as needed

                        float buttonWidth = 90;
                        float buttonHeight = 7;

                        float buttonPosX = contentMax.X - buttonWidth - padding;
                        float buttonPosY = contentMax.Y - buttonHeight - padding;

                        ImGui.SetCursorPos(new System.Numerics.Vector2(buttonPosX, buttonPosY));

                        if (ImGui.Button("Create"))
                        {
                            var mth = typeof(Scene).GetMethod("CreateEntity");
                            var mthRef = mth.MakeGenericMethod(tySelectedEntityType);
                            mthRef.Invoke(Scene.LoadedScene, new object[] { explorerSelEntity, EntityCName });
                            ShowEntityCreationWindow = false;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Close"))
                        {
                            ShowEntityCreationWindow = false;
                        }
                        ImGui.PopFont();
                        ImGui.End();
                    }
                }
                #endregion
                #region Properties Window

                if (ImGui.Begin("Properties"))
                {
                    if (explorerSelEntity != null)
                    {
                        ImGui.PushFont(Fonts["MontserratSB16p"]);

                        ImGui.Text("Identification");
                        ImGui.Separator();
                        ImGui.Text("Type");
                        string str = explorerSelEntity.GetType().Name;
                        ImGui.PushID(0);
                        ImGui.InputText("", ref str, 500, ImGuiInputTextFlags.ReadOnly);
                        ImGui.PopID();
                        ImGui.Text("Name");
                        ImGui.PushID(1);
                        if (ImGui.InputText("", ref pEntityName, 500))
                        {
                            explorerSelEntity.Name = pEntityName;
                        }
                        ImGui.PopID();
                        ImGui.NewLine();
                        ImGui.Text("Components");

                        ImGui.Separator();
                        var sepSize = ImGui.CalcItemWidth();


                        for (int x = 0; x < explorerSelEntity.Components.Count; x++)
                        {
                            var component = explorerSelEntity.Components[x];
                            if (component is TransformComponent)
                            {
                                var transform = (TransformComponent)component;

                                ImGui.SeparatorText("TransformComponent");
                                ImGui.Text("Position: ");
                                ImGui.SameLine();
                                ImGui.PushID(2);
                                ImGui.SetCursorPosX(ImGui.CalcItemWidth() / 2 - 5);
                                ImGui.InputFloat2("", ref transform.Position);
                                ImGui.PopID();
                                ImGui.Text("Size: ");
                                ImGui.SameLine();
                                ImGui.PushID(3);
                                ImGui.SetCursorPosX(ImGui.CalcItemWidth() / 2 - 5);
                                ImGui.InputFloat2("", ref transform.Size);
                                ImGui.PopID();
                                ImGui.Text("Scale: ");
                                ImGui.SameLine();
                                ImGui.PushID(4);
                                ImGui.SetCursorPosX(ImGui.CalcItemWidth() / 2 - 5);
                                ImGui.InputFloat2("", ref transform.Scale);
                                ImGui.PopID();
                                ImGui.Text("Rotation: ");
                                ImGui.SameLine();
                                ImGui.PushID(5);
                                ImGui.SetCursorPosX(ImGui.CalcItemWidth() / 2 - 5);
                                ImGui.InputFloat("", ref transform.Rotation);
                                ImGui.PopID();
                            }
                            else if (component is MaterialComponent)
                            {
                                var mat = (MaterialComponent)component;

                                ImGui.SeparatorText("MaterialComponent");
                                ImGui.Text("Color: ");
                                ImGui.SameLine();
                                ImGui.PushID(6);
                                if (ImGui.ColorEdit4("", ref matCompColor))
                                    mat.MaterialColor = new Color((byte)(matCompColor.X * 255), (byte)(matCompColor.Y * 255), (byte)(matCompColor.Z * 255), (byte)(matCompColor.W * 255));
                                ImGui.PopID();
                            }
                            else if (component is ScriptBehaviourComponent)
                            {
                                var scriptC = (ScriptBehaviourComponent)component;

                                ImGui.SeparatorText("ScriptBehaviourComponent");
                                ImGui.TextWrapped("Script: ");
                                ImGui.SameLine();
                                if (scriptC.ScriptPath != null)
                                {
                                    var thing = Path.GetFileName(scriptC.ScriptPath);
                                    ImGui.PushID(7);
                                    ImGui.InputText("", ref thing, 5000, ImGuiInputTextFlags.ReadOnly);
                                    ImGui.PopID();
                                    ImGui.SameLine();
                                }

                                ImGui.PushFont(ImGui.GetIO().FontDefault);
                                if (ImGui.Button(IconFonts.FontAwesome6.Upload, new Vector2(24, ImGui.GetItemRectSize().Y)))
                                {
                                    RunOnAssetSelected = () => { if (AssetSelected.Type == AssetType.CODE) scriptC.ScriptPath = AssetSelected.Path; };
                                    AssetSelFilter = ".lua";
                                    AssetSelectionWindow = true;
                                }

                                if (scriptC.ScriptPath != null)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button(IconFonts.FontAwesome6.TrashCan, new Vector2(24, ImGui.GetItemRectSize().Y)))
                                    {
                                        scriptC.ScriptPath = null;
                                        scriptC._Script = null;
                                    }
                                }
                                ImGui.NewLine();
                                if (ImGui.Button(IconFonts.FontAwesome6.TrashCan, new Vector2(24, ImGui.GetItemRectSize().Y)))
                                {
                                    if (!explorerSelEntity.RemoveComponent<ScriptBehaviourComponent>())
                                    {
                                        Console.WriteLine("Could not remove component.");
                                    }
                                }

                                ImGui.PopFont();
                                ImGui.PushFont(Fonts["MontserratSB16p"]);
                            }

                            else if (component is AudioSourceComponent)
                            {
                                var audio = (AudioSourceComponent)component;

                                ImGui.SeparatorText("AudioSourceComponent");
                                ImGui.TextWrapped("Audio: ");
                                ImGui.SameLine();
                                if (audio.AudioPath != null)
                                {
                                    var thing = Path.GetFileName(audio.AudioPath);
                                    ImGui.PushID(7);
                                    ImGui.InputText("", ref thing, 5000, ImGuiInputTextFlags.ReadOnly);
                                    ImGui.PopID();
                                    ImGui.SameLine();
                                }
                                ImGui.PushID(9);
                                ImGui.PushFont(ImGui.GetIO().FontDefault);
                                if (ImGui.Button(IconFonts.FontAwesome6.Upload, new Vector2(24, ImGui.GetItemRectSize().Y)))
                                {
                                    RunOnAssetSelected = () => { if (AssetSelected.Type == AssetType.AUDIO) audio.AudioPath = AssetSelected.Path; };
                                    AssetSelFilter = "all";
                                    Console.WriteLine("SELECTING AD");
                                    AssetSelectionWindow = true;
                                }
                                ImGui.PopFont();
                                ImGui.PushFont(Fonts["MontserratSB16p"]);
                                ImGui.PopID();
                                ImGui.TextWrapped("Volume: ");
                                ImGui.SameLine();

                                ImGui.InputFloat("", ref vol);
                                audio.Volume = vol;

                                ImGui.TextWrapped("Try Playback: ");
                                ImGui.SameLine();

                                ImGui.PushFont(ImGui.GetIO().FontDefault);

                                if (ImGui.Button(IconFonts.FontAwesome6.Play, new Vector2(20, 20)))
                                {
                                    audio.Play();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button(IconFonts.FontAwesome6.Pause, new Vector2(20, 20)))
                                {
                                    audio.Pause();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button(IconFonts.FontAwesome6.Stop, new Vector2(20, 20)))
                                {
                                    audio.Stop();
                                }
                                ImGui.NewLine();
                                if (ImGui.Button(IconFonts.FontAwesome6.TrashCan, new Vector2(24, ImGui.GetItemRectSize().Y)))
                                {
                                    explorerSelEntity.RemoveComponent<AudioSourceComponent>();
                                }

                                ImGui.PopFont();
                                ImGui.PushFont(Fonts["MontserratSB16p"]);
                            }
                        }

                        ImGui.Separator();
                        ImGui.SetCursorPosX((ImGui.GetContentRegionMax().X / 2) - ImGui.CalcTextSize("Add Components").X / 2);
                        if (ImGui.Button("Add Components"))
                        {
                            ComponentAdditionWindow = true;
                        }

                        if (ComponentAdditionWindow)
                        {
                            ImGui.SetCursorPosX(40);
                            if (ImGui.BeginListBox(""))
                            {
                                ImGui.PushItemWidth(125);
                                if (ImGui.Button("Box Collider"))
                                {
                                    tyComponentSelection = typeof(BoxColliderComponent);
                                }

                                if (ImGui.Button("Script Behaviour"))
                                {
                                    tyComponentSelection = typeof(ScriptBehaviourComponent);
                                }

                                if (ImGui.Button("Audio Source"))
                                {
                                    tyComponentSelection = typeof(AudioSourceComponent);
                                }
                                ImGui.PopItemWidth();
                                ImGui.EndListBox();
                            }
                            ImGui.SetCursorPosX((ImGui.GetContentRegionMax().X / 2) - ImGui.CalcTextSize("Add").X / 2);
                            if (ImGui.Button("Add"))
                            {
                                var mth = typeof(Entity).GetMethod("AddComponent");
                                var mthRef = mth.MakeGenericMethod(tyComponentSelection);
                                mthRef.Invoke(selectedEntity, new object[] { Activator.CreateInstance(tyComponentSelection) });
                                ComponentAdditionWindow = false;
                            }
                        }

                        if (explorerSelEntity is CameraEntity)
                        {
                            ImGui.Separator();
                            ImGui.SetCursorPosX((ImGui.GetContentRegionMax().X / 2) - ImGui.CalcTextSize("Set as Scene Camera").X / 2);
                            if (ImGui.Button("Set as Scene Camera"))
                            {
                                Scene.LoadedScene.SceneCamera = (CameraEntity)explorerSelEntity;
                            }
                        }

                        ImGui.PopFont();

                    }
                    else
                    {
                        ImGui.PushFont(Fonts["MontserratSB16p"]);

                        ImGui.Text("Identification");
                        ImGui.Separator();
                        ImGui.Text("Type");
                        string str = "Scene";
                        ImGui.PushID(0);
                        ImGui.InputText("", ref str, 500, ImGuiInputTextFlags.ReadOnly);
                        ImGui.PopID();
                        ImGui.Text("Name");
                        ImGui.PushID(1);
                        if (ImGui.InputText("", ref pEntityName, 500))
                        {
                            Scene.LoadedScene.SceneName = pEntityName;
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginItemTooltip();
                            ImGui.Text("Set the name of the currently loaded scene. It will appear in the filename.");
                            ImGui.EndTooltip();
                        }
                        ImGui.PopID();
                        ImGui.End();
                    }

                    ImGui.End();
                }
                #endregion
                #region Asset Explorer
                if (ImGui.Begin("Asset Explorer"))
                {
                    ImGui.BeginTabBar("asset_exp");

                    if (ImGui.BeginTabItem("Assets"))
                    {
                        int assetCount = AssetManager.LoadedAssetCount;
                        int columns = 5;

                        // Calculate the number of rows needed for the assets.
                        int rowCount = (assetCount + columns - 1) / columns;

                        if (ImGui.BeginTable("assets", 5, ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.NoBordersInBody))
                        {

                            ImGui.TableSetupColumn("");
                            ImGui.TableSetupColumn("");
                            ImGui.TableSetupColumn("");
                            ImGui.TableSetupColumn("");
                            ImGui.TableSetupColumn("");

                            int assetIndex = 0;
                            for (int i = 0; i < rowCount; i++)
                            {
                                ImGui.TableNextRow();

                                for (int j = 0; j < columns; j++)
                                {
                                    ImGui.TableSetColumnIndex(j);

                                    if (assetIndex < assetCount)
                                    {
                                        string assetPath = AssetManager.LoadedAssets.Keys.ElementAt(assetIndex);
                                        ImGui.PushFont(Fonts["MontserratSB16p"]);
                                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(31, 31, 31, 0));
                                        if (rlImGui.ImageButtonSize("", FileIconImage, new Vector2(128, 128)))
                                        {
                                            SelAssetBrowser = AssetManager.GetLoadedAsset(assetPath);
                                        }

                                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                        {
                                            Console.WriteLine("Loading VSCode...");

                                            ProcessStartInfo info = new ProcessStartInfo("code");
                                            info.CreateNoWindow = true;
                                            info.Arguments = "\"" + assetPath + "\"";
                                            info.UseShellExecute = true;
                                            Process.Start(info);
                                        }

                                        if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
                                        {
                                            SelAssetBrowser = AssetManager.GetLoadedAsset(assetPath);
                                            if (SelAssetBrowser.Type == AssetType.CODE)
                                            {
                                                SelAssetText = (SelAssetBrowser.Data as CodeAssetRef).Code;
                                            }
                                            ShowAssetTextViewer = true;
                                        }

                                        ImGui.Separator();
                                        ImGui.TextWrapped(Path.GetFileName(assetPath));
                                        ImGui.PopStyleColor();
                                        ImGui.PopFont();
                                        assetIndex++;
                                    }
                                }
                            }

                            ImGui.EndTable();
                        }

                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Scenes"))
                    {
                        if (ProjectFile.LoadedProject != null)
                        {
                            int assetCount = Directory.GetFiles(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes")).Count();
                            int columns = 5;

                            // Calculate the number of rows needed for the assets.
                            int rowCount = (assetCount + columns - 1) / columns;

                            if (ImGui.BeginTable("assets", 5, ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.NoBordersInBody))
                            {

                                ImGui.TableSetupColumn("");
                                ImGui.TableSetupColumn("");
                                ImGui.TableSetupColumn("");
                                ImGui.TableSetupColumn("");
                                ImGui.TableSetupColumn("");

                                int assetIndex = 0;
                                for (int i = 0; i < rowCount; i++)
                                {
                                    ImGui.TableNextRow();

                                    for (int j = 0; j < columns; j++)
                                    {
                                        ImGui.TableSetColumnIndex(j);

                                        if (assetIndex < assetCount)
                                        {
                                            string assetPath = Directory.GetFiles(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes")).ElementAt(assetIndex);
                                            ImGui.PushFont(Fonts["MontserratSB16p"]);
                                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(31, 31, 31, 0));
                                            ImGui.PushID(assetIndex);
                                            if (rlImGui.ImageButtonSize("", SceneIconImage, new Vector2(128, 128)))
                                            {
                                                DrawCNFPopupOK = true;

                                                RunEveryFrame = () =>
                                                {
                                                    ConfirmPopupOK(
                                                        "This action will close the current scene.",
                                                        () =>
                                                        {
                                                            Scene.LoadScene(Scene.LoadSceneFromFile(assetPath));
                                                        }
                                                    );
                                                };
                                            }
                                            ImGui.PopID();
                                            ImGui.Separator();
                                            ImGui.TextWrapped(Path.GetFileName(assetPath));
                                            ImGui.PopStyleColor();
                                            ImGui.PopFont();
                                            assetIndex++;
                                        }
                                    }
                                }

                                ImGui.EndTable();
                            }
                            ImGui.EndTabItem();
                        }
                    }
                    if (ImGui.BeginTabItem("Console"))
                    {
                        ImGui.TextWrapped(consoleOutput.ToString());
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                    ImGui.End();
                }

                #endregion
                #region Extras

                ImGui.SetNextWindowPos(new Vector2(engineApp.Settings.WindowWidth / 2, engineApp.Settings.WindowHeight / 2));

                if (WaitOnSceneCreation)
                {
                    Scene.LoadScene(Scene.CreateScene());
                    Scene.LoadedScene.SceneName = InputtedText;
                    Scene.LoadedScene.SaveToFile(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes", Scene.LoadedScene.SceneName + ".json"));
                    ProjectFile.LoadedProject.SceneFiles.Add(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes", Scene.LoadedScene.SceneName + ".json"));
                    WaitOnSceneCreation = false;
                }

                if (ProjectFile.LoadedProject != null)
                    if (!Directory.GetFiles(Path.Combine(ProjectFile.LoadedProject.Path, "Assets")).All(e => AssetManager.LoadedAssets.ContainsKey(e)))
                    {
                        foreach (var f in Directory.GetFiles(Path.Combine(ProjectFile.LoadedProject.Path, "Assets")))
                        {
                            if (!AssetManager.LoadedAssets.ContainsKey(f))
                            {
                                Console.WriteLine("Detected unloaded asset file " + f + ". Loading Asset...");

                                AssetManager.LoadAsset(f, AssetType.AUTO);
                                progressBarShow = true;
                            }
                        }
                    }

                if (ProjectFile.LoadedProject != null)
                    for (int m = 0; m < AssetManager.LoadedAssetCount; m++)
                    {
                        var asset = AssetManager.LoadedAssets.Values.ToImmutableList()[m];
                        if (asset.Type == AssetType.CODE)
                        {
                            var code = (CodeAssetRef)asset.Data;
                            var currentData = File.ReadAllText(asset.Path);

                            if (code.Code != currentData)
                            {
                                Console.WriteLine("Updating File " + asset.Path);
                                (AssetManager.LoadedAssets[asset.Path].Data as CodeAssetRef).Code = currentData;
                            }
                        }
                    }

                if (ProjectFile.LoadedProject != null)
                {
                    if (AssetManager.LoadedAssets.Keys.Any(e => !File.Exists(e)))
                    {
                        foreach (var x in AssetManager.LoadedAssets.Keys)
                        {
                            if (!File.Exists(x))
                            {
                                AssetManager.UnloadAsset(x);
                            }
                        }
                    }
                }

                if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.U))
                {
                    selectedEntity = null;
                }
                #endregion
                #region Popups and Input Windows
                if (progressBarShow)
                    if (ImGui.Begin("loadingsl", ImGuiWindowFlags.NoTitleBar))
                    {
                        if (igFrames % 1 == 0)
                            progressBarAM += 0.01f;

                        if (progressBarAM > 1f)
                        {
                            progressBarShow = false;
                            progressBarAM = 0f;
                        }


                        ImGui.ProgressBar(progressBarAM, new Vector2(180, 25));

                        ImGui.End();
                    }



                if (ShSceneOpen)
                    if (ImGui.Begin("Enter Scene Path", ImGuiWindowFlags.NoTitleBar))
                    {
                        ImGui.TextWrapped("Enter the Path to your Scene file (.json): ");
                        ImGui.InputText("", ref SelectedSceneFile, 5000);
                        if (ImGui.IsKeyPressed(ImGuiKey.Enter))
                        {
                            Scene.LoadSceneFromFile(SelectedSceneFile.Replace("$(Project-Path)", ProjectFile.LoadedProject.Path));
                            ShSceneOpen = false;
                        }
                        ImGui.End();
                    }

                if (OpenInputModal)
                {
                    ImGui.SetNextWindowSize(new Vector2(200, 95));
                    if (ImGui.Begin("Enter Input"))
                    {
                        ImGui.TextWrapped(InputModalText);
                        ImGui.InputText("", ref InputtedText, 5000);

                        if (ImGui.IsKeyPressed(ImGuiKey.Enter))
                        {
                            WaitOnSceneCreation = true;
                            OpenInputModal = false;
                        }

                        ImGui.End();
                    }
                }

                if (SelectMainScene)
                    if (ImGui.Begin("Select the Main Scene"))
                    {
                        ImGui.TextWrapped("Select a main project scene file.");

                        if (ImGui.Button("Create New Scene"))
                        {
                            if (ProjectFile.LoadedProject != null)
                            {
                                OpenInputModal = true;
                                InputModalText = "Enter Your Scene Name:";
                                WaitOnSceneCreation = false;
                            }
                        }

                        if (ImGui.ListBox("", ref MainSceneIdx, ProjectFile.LoadedProject.SceneFiles.ToArray(), ProjectFile.LoadedProject.SceneFiles.Count))
                        {

                        }

                        ImGui.Text("Selected\n" + Path.GetFileNameWithoutExtension(ProjectFile.LoadedProject.SceneFiles[MainSceneIdx]));

                        if (ImGui.Button("Use as Main Scene"))
                        {
                            ProjectFile.LoadedProject.PrimaryScene = ProjectFile.LoadedProject.SceneFiles[MainSceneIdx];
                            Scene.LoadSceneFromFile(ProjectFile.LoadedProject.PrimaryScene);
                            SelectMainScene = false;
                        }

                        ImGui.End();
                    }
                if (AssetSelectionWindow)
                    if (ImGui.Begin("Select an Asset", ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.TextWrapped("Select an Existing Asset");
                        if (ImGui.BeginListBox(""))
                        {
                            foreach (var asset in AssetManager.LoadedAssets)
                            {
                                if (Path.GetExtension(asset.Value.Path) == AssetSelFilter || AssetSelFilter == "all")
                                {
                                    var RelativePath = Path.GetRelativePath(Path.Combine(ProjectFile.LoadedProject.Path, "Assets"), asset.Value.Path);

                                    if (ImGui.Button(RelativePath))
                                    {
                                        AssetSelected = asset.Value;
                                    }
                                }
                            }
                            ImGui.EndListBox();
                        }
                        if (AssetSelected != null)
                            ImGui.TextWrapped("Selected: " + Path.GetFileName(AssetSelected.Path));

                        if (ImGui.Button("Select"))
                        {
                            if (RunOnAssetSelected != null)
                                RunOnAssetSelected();
                            AssetSelectionWindow = false;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Close"))
                        {
                            AssetSelectionWindow = false;
                        }

                        ImGui.End();
                    }

                #endregion


                ImGui.PopFont();
            }
            catch (Exception e)
            {
                Console.WriteLine("[EDITOR]: Exception caught " + e.GetType() + " | " + e.Message + " Stack Trace: " + e.StackTrace);
            }
            //System.Threading.Thread.Sleep(16);
            igFrames++;

            if (RunEveryFrame != null)
                RunEveryFrame();
        }

        public void OnRaylibInitialized(EventData data)
        {
            editorViewRTx = Rendertexture.RequestRTx(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Rendertexture.CurrentRTx = editorViewRTx;
            LoadFont("resources/fonts/montserrat/Montserrat-SemiBold.ttf", "MontserratSB14p", 14f);
            LoadFont("resources/fonts/montserrat/Montserrat-SemiBold.ttf", "MontserratSB16p", 16f);
            _propStyle.FrameBorderSize = 1.0f;

            FileIconImage = Raylib.LoadTexture("resources/images/FileIcon.png");
            FolderIconImage = Raylib.LoadTexture("resources/images/FolderIcon.png");
            TextureIconImage = Raylib.LoadTexture("resources/images/TextureIcon.png");
            SceneIconImage = Raylib.LoadTexture("resources/images/SceneIcon.png");

            rlImGui.ReloadFonts();
        }

        public ImFontPtr LoadFont(string path, string id, float fontSize)
        {
            var io = ImGui.GetIO();
            var fnt = io.Fonts.AddFontFromFileTTF(path, fontSize);

            rlImGui.ReloadFonts();
            Fonts.Add(id, fnt);

            return fnt;
        }

        public void OnProjectLoaded(EventData data)
        {
            var proj = ProjectFile.LoadedProject;

            Raylib.SetWindowTitle("Canvastry v.2024-1.0a - " + proj.ProjectName);
            proj.LastOpened = DateTime.Now.ToString();
        }

        Entity selectedEntity = null;

        /// <summary>
        /// Draws them based on the selectedEntity variable.
        /// </summary>
        public void DrawTransformGizmo()
        {
            var boxCollider = selectedEntity.GetComponent<BoxColliderComponent>();
            Raylib.DrawLineEx(boxCollider.Position + new Vector2(boxCollider.Size.X + 15, boxCollider.Size.Y / 2), boxCollider.Position + new Vector2(boxCollider.Size.X + 110, boxCollider.Size.Y / 2), 15, Color.Red);
            Raylib.DrawLineEx(boxCollider.Position + new Vector2(boxCollider.Size.X / 2, -15), boxCollider.Position + new Vector2(boxCollider.Size.X / 2, -95), 15, Color.Green);
            Raylib.DrawCircleV(boxCollider.Position + new Vector2(boxCollider.Size.X / 2, boxCollider.Size.Y / 2), 15, Color.Gray);
        }
        public void DrawScaleGizmo()
        {
            var boxCollider = selectedEntity.GetComponent<BoxColliderComponent>();
            Raylib.DrawRectangleV(boxCollider.Position + new Vector2(-35, boxCollider.Size.Y / 2), new Vector2(15, 15), Color.Red);
            Raylib.DrawRectangleV(boxCollider.Position + new Vector2(boxCollider.Size.X / 2, boxCollider.Size.Y + 25), new Vector2(15, 15), Color.Blue);
        }

        bool isDragging = false;
        Vector2 dragStart = new Vector2(0, 0);

        bool isDraggingY = false;
        Vector2 dragStartY = new Vector2(0, 0);

        bool isDraggingA = false;
        Vector2 dragStartA = new Vector2(0, 0);

        Vector2 lastSize;
        public void UpdateTransformGizmo()
        {
            var boxCollider = selectedEntity.GetComponent<BoxColliderComponent>();
            if (Input.IsMouseButtonDown(InputLib.MouseButton.Left))
            {
                if (!isDraggingXs && !isDraggingYs)
                    if (Raylib.CheckCollisionPointLine(RelativeMousePosition, boxCollider.Position + new Vector2(boxCollider.Size.X + 15, boxCollider.Size.Y / 2), boxCollider.Position + new Vector2(boxCollider.Size.X + 110, boxCollider.Size.Y / 2), 24))
                    {
                        if (isDragging == false)
                        {
                            isDragging = true;
                            dragStart = Raylib.GetMousePosition();
                        }
                    }

                if (isDragging)
                    if (dragStart.X != Raylib.GetMousePosition().X)
                    {
                        Vector2 dragEnd = Raylib.GetMousePosition();
                        Vector2 dragDelta = new Vector2(dragEnd.X - dragStart.X, 0);

                        selectedEntity.GetComponent<TransformComponent>().Position += dragDelta;

                        dragStart = dragEnd;
                    }
                if (!isDraggingXs && !isDraggingYs)
                    if (Raylib.CheckCollisionPointLine(RelativeMousePosition, boxCollider.Position + new Vector2(boxCollider.Size.X / 2, -15), boxCollider.Position + new Vector2(boxCollider.Size.X / 2, -95), 24))
                    {
                        if (isDraggingY == false)
                        {
                            isDraggingY = true;
                            dragStartY = Raylib.GetMousePosition();
                        }
                    }

                if (isDraggingY)
                    if (dragStartY.Y != Raylib.GetMousePosition().Y)
                    {
                        Vector2 dragEnd = Raylib.GetMousePosition();
                        Vector2 dragDelta = new Vector2(0, dragEnd.Y - dragStartY.Y);

                        selectedEntity.GetComponent<TransformComponent>().Position += dragDelta;

                        dragStartY = dragEnd;
                    }
            }
            else
            {
                isDragging = false;
                isDraggingY = false;
            }
        }

        Vector2 dragStartXs, dragStartYs;
        bool isDraggingXs = false, isDraggingYs = false;
        public void UpdateScaleGizmo()
        {
            var boxCollider = selectedEntity.GetComponent<BoxColliderComponent>();

            if (Input.IsMouseButtonDown(InputLib.MouseButton.Left))
            {
                if (!isDragging && !isDraggingY)
                    if (Raylib.CheckCollisionPointRec(RelativeMousePosition, new Rectangle(boxCollider.Position + new Vector2(-35, boxCollider.Size.Y / 2), new Vector2(35, 35))))
                    {
                        if (isDraggingXs == false)
                        {
                            isDraggingXs = true;
                            dragStartXs = Raylib.GetMousePosition();
                        }
                    }
                if (isDraggingXs)
                {
                    Vector2 dragEnd = Raylib.GetMousePosition();
                    Vector2 dragDelta = new Vector2(-(dragEnd.X - dragStartXs.X), 0);
                    selectedEntity.GetComponent<TransformComponent>().Size += dragDelta;

                    dragStartXs = dragEnd;
                }
                if (!isDragging && !isDraggingY)
                    if (Raylib.CheckCollisionPointRec(RelativeMousePosition, new Rectangle(boxCollider.Position + new Vector2(boxCollider.Size.X / 2, boxCollider.Size.Y + 25), new Vector2(35, 35))))
                    {
                        if (isDraggingYs == false)
                        {
                            isDraggingYs = true;
                            dragStartYs = Raylib.GetMousePosition();
                        }
                    }
                if (isDraggingYs)
                {
                    Vector2 dragEnd = Raylib.GetMousePosition();
                    Vector2 dragDelta = new Vector2(0, dragEnd.Y - dragStartYs.Y);

                    selectedEntity.GetComponent<TransformComponent>().Size += dragDelta;

                    dragStartYs = dragEnd;
                }

                if (selectedEntity.GetComponent<TransformComponent>().Size.X < 0)
                {
                    selectedEntity.GetComponent<TransformComponent>().Size = new Vector2(0, selectedEntity.GetComponent<TransformComponent>().Size.Y);
                }
                else if (selectedEntity.GetComponent<TransformComponent>().Size.Y < 0)
                {
                    selectedEntity.GetComponent<TransformComponent>().Size = new Vector2(selectedEntity.GetComponent<TransformComponent>().Size.X, 0);

                }
                else if (selectedEntity.GetComponent<TransformComponent>().Size.X < 0 && selectedEntity.GetComponent<TransformComponent>().Size.Y < 0)
                {
                    selectedEntity.GetComponent<TransformComponent>().Size = new Vector2(0, 0);
                }
            }
            else
            {
                isDraggingXs = false; isDraggingYs = false;
            }
        }

        public void OnAppLoop()
        {
            if (selectedEntity != null && Scene.LoadedScene.SceneEntities.Contains(selectedEntity))
            {
                var boxCollider = selectedEntity.GetComponent<BoxColliderComponent>();
                Raylib.DrawRectangleLinesEx(new Rectangle(boxCollider.Position, boxCollider.Size), 2, Color.Orange);

                DrawTransformGizmo();
                DrawScaleGizmo();
                UpdateTransformGizmo();
                UpdateScaleGizmo();
            }

            Raylib.DrawCircleV(RelativeMousePosition, 3, Color.Blue);
            bool found = false;
            if (Focused)
                if (Input.IsMouseButtonDown(InputLib.MouseButton.Left))
                {
                    foreach (var e in Scene.LoadedScene.SceneEntities)
                    {
                        if (e.HasComponent<BoxColliderComponent>())
                        {
                            var boxCollider = e.GetComponent<BoxColliderComponent>();

                            if (Raylib.CheckCollisionPointRec(RelativeMousePosition, new Rectangle(boxCollider.Position, boxCollider.Size)))
                            {
                                selectedEntity = e;
                                explorerSelEntity = e;
                                found = true;
                                break;
                            }
                        }
                    }
                }
        }

        public void Run()
        {
            engineApp = new Application(new ApplicationSettings(1720, 920, "Canvastry v.2024-1.0a - No Project Selected"));
            engineApp.Initialize();
            engineApp.StandaloneRun = false;
            engineApp.RenderCamera = new Camera2D(
                target: new Vector2(0, 0), // target
                offset: new Vector2(Raylib.GetScreenWidth() / 2.0f, Raylib.GetScreenHeight() / 2.0f), // offset
                rotation: 0, // rotation
                zoom: 1 // zoom
            );
            engineApp.Settings.WindowClearColor = Color.DarkGray;
            engineApp.ImGuiRenderFunction = DrawImGui;
            engineApp.RenderFunction = OnAppLoop;
            EventHandler.Subscribe<RaylibInitializedEvent>(OnRaylibInitialized);

            if (File.Exists("editor-settings.json"))
            {
                editorData = JsonConvert.DeserializeObject<EditorData>(File.ReadAllText("editor-settings.json"));

                // Cleanup non-existing projects:

                for (int x = 0; x < editorData.ProjectPaths.Count; x++)
                    if (!Directory.Exists(editorData.ProjectPaths[x]))
                        editorData.ProjectPaths.RemoveAt(x);
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CanvastryEditorData"));
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CanvastryEditorData", "Channel"));
            }

            EventHandler.Subscribe<ProjectLoadedEvent>(OnProjectLoaded);

            //Console.SetOut(consoleOutput);

            engineApp.Run();

            if (ProjectFile.LoadedProject != null)
            {
                if (Interaction.MsgBox("Do you want to save these changes?", "Save Changes", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    if (Scene.LoadedScene != null)
                    {
                        Scene.LoadedScene.SaveToFile(Path.Combine(ProjectFile.LoadedProject.Path, "Scenes", Scene.LoadedScene.SceneName + ".json"));
                    }
                    
                }
                ProjectFile.LoadedProject.SaveToFile(Path.Combine(ProjectFile.LoadedProject.Path, ProjectFile.LoadedProject.ProjectName + ".json"));
            }
            File.WriteAllText("editor-settings.json", JsonConvert.SerializeObject(editorData, Formatting.Indented));

            engineApp.Shutdown();
        }

        public EditorApplication() { }
    }
}
