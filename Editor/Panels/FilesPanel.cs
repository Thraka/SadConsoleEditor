using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Themes;

namespace SadConsoleEditor.Panels
{
    public class FilesPanel : CustomPanel
    {
        public Button NewButton;
        public Button LoadButton;
        public Button SaveButton;
        public Button ResizeButton;
        public Button CloseButton;

        private DrawingSurface documentsTitle;
        public ListBox DocumentsListbox;

        internal FilesPanel()
        {
            Title = "File";

            NewButton = new Button(7)
            {
                Text = "New",
                UseKeyboard = false,
            };
            NewButton.Click += (o, e) => MainConsole.Instance.ShowNewEditorPopup();

            LoadButton = new Button(8)
            {
                Text = "Load",
            };
            LoadButton.Click += (o, e) => MainConsole.Instance.ShowLoadEditorPopup();

            SaveButton = new Button(8)
            {
                Text = "Save",
            };
            SaveButton.Click += (o, e) => MainConsole.Instance.SaveEditor();

            ResizeButton = new Button(10)
            {
                Text = "Resize",
            };
            ResizeButton.Click += (o, e) => MainConsole.Instance.ShowResizeEditorPopup();

            CloseButton = new Button(9)
            {
                Text = "Close",
            };
            CloseButton.Click += (o, e) => MainConsole.Instance.ShowCloseConsolePopup();

            DocumentsListbox = new ListBox(Consoles.ToolPane.PanelWidthControls, 6, new EditorListBoxItem());
            DocumentsListbox.CompareByReference = true;

            DocumentsListbox.SelectedItemChanged += DocumentsListbox_SelectedItemChanged;

            documentsTitle = new DrawingSurface(13, 1);
            documentsTitle.OnDraw = (control) =>
            {
                var colors = control.GetThemeColors();
                control.Surface.Fill(colors.Green, colors.MenuBack, 0, null);
                control.Surface.Print(0, 0, new ColoredString("Opened Files", colors.Green, colors.MenuBack));
            };
            Controls = new ControlBase[] { NewButton, LoadButton, SaveButton, ResizeButton, CloseButton, documentsTitle, DocumentsListbox };
            

        }

        private void DocumentsListbox_SelectedItemChanged(object sender, ListBox.SelectedItemEventArgs e)
        {
            if (e.Item != null)
            {
                var editor = (Editors.IEditor)e.Item;

                if (MainConsole.Instance.ActiveEditor != editor)
                    MainConsole.Instance.ChangeActiveEditor(editor);
            }
        }

        public override void ProcessMouse(SadConsole.Input.MouseConsoleState info) { }

        public override void Loaded() { }

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

        public class EditorListBoxItem : ListBoxItemTheme
        {
            public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
            {
                var look = GetStateAppearance(itemState);
                string value = ((Editors.IEditor)item).Metadata.Title??((Editors.IEditor)item).Metadata.Id;
                if (value.Length < area.Width)
                    value += new string(' ', area.Width - value.Length);
                else if (value.Length > area.Width)
                    value = value.Substring(0, area.Width);
                surface.Print(area.Left, area.Top, value, look);
            }
        }
    }
}
