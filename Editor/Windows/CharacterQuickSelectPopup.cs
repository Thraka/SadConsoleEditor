using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Windows
{
    class CharacterQuickSelectPopup : SadConsole.Window
    {
        private Controls.CharacterPicker _picker;
        private int _selectedCharacter;

        public Microsoft.Xna.Framework.Graphics.SpriteEffects MirrorEffect { get { return _picker.MirrorEffect; } set { _picker.MirrorEffect = value; } }

        public int SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                _picker.SelectedCharacterChanged -= SelectedCharacterChangedOnControl;
                _selectedCharacter = _picker.SelectedCharacter = value;
                _picker.SelectedCharacterChanged += SelectedCharacterChangedOnControl;
            }
        }

        public CharacterQuickSelectPopup(int character)
            : base(18, 18)
        {
            Center();
            _picker = new Controls.CharacterPicker(SadConsole.Themes.Library.Default.Colors.OrangeDark, SadConsole.Themes.Library.Default.Colors.ControlBack, SadConsole.Themes.Library.Default.Colors.Yellow);
            _picker.Position = new Point(1, 1);
            _picker.SelectedCharacter = character;
            _picker.UseFullClick = true;
            _picker.SelectedCharacterChanged += SelectedCharacterChangedOnControl;
            Add(_picker);

            this.CloseOnEscKey = true;
            this.Title = "Pick a character";
        }

        private void SelectedCharacterChangedOnControl(object sender, CharacterPicker.SelectedCharacterEventArgs e)
        {
            SelectedCharacter = e.NewCharacter;
            this.Hide();
        }
    }
}
