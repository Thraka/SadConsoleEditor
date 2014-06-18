using System;
using SadConsole;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using Microsoft.Xna.Framework;

namespace SadConsoleEditor.Consoles
{
    class ToolPane: ControlsConsole
    {
        private const int RowFile = 1;
        private const int RowEditors = 4;
        private const int RowTools = 9;
        private const int RowToolSettings = 15;

        #region ColorCharacterArea
        private int _charTextRow;
        private int _charBackTextRow;
        private int _charForeTextRow;
        private bool _showCharacterList = true;
        private Color _charForegroundColor = Color.Red;
        private Color _charBackgroundColor = Color.Black;
        private int _selectedChar = 1;
        
        public bool ShowCharacterList
        {
            get { return _showCharacterList; }
            set { _showCharacterList = value; }
        }

        public Color CharacterForegroundColor
        {
            get { return _charForegroundColor; }
            set { _charForegroundColor = value; }
        }

        public Color CharacterBackgroundColor
        {
            get { return _charBackgroundColor; }
            set { _charBackgroundColor = value; }
        }

        public int SelectedCharacter
        {
            get { return _selectedChar; }
            set { _selectedChar = value; }
        }
        #endregion

        public ToolPane(): base(20, Game1.WindowSize.Y)
        {
            _cellData.DefaultBackground = Settings.Color_MenuBack;
            _cellData.DefaultForeground = Settings.Color_TitleText;
            _cellData.Clear();

            SetupFilePanel();
            SetupEditorsPanel();
            SetupToolsPanel();
            SetupToolsSettingsPane();
            
            //_cellData.Print(1, 2, "  Paint Brush", Settings.Yellow);
            ProcessMouseWithoutFocus = true;
        }

        private void SetupFilePanel()
        {
            _cellData.Print(1, RowFile, "File");

            var button = new SadConsole.Controls.Button(7, 1)
            {
                Text = "New",
                Position = new Microsoft.Xna.Framework.Point(1, RowFile + 1)
            };
            Add(button);

            button = new SadConsole.Controls.Button(8, 1)
            {
                Text = "Load",
                Position = new Microsoft.Xna.Framework.Point(_cellData.Width - 9, RowFile + 1)
            };
            Add(button);

            button = new SadConsole.Controls.Button(8, 1)
            {
                Text = "Save",
                Position = new Microsoft.Xna.Framework.Point(_cellData.Width - 9, RowFile + 2)
            };
            Add(button);
        }
        private void SetupEditorsPanel()
        {
            _cellData.Print(1, RowEditors, "Editors");

            var toolsListBox = new SadConsole.Controls.ListBox(20 - 2, 3);
            toolsListBox.Position = new Microsoft.Xna.Framework.Point(1, RowEditors + 1);
            this.Add(toolsListBox);
            toolsListBox.Items.Add("test 1");
            toolsListBox.Items.Add("test 2");
            toolsListBox.Items.Add("test 3");
            toolsListBox.Items.Add("test 4");
            toolsListBox.Items.Add("test 5");
            toolsListBox.Items.Add("test 6");
            toolsListBox.Items.Add("test 7");
            toolsListBox.HideBorder = true;
        }

        private void SetupToolsPanel()
        {
            _cellData.Print(1, RowTools, "Tools");

            var toolsListBox = new SadConsole.Controls.ListBox(20 - 2, 4);
            toolsListBox.Position = new Microsoft.Xna.Framework.Point(1, RowTools + 1);
            this.Add(toolsListBox);
            toolsListBox.Items.Add("test 1");
            toolsListBox.Items.Add("test 2");
            toolsListBox.Items.Add("test 3");
            toolsListBox.Items.Add("test 4");
            toolsListBox.Items.Add("test 5");
            toolsListBox.Items.Add("test 6");
            toolsListBox.Items.Add("test 7");
            toolsListBox.HideBorder = true;
        }

        private void SetupToolsSettingsPane()
        {

            int activeRow = RowToolSettings;
            _cellData.Print(1, activeRow,   "Console Editor");
            _cellData.Print(0, ++activeRow, new string((char)196, _cellData.Width));
            _cellData.Print(1, ++activeRow, "Foreground", Settings.Green);
            _charForeTextRow = activeRow;
            _cellData.Print(1, ++activeRow, "Background", Settings.Green);
            _charBackTextRow = activeRow;

            //activeRow += 1;

            _cellData.Print(1, ++activeRow, "Character", Settings.Green);
            _charTextRow = activeRow;

            activeRow += 2;
            
            SadConsoleEditor.Controls.CharacterPicker picker = new Controls.CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            picker.Position = new Microsoft.Xna.Framework.Point(2, activeRow);
            picker.SelectedCharacterChanged += (sender, e) =>
            {
                _selectedChar = e.NewCharacter;

                DrawCharacterState();
            };
            Add(picker);
            DrawCharacterState();
        }

        private void DrawCharacterState()
        {
            string charString = _selectedChar.ToString() + "    ";

            ColoredString coloredString = new ColoredString("   ", _charBackgroundColor, _charBackgroundColor, null);

            coloredString[1].Character = (char)_selectedChar;
            coloredString[1].Foreground = _charForegroundColor;

            _cellData.Print(11, _charTextRow, charString);
            _cellData.Print(_cellData.Width - 5, _charTextRow, coloredString);

            _cellData.Print(_cellData.Width - 5, _charForeTextRow, "   ", Color.Black, _charForegroundColor);
            _cellData.Print(_cellData.Width - 5, _charBackTextRow, "   ", Color.Black, _charBackgroundColor);

        }

        protected override void OnMouseLeftClicked(SadConsole.Input.MouseInfo info)
        {
            base.OnMouseLeftClicked(info);

            if (info.ConsoleLocation.X == _cellData.Width - 5 && info.ConsoleLocation.Y == _charBackTextRow)
            {
                Window a = new Window(50, 30);
                a.Add(new Controls.ColorPicker() { Position = new Point(1, 1) });
                a.Show(true);
            }
        }
    }
}
