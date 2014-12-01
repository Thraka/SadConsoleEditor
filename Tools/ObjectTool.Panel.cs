using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsole.Input;
using Microsoft.Xna.Framework;

namespace SadConsoleEditor.Tools
{
    class ObjectToolPanel : CustomPanel
    {
        InputBox _nameInput;
        InputBox _typeInput;
        Button _dummyButton;

        public ObjectToolPanel()
        {
            _nameInput = new InputBox(13);
            _typeInput = new InputBox(13);

            Controls = new ControlBase[] { _nameInput, _typeInput };

            Title = "Object";
        }

        public override void ProcessMouse(MouseInfo info)
        {
            
        }

        public override int Redraw(ControlBase control)
        {
            if (control == _nameInput)
            {
                control.Parent.CellData.Print(1, control.Position.Y, "Name");
                control.Position = new Point(6, control.Position.Y);
            }
            else if (control == _typeInput)
            {
                control.Parent.CellData.Print(1, control.Position.Y, "Type");
                control.Position = new Point(6, control.Position.Y);
            }
            return 0;
        }
    }
}
