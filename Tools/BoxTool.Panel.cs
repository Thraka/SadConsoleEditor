using Microsoft.Xna.Framework;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    class BoxToolPanel : CustomPanel
    {
        private CheckBox _fillBoxOption;
        private CheckBox _useCharBorder;
        private Controls.ColorPresenter _fillColor;
        private Controls.ColorPresenter _lineForeColor;
        private Controls.ColorPresenter _lineBackColor;

        public Color FillColor { get { return _fillColor.SelectedColor; } }
        public Color LineForeColor { get { return _lineForeColor.SelectedColor; } }
        public Color LineBackColor { get { return _lineBackColor.SelectedColor; } }
        public bool UseFill { get { return _fillBoxOption.IsSelected; } }
        public bool UseCharacterBorder { get { return _useCharBorder.IsSelected; } }


        public BoxToolPanel()
        {
            Title = "Extra Box Options";

            _fillBoxOption = new CheckBox(18, 1);
            _fillBoxOption.Text = "Fill";

            _useCharBorder = new CheckBox(18, 1);
            _useCharBorder.Text = "Char. Border";

            _lineForeColor = new Controls.ColorPresenter("Line Fore", Settings.Green, 18);
            _lineForeColor.SelectedColor = Color.White;

            _lineBackColor = new Controls.ColorPresenter("Line Back", Settings.Green, 18);
            _lineBackColor.SelectedColor = Color.Black;

            _fillColor = new Controls.ColorPresenter("Fill Color", Settings.Green, 18);
            _fillColor.SelectedColor = Color.Black;

            //TODO: Create character control that is displayed when use char border is checked.
            Controls = new ControlBase[] { _lineForeColor, _lineBackColor, _fillColor, _fillBoxOption, _useCharBorder };
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == _fillColor)
                return 0;
            else
                return -1;
        }

        public override void Loaded(SadConsole.CellSurface surface)
        {
        }

        public override void Unloaded(SadConsole.CellSurface surface)
        {
        }
    }
}
