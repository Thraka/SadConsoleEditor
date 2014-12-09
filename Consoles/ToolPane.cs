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
        private Dictionary<string, ITool> _tools;

        public ITool SelectedTool { get; private set; }
       
        private Button _newButton;
        private Button _loadButton;
        private Button _saveButton;
        private Button _resizeButton;

        public Tools.CharacterPickPane CommonCharacterPickerPanel;
        public Tools.LayersPanel LayersPanel;

        public static int PanelWidth;

        public ToolPane() : base(20, Game1.WindowSize.Y)
        {
            PanelWidth = 18;
            CommonCharacterPickerPanel = new CharacterPickPane("Settings", false, false, false);
            CanUseKeyboard = false;
        }

        public void FinishCreating()
        {
            _cellData.DefaultBackground = Settings.Color_MenuBack;
            _cellData.DefaultForeground = Settings.Color_TitleText;
            _cellData.Clear();

            _tools = new Dictionary<string, ITool>();
            _tools.Add(PaintTool.ID, new PaintTool());
            _tools.Add(RecolorTool.ID, new RecolorTool());
            _tools.Add(FillTool.ID, new FillTool());
            _tools.Add(TextTool.ID, new TextTool());
            _tools.Add(LineTool.ID, new LineTool());
            _tools.Add(BoxTool.ID, new BoxTool());
            _tools.Add(ObjectTool.ID, new ObjectTool());
            _tools.Add(CloneTool.ID, new CloneTool());

            SetupFilePanel();
            SetupToolsPanel();

            //_cellData.Print(1, 2, "  Paint Brush", Settings.Yellow);
            ProcessMouseWithoutFocus = true;
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
            _toolsListBox = new ListBox(20 - 2, 7);
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

                    LayersPanel = new Tools.LayersPanel();

                    EditorConsoleManager.Instance.AllowKeyboardToMoveConsole = true;
                    CommonCharacterPickerPanel.HideCharacter = false;
                    CommonCharacterPickerPanel.HideForeground = false;
                    CommonCharacterPickerPanel.HideBackground = false;
                    SelectedTool.OnSelected();
                    CommonCharacterPickerPanel.Reset();
                    RefreshControls();
                };

        }

        public void SetupEditor()
        {
            _toolsListBox.Items.Clear();

            foreach (var toolId in EditorConsoleManager.Instance.SelectedEditor.Tools)
                _toolsListBox.Items.Add(_tools[toolId]);

            _toolsListBox.SelectedItem = _tools.Values.First();
        }

        public void RefreshControls()
        {
            int activeRow;
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
            activeRow = RowToolSettings;

            // Layers panel
            LayersPanel.Loaded();
            _cellData.Print(1, activeRow++, LayersPanel.Title);
            _cellData.Print(0, activeRow++, new string((char)196, _cellData.Width));

            foreach (var control in LayersPanel.Controls)
            {
                Add(control);
                control.Position = new Point(1, activeRow);
                activeRow += LayersPanel.Redraw(control) + control.Height;
            }

            activeRow++;

            // Custom panels from the selected tool
            if (SelectedTool.ControlPanels != null)
            {
                foreach (var pane in SelectedTool.ControlPanels)
                {
                    pane.Loaded();
                    _cellData.Print(1, activeRow++, pane.Title);
                    _cellData.Print(0, activeRow++, new string((char)196, _cellData.Width));

                    foreach (var control in pane.Controls)
                    {
                        Add(control);
                        control.Position = new Point(1, activeRow);
                        activeRow += pane.Redraw(control) + control.Height;
                    }

                    activeRow += 1;
                }
            }
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            if (info.ScrollWheelValueChange != 0)
            {
                EditorConsoleManager.Instance.ScrollToolbox(info.ScrollWheelValueChange);
                return true;
            }

            return base.ProcessMouse(info);
        }

    }
}
