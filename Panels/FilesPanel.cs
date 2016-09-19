using Microsoft.Xna.Framework;
using SadConsole;
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

        private DrawingSurface documentsTitle;
        public ListBox DocumentsListbox;

        public FilesPanel()
        {
            Title = "File";

            NewButton = new Button(7, 1)
            {
                Text = " New",
                TextAlignment = System.Windows.HorizontalAlignment.Left,
                CanUseKeyboard = false,
            };
            //NewButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowNewConsolePopup(true);

            LoadButton = new Button(8, 1)
            {
                Text = "Load",
            };
            //LoadButton.ButtonClicked += (o, e) => EditorConsoleManager.LoadSurface();

            SaveButton = new Button(8, 1)
            {
                Text = "Save",
            };
            //SaveButton.ButtonClicked += (o, e) => EditorConsoleManager.SaveSurface();

            ResizeButton = new Button(10, 1)
            {
                Text = "Resize",
            };
            //ResizeButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowResizeConsolePopup();

            CloseButton = new Button(9, 1)
            {
                Text = "Close",
            };
            //CloseButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowCloseConsolePopup();

            DocumentsListbox = new ListBox(Consoles.ToolPane.PanelWidth - 2, 6);
            DocumentsListbox.HideBorder = true;
            DocumentsListbox.CompareByReference = true;

            DocumentsListbox.SelectedItemChanged += DocumentsListbox_SelectedItemChanged;

            documentsTitle = new DrawingSurface(13, 1);
            documentsTitle.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
            documentsTitle.Print(0, 0, new ColoredString("Opened Files", Settings.Blue, Settings.Color_MenuBack));

            Controls = new ControlBase[] { NewButton, LoadButton, SaveButton, ResizeButton, CloseButton, documentsTitle, DocumentsListbox };
            

        }

        private void DocumentsListbox_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            //EditorConsoleManager.Instance.ChangeEditor((Editors.IEditor)e.Item);
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
                LoadButton.Position = new Point(NewButton.Bounds.Right + 2, NewButton.Position.Y);
            }
            else if (control == SaveButton)
                SaveButton.Position = new Point(1, SaveButton.Position.Y);
            else if (control == ResizeButton)
            {
                ResizeButton.Position = new Point(SaveButton.Bounds.Right + 2, SaveButton.Position.Y);
                return -1;
            }
            else if (control == CloseButton)
            {
                CloseButton.Position = new Point(ResizeButton.Bounds.Right + 2, SaveButton.Position.Y);
            }
            else if (control == documentsTitle)
                return 1;

            return 0;
        }

        public override void Loaded()
        {
            
        }
    }
}
