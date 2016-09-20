using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
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
        public ListBox<EditorListBoxItem> DocumentsListbox;

        public FilesPanel()
        {
            Title = "File";

            NewButton = new Button(7, 1)
            {
                Text = "New",
                CanUseKeyboard = false,
            };
            NewButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowNewEditorPopup();

            LoadButton = new Button(8, 1)
            {
                Text = "Load",
            };
            LoadButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowLoadEditorPopup();

            SaveButton = new Button(8, 1)
            {
                Text = "Save",
            };
            SaveButton.ButtonClicked += (o, e) => EditorConsoleManager.SaveEditor();

            ResizeButton = new Button(10, 1)
            {
                Text = "Resize",
            };
            //ResizeButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowResizeConsolePopup();

            CloseButton = new Button(9, 1)
            {
                Text = "Close",
            };
            CloseButton.ButtonClicked += (o, e) => EditorConsoleManager.ShowCloseConsolePopup();

            DocumentsListbox = new ListBox<EditorListBoxItem>(Consoles.ToolPane.PanelWidth - 2, 6);
            DocumentsListbox.HideBorder = true;
            DocumentsListbox.CompareByReference = true;

            DocumentsListbox.SelectedItemChanged += DocumentsListbox_SelectedItemChanged;

            documentsTitle = new DrawingSurface(13, 1);
            documentsTitle.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
            documentsTitle.Print(0, 0, new ColoredString("Opened Files", Settings.Blue, Settings.Color_MenuBack));

            Controls = new ControlBase[] { NewButton, LoadButton, SaveButton, ResizeButton, CloseButton, documentsTitle, DocumentsListbox };
            

        }

        private void DocumentsListbox_SelectedItemChanged(object sender, ListBox<EditorListBoxItem>.SelectedItemEventArgs e)
        {
            if (e.Item != null)
            {
                var editor = (Editors.IEditor)e.Item;
                //EditorConsoleManager.Instance.ChangeEditor((Editors.IEditor)e.Item);
                if (EditorConsoleManager.ActiveEditor != editor)
                    EditorConsoleManager.ChangeActiveEditor(editor);
            }
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

        public class EditorListBoxItem : ListBoxItem
        {
            public override void Draw(ITextSurface surface, Rectangle area)
            {
                string value = ((Editors.IEditor)Item).EditorTypeName;
                if (value.Length < area.Width)
                    value += new string(' ', area.Width - value.Length);
                else if (value.Length > area.Width)
                    value = value.Substring(0, area.Width);
                var editor = new SurfaceEditor(surface);
                editor.Print(area.Left, area.Top, value, _currentAppearance);
                _isDirty = false;
            }
        }
    }
}
