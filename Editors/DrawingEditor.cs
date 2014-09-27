using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SadConsoleEditor.Tools;
using SadConsole.Input;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Consoles;

namespace SadConsoleEditor.Editors
{
    class DrawingEditor: IEditor
    {
        private int _width;
        private int _height;
        private LayeredConsole _consoleLayers;

        public EventHandler<MouseEventArgs> MouseEnter { get; set; }
        public EventHandler<MouseEventArgs> MouseExit { get; set; }
        public EventHandler<MouseEventArgs> MouseMove { get; set; }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }


        public Console Surface { get { return _consoleLayers; } }

        public const string ID = "DRAW";

        public string Id { get { return ID; } }

        public string Title { get { return "Drawing"; } }


        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID };
            }
        }


        public DrawingEditor()
        {
            _consoleLayers = new LayeredConsole(1, 10, 5);
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            //_consoleLayers[0].CellData.Fill(Color.Blue, Color.Yellow, 2, null);
            
            _width = 10;
            _height = 5;

            // THIS WHOLE MOUSE HANDLING IS VERY MESSY.
            // THERE ARE TOO MANY PATHS OBJ_1->OBJ_2->OBJ_1->OBJ_3 type of calling chain.
            // REWRITE NOW

            _consoleLayers.MouseMove += (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers.ActiveLayer, e); };
            _consoleLayers.MouseEnter += (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers.ActiveLayer, e); };
            _consoleLayers.MouseExit += (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers.ActiveLayer, e); };
        }

        public override string ToString()
        {
            return Title;
        }

        public void ProcessKeyboard(KeyboardInfo info)
        {
            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers[0].CellData);
        }

        public void ProcessMouse(MouseInfo info)
        {
            _consoleLayers.ProcessMouse(info);

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers[0].CellData);
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
    }
}
