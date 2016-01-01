using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Panels
{
    class LineToolPanel : CustomPanel
    {
        private DrawingSurface _statusBox;
        private int _lineLength;

        public int LineLength { get { return _lineLength; } set { _lineLength = value; RedrawBox(); } }
        

        public LineToolPanel()
        {
            Title = "Line Status";

            _statusBox = new DrawingSurface(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 2);
            RedrawBox();
            Controls = new ControlBase[] { _statusBox };
        }

        private void RedrawBox()
        {
            _statusBox.Fill(Settings.Yellow, Color.Transparent, 0, null);

            var widthText = "Length: ".CreateColored(Settings.Yellow, Color.Transparent, null) + _lineLength.ToString().CreateColored(Settings.Blue, Color.Transparent, null);

            _statusBox.Print(0, 0, widthText);
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
