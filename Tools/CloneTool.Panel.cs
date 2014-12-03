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
        NoCheckRadioButton _selectionMode;
        NoCheckRadioButton _cloneMode;
        
        private bool _selecting;
        private bool _movingClone;

        public CloneToolPanel()
        {
            _selectionMode = new NoCheckRadioButton(18, 1);
            _cloneMode = new NoCheckRadioButton(18, 1);
            
            _selectionMode.GroupName = "clone";
            _cloneMode.GroupName = "clone";

            _selectionMode.Text = "Select Area";
            _cloneMode.Text = "Clone & Move";

            _selectionMode.TextAlignment = _cloneMode.TextAlignment = System.Windows.HorizontalAlignment.Center;

            _selectionMode.IsSelectedChanged += _selectionMode_IsSelectedChanged;
            _cloneMode.IsSelectedChanged += _selectionMode_IsSelectedChanged;

            _selectionMode.IsSelected = true;

            Controls = new ControlBase[] { _selectionMode, _cloneMode};
            
            Title = "Clone";
        }

        private void _selectionMode_IsSelectedChanged(object sender, EventArgs e)
        {
            _selecting = _selectionMode.IsSelected;
            _movingClone = _cloneMode.IsSelected;

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
