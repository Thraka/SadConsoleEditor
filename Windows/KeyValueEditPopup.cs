using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Windows
{
    class KeyValueEditPopup : SadConsole.Consoles.Window
    {
        private struct SettingKeyValue
        {
            public string Key;
            public string Value;
        }

        private Button _saveButton;
        private Button _cancelButton;
        private Button _addFieldButton;
        private Button _removeFieldButton;
        private Button _updateFieldButton;
        private ListBox _objectSettingsListbox;
        private InputBox _settingNameInput;
        private InputBox _settingValueInput;

        private SpriteEffects _settingMirrorEffect;

        public Dictionary<string, string> SettingsDictionary;
        
        public KeyValueEditPopup(Dictionary<string, string> settings)
            : base(39, 29)
        {
            Title = "Settings";

            // Settings of the appearance fields
            Print(2, 2, "Name", Settings.Green);

            // Setting controls of the game object
            _objectSettingsListbox = new ListBox(18, 7);
            _settingNameInput = new InputBox(18);
            _settingValueInput = new InputBox(18);
            _objectSettingsListbox.HideBorder = true;


            Print(2, 10, "Settings/Flags", Settings.Green);
            _objectSettingsListbox.Position = new Point(2, 11);

            Print(2, 19, "Setting Name", Settings.Green);
            Print(2, 22, "Setting Value", Settings.Green);
            _settingNameInput.Position = new Point(2, 20);
            _settingValueInput.Position = new Point(2, 23);

            _addFieldButton = new Button(16, 1);
            _addFieldButton.Text = "Save New Setting";
            _addFieldButton.Position = new Point(textSurface.Width - 18, 20);

            _removeFieldButton = new Button(16, 1);
            _removeFieldButton.Text = "Remove Setting";
            _removeFieldButton.Position = new Point(textSurface.Width - 18, 21);
            _removeFieldButton.IsEnabled = false;

            _updateFieldButton = new Button(16, 1);
            _updateFieldButton.Text = "Update Setting";
            _updateFieldButton.Position = new Point(textSurface.Width - 18, 22);
            _updateFieldButton.IsEnabled = false;

            _objectSettingsListbox.SelectedItemChanged += _objectSettingsListbox_SelectedItemChanged;
            _addFieldButton.ButtonClicked += _addFieldButton_ButtonClicked;
            _removeFieldButton.ButtonClicked += _removeFieldButton_ButtonClicked;
            _updateFieldButton.ButtonClicked += _updateFieldButton_ButtonClicked;

            Add(_objectSettingsListbox);
            Add(_settingNameInput);
            Add(_settingValueInput);
            Add(_addFieldButton);
            Add(_removeFieldButton);
            Add(_updateFieldButton);

            // Save/close buttons
            _saveButton = new Button(10, 1);
            _cancelButton = new Button(10, 1);

            _saveButton.Text = "Save";
            _cancelButton.Text = "Cancel";

            _saveButton.ButtonClicked += _saveButton_ButtonClicked;
            _cancelButton.ButtonClicked += (o, e) => { DialogResult = false; Hide(); };

            _saveButton.Position = new Point(textSurface.Width - 12, 26);
            _cancelButton.Position = new Point(2, 26);

            Add(_saveButton);
            Add(_cancelButton);

            // Read the settings
            foreach (var item in settings)
                _objectSettingsListbox.Items.Add(new SettingKeyValue() { Key = item.Key, Value = item.Value });

            SettingsDictionary = settings;

            Redraw();
        }

        void _saveButton_ButtonClicked(object sender, EventArgs e)
        {
            DialogResult = true;
            Hide();
        }

        void _objectSettingsListbox_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            if (_objectSettingsListbox.SelectedItem != null)
            {
                _updateFieldButton.IsEnabled = true;
                _removeFieldButton.IsEnabled = true;

                var objectSetting = (KeyValuePair<string, string>)_objectSettingsListbox.SelectedItem;

                _settingNameInput.Text = objectSetting.Key;
                _settingValueInput.Text = objectSetting.Value;
            }
            else
            {
                _updateFieldButton.IsEnabled = false;
                _removeFieldButton.IsEnabled = false;

                _settingNameInput.Text = "";
                _settingValueInput.Text = "";
            }
        }

        void _updateFieldButton_ButtonClicked(object sender, EventArgs e)
        {
            var objectSetting = (SettingKeyValue)_objectSettingsListbox.SelectedItem;

            objectSetting.Key = _settingNameInput.Text;
            objectSetting.Value = _settingValueInput.Text;
        }

        void _removeFieldButton_ButtonClicked(object sender, EventArgs e)
        {
            if (_objectSettingsListbox.SelectedItem != null)
                _objectSettingsListbox.Items.Remove(_objectSettingsListbox.SelectedItem);
        }

        void _addFieldButton_ButtonClicked(object sender, EventArgs e)
        {
            SettingKeyValue objectSetting = new SettingKeyValue();
            objectSetting.Key = _settingNameInput.Text;
            objectSetting.Value = _settingValueInput.Text;
            _objectSettingsListbox.Items.Add(objectSetting);
        }

        public override void Show(bool modal)
        {
            base.Show(modal);

            Center();
        }

        public override void Hide()
        {
            if (DialogResult)
            {
                // Fill in the game object with all the new values
                SettingsDictionary = new Dictionary<string, string>();

                foreach (var item in _objectSettingsListbox.Items.Cast<SettingKeyValue>())
                    SettingsDictionary.Add(item.Key, item.Value);
            }

            base.Hide();
        }

        public override void Redraw()
        {
            base.Redraw();

            Print(2, 2, "Name", Settings.Green);
            Print(2, 10, "Settings/Flags", Settings.Green);
            Print(2, 19, "Setting Name", Settings.Green);
            Print(2, 22, "Setting Value", Settings.Green);

            SetGlyph(0, 9, 199);
            for (int i = 1; i < 20; i++)
                SetGlyph(i, 9, 196);

            SetGlyph(20, 9, 191);

            for (int i = 10; i < 18; i++)
                SetGlyph(20, i, 179);

            SetGlyph(20, 18, 192);

            for (int i = 21; i < textSurface.Width; i++)
                SetGlyph(i, 18, 196);

            SetGlyph(textSurface.Width - 1, 18, 182);

            // Long line under field
            SetGlyph(0, 24, 199);
            for (int i = 1; i < textSurface.Width; i++)
                SetGlyph(i, 24, 196);
            SetGlyph(textSurface.Width - 1, 24, 182);
        }
    }
}
