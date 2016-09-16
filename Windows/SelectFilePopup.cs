using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsole.Consoles;
namespace SadConsoleEditor.Windows
{
    class FileLoaderListBoxItem: ListBoxItem
    {
        public override void Draw(ITextSurface surface, Rectangle area)
        {
            string value = ((FileLoaders.IFileLoader)Item).FileTypeName;
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            var editor = new SurfaceEditor(surface);
            editor.Print(area.Left, area.Top, value, _currentAppearance);
            _isDirty = false;
        }
    }

    class SelectFilePopup : Window
    {
        #region Fields
        private string currentFolder;
        private string fileFilterString;
        private SadConsoleEditor.Controls.FileDirectoryListbox directoryListBox;
        private InputBox fileName;
        private Button selectButton;
        private Button cancelButton;
        private ListBox<FileLoaderListBoxItem> fileLoadersList;
        #endregion

        #region Properties
        public string CurrentFolder
        {
            get { return directoryListBox.CurrentFolder; }
            set { directoryListBox.CurrentFolder = value; }
        }

        public bool AllowCancel
        {
            set { cancelButton.IsEnabled = !value; }
        }

        public IEnumerable<FileLoaders.IFileLoader> FileLoaderTypes
        {
            set
            {
                fileLoadersList.Clear();

                foreach (var loader in value)
                    fileLoadersList.Items.Add(loader);
            }
        }

        public string PreferredExtensions
        {
            get { return directoryListBox.HighlightedExtentions; }
            set { directoryListBox.HighlightedExtentions = value; }
        }

        public string SelectedFile { get; private set; }

        public bool SkipFileExistCheck { get; set; }

        public string SelectButtonText { get { return selectButton.Text; } set { selectButton.Text = value; } }
        #endregion

        #region Constructors

        public SelectFilePopup()
            : base(70, 30)
        {
            Title = "Select File";
            directoryListBox = new SadConsoleEditor.Controls.FileDirectoryListbox(this.TextSurface.Width - 2, this.TextSurface.Height - 5)
            {
                Position = new Point(1, 1),
                HideBorder = true
            };
            directoryListBox.HighlightedExtentions = ".con;.console;.brush";
            directoryListBox.SelectedItemChanged += _directoryListBox_SelectedItemChanged;
            directoryListBox.SelectedItemExecuted += _directoryListBox_SelectedItemExecuted;

            fileName = new InputBox(this.TextSurface.Width - 11)
            {
                Position = new Point(2, this.TextSurface.Height - 3),
            };
            fileName.TextChanged += _fileName_TextChanged;

            selectButton = new Button(6, 1)
            {
                Text = "Open",
                Position = new Point(this.TextSurface.Width - 8, this.TextSurface.Height - 3),
                IsEnabled = false
            };
            selectButton.ButtonClicked += new EventHandler(_selectButton_Action);

            cancelButton = new Button(6, 1)
            {
                Text = "Cancel",
                Position = new Point(this.TextSurface.Width - 8, this.TextSurface.Height - 2)
            };
            cancelButton.ButtonClicked += new EventHandler(_cancelButton_Action);

            fileLoadersList = new ListBox<FileLoaderListBoxItem>(15, Height - 2);
            fileLoadersList.Position = new Point(1, 1);
            fileLoadersList.SelectedItemChanged += FileLoadersList_SelectedItemChanged;

            Add(directoryListBox);
            Add(fileName);
            Add(selectButton);
            Add(cancelButton);
            Add(fileLoadersList);
        }

        private void FileLoadersList_SelectedItemChanged(object sender, ListBox<FileLoaderListBoxItem>.SelectedItemEventArgs e)
        {
            if (e.Item != null)
            {
                List<string> filters = new List<string>();
                foreach (var ext in ((FileLoaders.IFileLoader)e.Item).Extensions)
                    filters.Add($"*.{ext};");

                fileFilterString = string.Concat(filters);
                directoryListBox.HighlightedExtentions = fileFilterString.Replace("*", "");
            }
        }
        #endregion

        public override void Show(bool modal)
        {
            SelectedFile = "";
            fileLoadersList.SelectedItem = null;
            fileLoadersList.SelectedItem = fileLoadersList.Items[0];
            Print(2, textSurface.Height - 2, fileFilterString.Replace(';', ' ').Replace("*", ""));
            base.Show(modal);
        }

        void _cancelButton_Action(object sender, EventArgs e)
        {
            DialogResult = false;
            Hide();
        }

        void _selectButton_Action(object sender, EventArgs e)
        {
            if (fileName.Text != string.Empty)
            {
                SelectedFile = System.IO.Path.Combine(directoryListBox.CurrentFolder, fileName.Text);

                var extensions = fileFilterString.Replace("*", "").Trim(';').Split(';');
                bool foundExtension = false;
                foreach (var item in extensions)
                {
                    if (SelectedFile.ToLower().EndsWith(item))
                    {
                        foundExtension = true;
                        break;
                    }
                }

                if (!foundExtension)
                    SelectedFile += extensions[0];

                DialogResult = true;
                Hide();
            }
        }

        void _directoryListBox_SelectedItemExecuted(object sender, SadConsoleEditor.Controls.FileDirectoryListbox.SelectedItemEventArgs e)
        {

        }

        void _directoryListBox_SelectedItemChanged(object sender, SadConsoleEditor.Controls.FileDirectoryListbox.SelectedItemEventArgs e)
        {
            if (e.Item is System.IO.FileInfo)
                fileName.Text = ((System.IO.FileInfo)e.Item).Name;
            else if (e.Item is SadConsoleEditor.Controls.HighlightedExtFile)
                fileName.Text = ((SadConsoleEditor.Controls.HighlightedExtFile)e.Item).Name;
            else
                fileName.Text = "";
        }

        void _fileName_TextChanged(object sender, EventArgs e)
        {
            selectButton.IsEnabled = fileName.Text != "" && (SkipFileExistCheck || System.IO.File.Exists(System.IO.Path.Combine(directoryListBox.CurrentFolder, fileName.Text)));
        }

        public override void Redraw()
        {
            base.Redraw();

            if (directoryListBox != null)
                Print(2, textSurface.Height - 2, fileFilterString.Replace(';', ' ').Replace("*", ""));
        }
    }
}
