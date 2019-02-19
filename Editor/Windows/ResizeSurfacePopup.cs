using Microsoft.Xna.Framework;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SadConsoleEditor.Windows
{
    public class ResizeSurfacePopup: SadConsole.Window
    {
        private Button okButton;
        private Button cancelButton;

        private TextBox widthBox;
        private TextBox heightBox;
        //private InputBox _name;

        public int SettingHeight { get; private set; }
        public int SettingWidth { get; private set; }

        public ResizeSurfacePopup(int width, int height)
            : base(22, 7)
        {
            //this.DefaultShowPosition = StartupPosition.CenterScreen;
            Title = "Resize";

            okButton = new Button(8)
            {
                Text = "Accept",
                Position = new Microsoft.Xna.Framework.Point(Width - 10, 5)
            };
            okButton.Click += new EventHandler(_okButton_Action);

            cancelButton = new Button(8)
            {
                Text = "Cancel",
                Position = new Microsoft.Xna.Framework.Point(2, 5)
            };
            cancelButton.Click += new EventHandler(_cancelButton_Action);

            //Print(2, 3, "Name");
            Print(2, 2, "Width");
            Print(2, 3, "Height");

            widthBox = new TextBox(3)
            {
                Text = width.ToString(),
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(Width - 5, 2)
            };

            heightBox = new TextBox(3)
            {
                Text = height.ToString(),
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(Width - 5, 3)
            };

            //_name = new InputBox(20)
            //{
            //    Text = name,
            //    Position = new Microsoft.Xna.Framework.Point(9, 3)
            //};

            Add(widthBox);
            Add(heightBox);
            Add(cancelButton);
            Add(okButton);
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

            int width = int.Parse(widthBox.Text);
            int height = int.Parse(heightBox.Text);

            SettingWidth = width < 1 ? 1 : width;
            SettingHeight = height < 1 ? 1 : height;
            //Name = _name.Text;

            Hide();
        }
    }
}
