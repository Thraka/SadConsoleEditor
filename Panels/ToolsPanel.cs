using SadConsole.Controls;
using SadConsoleEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Panels
{
    class ToolsPanel: CustomPanel
    {
        public ListBox ToolsListBox;

        public ToolsPanel()
        {
            Title = "Tools";

            ToolsListBox = new ListBox(20 - 2, 7);
            ToolsListBox.HideBorder = true;
            ToolsListBox.CanUseKeyboard = false;

            

            Controls = new ControlBase[] { ToolsListBox };
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
