using Microsoft.Xna.Framework;
using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Panels
{
    class FilesPanel : CustomPanel
    {
        private Button _newButton;
        private Button _loadButton;
        private Button _saveButton;
        private Button _resizeButton;

        public FilesPanel()
        {
            Title = "File";

            _newButton = new Button(8, 1)
            {
                Text = " New",
                TextAlignment = System.Windows.HorizontalAlignment.Left,
                CanUseKeyboard = false,
            };
            _newButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.ShowNewConsolePopup(true);

            _loadButton = new Button(8, 1)
            {
                Text = "Load",
            };
            _loadButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.LoadSurface();

            _saveButton = new Button(8, 1)
            {
                Text = "Save",
            };
            _saveButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.SaveSurface();

            _resizeButton = new Button(8, 1)
            {
                Text = "Resize",
            };
            _resizeButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.ShowResizeConsolePopup();


            Controls = new ControlBase[] { _newButton, _loadButton, _saveButton, _resizeButton };
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == _newButton)
                _newButton.Position = new Point(1, _newButton.Position.Y);
            else if (control == _loadButton)
            {
                _loadButton.Position = new Point(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 8, _newButton.Position.Y);
                return -1;
            }
            else if (control == _saveButton)
                _saveButton.Position = new Point(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 8, _saveButton.Position.Y);
            else if (control == _resizeButton)
            {
                _resizeButton.Position = new Point(1, _saveButton.Position.Y);
                return -1;
            }
            
            return 0;
        }

        public override void Loaded()
        {
            
        }
    }
}
