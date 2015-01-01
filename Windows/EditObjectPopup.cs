using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.GameHelpers;

namespace SadConsoleEditor.Windows
{
    class EditObjectPopup : SadConsole.Consoles.Window
    {
        private Button _saveButton;
        private Button _cancelButton;
        private Button _addFieldButton;
        private Button _removeFieldButton;
        private Button _updateFieldButton;
        private ListBox _objectSettingsListbox;
        private InputBox _nameInput;
        private InputBox _settingNameInput;
        private InputBox _settingValueInput;
        private Controls.ColorPresenter _foregroundPresenter;
        private Controls.ColorPresenter _backgroundPresenter;
        private Controls.ColorPresenter _characterPresenter;
        private CheckBox _mirrorHorizCheck;
        private CheckBox _mirrorVertCheck;
        private Controls.CharacterPicker _characterPicker;

        private SpriteEffects _settingMirrorEffect;

        private GameObject _gameObject;

        public CellAppearance SettingAppearance
        {
            get { return new CellAppearance(_foregroundPresenter.SelectedColor, _backgroundPresenter.SelectedColor, _characterPresenter.Character); }
            set
            {
                _foregroundPresenter.SelectedColor = value.Foreground;
                _backgroundPresenter.SelectedColor = value.Background;
                _characterPresenter.SelectedColor = value.Background;
                _characterPresenter.CharacterColor = value.Foreground;
                _characterPresenter.Character = value.CharacterIndex;
            }
        }
        
        public EditObjectPopup(GameObject gameObject)
            : base(39, 29)
        {
            Font = Settings.ScreenFont;
            _gameObject = gameObject;
            Title = "Object Editor";

            // Settings of the appearance fields
            _nameInput = new InputBox(13);
            _characterPicker = new SadConsoleEditor.Controls.CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            _foregroundPresenter = new SadConsoleEditor.Controls.ColorPresenter("Foreground", Settings.Green, 18);
            _backgroundPresenter = new SadConsoleEditor.Controls.ColorPresenter("Background", Settings.Green, 18);
            _characterPresenter = new SadConsoleEditor.Controls.ColorPresenter("Preview", Settings.Green, 18);
            _mirrorHorizCheck = new CheckBox(18, 1);
            _mirrorVertCheck = new CheckBox(18, 1);

            _characterPicker.SelectedCharacterChanged += (o, e) => _characterPresenter.Character = _characterPicker.SelectedCharacter;
            _foregroundPresenter.ColorChanged += (o, e) => _characterPresenter.CharacterColor = _foregroundPresenter.SelectedColor;
            _backgroundPresenter.ColorChanged += (o, e) => _characterPresenter.SelectedColor = _backgroundPresenter.SelectedColor;

            _cellData.Print(2, 2, "Name", Settings.Green);
            _nameInput.Position = new Point(7, 2);
            _foregroundPresenter.Position = new Point(2, 4);
            _backgroundPresenter.Position = new Point(2, 5);
            _characterPresenter.Position = new Point(2, 6);
            _characterPicker.Position = new Point(21, 2);
            _mirrorHorizCheck.Position = new Point(2, 7);
            _mirrorVertCheck.Position = new Point(2, 8);

            _nameInput.Text = "New";

            _mirrorHorizCheck.IsSelectedChanged += Mirror_IsSelectedChanged;
            _mirrorVertCheck.IsSelectedChanged += Mirror_IsSelectedChanged;
            _mirrorHorizCheck.Text = "Mirror Horiz.";
            _mirrorVertCheck.Text = "Mirror Vert.";

            _foregroundPresenter.SelectedColor = Color.White;
            _backgroundPresenter.SelectedColor = Color.DarkRed;

            _characterPresenter.CharacterColor = _foregroundPresenter.SelectedColor;
            _characterPresenter.SelectedColor = _backgroundPresenter.SelectedColor;
            _characterPicker.SelectedCharacter = 1;

            Add(_nameInput);
            Add(_characterPicker);
            Add(_foregroundPresenter);
            Add(_backgroundPresenter);
            Add(_characterPresenter);
            Add(_mirrorHorizCheck);
            Add(_mirrorVertCheck);

            // Setting controls of the game object
            _objectSettingsListbox = new ListBox(18, 7);
            _settingNameInput = new InputBox(18);
            _settingValueInput = new InputBox(18);
            _objectSettingsListbox.HideBorder = true;


            _cellData.Print(2, 10, "Settings/Flags", Settings.Green);
            _objectSettingsListbox.Position = new Point(2, 11);

            _cellData.Print(2, 19, "Setting Name", Settings.Green);
            _cellData.Print(2, 22, "Setting Value", Settings.Green);
            _settingNameInput.Position = new Point(2, 20);
            _settingValueInput.Position = new Point(2, 23);

            _addFieldButton = new Button(16, 1);
            _addFieldButton.Text = "Save New Setting";
            _addFieldButton.Position = new Point(_cellData.Width - 18, 20);

            _removeFieldButton = new Button(16, 1);
            _removeFieldButton.Text = "Remove Setting";
            _removeFieldButton.Position = new Point(_cellData.Width - 18, 21);
            _removeFieldButton.IsEnabled = false;

            _updateFieldButton = new Button(16, 1);
            _updateFieldButton.Text = "Update Setting";
            _updateFieldButton.Position = new Point(_cellData.Width - 18, 22);
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

            _saveButton.Position = new Point(_cellData.Width - 12, 26);
            _cancelButton.Position = new Point(2, 26);

            Add(_saveButton);
            Add(_cancelButton);

            // Read the gameobject
            _nameInput.Text = _gameObject.Name;
            _mirrorHorizCheck.IsSelected = (_gameObject.Character.SpriteEffect & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            _mirrorVertCheck.IsSelected = (_gameObject.Character.SpriteEffect & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            _characterPicker.SelectedCharacter = _gameObject.Character.CharacterIndex;
            _foregroundPresenter.SelectedColor = _gameObject.Character.Foreground;
            _backgroundPresenter.SelectedColor = _gameObject.Character.Background;

            foreach (var item in _gameObject.Settings)
            {
                var newSetting = new Setting() { Name = item.Name, Value = item.Value };
                _objectSettingsListbox.Items.Add(newSetting);
            }

            Redraw();
        }

        void _saveButton_ButtonClicked(object sender, EventArgs e)
        {
            _gameObject.Name = _nameInput.Text;
            _gameObject.Character = new CellAppearance(_foregroundPresenter.SelectedColor, _backgroundPresenter.SelectedColor, _characterPicker.SelectedCharacter, _settingMirrorEffect);
            _gameObject.Settings.Clear();

            foreach (var item in _objectSettingsListbox.Items.Cast<Setting>())
                _gameObject.Settings.Add(item);

            DialogResult = true;
            Hide();
        }

        void _objectSettingsListbox_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            if (_objectSettingsListbox.SelectedItem != null)
            {
                _updateFieldButton.IsEnabled = true;
                _removeFieldButton.IsEnabled = true;

                var objectSetting = (Setting)_objectSettingsListbox.SelectedItem;

                _settingNameInput.Text = objectSetting.Name;
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
            var objectSetting = (Setting)_objectSettingsListbox.SelectedItem;

            objectSetting.Name = _settingNameInput.Text;
            objectSetting.Value = _settingValueInput.Text;
        }

        void _removeFieldButton_ButtonClicked(object sender, EventArgs e)
        {
            if (_objectSettingsListbox.SelectedItem != null)
                _objectSettingsListbox.Items.Remove(_objectSettingsListbox.SelectedItem);
        }

        void _addFieldButton_ButtonClicked(object sender, EventArgs e)
        {
            Setting objectSetting = new Setting();
            objectSetting.Name = _settingNameInput.Text;
            objectSetting.Value = _settingValueInput.Text;
            _objectSettingsListbox.Items.Add(objectSetting);
        }

        public override void Show(bool modal)
        {
            base.Show(modal);

            Center();
        }

        void Mirror_IsSelectedChanged(object sender, EventArgs e)
        {
            if (_mirrorHorizCheck.IsSelected && _mirrorVertCheck.IsSelected)
                _characterPresenter[_characterPresenter.Width - 2, 0].SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically | Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            else if (_mirrorHorizCheck.IsSelected)
                _characterPresenter[_characterPresenter.Width - 2, 0].SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            else if (_mirrorVertCheck.IsSelected)
                _characterPresenter[_characterPresenter.Width - 2, 0].SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            else
                _characterPresenter[_characterPresenter.Width - 2, 0].SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

            _characterPicker.MirrorEffect = _characterPicker.MirrorEffect = _settingMirrorEffect = _characterPresenter[_characterPresenter.Width - 2, 0].SpriteEffect;
        }

        public override void Hide()
        {
            if (DialogResult)
            {
                // Fill in the game object with all the new values
                _gameObject.Name = _nameInput.Text;
                _gameObject.Character.Foreground = _foregroundPresenter.SelectedColor;
                _gameObject.Character.Background = _backgroundPresenter.SelectedColor;
                _gameObject.Character.CharacterIndex = _characterPicker.SelectedCharacter;
                _gameObject.Character.SpriteEffect = _settingMirrorEffect;
            }

            base.Hide();
        }

        public override void Redraw()
        {
            base.Redraw();

            _cellData.Print(2, 2, "Name", Settings.Green);
            _cellData.Print(2, 10, "Settings/Flags", Settings.Green);
            _cellData.Print(2, 19, "Setting Name", Settings.Green);
            _cellData.Print(2, 22, "Setting Value", Settings.Green);

            _cellData.SetCharacter(0, 9, 199);
            for (int i = 1; i < 20; i++)
                _cellData.SetCharacter(i, 9, 196);

            _cellData.SetCharacter(20, 9, 191);

            for (int i = 10; i < 18; i++)
                _cellData.SetCharacter(20, i, 179);

            _cellData.SetCharacter(20, 18, 192);

            for (int i = 21; i < _cellData.Width; i++)
                _cellData.SetCharacter(i, 18, 196);

            _cellData.SetCharacter(_cellData.Width - 1, 18, 182);

            // Long line under field
            _cellData.SetCharacter(0, 24, 199);
            for (int i = 1; i < _cellData.Width; i++)
                _cellData.SetCharacter(i, 24, 196);
            _cellData.SetCharacter(_cellData.Width - 1, 24, 182);
        }
    }
}
