using System;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsole.GameHelpers;

namespace SadConsoleEditor.Editors
{
    class GameScreenEditor : IEditor
    {
        private int _width;
        private int _height;
        private LayeredConsole _consoleLayers;
        private SadConsole.Consoles.CellsRenderer _objectsSurface;
        private bool _displayObjectLayer;

        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseExit;
        public event EventHandler<MouseEventArgs> MouseMove;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }


        public Consoles.LayeredConsole Surface { get { return _consoleLayers; } }

        public GameObjectCollection GameObjects { get; set; }

        public bool DisplayObjectLayer { set { _displayObjectLayer = value; } }

        public const string ID = "GAME";

        public string Id { get { return ID; } }

        public string Title { get { return "Game Screen Editor"; } }

        public string FileExtensions { get { return "*.screen"; } }
        public CustomPanel[] ControlPanels { get; private set; }

        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID, ObjectTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public GameScreenEditor()
        {
            Reset();
        }

        public void Reset()
        {
            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }

            _objectsSurface = new SadConsole.Consoles.Console(25, 10);
            _objectsSurface.Font = Settings.ScreenFont;
            _objectsSurface.CellData.DefaultForeground = Color.White;
            _objectsSurface.CellData.DefaultBackground = Color.Transparent;
            _objectsSurface.CellData.Clear();
            _objectsSurface.BeforeRenderHandler = (cr) => cr.Batch.Draw(SadConsole.Engine.BackgroundCell, cr.RenderBox, null, new Color(0, 0, 0, 0.5f)); 

            _consoleLayers = new LayeredConsole(1, 25, 10);
            _consoleLayers.Font = Settings.ScreenFont;
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            _consoleLayers.GetLayerMetadata(0).Name = "Root";
            _consoleLayers.GetLayerMetadata(0).IsRemoveable = false;
            _consoleLayers.GetLayerMetadata(0).IsMoveable = false;
            
            _width = 25;
            _height = 10;

            GameObjects = new GameObjectCollection();

            _mouseMoveHandler = (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseEnterHandler = (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseExitHandler = (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };

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
            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.ActiveLayer);
        }

        public void ProcessMouse(MouseInfo info)
        {
            _consoleLayers.ProcessMouse(info);

            if (_consoleLayers.IsMouseOver)
                EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers.ActiveLayer);
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            _consoleLayers.Resize(width, height);
            _objectsSurface.CellData.Resize(width, height);

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();
        }

        public void Position(int x, int y)
        {
            _consoleLayers.Move(new Point(x, y));
            _objectsSurface.Position = new Point(x, y);
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Move(newPosition);
            _objectsSurface.Position = newPosition;
        }

        public Point GetPosition()
        {
            return _consoleLayers.Position;
        }

        public void Render()
        {
            Surface.Render();

            if (_displayObjectLayer)
            {
                _objectsSurface.Render();
            }
        }

        public void Save(string file)
        {
            LayeredConsole.Save(_consoleLayers, file);
            GameObjectCollection.Save(GameObjects, file.Replace(System.IO.Path.GetExtension(file), ".objects"));
        }

        public void Load(string file)
        {
            string objectsFile = file.Replace(System.IO.Path.GetExtension(file), ".objects");

            if (System.IO.File.Exists(file))
            {
                if (_consoleLayers != null)
                {
                    _consoleLayers.MouseMove -= _mouseMoveHandler;
                    _consoleLayers.MouseEnter -= _mouseEnterHandler;
                    _consoleLayers.MouseExit -= _mouseExitHandler;
                }

                _consoleLayers = LayeredConsole.Load(file);
                _consoleLayers.Font = Settings.ScreenFont;

                _consoleLayers.MouseMove += _mouseMoveHandler;
                _consoleLayers.MouseEnter += _mouseEnterHandler;
                _consoleLayers.MouseExit += _mouseExitHandler;

                _width = _consoleLayers.Width;
                _height = _consoleLayers.Height;

                EditorConsoleManager.Instance.UpdateBox();

                if (System.IO.File.Exists(objectsFile))
                {
                    GameObjects = GameObjectCollection.Load(objectsFile);
                    SyncObjectsToLayer();
                }
                else
                {
                    GameObjects = new GameObjectCollection();
                    SyncObjectsToLayer();
                }
            }
        }

        public void SyncObjectsToLayer()
        {
            _objectsSurface.CellData.Clear();

            foreach (var item in GameObjects)
            {
                _objectsSurface.CellData.Print(item.Key.X, item.Key.Y, " ", item.Value.Character);
                _objectsSurface.CellData.SetCharacter(item.Key.X, item.Key.Y, item.Value.Character.CharacterIndex);
            }
        }
    }
}
