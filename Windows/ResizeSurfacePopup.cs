using Microsoft.Xna.Framework;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SadConsoleEditor.Windows
{
    public class ResizeSurfacePopup: SadConsole.Consoles.Window
    {
        #region Fields
        private Button _okButton;
        private Button _cancelButton;

        private InputBox _widthBox;
        private InputBox _heightBox;
        //private InputBox _name;
        #endregion

        #region Properties
        public int SettingHeight { get; private set; }
        public int SettingWidth { get; private set; }
        #endregion

        public ResizeSurfacePopup(int width, int height)
            : base(22, 7)
        {
            //this.DefaultShowPosition = StartupPosition.CenterScreen;
            Title = "Resize";

            _cellData.DefaultBackground = Settings.Color_MenuBack;
            _cellData.DefaultForeground = Settings.Color_TitleText;

            _okButton = new Button(8, 1)
            {
                Text = "Accept",
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 10, 5)
            };
            _okButton.ButtonClicked += new EventHandler(_okButton_Action);

            _cancelButton = new Button(8, 1)
            {
                Text = "Cancel",
                Position = new Microsoft.Xna.Framework.Point(2, 5)
            };
            _cancelButton.ButtonClicked += new EventHandler(_cancelButton_Action);

            //Print(2, 3, "Name");
            CellData.Print(2, 2, "Width");
            CellData.Print(2, 3, "Height");

            _widthBox = new InputBox(3)
            {
                Text = width.ToString(),
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 5, 2)
            };

            _heightBox = new InputBox(3)
            {
                Text = height.ToString(),
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 5, 3)
            };

            //_name = new InputBox(20)
            //{
            //    Text = name,
            //    Position = new Microsoft.Xna.Framework.Point(9, 3)
            //};

            Add(_widthBox);
            Add(_heightBox);
            Add(_cancelButton);
            Add(_okButton);
            //Add(_name);
        }

        void _cancelButton_Action(object sender, EventArgs e)
        {
            DialogResult = false;
            Hide();
        }

        void _okButton_Action(object sender, EventArgs e)
        {
            DialogResult = true;

            int width = int.Parse(_widthBox.Text);
            int height = int.Parse(_heightBox.Text);

            SettingWidth = width < 1 ? 1 : width;
            SettingHeight = height < 1 ? 1 : height;
            //Name = _name.Text;

            Hide();
        }
    }
}
