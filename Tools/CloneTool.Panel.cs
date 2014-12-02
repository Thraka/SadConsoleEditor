using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsole.Input;
using Microsoft.Xna.Framework;
using SadConsole;

namespace SadConsoleEditor.Tools
{
    class CloneToolPanel : CustomPanel
    {
        RadioButton _selectionMode;
        RadioButton _cloneMode;

        private bool _selecting;
        private bool _movingClone;

        public CloneToolPanel()
        {
            _selectionMode = new RadioButton(10, 1);
            _cloneMode = new RadioButton(10, 1);

            _selectionMode.GroupName = "clone";
            _cloneMode.GroupName = "clone";

            _selectionMode.Text = "Select Area";
            _selectionMode.Text = "Clone & Move";

            _selectionMode.IsSelectedChanged += _selectionMode_IsSelectedChanged;
            _cloneMode.IsSelectedChanged += _selectionMode_IsSelectedChanged;

            Controls = new ControlBase[] { _selectionMode, _cloneMode};

            Title = "Clone";
        }

        private void _selectionMode_IsSelectedChanged(object sender, EventArgs e)
        {
            if (_selectionMode.IsSelected)
            {

            }
            else
            {

            }
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            return 0;
        }

        public override void Loaded(CellSurface surface)
        {
            _selectionMode.IsSelected = true;
        }

        public override void Unloaded(CellSurface surface)
        {
        }
    }
}
