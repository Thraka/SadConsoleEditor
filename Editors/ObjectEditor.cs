using System;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;

namespace SadConsoleEditor.Editors
{
    class ObjectEditor : IEditor
    {
        private int _width;
        private int _height;
        private LayeredConsole _consoleLayers;

        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseExit;
        public event EventHandler<MouseEventArgs> MouseMove;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public Consoles.LayeredConsole Surface { get { return _consoleLayers; } }

        public const string ID = "OBJ";

        public string Id { get { return ID; } }

        public string Title { get { return "Game Console"; } }

        public string FileExtensions { get { return ".gconsole"; } }
        public CustomPanel[] ControlPanels { get; private set; }


        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID, FillTool.ID, TextTool.ID, LineTool.ID, BoxTool.ID, ObjectTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public ObjectEditor()
        {
            _consoleLayers = new LayeredConsole(1, 10, 5);
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            //_consoleLayers[0].CellData.Fill(Color.Blue, Color.Yellow, 2, null);

            _width = 10;
            _height = 5;

            // THIS WHOLE MOUSE HANDLING IS VERY MESSY.
            // THERE ARE TOO MANY PATHS OBJ_1->OBJ_2->OBJ_1->OBJ_3 type of calling chain.
            // REWRITE
            _mouseMoveHandler = (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseEnterHandler = (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseExitHandler = (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };


            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;
        }

        public void Reset()
        {
            _consoleLayers.MouseMove -= _mouseMoveHandler;
            _consoleLayers.MouseEnter -= _mouseEnterHandler;
            _consoleLayers.MouseExit -= _mouseExitHandler;

            _consoleLayers = new LayeredConsole(1, 10, 5);
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;

            _width = 10;
            _height = 5;

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

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();
        }

        public void Position(int x, int y)
        {
            _consoleLayers.Move(new Point(x, y));
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Move(newPosition);
        }

        public Point GetPosition()
        {
            return _consoleLayers.Position;
        }

        public void Save(string file)
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(LayeredConsole), new Type[] { typeof(LayeredConsole) });
            var stream = System.IO.File.OpenWrite(file);

            serializer.WriteObject(stream, _consoleLayers);
            stream.Dispose();
        }

        public void Load(string file)
        {
            var fileObject = System.IO.File.OpenRead(file);
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(LayeredConsole), new Type[] { typeof(LayeredConsole) });

            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }


            _consoleLayers = serializer.ReadObject(fileObject) as LayeredConsole;

            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;

            _width = _consoleLayers.Width;
            _height = _consoleLayers.Height;

            EditorConsoleManager.Instance.UpdateBox();

        }
    }
}
