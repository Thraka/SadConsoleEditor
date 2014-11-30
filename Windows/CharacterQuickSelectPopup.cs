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
    class CharacterQuickSelectPopup : SadConsole.Consoles.Window
    {
        public int SelectedCharacter { get; private set; }

        public CharacterQuickSelectPopup(int character)
            : base(18, 18)
        {
            Center();
            Controls.CharacterPicker picker = new Controls.CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            picker.Position = new Point(1, 1);
            picker.SelectedCharacter = character;
            picker.SelectedCharacterChanged += (sender, e) =>
            {
                SelectedCharacter = e.NewCharacter;
                this.Hide();
            };
            Add(picker);

            this.CloseOnESC = true;
            this.Title = "Pick a character";
        }
    }
}
