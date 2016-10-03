using System;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsole.Consoles;
using SadConsole;
using System.IO;
using System.Collections.Generic;
using SadConsole.Game;
using System.Linq;

namespace SadConsoleEditor.Editors
{
    class SceneEditor : IEditor
    {
        private LayeredTextSurface textSurface;
        private Console consoleWrapper;
        private CustomPanel[] panels;
        private LayersPanel layerManagementPanel;
        private ToolsPanel toolsPanel;
        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;

        public Panels.EntityManagementPanel GameObjectPanel;
        public Panels.Scene.AnimationListPanel AnimationsPanel;


        private GameObject _selectedGameObject;
        public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
        public GameObjectCollection GameObjects;

        public GameObject SelectedEntity
        {
            get { return _selectedGameObject; }
            set { _selectedGameObject = value; AnimationsPanel.RebuildListBox(); }
        }


        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.Console; } }

        public string EditorTypeName { get { return "Scene"; } }

        public string Title { get; set; }

        public int Height { get { return textSurface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public int Width { get { return textSurface.Width; } }

        public CustomPanel[] Panels { get { return panels; } }

        public Console RenderedConsole { get { return consoleWrapper; } }

        private Tools.ITool SelectedTool
        {
            get { return selectedTool; }
            set
            {
                toolsPanel.ToolsListBox.SelectedItem = value;
            }
        }

        public SceneEditor()
        {
            consoleWrapper = new Console(1, 1);
            consoleWrapper.Renderer = new LayeredTextRenderer();
            consoleWrapper.MouseHandler = ProcessMouse;
            consoleWrapper.CanUseKeyboard = false;

            consoleWrapper.MouseMove += (o, e) => { toolsPanel.SelectedTool?.MouseMoveSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseEnter += (o, e) => { toolsPanel.SelectedTool?.MouseEnterSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseExit += (o, e) => { toolsPanel.SelectedTool?.MouseExitSurface(e.OriginalMouseInfo, textSurface); };

            layerManagementPanel = new LayersPanel();
            toolsPanel = new ToolsPanel();

            // Fill tools
            tools = new Dictionary<string, Tools.ITool>();
            tools.Add(Tools.PaintTool.ID, new Tools.PaintTool());
            tools.Add(Tools.LineTool.ID, new Tools.LineTool());
            tools.Add(Tools.CircleTool.ID, new Tools.CircleTool());
            tools.Add(Tools.RecolorTool.ID, new Tools.RecolorTool());
            tools.Add(Tools.FillTool.ID, new Tools.FillTool());
            tools.Add(Tools.BoxTool.ID, new Tools.BoxTool());
            tools.Add(Tools.SelectionTool.ID, new Tools.SelectionTool());
            tools.Add(Tools.SceneEntityMoveTool.ID, new Tools.SceneEntityMoveTool());
            
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SceneEntityMoveTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.CircleTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.RecolorTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.FillTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.BoxTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SelectionTool.ID]);

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            GameObjectPanel = new Panels.EntityManagementPanel();
            AnimationsPanel = new Panels.Scene.AnimationListPanel();
            GameObjects = new GameObjectCollection();
            LinkedGameObjects = new Dictionary<GameObject, GameObject>();

            panels = new CustomPanel[] { layerManagementPanel, GameObjectPanel, AnimationsPanel, toolsPanel };
        }

        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox<SadConsole.Controls.ListBoxItem>.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {
                selectedTool = tool;

                List<CustomPanel> newPanels = new List<CustomPanel>() { layerManagementPanel, GameObjectPanel, AnimationsPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                panels = newPanels.ToArray();
                EditorConsoleManager.ToolsPane.RedrawPanels();
            }
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            // Create the new text surface
            textSurface = new LayeredTextSurface(width, height, 1);

            // Update metadata
            LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));
            textSurface.SetActiveLayer(0);
            textSurface.Font = Settings.Config.ScreenFont;

            // Update the layer management panel
            layerManagementPanel.SetLayeredTextSurface(textSurface);

            // Set the text surface as the one we're displaying
            consoleWrapper.TextSurface = textSurface;

            // Update the border
            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
        }
        
        public void Save()
        {
            var popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.SkipFileExistCheck = true;
            popup.Closed += (s, e) =>
            {
                if (popup.DialogResult)
                {
                    SadConsole.Game.Scene scene = new Scene(textSurface);
                    scene.Objects = this.GameObjects;

                    popup.SelectedLoader.Save(scene, popup.SelectedFile);

                    //GameObject[] objects = Entities.ToArray();

                    //SadConsole.Serializer.Save(objects, popup.SelectedFile + ".objects");
                }
            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.Scene() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Render()
        {
            foreach (var entity in GameObjects)
            {
                entity.Render();
            }
        }

        public void Update()
        {
            selectedTool.Update();

            foreach (var entity in GameObjects)
            {
                entity.Update();
            }
        }

        public void Resize(int width, int height)
        {
            //_width = width;
            //_height = height;

            //var oldSurface = (LayeredTextSurface)_consoleLayers.TextSurface;
            //var newSurface = new LayeredTextSurface(width, height, oldSurface.LayerCount);

            //for (int i = 0; i < oldSurface.LayerCount; i++)
            //{
            //    var oldLayer = oldSurface.GetLayer(i);
            //    var newLayer = newSurface.GetLayer(i);
            //    oldSurface.SetActiveLayer(i);
            //    newSurface.SetActiveLayer(i);
            //    oldSurface.Copy(newSurface);
            //    newLayer.Metadata = oldLayer.Metadata;
            //    newLayer.IsVisible = oldLayer.IsVisible;
            //}

            //_consoleLayers.TextSurface = newSurface;
            //_consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            //// inform the outer box we've changed size
            //EditorConsoleManager.Instance.UpdateBox();
        }

        public void Reset()
        {
            
        }

        

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            EditorConsoleManager.UpdateBrush();

            foreach (var entity in GameObjects)
                entity.RenderOffset = consoleWrapper.Position;
        }

        public void OnSelected()
        {
            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
                SelectedTool = selectedTool;

            foreach (var item in EditorConsoleManager.OpenEditors)
            {
                var editor = item as GameObjectEditor;

                if (editor != null && editor.LinkedEditor == this)
                {
                    // sync back up any entities.
                    foreach (var gameObject in GameObjects)
                    {
                        var animationName = gameObject.Animation.Name;
                        gameObject.Animations.Clear();

                        gameObject.Name = LinkedGameObjects[gameObject].Name;

                        foreach (var animation in LinkedGameObjects[gameObject].Animations)
                            gameObject.Animations.Add(animation.Key, animation.Value);

                        if (animationName != null && gameObject.Animations.ContainsKey(animationName))
                            gameObject.Animation = gameObject.Animations[animationName];
                        else
                            gameObject.Animation = gameObject.Animations.First().Value;
                    }
                }
            }

            
            AnimationsPanel.RebuildListBox();
            GameObjectPanel.RebuildListBox();
        }

        public void OnDeselected()
        {

        }

        public void OnClosed()
        {
            foreach (var item in EditorConsoleManager.OpenEditors)
            {
                var editor = item as GameObjectEditor;

                if (editor != null && editor.LinkedEditor == this)
                    editor.LinkedEditor = null;
            }
        }
        
        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            ClearEntities();

            //if (loader is FileLoaders.TextSurface)
            //{
            //    // Load the plain surface
            //    TextSurface surface = (TextSurface)loader.Load(file);

            //    // Load up a new layered text surface
            //    textSurface = new LayeredTextSurface(surface.Width, surface.Height, 1);

            //    // Setup metadata
            //    LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));

            //    // Use the loaded surface
            //    textSurface.ActiveLayer.Cells = surface.Cells;
            //    textSurface.SetActiveLayer(0);

            //    // Set the text surface as the one we're displaying
            //    consoleWrapper.TextSurface = textSurface;

            //    // Update the border
            //    if (EditorConsoleManager.ActiveEditor == this)
            //        EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            //}
            //else if (loader is FileLoaders.LayeredTextSurface)
            //{
            //    textSurface = (LayeredTextSurface)loader.Load(file);
            //    consoleWrapper.TextSurface = textSurface;

            //    if (EditorConsoleManager.ActiveEditor == this)
            //        EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            //}
            if (loader is FileLoaders.Scene)
            {
                var scene = (SadConsole.Game.Scene)loader.Load(file);
                textSurface = scene.BackgroundSurface;
                consoleWrapper.TextSurface = textSurface;

                foreach (var item in scene.Objects)
                    LoadEntity(item);

                if (EditorConsoleManager.ActiveEditor == this)
                    EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }

            textSurface.Font = Settings.Config.ScreenFont;
            Title = Path.GetFileName(file);
            // Update the layer management panel
            layerManagementPanel.SetLayeredTextSurface(textSurface);

            // Load game objects
            //file += ".objects";

            //if (System.IO.File.Exists(file))
            //{
            //    GameObject[] objects = SadConsole.Serializer.Load<GameObject[]>(file);

            //    foreach (var item in objects)
            //    {
            //        LoadEntity(item);
            //    }
            //}

            
        }
        
        public bool ProcessKeyboard(KeyboardInfo info)
        {
            if (!toolsPanel.SelectedTool.ProcessKeyboard(info, textSurface))
            {
                var keys = info.KeysReleased.Select(k => k.Character).ToList();

                foreach (var item in tools.Values)
                {
                    if (keys.Contains(item.Hotkey))
                    {
                        SelectedTool = item;
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public bool ProcessMouse(IConsole console, SadConsole.Input.MouseInfo info)
        {
            consoleWrapper.MouseHandler = null;
            consoleWrapper.CanUseMouse = true;
            consoleWrapper.ProcessMouse(info);
            consoleWrapper.MouseHandler = ProcessMouse;

            toolsPanel.SelectedTool?.ProcessMouse(info, textSurface);

            if (consoleWrapper.IsMouseOver)
            {
                EditorConsoleManager.SurfaceMouseLocation = info.ConsoleLocation;
                return true;
            }
            else
                EditorConsoleManager.SurfaceMouseLocation = Point.Zero;

            consoleWrapper.CanUseMouse = false;
            return false;
        }


        public override string ToString()
        {
            return this.ToString();
        }

        public bool LoadEntity(GameObject entity)
        {
            var editor = new GameObjectEditor();
            editor.SetEntity(entity);
            editor.LinkedEditor = this;
            EditorConsoleManager.AddEditor(editor, false);

            var localEntity = new GameObject(entity.Font);

            foreach (var item in entity.Animations.Values)
                localEntity.Animations.Add(item.Name, item);

            localEntity.Animation = localEntity.Animations[entity.Animation.Name];

            localEntity.RenderOffset = consoleWrapper.Position;
            GameObjects.Add(localEntity);
            GameObjectPanel.RebuildListBox();

            localEntity.Position = entity.Position;

            LinkedGameObjects.Add(localEntity, entity);

            FixLinkedObjectTitles();
            return true;
        }

        private void ClearEntities()
        {
            GameObjects.Clear();

            List<IEditor> docs = new List<IEditor>();

            foreach (var doc in EditorConsoleManager.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).LinkedEditor == this)
                        docs.Add(doc);

            LinkedGameObjects.Clear();

            foreach (var doc in docs)
                EditorConsoleManager.RemoveEditor(doc);
        }

        private void FixLinkedObjectTitles()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                var linkedEntity = LinkedGameObjects[GameObjects[i]];
                IEditor linkedEditor = EditorConsoleManager.OpenEditors.Where(e => e.EditorType == Editors.GameObject && ((GameObjectEditor)e).GameObject == linkedEntity).FirstOrDefault();

                if (linkedEditor != null)
                {
                    // last one
                    if (i == GameObjects.Count - 1)
                    {
                        linkedEditor.Title = (char)192 + " " + linkedEntity.Name;
                    }
                    else
                    {
                        linkedEditor.Title = (char)195 + " " + linkedEntity.Name;

                    }
                }

                EditorConsoleManager.ToolsPane.PanelFiles.DocumentsListbox.IsDirty = true;
            }
        }
    }
    
}
