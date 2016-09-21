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

        private Tools.ITool selectedTool;

        public Tools.ITool SelectedTool
        {
            get { return selectedTool; }
            set
            {
                ToolsListBox.SelectedItem = value;
            }
        }

        public event EventHandler SelectedToolChanged;

        public ToolsPanel()
        {
            Title = "Tools";

            ToolsListBox = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 7);
            ToolsListBox.HideBorder = true;
            ToolsListBox.CanUseKeyboard = false;

            ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            Controls = new ControlBase[] { ToolsListBox };
        }

        private void ToolsListBox_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            if (selectedTool != null)
            {
                selectedTool.OnDeselected();
                //if (selectedTool.ControlPanels != null)
                //    foreach (var pane in selectedTool.ControlPanels)
                //    {
                //        foreach (var control in pane.Controls)
                //        {
                //            Remove(control);
                //        }
                //    }
            }

            if (e.Item != null)
            {
                selectedTool = (ITool)e.Item;
                EditorConsoleManager.ToolName = selectedTool.Title;

                //EditorConsoleManager.AllowKeyboardToMoveConsole = true;

                Panels.CharacterPickPanel.SharedInstance.HideCharacter = false;
                Panels.CharacterPickPanel.SharedInstance.HideForeground = false;
                Panels.CharacterPickPanel.SharedInstance.HideBackground = false;

                selectedTool.OnSelected();
                Panels.CharacterPickPanel.SharedInstance.Reset();
            }
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
