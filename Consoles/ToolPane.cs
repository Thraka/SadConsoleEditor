using System;
using SadConsole;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Editors;
using SadConsoleEditor.Tools;
using SadConsoleEditor.Windows;

namespace SadConsoleEditor.Consoles
{
    class ToolPane : ControlsConsole
    {
        private const int RowFile = 1;
        private const int RowEditors = 4;
        private const int RowTools = 9;
        private const int RowToolSettings = 15;

        private SadConsole.Controls.ListBox _editorsListBox;
        private SadConsole.Controls.ListBox _toolsListBox;
        private ColorPickerPopup _colorPicker;
        private bool _colorPickerModeForeground;

        private Dictionary<string, IEditor> _editors;
        private Dictionary<string, ITool> _tools;

        public ITool SelectedTool { get; private set; }
        public IEditor SelectedEditor { get; private set; }

        public EventHandler SelectedEditorChanged;

        public EventHandler BrushChanged;

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
            set
            {
                if (_charForegroundColor != value)
                {
                    _charForegroundColor = value;

                    if (BrushChanged != null)
                        BrushChanged(this, EventArgs.Empty);

                    SelectedTool.RefreshTool();
                }
            }
        }

        public Color CharacterBackgroundColor
        {
            get { return _charBackgroundColor; }
            set
            {
                if (_charBackgroundColor != value)
                {
                    _charBackgroundColor = value;

                    if (BrushChanged != null)
                        BrushChanged(this, EventArgs.Empty);

                    SelectedTool.RefreshTool();
                }
            }
        }

        public int SelectedCharacter
        {
            get { return _selectedChar; }
            set
            {
                if (_selectedChar != value)
                {
                    _selectedChar = value;

                    if (BrushChanged != null)
                        BrushChanged(this, EventArgs.Empty);

                    SelectedTool.RefreshTool();
                }
            }
        }
        #endregion

        public ToolPane() : base(20, Game1.WindowSize.Y)
        {
            CanUseKeyboard = false;
        }

        public void FinishCreating()
        {
            _cellData.DefaultBackground = Settings.Color_MenuBack;
            _cellData.DefaultForeground = Settings.Color_TitleText;
            _cellData.Clear();

            _editors = new Dictionary<string, IEditor>();
            _editors.Add(DrawingEditor.ID, new DrawingEditor());

            _tools = new Dictionary<string, ITool>();
            _tools.Add(PaintTool.ID, new PaintTool());
            _tools.Add(FillTool.ID, new FillTool());
            _tools.Add(TextTool.ID, new TextTool());
            _tools.Add(LineTool.ID, new LineTool());

            SetupFilePanel();
            SetupEditorsPanel();
            SetupToolsPanel();
            SetupToolsSettingsPane();

            _editorsListBox.SelectedItem = _editors.Values.First();

            //_cellData.Print(1, 2, "  Paint Brush", Settings.Yellow);
            ProcessMouseWithoutFocus = true;

            _colorPicker = new ColorPickerPopup();
            _colorPicker.Closed += (sender, e) =>
            {
                if (_colorPicker.DialogResult)
                {
                    if (_colorPickerModeForeground)
                        CharacterForegroundColor = _colorPicker.SelectedColor;
                    else
                        CharacterBackgroundColor = _colorPicker.SelectedColor;

                    DrawCharacterState();
                }
            };
        }

        private void SetupFilePanel()
        {
            _cellData.Print(1, RowFile, "File");

            var button = new SadConsole.Controls.Button(8, 1)
            {
                Text = " New",
                Position = new Point(1, RowFile + 1),
                TextAlignment = System.Windows.HorizontalAlignment.Left,
                CanUseKeyboard = false,
            };
            Add(button);

            button = new SadConsole.Controls.Button(8, 1)
            {
                Text = "Load",
                Position = new Point(_cellData.Width - 9, RowFile + 1)
            };
            button.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.LoadSurface();
            };
            Add(button);

            button = new SadConsole.Controls.Button(8, 1)
            {
                Text = "Save",
                Position = new Point(_cellData.Width - 9, RowFile + 2)
            };
            button.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.SaveSurface();
            };
            Add(button);

            button = new SadConsole.Controls.Button(8, 1)
            {
                Text = "Resize",
                Position = new Point(1, RowFile + 2)
            };
            button.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.ShowResizeConsolePopup();
            };
            Add(button);
        }

        private void SetupEditorsPanel()
        {
            _cellData.Print(1, RowEditors, "Editors");

            _editorsListBox = new SadConsole.Controls.ListBox(20 - 2, 3);
            _editorsListBox.Position = new Point(1, RowEditors + 1);
            _editorsListBox.HideBorder = true;
            Add(_editorsListBox);

            foreach (var editor in _editors.Values)
            {
                _editorsListBox.Items.Add(editor);
            }

            _editorsListBox.SelectedItemChanged += (sender, e) =>
                {
                    IEditor item = (IEditor)e.Item;

                    _toolsListBox.Items.Clear();

                    foreach (var toolId in item.Tools)
                        _toolsListBox.Items.Add(_tools[toolId]);

                    SelectedEditor = item;

                    _toolsListBox.SelectedItem = _tools.Values.First();

                    if (SelectedEditorChanged != null)
                        SelectedEditorChanged(this, EventArgs.Empty);
                };

        }

        private void SetupToolsPanel()
        {
            _cellData.Print(1, RowTools, "Tools");

            _toolsListBox = new SadConsole.Controls.ListBox(20 - 2, 4);
            _toolsListBox.Position = new Point(1, RowTools + 1);
            _toolsListBox.HideBorder = true;
            _toolsListBox.CanUseKeyboard = false;
            Add(_toolsListBox);

            _toolsListBox.SelectedItemChanged += (sender, e) =>
                {
                    if (SelectedTool != null)
                        SelectedTool.OnDeselected();

                    SelectedTool = (ITool)e.Item;

                    SelectedTool.OnSelected();
                };
        }

        private void SetupToolsSettingsPane()
        {

            int activeRow = RowToolSettings;
            _cellData.Print(1, activeRow, "Console Editor");
            _cellData.Print(0, ++activeRow, new string((char)196, _cellData.Width));
            _cellData.Print(1, ++activeRow, "Foreground", Settings.Green);
            _charForeTextRow = activeRow;
            _cellData.Print(1, ++activeRow, "Background", Settings.Green);
            _charBackTextRow = activeRow;

            //activeRow += 1;

            _cellData.Print(1, ++activeRow, "Character", Settings.Green);
            _charTextRow = activeRow;

            activeRow += 2;

            Controls.CharacterPicker picker = new Controls.CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            picker.Position = new Point(2, activeRow);
            picker.SelectedCharacterChanged += (sender, e) =>
            {
                SelectedCharacter = e.NewCharacter;

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

            if (info.ConsoleLocation.Y == _charBackTextRow && info.ConsoleLocation.X >= _cellData.Width - 5 && info.ConsoleLocation.X < _cellData.Width - 2)
            {
                _colorPickerModeForeground = false;
                _colorPicker.SelectedColor = CharacterBackgroundColor;
                _colorPicker.Show(true);
            }
            else if (info.ConsoleLocation.Y == _charForeTextRow && info.ConsoleLocation.X >= _cellData.Width - 5 && info.ConsoleLocation.X < _cellData.Width - 2)
            {
                _colorPickerModeForeground = true;
                _colorPicker.SelectedColor = CharacterForegroundColor;
                _colorPicker.Show(true);
            }
        }
    }
}
