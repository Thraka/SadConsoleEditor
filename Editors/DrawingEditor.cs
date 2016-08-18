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

        public ITextSurface Surface { get { return _consoleLayers.TextSurface; } }

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
                return new string[] { PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID };
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
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.LayersPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };
            //ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };

            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }

            _consoleLayers.TextSurface = new LayeredTextSurface(10, 10, 1);
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
        }

        public void Update()
        {
            _consoleLayers.Update();
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
            ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file, typeof(LayerMetadata));
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
                    using (var filestream = new FileStream(file, FileMode.Open))
                        _consoleLayers.TextSurface = SadConsole.Readers.REXPaint.Image.Load(filestream).ToTextSurface();
                }
                else
                {
                    // TODO: Is there an API to load the JSON and examine it? Do that and see what the root type is
                    // then deserialize it into the appropriate type.
                    
                    _consoleLayers.TextSurface = LayeredTextSurface.Load(file, typeof(LayerMetadata));
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
