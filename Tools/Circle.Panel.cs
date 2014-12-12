using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    class CircleToolPanel : CustomPanel
    {
        private DrawingSurface _statusBox;
        private int _circleWidth;
        private int _circleHeight;

        public int CircleWidth { get {return _circleWidth;} set{_circleWidth = value; RedrawBox();} }
        public int CircleHeight { get { return _circleHeight; } set { _circleHeight = value; RedrawBox(); } }

        public CircleToolPanel()
        {
            Title = "Circle Status";

            _statusBox = new DrawingSurface(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 2);
            RedrawBox();
            Controls = new ControlBase[] { _statusBox };
        }

        private void RedrawBox()
        {
            _statusBox.Fill(Settings.Yellow, Color.Transparent, 0, null);
            
            var widthText = "Width: ".CreateColored(Settings.Yellow, Color.Transparent, null) + _circleWidth.ToString().CreateColored(Settings.Blue, Color.Transparent, null);
            var heightText = "Height: ".CreateColored(Settings.Yellow, Color.Transparent, null) + _circleHeight.ToString().CreateColored(Settings.Blue, Color.Transparent, null);

            _statusBox.Print(0, 0, widthText);
            _statusBox.Print(0, 1, heightText);
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return 0;
        }

        public override void Loaded()
        {
        }


    }
}
