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
using Microsoft.Xna.Framework.Graphics;

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

        public Panels.GameObjectManagementPanel GameObjectPanel;
        public Panels.RegionManagementPanel ZonesPanel;
        public Panels.HotspotToolPanel HotspotPanel;

        private GameObject _selectedGameObject;
        public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
        public List<ResizableObject> Objects;
        public List<ResizableObject<Zone>> Zones;
        public List<Hotspot> Hotspots;

        public GameObject SelectedEntity
        {
            get { return _selectedGameObject; }
            set { _selectedGameObject = value; }
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

            layerManagementPanel = new LayersPanel() { IsCollapsed = true };
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
            tools.Add(Tools.SceneObjectMoveResizeTool.ID, new Tools.SceneObjectMoveResizeTool());
            tools.Add(Tools.HotspotTool.ID, new Tools.HotspotTool());

            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SceneObjectMoveResizeTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.HotspotTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.CircleTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.RecolorTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.FillTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.BoxTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SelectionTool.ID]);

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            GameObjectPanel = new Panels.GameObjectManagementPanel();
            ZonesPanel = new RegionManagementPanel() { IsCollapsed = true };
            HotspotPanel = new HotspotToolPanel() { IsCollapsed = true };

            LinkedGameObjects = new Dictionary<GameObject, GameObject>();
            Objects = new List<ResizableObject>();
            Zones = new List<ResizableObject<Zone>>();
            Hotspots = new List<Hotspot>();

            panels = new CustomPanel[] { layerManagementPanel, GameObjectPanel, ZonesPanel, HotspotPanel, toolsPanel };
        }
        
        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox<SadConsole.Controls.ListBoxItem>.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {
                selectedTool = tool;

                List<CustomPanel> newPanels = new List<CustomPanel>() { layerManagementPanel, GameObjectPanel, ZonesPanel, HotspotPanel, toolsPanel };

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
                    scene.Objects = new GameObjectCollection(this.Objects.Select(g => g.GameObject).ToArray());
                    scene.Zones = new List<Zone>(
                                                 this.Zones.Select(
                                                     z => new Zone()
                                                                    { Area = new Rectangle(z.GameObject.Position.X, z.GameObject.Position.Y, z.GameObject.Width, z.GameObject.Height),
                                                                      DebugAppearance = new CellAppearance(Color.White, z.GameObject.RenderCells[0].Background, 0),
                                                                      Title = z.GameObject.Name }));
                    scene.Hotspots = this.Hotspots;
                    popup.SelectedLoader.Save(scene, popup.SelectedFile);
                }
            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.Scene() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Render()
        {
            if (HotspotPanel.DrawHotspots && Hotspots.Count != 0)
            {
                SpriteBatch batch = new SpriteBatch(Engine.Device);
                batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);

                foreach (var spot in Hotspots)
                {
                    Cell cell = new Cell();
                    spot.DebugAppearance.CopyAppearanceTo(cell);
                    Point offset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;
                    foreach (var position in spot.Positions)
                    {
                        Point adjustedPosition = position + offset;
                        if (consoleWrapper.TextSurface.RenderArea.Contains(position))
                            cell.Render(batch, 
                                        new Rectangle(adjustedPosition.ConsoleLocationToWorld(Settings.Config.ScreenFont.Size.X, Settings.Config.ScreenFont.Size.Y), Settings.Config.ScreenFont.Size), 
                                        Settings.Config.ScreenFont);

                    }

                }

                batch.End();
            }
            if (ZonesPanel.DrawZones)
                foreach (var zone in Zones)
                {
                    zone.Render();
                }

            if (GameObjectPanel.DrawObjects)
                foreach (var entity in Objects)
                {
                    entity.Render();
                }
        }

        public void Update()
        {
            selectedTool.Update();

            foreach (var entity in Objects)
            {
                entity.GameObject.Update();
            }
        }

        public void Resize(int width, int height)
        {
            var oldSurface = textSurface;
            var newSurface = new LayeredTextSurface(width, height, Settings.Config.ScreenFont, oldSurface.LayerCount);

            for (int i = 0; i < oldSurface.LayerCount; i++)
            {
                var oldLayer = oldSurface.GetLayer(i);
                var newLayer = newSurface.GetLayer(i);
                oldSurface.SetActiveLayer(i);
                newSurface.SetActiveLayer(i);
                oldSurface.Copy(newSurface);
                newLayer.Metadata = oldLayer.Metadata;
                newLayer.IsVisible = oldLayer.IsVisible;
            }

            consoleWrapper.TextSurface = textSurface = newSurface;
            layerManagementPanel.SetLayeredTextSurface(textSurface);
            toolsPanel.SelectedTool = toolsPanel.SelectedTool;

            if (EditorConsoleManager.ActiveEditor == this)
            {
                EditorConsoleManager.CenterEditor();
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }
        }

        public void Reset()
        {
            
        }

        public void RenameGameObject(ResizableObject gameObject, string newName)
        {
            gameObject.Name = newName;
            LinkedGameObjects[gameObject.GameObject].Name = newName;
            FixLinkedObjectTitles();
        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            EditorConsoleManager.UpdateBrush();

            foreach (var entity in Objects)
                entity.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;

            foreach (var zone in Zones)
                zone.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;
        }

        public void OnSelected()
        {
            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
            {
                var oldTool = selectedTool;
                SelectedTool = null;
                SelectedTool = selectedTool;
            }

            foreach (var item in EditorConsoleManager.OpenEditors)
            {
                var editor = item as GameObjectEditor;

                if (editor != null && editor.LinkedEditor == this)
                {
                    // sync back up any entities.
                    foreach (var resizeObject in Objects)
                    {
                        var gameObject = resizeObject.GameObject;
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

            EditorConsoleManager.ToolsPane.PanelFiles.DocumentsListbox.IsDirty = true;

            GameObjectPanel.RebuildListBox();
        }

        public void OnDeselected()
        {

        }

        public void OnClosed()
        {
            var editors = EditorConsoleManager.OpenEditors.ToList();
            foreach (var item in editors)
            {
                var editor = item as GameObjectEditor;

                if (editor != null && editor.LinkedEditor == this)
                {
                    EditorConsoleManager.RemoveEditor(editor);
                }
            }
        }
        
        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            ClearEntities();
            ClearZones();
            ClearHotspots();
            
            if (loader is FileLoaders.Scene)
            {
                var scene = (SadConsole.Game.Scene)loader.Load(file);
                textSurface = scene.BackgroundSurface;
                consoleWrapper.TextSurface = textSurface;

                foreach (var item in scene.Objects)
                    LoadEntity(item);

                foreach (var zone in scene.Zones)
                    LoadZone(zone);

                foreach (var spot in scene.Hotspots)
                    LoadHotspot(spot);

                if (EditorConsoleManager.ActiveEditor == this)
                    EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }

            textSurface.Font = Settings.Config.ScreenFont;
            Title = Path.GetFileName(file);

            // Update the layer management panel
            layerManagementPanel.SetLayeredTextSurface(textSurface);
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

            // Check if tool is our special tool...
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
            Objects.Add(new ResizableObject(ResizableObject.ObjectType.GameObject, localEntity));
            GameObjectPanel.RebuildListBox();

            localEntity.Position = entity.Position;
            localEntity.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;

            LinkedGameObjects.Add(localEntity, entity);

            FixLinkedObjectTitles();
            return true;
        }

        public bool LoadZone(Zone zone)
        {
            var gameObject = new GameObject(Settings.Config.ScreenFont);
            var animation = new AnimatedTextSurface("default", 10, 10);
            var frame = animation.CreateFrame();
            frame.DefaultBackground = zone.DebugAppearance.Background;

            gameObject.Name = zone.Title;

            Settings.QuickEditor.TextSurface = frame;
            Settings.QuickEditor.Clear();
            Settings.QuickEditor.Print(0, 0, zone.Title, Color.DarkGray);

            gameObject.Animation = animation;
            gameObject.Position = new Point(zone.Area.Left, zone.Area.Top);
            gameObject.Update();

            var resizable = new ResizableObject<Zone>(ResizableObject.ObjectType.Zone, gameObject, zone);
            resizable.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;
            Zones.Add(resizable);

            ZonesPanel.RebuildListBox();

            return true;
        }

        public bool LoadHotspot(Hotspot spot)
        {
            Hotspots.Add(spot);
            HotspotPanel.RebuildListBox();

            return true;
        }

        public void RemoveGameObject(ResizableObject gameObject)
        {
            var otherObject = LinkedGameObjects[gameObject.GameObject];
            GameObjectEditor foundDoc = null;

            foreach (var doc in EditorConsoleManager.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).GameObject == otherObject)
                        foundDoc = (GameObjectEditor)doc;

            if (foundDoc != null)
            {
                EditorConsoleManager.RemoveEditor(foundDoc);
                LinkedGameObjects.Remove(gameObject.GameObject);
                Objects.Remove(gameObject);
            }
        }

        private void ClearEntities()
        {
            Objects.Clear();

            List<IEditor> docs = new List<IEditor>();

            foreach (var doc in EditorConsoleManager.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).LinkedEditor == this)
                        docs.Add(doc);

            LinkedGameObjects.Clear();

            foreach (var doc in docs)
                EditorConsoleManager.RemoveEditor(doc);
        }

        private void ClearZones()
        {
            Zones.Clear();
        }

        public void ClearHotspots()
        {
            Hotspots.Clear();
        }

        private void FixLinkedObjectTitles()
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                var linkedEntity = LinkedGameObjects[Objects[i].GameObject];
                IEditor linkedEditor = EditorConsoleManager.OpenEditors.Where(e => e.EditorType == Editors.GameObject && ((GameObjectEditor)e).GameObject == linkedEntity).FirstOrDefault();

                if (linkedEditor != null)
                {
                    var name = string.IsNullOrWhiteSpace(linkedEntity.Name) ? "<no name>" : linkedEntity.Name;
                    // last one
                    if (i == Objects.Count - 1)
                    {
                        linkedEditor.Title = (char)192 + " " + name;
                    }
                    else
                    {
                        linkedEditor.Title = (char)195 + " " + name;

                    }
                }

                EditorConsoleManager.ToolsPane.PanelFiles.DocumentsListbox.IsDirty = true;
            }
        }
    }
    
}
