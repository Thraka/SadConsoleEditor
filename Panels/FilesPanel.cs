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
        public Button NewButton;
        public Button LoadButton;
        public Button SaveButton;
        public Button ResizeButton;
        public Button CloseButton;

        public FilesPanel()
        {
            Title = "File";

            NewButton = new Button(8, 1)
            {
                Text = " New",
                TextAlignment = System.Windows.HorizontalAlignment.Left,
                CanUseKeyboard = false,
            };
            NewButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.ShowNewConsolePopup(true);

            LoadButton = new Button(8, 1)
            {
                Text = "Import",
            };
            LoadButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.LoadSurface();

            SaveButton = new Button(8, 1)
            {
                Text = "Save",
            };
            SaveButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.SaveSurface();

            ResizeButton = new Button(8, 1)
            {
                Text = "Resize",
            };
            ResizeButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.ShowResizeConsolePopup();

            CloseButton = new Button(8, 1)
            {
                Text = " Close",
                TextAlignment = System.Windows.HorizontalAlignment.Left
            };
            CloseButton.ButtonClicked += (o, e) => EditorConsoleManager.Instance.ShowCloseConsolePopup();

            Controls = new ControlBase[] { NewButton, LoadButton, SaveButton, ResizeButton, CloseButton };
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == NewButton)
                NewButton.Position = new Point(1, NewButton.Position.Y);
            else if (control == LoadButton)
            {
                LoadButton.Position = new Point(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 8, NewButton.Position.Y);
                return -1;
            }
            else if (control == SaveButton)
                SaveButton.Position = new Point(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 8, SaveButton.Position.Y);
            else if (control == ResizeButton)
            {
                ResizeButton.Position = new Point(1, SaveButton.Position.Y);
                return -1;
            }
            
            return 0;
        }

        public override void Loaded()
        {
            
        }
    }
}
