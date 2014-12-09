using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    class RecolorToolPanel: CustomPanel
    {
        private CheckBox _ignoreForeCheck;
        private CheckBox _ignoreBackCheck;

        public bool IgnoreForeground { get { return _ignoreForeCheck.IsSelected; } }
        public bool IgnoreBackground { get { return _ignoreBackCheck.IsSelected; } }


        public RecolorToolPanel()
        {
            _ignoreBackCheck = new CheckBox(18, 1);
            _ignoreBackCheck.Text = "Ignore Back";

            _ignoreForeCheck = new CheckBox(18, 1);
            _ignoreForeCheck.Text = "Ignore Fore";

            Title = "Recolor Options";

            Controls = new ControlBase[] { _ignoreForeCheck, _ignoreBackCheck };
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
