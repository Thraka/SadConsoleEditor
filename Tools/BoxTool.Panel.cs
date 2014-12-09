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
        private Controls.CharacterPicker _characterPicker;

        public Color FillColor { get { return _fillColor.SelectedColor; } }
        public Color LineForeColor { get { return _lineForeColor.SelectedColor; } }
        public Color LineBackColor { get { return _lineBackColor.SelectedColor; } }
        public bool UseFill { get { return _fillBoxOption.IsSelected; } }
        public bool UseCharacterBorder { get { return _useCharBorder.IsSelected; } }

        public int BorderCharacter { get { return _characterPicker.SelectedCharacter; } }


        public BoxToolPanel()
        {
            Title = "Settings";

            _fillBoxOption = new CheckBox(18, 1);
            _fillBoxOption.Text = "Fill";

            _useCharBorder = new CheckBox(18, 1);
            _useCharBorder.Text = "Char. Border";
            _useCharBorder.IsSelectedChanged += (s, o) => { _characterPicker.IsVisible = _useCharBorder.IsSelected; EditorConsoleManager.Instance.ToolPane.RefreshControls(); };

            _lineForeColor = new Controls.ColorPresenter("Border Fore", Settings.Green, 18);
            _lineForeColor.SelectedColor = Color.White;

            _lineBackColor = new Controls.ColorPresenter("Border Back", Settings.Green, 18);
            _lineBackColor.SelectedColor = Color.Black;

            _fillColor = new Controls.ColorPresenter("Fill Color", Settings.Green, 18);
            _fillColor.SelectedColor = Color.Black;

            _characterPicker = new Controls.CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            _characterPicker.IsVisible = false;

            Controls = new ControlBase[] { _lineForeColor, _lineBackColor, _fillColor, _fillBoxOption, _useCharBorder, _characterPicker };
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == _characterPicker)
                _characterPicker.Position = new Point(_characterPicker.Position.X + 1, _characterPicker.Position.Y);

            if (control == _useCharBorder)
                return 1;

            if (control != _fillColor)
                return 0;
            else
                return 1;
        }

        public override void Loaded(SadConsole.CellSurface surface)
        {
        }

        public override void Unloaded(SadConsole.CellSurface surface)
        {
        }
    }
}
