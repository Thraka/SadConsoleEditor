using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Windows
{
    class AnimationSpeedPopup : SadConsole.Consoles.Window
    {
        private Button _okButton;
        private Button _cancelButton;
        private InputBox _textBox;

        public float NewSpeed { get { return float.Parse(_textBox.Text); } }

        public AnimationSpeedPopup(float speed) : base(22, 6)
        {
            Title = "Animation Speed";

            _okButton = new Button(8, 1);
            _cancelButton = new Button(8, 1);
            _textBox = new InputBox(_cellData.Width - 4);
            _textBox.IsNumeric = true;
            _textBox.AllowDecimal = true;
            _textBox.Text = speed.ToString();

            _okButton.Position = new Microsoft.Xna.Framework.Point(_cellData.Width - _okButton.Width - 2, _cellData.Height - 2);
            _cancelButton.Position = new Microsoft.Xna.Framework.Point(2, _cellData.Height - 2);
            _textBox.Position = new Microsoft.Xna.Framework.Point(2, 2);

            _okButton.ButtonClicked += (o, e) => { DialogResult = true; Hide(); };
            _cancelButton.ButtonClicked += (o, e) => { DialogResult = false; Hide(); };

            _okButton.Text = "Ok";
            _cancelButton.Text = "Cancel";

            Add(_okButton);
            Add(_cancelButton);
            Add(_textBox);
        }
    }
}
