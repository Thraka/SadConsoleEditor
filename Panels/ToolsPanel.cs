using SadConsole.Controls;
using SadConsoleEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Panels
{
    class ToolsPanel: CustomPanel
    {
        public ListBox ToolsListBox;

        public ToolsPanel()
        {
            Title = "Tools";

            ToolsListBox = new ListBox(20 - 2, 7);
            ToolsListBox.HideBorder = true;
            ToolsListBox.CanUseKeyboard = false;

            ToolsListBox.SelectedItemChanged += (sender, e) =>
            {
                if (EditorConsoleManager.Instance.ToolPane.SelectedTool != null)
                {
                    EditorConsoleManager.Instance.ToolPane.SelectedTool.OnDeselected();
                    if (EditorConsoleManager.Instance.ToolPane.SelectedTool.ControlPanels != null)
                        foreach (var pane in EditorConsoleManager.Instance.ToolPane.SelectedTool.ControlPanels)
                        {
                            foreach (var control in pane.Controls)
                            {
                                EditorConsoleManager.Instance.ToolPane.Remove(control);
                            }
                        }
                }
                if (e.Item != null)
                {
                    EditorConsoleManager.Instance.ToolPane.SelectedTool = (ITool)e.Item;

                    EditorConsoleManager.Instance.AllowKeyboardToMoveConsole = true;
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = false;
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideForeground = false;
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideBackground = false;

                    EditorConsoleManager.Instance.ToolPane.SelectedTool.OnSelected();
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Reset();
                    EditorConsoleManager.Instance.ToolPane.RefreshControls();
                }

            };

            Controls = new ControlBase[] { ToolsListBox };
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return 0;
        }

        public override void Loaded()
        {
        }
    }
}
