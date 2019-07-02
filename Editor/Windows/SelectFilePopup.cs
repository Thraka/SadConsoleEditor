﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsole.Themes;
using SadConsoleEditor.Controls;

namespace SadConsoleEditor.Windows
{
    public class SelectFilePopup : Window
    {
        private string currentFolder;
        private string fileFilterString;
        private Controls.FileDirectoryListbox directoryListBox;
        private TextBox fileName;
        private Button selectButton;
        private Button cancelButton;
        private ListBox fileLoadersList;

        public string CurrentFolder
        {
            get { return directoryListBox.CurrentFolder; }
            set { directoryListBox.CurrentFolder = value; }
        }

        public bool AllowCancel
        {
            set { cancelButton.IsEnabled = value; }
        }
        
        public string PreferredExtensions
        {
            get { return directoryListBox.HighlightedExtentions; }
            set { directoryListBox.HighlightedExtentions = value; }
        }

        public string SelectedFile { get; private set; }

        public FileLoaders.IFileLoader SelectedLoader { get; private set; }

        public bool SkipFileExistCheck { get; set; }

        public string SelectButtonText { get { return selectButton.Text; } set { selectButton.Text = value; } }

        public SelectFilePopup(string[] loaders)
            : base(70, 30)
        {
            fileLoadersList = new ListBox(15, Height - 6, new FileLoaderListBoxItem());
            fileLoadersList.Position = new Point(2, 4);
            fileLoadersList.SelectedItemChanged += FileLoadersList_SelectedItemChanged;

            foreach (var item in loaders)
                fileLoadersList.Items.Add(MainConsole.Instance.FileLoaders[item]);

            directoryListBox = new SadConsoleEditor.Controls.FileDirectoryListbox(Width - fileLoadersList.Bounds.Right - 3, Height - 10)
            {
                Position = new Point(fileLoadersList.Bounds.Right + 1, fileLoadersList.Bounds.Top),
            };

            directoryListBox.HighlightedExtentions = ".con;.console;.brush";
            directoryListBox.SelectedItemChanged += _directoryListBox_SelectedItemChanged;
            directoryListBox.SelectedItemExecuted += _directoryListBox_SelectedItemExecuted;
            directoryListBox.OnlyRootAndSubDirs = true;
            directoryListBox.CurrentFolder = Environment.CurrentDirectory;
            //directoryListBox.HideBorder = true;

            Print(directoryListBox.Bounds.Left, directoryListBox.Bounds.Top - 2, "Files", Theme.Colors.TitleText);
            Print(directoryListBox.Bounds.Left, directoryListBox.Bounds.Top - 1, new string((char)196, directoryListBox.Width));

            fileName = new TextBox(directoryListBox.Width)
            {
                Position = new Point(directoryListBox.Bounds.Left, directoryListBox.Bounds.Bottom + 2),
            };
            fileName.TextChanged += _fileName_TextChanged;
            Print(fileName.Bounds.Left, fileName.Bounds.Top - 1, "Selected file", Theme.Colors.TitleText);

            selectButton = new Button(8)
            {
                Text = "Open",
                Position = new Point(Width - 10, Height - 2),
                IsEnabled = false
            };
            selectButton.Click += new EventHandler(_selectButton_Action);

            cancelButton = new Button(8)
            {
                Text = "Cancel",
                Position = new Point(2, Height - 2)
            };
            cancelButton.Click += new EventHandler(_cancelButton_Action);

            Add(directoryListBox);
            Add(fileName);
            Add(selectButton);
            Add(cancelButton);
            Add(fileLoadersList);

            fileLoadersList.SelectedItem = fileLoadersList.Items[0];
            Title = "Select File";
        }

        private void FileLoadersList_SelectedItemChanged(object sender, ListBox.SelectedItemEventArgs e)
        {
            if (e.Item != null)
            {
                List<string> filters = new List<string>();
                foreach (var ext in ((FileLoaders.IFileLoader)e.Item).Extensions)
                    filters.Add($"*.{ext};");

                fileFilterString = string.Concat(filters);
                directoryListBox.FileFilter = fileFilterString;
                Print(fileName.Bounds.Left, fileName.Bounds.Bottom, new string(' ', Width - fileName.Bounds.Left - 1));
                Print(fileName.Bounds.Left, fileName.Bounds.Bottom, fileFilterString.Replace("*", "").Replace(";", " "));

                SelectedLoader = (FileLoaders.IFileLoader)e.Item;
            }
        }
        public override void Show(bool modal)
        {
            SelectedFile = "";
            fileLoadersList.SelectedItem = null;
            fileLoadersList.SelectedItem = fileLoadersList.Items[0];
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
                var rootDir = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
                var folder = directoryListBox.CurrentFolder.Remove(0, rootDir.Length);
                SelectedFile = System.IO.Path.Combine(folder, fileName.Text);
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

        public override void Invalidate()
        {
            base.Invalidate();

            //    Print(2, Height - 2, fileFilterString.Replace(';', ' ').Replace("*", ""));

            Print(fileLoadersList.Bounds.Left, fileLoadersList.Bounds.Top - 2, "Type of file", Theme.Colors.TitleText);
            Print(fileLoadersList.Bounds.Left, fileLoadersList.Bounds.Top - 1, new string((char)196, fileLoadersList.Width));

            Print(directoryListBox.Bounds.Left, directoryListBox.Bounds.Top - 2, "Files", Theme.Colors.TitleText);
            Print(directoryListBox.Bounds.Left, directoryListBox.Bounds.Top - 1, new string((char)196, directoryListBox.Width));

            Print(fileName.Bounds.Left, fileName.Bounds.Top - 1, "Selected file", Theme.Colors.TitleText);

            if (fileLoadersList.SelectedItem is FileLoaders.IFileLoader loader)
            {
                List<string> filters = new List<string>();
                foreach (var ext in loader.Extensions)
                    filters.Add($"*.{ext};");

                fileFilterString = string.Concat(filters);
                directoryListBox.FileFilter = fileFilterString;
                Print(fileName.Bounds.Left, fileName.Bounds.Bottom, new string(' ', Width - fileName.Bounds.Left - 1));
                Print(fileName.Bounds.Left, fileName.Bounds.Bottom, fileFilterString.Replace("*", "").Replace(";", " "));
            }
        }
    }
}
