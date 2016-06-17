using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SadConsole.Controls;
using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Consoles;

namespace SadEditor.Controls
{
    internal class FileDirectoryListbox : ListBox<FileDirectoryListboxItem>
    {
        #region Fields
        private string _currentFolder;
        private string _extFilter = "*.*";
        #endregion

        #region Properties
        public string CurrentFolder
        {
            get { return _currentFolder; }
            set
            {
                if (DisplayFolder(value))
                    _currentFolder = value;
            }
        }

        public string FileFilter
        {
            get { return _extFilter; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _extFilter = "*.*";
                else
                    _extFilter = value;

                DisplayFolder(_currentFolder);
            }
        }

        public bool HideNonFilterFiles { get; set; }

        public string HighlightedExtentions { get; set; }
        #endregion

        #region Constructors
        public FileDirectoryListbox(int width, int height) : base(width, height)
        {
            HighlightedExtentions = "";
        }
        #endregion

        #region Methods
        private bool DisplayFolder(string folder)
        {
            if (System.IO.Directory.Exists(folder))
            {
                try
                {
                    List<object> newItems = new List<object>(20);
                    var dir = new System.IO.DirectoryInfo(folder);

                    if (dir.Parent != null)
                        newItems.Add(new FauxDirectory { Name = ".." });

                    foreach (var item in System.IO.Directory.GetDirectories(folder))
                        newItems.Add(new System.IO.DirectoryInfo(item));
                    var highlightExts = HighlightedExtentions.Split(';');
                    var filterExts = _extFilter.Split(';');

                    foreach (var filter in filterExts)
                    {
                        foreach (var item in System.IO.Directory.GetFiles(folder, filter))
                        {
                            var fileInfo = new System.IO.FileInfo(item);


                            if (highlightExts.Contains(fileInfo.Extension))
                                newItems.Add(new HighlightedExtFile() { Name = fileInfo.Name });
                            else
                                newItems.Add(fileInfo);
                        }
                    }
                    

                    base.Items.Clear();

                    foreach (var item in newItems)
                        base.Items.Add(item);

                    return true;
                }
                catch (Exception e1)
                {
                    return false;
                }
            }
            else
                return false;
        }

        protected override void OnItemAction()
        {
            base.OnItemAction();

            if (_selectedItem is System.IO.DirectoryInfo)
            {
                CurrentFolder = ((System.IO.DirectoryInfo)_selectedItem).FullName;
                if (Items.Count > 0)
                    SelectedItem = Items[0];
            }
            else if (_selectedItem is FauxDirectory)
            {
                if (((FauxDirectory)_selectedItem).Name == "..")
                {
                    CurrentFolder = System.IO.Directory.GetParent(_currentFolder).FullName;
                    if (Items.Count > 0)
                        SelectedItem = Items[0];
                }
            }
            else if (_selectedItem is System.IO.FileInfo)
            {

            }
        }

        #endregion
    }

    #region Classes

    internal class FauxDirectory
    {
        public string Name;
    }

    internal class HighlightedExtFile
    {
        public string Name;
    }

    internal class FileDirectoryListboxItem : ListBoxItem
    {
        private string _displayString;
        private CellAppearance _directoryAppNormal = new CellAppearance(Color.Purple, Color.Transparent);
        private CellAppearance _directoryAppMouseOver = new CellAppearance(Color.Purple, new Color(30, 30, 30));
        private CellAppearance _directoryAppSelected = new CellAppearance(new Color(255, 0, 255), Color.Transparent);
        private CellAppearance _directoryAppSelectedOver = new CellAppearance(new Color(255, 0, 255), new Color(30, 30, 30));
        private CellAppearance _fileAppNormal = new CellAppearance(Color.Gray, Color.Transparent);
        private CellAppearance _fileAppMouseOver = new CellAppearance(Color.Gray, new Color(30, 30, 30));
        private CellAppearance _fileAppSelected = new CellAppearance(Color.White, Color.Transparent);
        private CellAppearance _fileAppSelectedOver = new CellAppearance(Color.White, new Color(30, 30, 30));
        private CellAppearance _highExtAppNormal = new CellAppearance(ColorAnsi.Yellow, Color.Transparent);
        private CellAppearance _highExtAppMouseOver = new CellAppearance(ColorAnsi.Yellow, new Color(30, 30, 30));
        private CellAppearance _highExtAppSelected = new CellAppearance(Color.Yellow, Color.Transparent);
        private CellAppearance _highExtAppSelectedOver = new CellAppearance(Color.Yellow, new Color(30, 30, 30));

        protected override void DetermineAppearance()
        {
            var oldAppearance = base._currentAppearance;

            if (base.Item is System.IO.DirectoryInfo || base.Item is FauxDirectory)
            {
                if (_isMouseOver && _isSelected)
                    base._currentAppearance = _directoryAppSelectedOver;
                else if (_isMouseOver)
                    base._currentAppearance = _directoryAppMouseOver;
                else if (_isSelected)
                    base._currentAppearance = _directoryAppSelected;
                else
                    base._currentAppearance = _directoryAppNormal;
            }
            else if (base.Item is System.IO.FileInfo)
            {
                if (_isMouseOver && _isSelected)
                    base._currentAppearance = _fileAppSelectedOver;
                else if (_isMouseOver)
                    base._currentAppearance = _fileAppMouseOver;
                else if (_isSelected)
                    base._currentAppearance = _fileAppSelected;
                else
                    base._currentAppearance = _fileAppNormal;
            }
            else if (base.Item is HighlightedExtFile)
            {
                if (_isMouseOver && _isSelected)
                    base._currentAppearance = _highExtAppSelectedOver;
                else if (_isMouseOver)
                    base._currentAppearance = _highExtAppMouseOver;
                else if (_isSelected)
                    base._currentAppearance = _highExtAppSelected;
                else
                    base._currentAppearance = _highExtAppNormal;
            }

            if (oldAppearance != base._currentAppearance)
                IsDirty = true;
        }

        protected override void OnItemChanged(object oldItem, object newItem)
        {
            if (base.Item is System.IO.DirectoryInfo)
                _displayString = "<" + ((System.IO.DirectoryInfo)base.Item).Name + ">";
            else if (base.Item is FauxDirectory)
                _displayString = "<" + ((FauxDirectory)base.Item).Name + ">";
            else if (base.Item is System.IO.FileInfo)
                _displayString = ((System.IO.FileInfo)base.Item).Name;
            else if (base.Item is HighlightedExtFile)
                _displayString = ((HighlightedExtFile)base.Item).Name;

            DetermineAppearance();

            base.OnItemChanged(oldItem, newItem);

        }

        public override void Draw(ITextSurface surface, Rectangle area)
        {
            SadConsoleEditor.Settings.QuickEditor.TextSurface = surface;
            SadConsoleEditor.Settings.QuickEditor.Print(area.X, area.Y, _displayString.Align(System.Windows.HorizontalAlignment.Left, area.Width), base._currentAppearance);
            IsDirty = false;
        }


    }
    #endregion
}
