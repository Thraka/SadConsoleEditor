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
        private int _width;
        private int _height;
        private List<FileLoaders.IFileLoader> loadersLoad;
        private List<FileLoaders.IFileLoader> loadersSave;
        public GameObject _selectedEntity;
        private Console _consoleLayers;

        public Dictionary<GameObject, GameObject> LinkedGameObjects = new Dictionary<GameObject, GameObject>();
        public SadConsole.Game.GameObjectCollection Entities;

        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseExit;
        public event EventHandler<MouseEventArgs> MouseMove;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public EditorSettings Settings { get { return SadConsoleEditor.Settings.Config.ConsoleEditor; } }

        public ITextSurface Surface { get { return _consoleLayers.TextSurface; } }

        public const string ID = "SCENE";

        public string ShortName { get { return "Scene"; } }

        public string Id { get { return ID; } }

        public string Title { get { return "Scene Maker"; } }

        public IEnumerable<FileLoaders.IFileLoader> FileExtensionsLoad { get { return loadersLoad; } }

        public IEnumerable<FileLoaders.IFileLoader> FileExtensionsSave { get { return loadersSave; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public Panels.EntityManagementPanel EntityPanel;
        public Panels.Scene.AnimationListPanel AnimationsPanel;

        public GameObject SelectedEntity
        {
            get { return _selectedEntity; }
            set { _selectedEntity = value; AnimationsPanel.RebuildListBox(); }
        }

        public string[] Tools
        {
            get
            {
                return new string[] { SceneEntityMoveTool.ID, PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public SceneEditor()
        {
            loadersSave = new List<FileLoaders.IFileLoader>() { new FileLoaders.Scene() };
            loadersLoad = new List<FileLoaders.IFileLoader>(loadersSave);
            _consoleLayers = new Console(20, 25);
            _consoleLayers.Renderer = new LayeredTextRenderer();
            EntityPanel = new Panels.EntityManagementPanel();
            AnimationsPanel = new Panels.Scene.AnimationListPanel();
            Entities = new GameObjectCollection();
            LinkedGameObjects = new Dictionary<GameObject, GameObject>();
            Reset();
        }


        public void Reset()
        {
            Entities.Clear();

            List<IEditor> docs = new List<IEditor>();

            foreach (var doc in EditorConsoleManager.Instance.Documents)
                if (doc is EntityEditor)
                    if (((EntityEditor)doc).LinkedEditor == this)
                        docs.Add(doc);

            LinkedGameObjects.Clear();

            foreach (var doc in docs)
                EditorConsoleManager.Instance.CloseDocument(doc);
            
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.LayersPanel, EntityPanel, AnimationsPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };

            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }

            _consoleLayers.TextSurface = new LayeredTextSurface(25, 25, 1);
            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;
            LayerMetadata.Create("Root", false, false, false, ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(0));
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            

            _width = 25;
            _height = 10;

            _mouseMoveHandler = (o, e) => { MouseMove?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseEnterHandler = (o, e) => { MouseEnter?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseExitHandler = (o, e) => { MouseExit?.Invoke(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };

            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;

        }

        internal bool LoadEntity(string selectedFile)
        {
            var entity = SadConsole.Game.GameObject.Load(selectedFile);

            return LoadEntity(entity);
        }

        internal bool LoadEntity(GameObject entity)
        {
            var editor = new Editors.EntityEditor();
            editor.SetEntity(entity);
            editor.LinkedEditor = this;
            EditorConsoleManager.Instance.AddDocument(editor, false);

            var localEntity = new GameObject(entity.Font);

            foreach (var item in entity.Animations.Values)
                localEntity.Animations.Add(item.Name, item);

            localEntity.Animation = localEntity.Animations[entity.Animation.Name];

            localEntity.RenderOffset = _consoleLayers.Position;
            Entities.Add(localEntity);
            EntityPanel.RebuildListBox();

            localEntity.Position = entity.Position;

            LinkedGameObjects.Add(localEntity, entity);

            return true;
        }

        public override string ToString()
        {
            return Title;
        }

        public void ProcessKeyboard(KeyboardInfo info)
        {
            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);
        }

        public void ProcessMouse(MouseInfo info)
        {
            _consoleLayers.ProcessMouse(info);

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers.TextSurface);

            if (_consoleLayers.IsMouseOver)
                EditorConsoleManager.Instance.SurfaceMouseLocation = info.ConsoleLocation;
            else
                EditorConsoleManager.Instance.SurfaceMouseLocation = Point.Zero;

        }

        public void Render()
        {
            _consoleLayers.Render();

            foreach (var entity in Entities)
            {
                entity.Render();
            }
        }

        public void Update()
        {
            _consoleLayers.Update();

            foreach (var entity in Entities)
            {
                entity.Update();
            }
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            var oldSurface = (LayeredTextSurface)_consoleLayers.TextSurface;
            var newSurface = new LayeredTextSurface(width, height, oldSurface.LayerCount);

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

            _consoleLayers.TextSurface = newSurface;
            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();
        }

        public void Position(int x, int y)
        {
            Position(new Point(x, y));
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Position = newPosition;

            foreach (var entity in Entities)
            {
                entity.RenderOffset = newPosition;
            }
        }

        public Point GetPosition()
        {
            return _consoleLayers.Position;
        }

        public void OnSelected()
        {
            foreach (var item in EditorConsoleManager.Instance.Documents)
            {
                var editor = item as EntityEditor;

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
            foreach (var item in EditorConsoleManager.Instance.Editors.Values)
            {
                var editor = item as EntityEditor;

                if (editor != null && editor.LinkedEditor == this)
                    editor.LinkedEditor = null;
            }
        }

        public void Save(string file, FileLoaders.IFileLoader loader)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file, typeof(LayerMetadata));

            GameObject[] objects = Entities.ToArray();

            SadConsole.Serializer.Save(objects, file + ".objects");
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (System.IO.File.Exists(file))
            {
                Reset();

                EntityPanel.RebuildListBox();
                AnimationsPanel.RebuildListBox();

                if (_consoleLayers != null)
                {
                    _consoleLayers.MouseMove -= _mouseMoveHandler;
                    _consoleLayers.MouseEnter -= _mouseEnterHandler;
                    _consoleLayers.MouseExit -= _mouseExitHandler;
                }

                // Support REXPaint
                //if (file.EndsWith(".xp"))
                //{
                //    using (var filestream = new FileStream(file, FileMode.Open))
                //        _consoleLayers.TextSurface = SadConsole.Readers.REXPaint.Image.Load(filestream).ToTextSurface();
                //}
                //else
                //{
                loader = new FileLoaders.Scene();
                _consoleLayers.TextSurface = loader.Load(file);
                //}

                _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

                EditorConsoleManager.Instance.ToolPane.LayersPanel.RebuildListBox();

                _consoleLayers.MouseMove += _mouseMoveHandler;
                _consoleLayers.MouseEnter += _mouseEnterHandler;
                _consoleLayers.MouseExit += _mouseExitHandler;

                _width = _consoleLayers.Width;
                _height = _consoleLayers.Height;

                EditorConsoleManager.Instance.UpdateBox();
                

                // Load game objects
                file += ".objects";

                if (System.IO.File.Exists(file))
                {
                    GameObject[] objects = SadConsole.Serializer.Load<GameObject[]>(file);

                    foreach (var item in objects)
                    {
                        LoadEntity(item);
                    }
                }
            }
        }

        public void RemoveLayer(int index)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(index);
        }

        public void MoveLayerUp(int index)
        {
            var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
            ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index + 1);
        }

        public void MoveLayerDown(int index)
        {
            var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
            ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index - 1);
        }

        public void AddNewLayer(string name)
        {
            LayerMetadata.Create(name, true, true, true, ((LayeredTextSurface)_consoleLayers.TextSurface).Add());
        }

        public bool LoadLayer(string file)
        {
            if (System.IO.File.Exists(file))
            {
                //TODO: Load layer types.. XP, TextSurface, Layer


                //typeof(LayerMetadata)
                var surface = SadConsole.Consoles.TextSurface.Load(file);

                if (surface.Width != EditorConsoleManager.Instance.SelectedEditor.Surface.Width || surface.Height != EditorConsoleManager.Instance.SelectedEditor.Height)
                {
                    var newLayer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
                    LayerMetadata.Create("Loaded", true, true, true, newLayer);
                    var tempSurface = new TextSurface(_consoleLayers.Width, _consoleLayers.Height,
                                                      newLayer.Cells, _consoleLayers.TextSurface.Font);
                    surface.Copy(tempSurface);
                    newLayer.Cells = tempSurface.Cells;
                }
                else
                {
                    var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
                    LayerMetadata.Create("Loaded", true, true, true, layer);
                    layer.Cells = surface.Cells;

                }

                return true;
            }
            else
                return false;
        }

        public void SaveLayer(int index, string file)
        {
            SadConsole.Serializer.Save(((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index), file, new Type[] { typeof(LayerMetadata) });
        }

        public void SetActiveLayer(int index)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(index);
        }
    }
    
}
