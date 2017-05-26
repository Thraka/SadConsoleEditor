using System;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsole;
using SadConsole.Surfaces;
using SadConsole;
using System.IO;
using System.Collections.Generic;
using SadConsole.GameHelpers;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using SadConsole.Renderers;

namespace SadConsoleEditor.Editors
{
    class SceneEditor : IEditor
    {
        public enum HighlightTypes
        {
            GameObject,
            HotSpot,
            Zone
        }

        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;
        private ToolsPanel toolsPanel;

        private CustomPanel[] panels;
        private LayersPanel layerManagementPanel;

        private LayeredSurfaceRenderer renderer;
        private SadConsole.Surfaces.LayeredSurface surface;
        private BasicSurface hotspotSurface;
        private SurfaceRenderer hotspotRenderer;

        public Panels.GameObjectManagementPanel GameObjectPanel;
        public Panels.RegionManagementPanel ZonesPanel;
        public Panels.HotspotToolPanel HotspotPanel;

        private GameObject _selectedGameObject;
        public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
        public List<ResizableObject> Objects;
        public List<ResizableObject<Zone>> Zones;
        public List<Hotspot> Hotspots;

        public SceneEditor.HighlightTypes HighlightType;

        public GameObject SelectedEntity
        {
            get { return _selectedGameObject; }
            set { _selectedGameObject = value; }
        }

        public bool ShowDarkLayer;

        public string DocumentTitle { get; set; }

        public ISurface Surface => surface;

        public ISurfaceRenderer Renderer => renderer;

        public Editors EditorType  => Editors.Console;

        public IEditor LinkedEditor { get; set; }
        
        public string EditorTypeName { get { return "Scene"; } }

        public string Title { get; set; }

        public int Height => surface.Height;
        
        public int Width => surface.Width;

        public CustomPanel[] Panels => panels;
        
        

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
            layerManagementPanel = new LayersPanel() { IsCollapsed = true };
            toolsPanel = new ToolsPanel();

            // Fill tools
            tools = new Dictionary<string, Tools.ITool>
            {
                { Tools.PaintTool.ID, new Tools.PaintTool() },
                { Tools.LineTool.ID, new Tools.LineTool() },
                { Tools.CircleTool.ID, new Tools.CircleTool() },
                { Tools.RecolorTool.ID, new Tools.RecolorTool() },
                { Tools.FillTool.ID, new Tools.FillTool() },
                { Tools.BoxTool.ID, new Tools.BoxTool() },
                { Tools.SelectionTool.ID, new Tools.SelectionTool() },
                { Tools.SceneObjectMoveResizeTool.ID, new Tools.SceneObjectMoveResizeTool() },
                { Tools.HotspotTool.ID, new Tools.HotspotTool() }
            };

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
            toolsPanel.ToolsListBox.SelectedItem = tools[Tools.PaintTool.ID];

            GameObjectPanel = new Panels.GameObjectManagementPanel();
            ZonesPanel = new RegionManagementPanel() { IsCollapsed = true };
            HotspotPanel = new HotspotToolPanel() { IsCollapsed = true };

            LinkedGameObjects = new Dictionary<GameObject, GameObject>();
            Objects = new List<ResizableObject>();
            Zones = new List<ResizableObject<Zone>>();
            Hotspots = new List<Hotspot>();

            panels = new CustomPanel[] { layerManagementPanel, GameObjectPanel, ZonesPanel, HotspotPanel, toolsPanel };
            renderer = new LayeredSurfaceRenderer();
            hotspotRenderer = new SurfaceRenderer();
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            int renderWidth = Math.Min(MainScreen.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainScreen.Instance.InnerEmptyBounds.Height, height);

            hotspotSurface = new BasicSurface(renderWidth, renderHeight, Settings.Config.ScreenFont);

            // Create the new text surface
            surface = new LayeredSurface(width, height, new Rectangle(0, 0, renderWidth, renderHeight), 1);

            // Update metadata
            LayerMetadata.Create("main", false, false, true, surface.GetLayer(0));
            surface.SetActiveLayer(0);
            surface.Font = Settings.Config.ScreenFont;

            // Update the layer management panel
            layerManagementPanel.SetLayeredSurface(surface);

            // Set the text surface as the one we're displaying

            // Update the border
            if (MainScreen.Instance.ActiveEditor == this)
                MainScreen.Instance.RefreshBorder();
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
                    Scene scene = new Scene(surface, new LayeredSurfaceRenderer());
                    scene.Objects = Objects.Select(g => g.GameObject).ToList();
                    scene.Zones = new List<Zone>(
                                                 this.Zones.Select(
                                                     z => new Zone()
                                                                    { Area = new Rectangle(z.GameObject.Position.X, z.GameObject.Position.Y, z.GameObject.Animation.Width, z.GameObject.Animation.Height),
                                                                      DebugAppearance = new Cell(Color.White, z.GameObject.Animation.Cells[0].Background, 0),
                                                                      Title = z.GameObject.Name }));
                    scene.Hotspots = this.Hotspots;

                    scene.Objects.ForEach(o => o.PositionOffset -= MainScreen.Instance.InnerBorderPosition);

                    popup.SelectedLoader.Save(scene, popup.SelectedFile);

                    scene.Objects.ForEach(o => o.PositionOffset += MainScreen.Instance.InnerBorderPosition);
                }
            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.Scene() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            ClearEntities();
            ClearZones();
            ClearHotspots();

            if (loader is FileLoaders.Scene)
            {
                var scene = (SadConsole.GameHelpers.Scene)loader.Load(file);

                surface = (LayeredSurface)scene.Surface.TextSurface;
                int renderWidth = Math.Min(MainScreen.Instance.InnerEmptyBounds.Width, surface.Width);
                int renderHeight = Math.Min(MainScreen.Instance.InnerEmptyBounds.Height, surface.Height);
                surface.RenderArea = new Rectangle(0, 0, renderWidth, renderHeight);
                hotspotSurface = new BasicSurface(renderWidth, renderHeight, Settings.Config.ScreenFont);

                foreach (var item in scene.Objects)
                    LoadEntity(item);

                foreach (var zone in scene.Zones)
                    LoadZone(zone);

                foreach (var spot in scene.Hotspots)
                    LoadHotspot(spot);

            }

            surface.Font = Settings.Config.ScreenFont;
            Title = Path.GetFileName(file);

        }

        public void Draw()
        {
            if (ShowDarkLayer)
            {
                switch (HighlightType)
                {
                    case SceneEditor.HighlightTypes.GameObject:
                        DrawHotspots(surface);
                        DrawDark(surface);
                        DrawZones(surface);
                        DrawGameObjects(surface);
                        break;
                    case SceneEditor.HighlightTypes.HotSpot:
                        DrawZones(surface);
                        DrawGameObjects(surface);
                        DrawDark(surface);
                        DrawHotspots(surface);
                        break;
                    case SceneEditor.HighlightTypes.Zone:
                        DrawHotspots(surface);
                        DrawDark(surface);
                        DrawGameObjects(surface);
                        DrawZones(surface);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                DrawZones(surface);
                DrawHotspots(surface);
                DrawGameObjects(surface);
            }
        }

        public void Update()
        {
            toolsPanel.SelectedTool?.Update();

            foreach (var entity in Objects)
            {
                entity.GameObject.Update(Global.GameTimeUpdate.ElapsedGameTime);
                entity.RenderOffset = surface.RenderArea.Location + MainScreen.Instance.InnerBorderPosition;
            }
        }

        private void DrawHotspots(ISurface surface)
        {
            if (HotspotPanel.DrawHotspots && Hotspots.Count != 0)
            {
                Settings.QuickEditor.TextSurface = hotspotSurface;
                Settings.QuickEditor.Fill(Color.Transparent, Color.Transparent, 0);

                foreach (var spot in Hotspots)
                {

                    foreach (var position in spot.Positions)
                    {
                        if (surface.RenderArea.Contains(position))
                        {

                            var renderPosition = position - surface.RenderArea.Location;

                            Settings.QuickEditor.SetCell(renderPosition.X, renderPosition.Y, spot.DebugAppearance);
                        }

                    }

                }

                hotspotRenderer.Render(hotspotSurface);
                Global.DrawCalls.Add(new DrawCallSurface(hotspotSurface, MainScreen.Instance.InnerBorderPosition, false));
            }
        }

        private void DrawZones(ISurface surface)
        {
            if (ZonesPanel.DrawZones)
                foreach (var zone in Zones)
                {
                    zone.RenderOffset = MainScreen.Instance.InnerBorderPosition - surface.RenderArea.Location;
                    zone.Draw();
                }
        }

        private void DrawGameObjects(ISurface surface)
        {
            if (GameObjectPanel.DrawObjects)
                foreach (var entity in Objects)
                {
                    entity.RenderOffset = MainScreen.Instance.InnerBorderPosition - surface.RenderArea.Location;
                    entity.Draw();
                }
        }

        private void DrawDark(ISurface surface)
        {
            Global.DrawCalls.Add(new DrawCallColoredRect(new Rectangle(MainScreen.Instance.InnerBorderPosition.ConsoleLocationToPixel(Settings.Config.ScreenFont), surface.AbsoluteArea.Size), Color.Black * 0.6f));
        }

        public void Resize(int width, int height)
        {
            int renderWidth = Math.Min(MainScreen.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainScreen.Instance.InnerEmptyBounds.Height, height);

            var oldSurface = surface;
            var newSurface = new LayeredSurface(width, height, Settings.Config.ScreenFont, new Rectangle(0,0,renderWidth, renderHeight), oldSurface.LayerCount);
            hotspotSurface = new BasicSurface(renderWidth, renderHeight, Settings.Config.ScreenFont);

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

            surface = newSurface;
            layerManagementPanel.SetLayeredSurface(surface);
            toolsPanel.SelectedTool = toolsPanel.SelectedTool;

            if (MainScreen.Instance.ActiveEditor == this)
                MainScreen.Instance.RefreshBorder();
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

        //public void Move(int x, int y)
        //{
        //    consoleWrapper.Position = new Point(x, y);

        //    if (MainScreen.Instance.ActiveEditor == this)
        //        MainScreen.Instance.UpdateBorder(consoleWrapper.Position);

        //    MainScreen.Instance.UpdateBrush();

        //    foreach (var entity in Objects)
        //        entity.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;

        //    foreach (var zone in Zones)
        //        zone.RenderOffset = consoleWrapper.Position - consoleWrapper.TextSurface.RenderArea.Location;
        //}

        public void OnSelected()
        {
            MainScreen.Instance.RefreshBorder();
            layerManagementPanel.SetLayeredSurface(surface);

            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
            {
                var oldTool = selectedTool;
                SelectedTool = null;
                SelectedTool = selectedTool;
            }

            foreach (var item in MainScreen.Instance.OpenEditors)
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

            MainScreen.Instance.ToolsPane.PanelFiles.DocumentsListbox.IsDirty = true;

            GameObjectPanel.RebuildListBox();
        }

        public void OnDeselected()
        {

        }

        public void OnClosed()
        {
            var editors = MainScreen.Instance.OpenEditors.ToList();
            foreach (var item in editors)
            {
                var editor = item as GameObjectEditor;

                if (editor != null && editor.LinkedEditor == this)
                {
                    MainScreen.Instance.RemoveEditor(editor);
                }
            }
        }
        
        public bool ProcessKeyboard(Keyboard info)
        {
            if (!toolsPanel.SelectedTool.ProcessKeyboard(info, surface))
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

        public bool ProcessMouse(SadConsole.Input.MouseConsoleState info, bool isInBounds)
        {
            // Check if tool is our special tool...
            toolsPanel.SelectedTool?.ProcessMouse(info, surface, isInBounds);

            MainScreen.Instance.SurfaceMouseLocation = info.CellPosition;

            if (info.IsOnConsole)
                return true;

            return false;
        }

        
        public bool LoadEntity(GameObject entity)
        {
            var editor = new GameObjectEditor();
            editor.SetEntity(entity);
            editor.LinkedEditor = this;
            MainScreen.Instance.AddEditor(editor, false);

            var localEntity = new GameObject(entity.Animation);

            foreach (var item in entity.Animations.Values)
                localEntity.Animations[item.Name] = item;

            Objects.Add(new ResizableObject(ResizableObject.ObjectType.GameObject, localEntity));
            GameObjectPanel.RebuildListBox();

            localEntity.Position = entity.Position;

            LinkedGameObjects.Add(localEntity, entity);

            FixLinkedObjectTitles();
            return true;
        }

        public bool LoadZone(Zone zone)
        {
            var gameObject = new GameObject(1, 1, Settings.Config.ScreenFont);
            var animation = new AnimatedSurface("default", zone.Area.Width, zone.Area.Height);
            var frame = animation.CreateFrame();
            frame.DefaultBackground = zone.DebugAppearance.Background;

            gameObject.Name = zone.Title;

            Settings.QuickEditor.TextSurface = frame;
            Settings.QuickEditor.Clear();
            Settings.QuickEditor.Print(0, 0, zone.Title, Color.DarkGray);

            gameObject.Animation = animation;
            gameObject.Position = new Point(zone.Area.Left, zone.Area.Top);
            

            var resizable = new ResizableObject<Zone>(ResizableObject.ObjectType.Zone, gameObject, zone);
            resizable.RenderOffset = MainScreen.Instance.InnerBorderPosition - surface.RenderArea.Location;
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

            foreach (var doc in MainScreen.Instance.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).GameObject == otherObject)
                        foundDoc = (GameObjectEditor)doc;

            if (foundDoc != null)
            {
                MainScreen.Instance.RemoveEditor(foundDoc);
                LinkedGameObjects.Remove(gameObject.GameObject);
                Objects.Remove(gameObject);
            }
        }

        private void ClearEntities()
        {
            Objects.Clear();

            List<IEditor> docs = new List<IEditor>();

            foreach (var doc in MainScreen.Instance.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).LinkedEditor == this)
                        docs.Add(doc);

            LinkedGameObjects.Clear();

            foreach (var doc in docs)
                MainScreen.Instance.RemoveEditor(doc);
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
                IEditor linkedEditor = MainScreen.Instance.OpenEditors.Where(e => e.EditorType == Editors.GameObject && ((GameObjectEditor)e).GameObject == linkedEntity).FirstOrDefault();

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

                MainScreen.Instance.ToolsPane.PanelFiles.DocumentsListbox.IsDirty = true;
            }
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
                MainScreen.Instance.ToolsPane.RedrawPanels();
            }
        }
    }
    
}
