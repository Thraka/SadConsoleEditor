using Microsoft.Xna.Framework;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SadConsoleEditor.Windows
{
    public class NewConsolePopup : SadConsole.Consoles.Window
    {
        #region Fields
        private Button _okButton;
        private Button _cancelButton;
        private ListBox _editorsListBox;

        private InputBox _widthBox;
        private InputBox _heightBox;

        private Controls.ColorPresenter _backgroundPicker;
        private Controls.ColorPresenter _foregroundPicker;
        //private InputBox _name;
        #endregion

        #region Properties
        public int SettingHeight { get; private set; }
        public int SettingWidth { get; private set; }
        public Editors.IEditor Editor { get; private set; }

        public Color SettingForeground { get { return _foregroundPicker.SelectedColor; } }
        public Color SettingBackground { get { return _backgroundPicker.SelectedColor; } }
        #endregion

        public NewConsolePopup() : base(30, 14)
        {
            //this.DefaultShowPosition = StartupPosition.CenterScreen;
            Title = "New Console";

            _cellData.DefaultBackground = Settings.Color_MenuBack;
            _cellData.DefaultForeground = Settings.Color_TitleText;
            _cellData.Clear();
            Redraw();

            _okButton = new Button(8, 1)
            {
                Text = "Accept",
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 10, 12)
            };
            _okButton.ButtonClicked += new EventHandler(_okButton_Action);

            _cancelButton = new Button(8, 1)
            {
                Text = "Cancel",
                Position = new Microsoft.Xna.Framework.Point(2, 12)
            };
            _cancelButton.ButtonClicked += new EventHandler(_cancelButton_Action);

            //Print(2, 3, "Name");
            CellData.Print(2, 2, "Editor");
            CellData.Print(2, 7, "Width");
            CellData.Print(2, 8, "Height");

            _editorsListBox = new ListBox(19, 4)
            {
                Position = new Point(9, 2),
                HideBorder = true
            };
            _editorsListBox.SelectedItemChanged += editorsListBox_SelectedItemChanged;

            _widthBox = new InputBox(3)
            {
                Text = "0",
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 5, 7)
            };

            _heightBox = new InputBox(3)
            {
                Text = "0",
                MaxLength = 3,
                IsNumeric = true,
                Position = new Microsoft.Xna.Framework.Point(base.CellData.Width - 5, 8)
            };

            //_name = new InputBox(20)
            //{
            //    Text = name,
            //    Position = new Microsoft.Xna.Framework.Point(9, 3)
            //};

            _foregroundPicker = new SadConsoleEditor.Controls.ColorPresenter("Foreground", Theme.FillStyle.Foreground, _cellData.Width - 4);
            _foregroundPicker.Position = new Point(2, 9);
            _foregroundPicker.SelectedColor = Color.White;

            _backgroundPicker = new SadConsoleEditor.Controls.ColorPresenter("Background", Theme.FillStyle.Foreground, _cellData.Width - 4);
            _backgroundPicker.Position = new Point(2, 10);
            _backgroundPicker.SelectedColor = Color.Black;

            Add(_editorsListBox);
            Add(_widthBox);
            Add(_heightBox);
            Add(_cancelButton);
            Add(_okButton);
            Add(_foregroundPicker);
            Add(_backgroundPicker);
            //Add(_name);

            foreach (var editor in EditorConsoleManager.Instance.Editors)
                _editorsListBox.Items.Add(editor.Value);

            _editorsListBox.SelectedItem = _editorsListBox.Items[0];
        }

        private void editorsListBox_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            Editor = (Editors.IEditor)_editorsListBox.SelectedItem;
            _widthBox.Text = Editor.Settings.DefaultWidth.ToString();
            _heightBox.Text = Editor.Settings.DefaultHeight.ToString();
            _foregroundPicker.SelectedColor = Editor.Settings.DefaultForeground;
            _backgroundPicker.SelectedColor = Editor.Settings.DefaultBackground;
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
