using System;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsole.Consoles;

namespace SadConsoleEditor.Editors
{
    class DrawingEditor: IEditor
    {
        private int _width;
        private int _height;
        private Console _consoleLayers;

        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseExit;
        public event EventHandler<MouseEventArgs> MouseMove;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public EditorSettings Settings { get { return SadConsoleEditor.Settings.Config.ConsoleEditor; } }

        public Console Surface { get { return _consoleLayers; } }

        public const string ID = "DRAW";

        public string ShortName { get { return "Con"; } }

        public string Id { get { return ID; } }

        public string Title { get { return "Console"; } }

        public string FileExtensionsLoad { get { return "*.lconsole;*.xp"; } }

        public string FileExtensionsSave { get { return "*.lconsole;"; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID, RecolorTool.ID }; //, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public DrawingEditor()
        {
            _consoleLayers = new Console(new LayeredTextSurface(10, 10, 2));
            _consoleLayers.Renderer = new LayeredTextRenderer();
            Reset();
        }

        public void Reset()
        {
            //ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.LayersPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };

            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }

            _consoleLayers.TextSurface = new LayeredTextSurface(10, 10, 2);
            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            //_consoleLayers.GetLayerMetadata(0).Name = "Root";
            //_consoleLayers.GetLayerMetadata(0).IsRemoveable = false;
            //_consoleLayers.GetLayerMetadata(0).IsMoveable = false;

            _width = 25;
            _height = 10;

            _mouseMoveHandler = (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseEnterHandler = (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseExitHandler = (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers.TextSurface, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };

            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;

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
            Surface.Render();
        }
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            _consoleLayers.TextSurface = new LayeredTextSurface(width, height, 1);
            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();
        }

        public void Position(int x, int y)
        {
            _consoleLayers.Position = new Point(x, y);
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Position = newPosition;
        }

        public Point GetPosition()
        {
            return _consoleLayers.Position;
        }

        public void Save(string file)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file);
        }

        public void Load(string file)
        {
            if (System.IO.File.Exists(file))
            {
                if (_consoleLayers != null)
                {
                    _consoleLayers.MouseMove -= _mouseMoveHandler;
                    _consoleLayers.MouseEnter -= _mouseEnterHandler;
                    _consoleLayers.MouseExit -= _mouseExitHandler;
                }

                // Support REXPaint
                if (file.EndsWith(".xp"))
                {
                    //SadConsole.Readers.REXPaint
                    //SadConsole.Readers.REXPaint.RexReader reader = new SadConsole.Readers.REXPaint.RexReader(file);
                    //_consoleLayers = reader.GetMap().ToLayeredConsole();
                }
                else
                {
                    // TODO: Is there an API to load the JSON and examine it? Do that and see what the root type is
                    // then deserialize it into the appropriate type.
                    _consoleLayers.TextSurface = SadConsole.Serializer.Load<LayeredTextSurface>(file);
                }

                _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

                _consoleLayers.MouseMove += _mouseMoveHandler;
                _consoleLayers.MouseEnter += _mouseEnterHandler;
                _consoleLayers.MouseExit += _mouseExitHandler;

                _width = _consoleLayers.Width;
                _height = _consoleLayers.Height;

                EditorConsoleManager.Instance.UpdateBox();
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
            LayerMetadata.Create(name, ((LayeredTextSurface)_consoleLayers.TextSurface).Add());
        }

        public bool LoadLayer(string file)
        {
            if (System.IO.File.Exists(file))
            {
                //typeof(LayerMetadata)
                var surface = SadConsole.Consoles.TextSurface.Load(file);

                if (surface.Width != EditorConsoleManager.Instance.SelectedEditor.Surface.Width || surface.Height != EditorConsoleManager.Instance.SelectedEditor.Height)
                {
                    var newLayer = ((LayeredTextSurface)EditorConsoleManager.Instance.SelectedEditor.Surface.TextSurface).Add();
                    LayerMetadata.Create("Loaded", newLayer);
                    var tempSurface = new TextSurface(EditorConsoleManager.Instance.SelectedEditor.Surface.Width,
                                                      EditorConsoleManager.Instance.SelectedEditor.Surface.Height,
                                                      EditorConsoleManager.Instance.SelectedEditor.Surface.TextSurface.Font, newLayer.Cells);
                    TextSurface.Copy(surface, tempSurface);
                    newLayer.Cells = tempSurface.Cells;
                }
                else
                {
                    var layer = ((LayeredTextSurface)EditorConsoleManager.Instance.SelectedEditor.Surface.TextSurface).Add();
                    LayerMetadata.Create("Loaded", layer);
                    layer.Cells = surface.Cells;

                }

                return true;
            }
            else
                return false;
        }

        public void SaveLayer(int index, string file)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file, typeof(LayerMetadata));
        }

        public void SetActiveLayer(int index)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(index);
        }
    }
}
