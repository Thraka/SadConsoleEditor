﻿using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    public class CharacterPickPane: CustomPanel
    {
        public event EventHandler Changed;

        private Controls.ColorPresenter _foreColor;
        private Controls.ColorPresenter _backColor;
        private Controls.ColorPresenter _charPreview;
        private Controls.CharacterPicker _characterPicker;
        private Windows.CharacterQuickSelectPopup _popupCharacterWindow;

        public Color SettingForeground { get { return _foreColor.SelectedColor; } set { _foreColor.SelectedColor = value; } }
        public Color SettingBackground { get { return _backColor.SelectedColor; } set { _backColor.SelectedColor = value; } }
        public int SettingCharacter { get { return _characterPicker.SelectedCharacter; } set { _characterPicker.SelectedCharacter = value; } }

        public bool HideCharacter;
        public bool HideForeground;
        public bool HideBackground;

        public Font PickerFont { get { return _characterPicker.AlternateFont; } set { _characterPicker.AlternateFont = value; } }

        public CharacterPickPane(string title, bool hideCharacter, bool hideForeground, bool hideBackground)
        {
            Title = title;

            _foreColor = new ColorPresenter("Foreground", Settings.Green, SadConsoleEditor.Consoles.ToolPane.PanelWidth);
            _backColor = new ColorPresenter("Background", Settings.Green, SadConsoleEditor.Consoles.ToolPane.PanelWidth);
            _charPreview = new ColorPresenter("Character", Settings.Green, SadConsoleEditor.Consoles.ToolPane.PanelWidth);
            _characterPicker = new CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            _popupCharacterWindow = new Windows.CharacterQuickSelectPopup(0);

            _foreColor.SelectedColor = Color.White;
            _backColor.SelectedColor = Color.Black;

            _charPreview.CharacterColor = _foreColor.SelectedColor;
            _charPreview.SelectedColor = _backColor.SelectedColor;
            _charPreview.Character = 0;
            _charPreview.DisableColorPicker = true;

            _popupCharacterWindow.Font = Settings.ScreenFont;
            _popupCharacterWindow.Closed += (o, e) => { _characterPicker.SelectedCharacter = _popupCharacterWindow.SelectedCharacter; };

            _foreColor.ColorChanged += (o, e) => { _charPreview.CharacterColor = _foreColor.SelectedColor; OnChanged(); };
            _backColor.ColorChanged += (o, e) => { _charPreview.SelectedColor = _backColor.SelectedColor; OnChanged(); };
            _characterPicker.SelectedCharacterChanged += (sender, e) => { _charPreview.Character = e.NewCharacter; _charPreview.Title = "Character (" + e.NewCharacter.ToString() + ")"; OnChanged(); };
            _characterPicker.SelectedCharacter = 1;
            _charPreview.MouseButtonClicked += (o, e) => { 
                if (e.RightButtonClicked)
                {
                    _popupCharacterWindow.Center();
                    _popupCharacterWindow.Show(true);
                }
            };


            HideCharacter = hideCharacter;
            HideForeground = hideForeground;
            HideBackground = hideBackground;

            Reset();
        }

        public void Reset()
        {
            var tempControls = new List<ControlBase>() { _foreColor, _backColor, _charPreview, _characterPicker };

            if (HideForeground) tempControls.Remove(_foreColor);
            if (HideBackground) tempControls.Remove(_backColor);
            if (HideCharacter) { tempControls.Remove(_charPreview); tempControls.Remove(_characterPicker); }

            Controls = tempControls.ToArray();
        }

        private void OnChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            if (control == _charPreview)
                return 1;

            if (control == _characterPicker)
                control.Position = new Point(2, control.Position.Y);

            return 0;
        }

        public override void Loaded()
        {
        }
    }
}