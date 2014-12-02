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
using SadConsole.Controls;
using SadConsoleEditor.Controls;

namespace SadConsoleEditor.Consoles
{
    class ToolPane : ControlsConsole
    {
        private int RowFile = 1;
        private int RowTools = 6;
        private int RowToolSettings = 13;

        private ListBox _toolsListBox;
        private ColorPickerPopup _colorPicker;
        private bool _colorPickerModeForeground;

        private Dictionary<string, ITool> _tools;

        public ITool SelectedTool { get; private set; }

        public EventHandler BrushChanged;


        #region ColorCharacterArea
        private int _charTextRow;
        private int _charBackTextRow;
        private int _charForeTextRow;
        private bool _showCharacterList = true;
        private Color _charForegroundColor = Color.Red;
        private Color _charBackgroundColor = Color.Black;
        private int _selectedChar = 1;

        private Button _newButton;
        private Button _loadButton;
        private Button _saveButton;
        private Button _resizeButton;
        private CharacterPicker _picker;

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

            _tools = new Dictionary<string, ITool>();
            _tools.Add(PaintTool.ID, new PaintTool());
            _tools.Add(FillTool.ID, new FillTool());
            _tools.Add(TextTool.ID, new TextTool());
            _tools.Add(LineTool.ID, new LineTool());
            _tools.Add(BoxTool.ID, new BoxTool());
            _tools.Add(ObjectTool.ID, new ObjectTool());
            _tools.Add(CloneTool.ID, new CloneTool());

            SetupFilePanel();
            SetupToolsPanel();
            SetupToolsSettingsPane();

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
            _newButton = new Button(8, 1)
            {
                Text = " New",
                TextAlignment = System.Windows.HorizontalAlignment.Left,
                CanUseKeyboard = false,
            };
            _newButton.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.ShowNewConsolePopup();
            };
            Add(_newButton);

            _loadButton = new Button(8, 1)
            {
                Text = "Load",
            };
            _loadButton.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.LoadSurface();
            };
            Add(_loadButton);

            _saveButton = new Button(8, 1)
            {
                Text = "Save",
            };
            _saveButton.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.SaveSurface();
            };
            Add(_saveButton);

            _resizeButton = new Button(8, 1)
            {
                Text = "Resize",
            };
            _resizeButton.ButtonClicked += (o, e) =>
            {
                EditorConsoleManager.Instance.ShowResizeConsolePopup();
            };
            Add(_resizeButton);
        }

        private void SetupToolsPanel()
        {
            _toolsListBox = new ListBox(20 - 2, 4);
            _toolsListBox.HideBorder = true;
            _toolsListBox.CanUseKeyboard = false;
            Add(_toolsListBox);

            _toolsListBox.SelectedItemChanged += (sender, e) =>
                {
                    if (SelectedTool != null)
                    {
                        SelectedTool.OnDeselected();
                        if (SelectedTool.ControlPanels != null)
                            foreach (var pane in SelectedTool.ControlPanels)
                            {
                                foreach (var control in pane.Controls)
                                {
                                    this.Remove(control);
                                }
                            }
                    }
                    SelectedTool = (ITool)e.Item;

                    SelectedTool.OnSelected();
                    RefreshControls();
                };
        }

        private void SetupToolsSettingsPane()
        {
            _picker = new CharacterPicker(Settings.Red, Settings.Color_ControlBack, Settings.Green);
            _picker.SelectedCharacterChanged += (sender, e) =>
            {
                SelectedCharacter = e.NewCharacter;

                DrawCharacterState();
            };
            Add(_picker);
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
            else if (info.ConsoleLocation.Y == _charTextRow && info.ConsoleLocation.X >= _cellData.Width - 5 && info.ConsoleLocation.X < _cellData.Width - 2)
            {
                CharacterQuickSelectPopup popup = new CharacterQuickSelectPopup(SelectedCharacter);
                popup.Show(true);
            }
        }

        public void SetupEditor()
        {
            _toolsListBox.Items.Clear();

            foreach (var toolId in EditorConsoleManager.Instance.SelectedEditor.Tools)
                _toolsListBox.Items.Add(_tools[toolId]);

            _toolsListBox.SelectedItem = _tools.Values.First();
        }

        private void RefreshControls()
        {
            _cellData.Clear();

            RowFile = 0;
            _cellData.Print(1, RowFile, "File");
            _cellData.Print(0, RowFile + 1, new string((char)196, _cellData.Width));
            _newButton.Position = new Point(1, RowFile + 2);
            _loadButton.Position = new Point(_cellData.Width - 9, RowFile + 2);
            _saveButton.Position = new Point(_cellData.Width - 9, RowFile + 3);
            _resizeButton.Position = new Point(1, RowFile + 3);

            RowTools = RowFile + 5;
            _cellData.Print(1, RowTools, "Tools");
            _cellData.Print(0, RowTools + 1, new string((char)196, _cellData.Width));
            _toolsListBox.Position = new Point(1, RowTools + 2);

            RowToolSettings = _toolsListBox.Position.Y + _toolsListBox.Height + 1;
            int activeRow = RowToolSettings;
            _cellData.Print(1, activeRow, "Console Editor");
            _cellData.Print(0, ++activeRow, new string((char)196, _cellData.Width));
            _cellData.Print(1, ++activeRow, "Foreground", Settings.Green);
            _charForeTextRow = activeRow;
            _cellData.Print(1, ++activeRow, "Background", Settings.Green);
            _charBackTextRow = activeRow;

            _cellData.Print(1, ++activeRow, "Character", Settings.Green);
            _charTextRow = activeRow;

            activeRow += 2;
            _picker.Position = new Point(2, activeRow);

            DrawCharacterState();

            activeRow = _picker.Position.Y + _picker.Height;
            if (SelectedTool.ControlPanels != null)
            {
                foreach (var pane in SelectedTool.ControlPanels)
                {
                    _cellData.Print(1, ++activeRow, pane.Title);
                    _cellData.Print(0, ++activeRow, new string((char)196, _cellData.Width));
                    activeRow++;

                    foreach (var control in pane.Controls)
                    {
                        Add(control);
                        control.Position = new Point(1, activeRow);
                        activeRow += pane.Redraw(control) + 1;
                    }

                    activeRow += 2;
                }
            }
        }
    }
}
