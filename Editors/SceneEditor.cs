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

        public Panels.EntityManagementPanel EntityPanel;
        public Panels.Scene.AnimationListPanel AnimationsPanel;


        private GameObject _selectedEntity;
        public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
        public GameObjectCollection Entities;

        public GameObject SelectedEntity
        {
            get { return _selectedEntity; }
            set { _selectedEntity = value; AnimationsPanel.RebuildListBox(); }
        }


        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.Console; } }

        public string EditorTypeName { get { return "Scene"; } }

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

            EntityPanel = new Panels.EntityManagementPanel();
            AnimationsPanel = new Panels.Scene.AnimationListPanel();
            Entities = new GameObjectCollection();
            LinkedGameObjects = new Dictionary<GameObject, GameObject>();

            panels = new CustomPanel[] { layerManagementPanel, EntityPanel, AnimationsPanel, toolsPanel };
        }

        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox<SadConsole.Controls.ListBoxItem>.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {

                List<CustomPanel> newPanels = new List<CustomPanel>() { layerManagementPanel, EntityPanel, AnimationsPanel, toolsPanel };

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
                    scene.Objects = this.Entities;

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
            foreach (var entity in Entities)
            {
                entity.Render();
            }
        }

        public void Update()
        {
            foreach (var entity in Entities)
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

        private void ClearEntities()
        {
            Entities.Clear();

            List<IEditor> docs = new List<IEditor>();

            foreach (var doc in EditorConsoleManager.OpenEditors)
                if (doc is GameObjectEditor)
                    if (((GameObjectEditor)doc).LinkedEditor == this)
                        docs.Add(doc);

            LinkedGameObjects.Clear();

            foreach (var doc in docs)
                EditorConsoleManager.RemoveEditor(doc);
        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            EditorConsoleManager.UpdateBrush();

            foreach (var entity in Entities)
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
                    foreach (var gameObject in Entities)
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
            EntityPanel.RebuildListBox();
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
        
        
        public bool ProcessKeyboard(IConsole console, SadConsole.Input.KeyboardInfo info)
        {
            //EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);
            return false;
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
            Entities.Add(localEntity);
            EntityPanel.RebuildListBox();

            localEntity.Position = entity.Position;

            LinkedGameObjects.Add(localEntity, entity);
            
            return true;
        }
    }


    //class SceneEditor2 : IEditor
    //{
    //    private int _width;
    //    private int _height;
    //    private List<FileLoaders.IFileLoader> loadersLoad;
    //    private List<FileLoaders.IFileLoader> loadersSave;
    //    public GameObject _selectedEntity;
    //    private Console _consoleLayers;

    //    public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
    //    public SadConsole.Game.GameObjectCollection Entities;

    //    public event EventHandler<MouseEventArgs> MouseEnter;
    //    public event EventHandler<MouseEventArgs> MouseExit;
    //    public event EventHandler<MouseEventArgs> MouseMove;

    //    public int Width { get { return _width; } }
    //    public int Height { get { return _height; } }

    //    public EditorSettings Settings { get { return SadConsoleEditor.Settings.Config.ConsoleEditor; } }

    //    public ITextSurface Surface { get { return _consoleLayers.TextSurface; } }

    //    public const string ID = "SCENE";

    //    public string ShortName { get { return "Scene"; } }

    //    public string Id { get { return ID; } }

    //    public string Title { get { return "Scene Maker"; } }

    //    public IEnumerable<FileLoaders.IFileLoader> FileExtensionsLoad { get { return loadersLoad; } }

    //    public IEnumerable<FileLoaders.IFileLoader> FileExtensionsSave { get { return loadersSave; } }

    //    public CustomPanel[] ControlPanels { get; private set; }

    //    public Panels.EntityManagementPanel EntityPanel;
    //    public Panels.Scene.AnimationListPanel AnimationsPanel;

    //    public GameObject SelectedEntity
    //    {
    //        get { return _selectedEntity; }
    //        set { _selectedEntity = value; AnimationsPanel.RebuildListBox(); }
    //    }

    //    public string[] Tools
    //    {
    //        get
    //        {
    //            return new string[] { SceneEntityMoveTool.ID, PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID };
    //        }
    //    }

    //    private EventHandler<MouseEventArgs> _mouseMoveHandler;
    //    private EventHandler<MouseEventArgs> _mouseEnterHandler;
    //    private EventHandler<MouseEventArgs> _mouseExitHandler;


    //    public SceneEditor2()
    //    {
    //        loadersSave = new List<FileLoaders.IFileLoader>() { new FileLoaders.Scene() };
    //        loadersLoad = new List<FileLoaders.IFileLoader>(loadersSave);
    //        _consoleLayers = new Console(20, 25);
    //        _consoleLayers.Renderer = new LayeredTextRenderer();
    //        EntityPanel = new Panels.EntityManagementPanel();
    //        AnimationsPanel = new Panels.Scene.AnimationListPanel();
    //        Entities = new GameObjectCollection();
    //        LinkedGameObjects = new Dictionary<GameObject, GameObject>();
    //        Reset();
    //    }


    //    public void Reset()
    //    {
    //        Entities.Clear();

    //        List<IEditor> docs = new List<IEditor>();

    //        foreach (var doc in EditorConsoleManager.Instance.Documents)
    //            if (doc is EntityEditor)
    //                if (((EntityEditor)doc).LinkedEditor == this)
    //                    docs.Add(doc);

    //        LinkedGameObjects.Clear();

    //        foreach (var doc in docs)
    //            EditorConsoleManager.Instance.CloseDocument(doc);
            
    //        ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.LayersPanel, EntityPanel, AnimationsPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };

    //        if (_consoleLayers != null)
    //        {
    //            _consoleLayers.MouseMove -= _mouseMoveHandler;
    //            _consoleLayers.MouseEnter -= _mouseEnterHandler;
    //            _consoleLayers.MouseExit -= _mouseExitHandler;
    //        }

    //        _consoleLayers.TextSurface = new LayeredTextSurface(25, 25, 1);
    //        _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;
    //        LayerMetadata.Create("Root", false, false, false, ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(0));
    //        _consoleLayers.CanUseMouse = true;
    //        _consoleLayers.CanUseKeyboard = true;
            

    //        _width = 25;
    //        _height = 10;

    //        _mouseMoveHandler = (o, e) => { MouseMove?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
    //        _mouseEnterHandler = (o, e) => { MouseEnter?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
    //        _mouseExitHandler = (o, e) => { MouseExit?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };

    //        _consoleLayers.MouseMove += _mouseMoveHandler;
    //        _consoleLayers.MouseEnter += _mouseEnterHandler;
    //        _consoleLayers.MouseExit += _mouseExitHandler;

    //    }

    //    internal bool LoadEntity(string selectedFile)
    //    {
    //        var entity = SadConsole.Game.GameObject.Load(selectedFile);

    //        return LoadEntity(entity);
    //    }

    //    internal bool LoadEntity(GameObject entity)
    //    {
    //        var editor = new Editors.EntityEditor();
    //        editor.SetEntity(entity);
    //        editor.LinkedEditor = this;
    //        EditorConsoleManager.Instance.AddDocument(editor, false);

    //        var localEntity = new GameObject(entity.Font);

    //        foreach (var item in entity.Animations.Values)
    //            localEntity.Animations.Add(item.Name, item);

    //        localEntity.Animation = localEntity.Animations[entity.Animation.Name];

    //        localEntity.RenderOffset = _consoleLayers.Position;
    //        Entities.Add(localEntity);
    //        EntityPanel.RebuildListBox();

    //        localEntity.Position = entity.Position;

    //        LinkedGameObjects.Add(localEntity, entity);

    //        return true;
    //    }

    //    public override string ToString()
    //    {
    //        return Title;
    //    }

    //    public void ProcessKeyboard(KeyboardInfo info)
    //    {
    //        EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);
    //    }

    //    public void ProcessMouse(MouseInfo info)
    //    {
    //        _consoleLayers.ProcessMouse(info);

    //        EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers.TextSurface);

    //        if (_consoleLayers.IsMouseOver)
    //            EditorConsoleManager.Instance.SurfaceMouseLocation = info.ConsoleLocation;
    //        else
    //            EditorConsoleManager.Instance.SurfaceMouseLocation = Point.Zero;

    //    }

    //    public void Render()
    //    {
    //        _consoleLayers.Render();

    //        foreach (var entity in Entities)
    //        {
    //            entity.Render();
    //        }
    //    }

    //    public void Update()
    //    {
    //        _consoleLayers.Update();

    //        foreach (var entity in Entities)
    //        {
    //            entity.Update();
    //        }
    //    }

    //    public void Resize(int width, int height)
    //    {
    //        _width = width;
    //        _height = height;

    //        var oldSurface = (LayeredTextSurface)_consoleLayers.TextSurface;
    //        var newSurface = new LayeredTextSurface(width, height, oldSurface.LayerCount);

    //        for (int i = 0; i < oldSurface.LayerCount; i++)
    //        {
    //            var oldLayer = oldSurface.GetLayer(i);
    //            var newLayer = newSurface.GetLayer(i);
    //            oldSurface.SetActiveLayer(i);
    //            newSurface.SetActiveLayer(i);
    //            oldSurface.Copy(newSurface);
    //            newLayer.Metadata = oldLayer.Metadata;
    //            newLayer.IsVisible = oldLayer.IsVisible;
    //        }

    //        _consoleLayers.TextSurface = newSurface;
    //        _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

    //        // inform the outer box we've changed size
    //        EditorConsoleManager.Instance.UpdateBox();
    //    }

    //    public void Position(int x, int y)
    //    {
    //        Position(new Point(x, y));
    //    }

    //    public void Position(Point newPosition)
    //    {
    //        _consoleLayers.Position = newPosition;

    //        foreach (var entity in Entities)
    //        {
    //            entity.RenderOffset = newPosition;
    //        }
    //    }

    //    public Point GetPosition()
    //    {
    //        return _consoleLayers.Position;
    //    }

    //    public void OnSelected()
    //    {
    //        foreach (var item in EditorConsoleManager.Instance.Documents)
    //        {
    //            var editor = item as EntityEditor;

    //            if (editor != null && editor.LinkedEditor == this)
    //            {
    //                // sync back up any entities.
    //                foreach (var gameObject in Entities)
    //                {
    //                    var animationName = gameObject.Animation.Name;
    //                    gameObject.Animations.Clear();

    //                    gameObject.Name = LinkedGameObjects[gameObject].Name;

    //                    foreach (var animation in LinkedGameObjects[gameObject].Animations)
    //                        gameObject.Animations.Add(animation.Key, animation.Value);

    //                    if (animationName != null && gameObject.Animations.ContainsKey(animationName))
    //                        gameObject.Animation = gameObject.Animations[animationName];
    //                    else
    //                        gameObject.Animation = gameObject.Animations.First().Value;
    //                }
    //            }
    //        }

    //        AnimationsPanel.RebuildListBox();
    //        EntityPanel.RebuildListBox();
    //    }

    //    public void OnDeselected()
    //    {

    //    }

    //    public void OnClosed()
    //    {
    //        foreach (var item in EditorConsoleManager.Instance.Editors.Values)
    //        {
    //            var editor = item as EntityEditor;

    //            if (editor != null && editor.LinkedEditor == this)
    //                editor.LinkedEditor = null;
    //        }
    //    }

    //    public void Save(string file, FileLoaders.IFileLoader loader)
    //    {
    //        ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file, typeof(LayerMetadata));

    //        GameObject[] objects = Entities.ToArray();

    //        SadConsole.Serializer.Save(objects, file + ".objects");
    //    }

    //    public void Load(string file, FileLoaders.IFileLoader loader)
    //    {
    //        if (System.IO.File.Exists(file))
    //        {
    //            Reset();

    //            EntityPanel.RebuildListBox();
    //            AnimationsPanel.RebuildListBox();

    //            if (_consoleLayers != null)
    //            {
    //                _consoleLayers.MouseMove -= _mouseMoveHandler;
    //                _consoleLayers.MouseEnter -= _mouseEnterHandler;
    //                _consoleLayers.MouseExit -= _mouseExitHandler;
    //            }

    //            // Support REXPaint
    //            //if (file.EndsWith(".xp"))
    //            //{
    //            //    using (var filestream = new FileStream(file, FileMode.Open))
    //            //        _consoleLayers.TextSurface = SadConsole.Readers.REXPaint.Image.Load(filestream).ToTextSurface();
    //            //}
    //            //else
    //            //{
    //            loader = new FileLoaders.Scene();
    //            _consoleLayers.TextSurface = loader.Load(file);
    //            //}

    //            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

    //            EditorConsoleManager.Instance.ToolPane.LayersPanel.RebuildListBox();

    //            _consoleLayers.MouseMove += _mouseMoveHandler;
    //            _consoleLayers.MouseEnter += _mouseEnterHandler;
    //            _consoleLayers.MouseExit += _mouseExitHandler;

    //            _width = _consoleLayers.Width;
    //            _height = _consoleLayers.Height;

    //            EditorConsoleManager.Instance.UpdateBox();
                

    //            // Load game objects
    //            file += ".objects";

    //            if (System.IO.File.Exists(file))
    //            {
    //                GameObject[] objects = SadConsole.Serializer.Load<GameObject[]>(file);

    //                foreach (var item in objects)
    //                {
    //                    LoadEntity(item);
    //                }
    //            }
    //        }
    //    }

    //    public void RemoveLayer(int index)
    //    {
    //        ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(index);
    //    }

    //    public void MoveLayerUp(int index)
    //    {
    //        var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
    //        ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index + 1);
    //    }

    //    public void MoveLayerDown(int index)
    //    {
    //        var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
    //        ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index - 1);
    //    }

    //    public void AddNewLayer(string name)
    //    {
    //        LayerMetadata.Create(name, true, true, true, ((LayeredTextSurface)_consoleLayers.TextSurface).Add());
    //    }

    //    public bool LoadLayer(string file)
    //    {
    //        if (System.IO.File.Exists(file))
    //        {
    //            //TODO: Load layer types.. XP, TextSurface, Layer


    //            //typeof(LayerMetadata)
    //            var surface = SadConsole.Consoles.TextSurface.Load(file);

    //            if (surface.Width != EditorConsoleManager.Instance.SelectedEditor.Surface.Width || surface.Height != EditorConsoleManager.Instance.SelectedEditor.Height)
    //            {
    //                var newLayer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
    //                LayerMetadata.Create("Loaded", true, true, true, newLayer);
    //                var tempSurface = new TextSurface(_consoleLayers.Width, _consoleLayers.Height,
    //                                                  newLayer.Cells, _consoleLayers.TextSurface.Font);
    //                surface.Copy(tempSurface);
    //                newLayer.Cells = tempSurface.Cells;
    //            }
    //            else
    //            {
    //                var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
    //                LayerMetadata.Create("Loaded", true, true, true, layer);
    //                layer.Cells = surface.Cells;

    //            }

    //            return true;
    //        }
    //        else
    //            return false;
    //    }

    //    public void SaveLayer(int index, string file)
    //    {
    //        SadConsole.Serializer.Save(((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index), file, new Type[] { typeof(LayerMetadata) });
    //    }

    //    public void SetActiveLayer(int index)
    //    {
    //        ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(index);
    //    }
    //}
    
}
