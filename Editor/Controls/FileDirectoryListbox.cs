using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SadConsole.Controls;
using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Themes;

namespace SadConsoleEditor.Controls
{
    internal class FileDirectoryListbox : ListBox
    {
        private string _currentFolder = null;
        private string _extFilter = "*.*";
        private string originalRootFolder;

        public string CurrentFolder
        {
            get { return _currentFolder; }
            set
            {
                if (_currentFolder == null)
                    originalRootFolder = value;

                if (DisplayFolder(value))
                {
                    _currentFolder = value;
                }
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
                    _extFilter = value.ToLower();

                DisplayFolder(_currentFolder);
            }
        }

        public bool OnlyRootAndSubDirs { get; set; }

        public bool HideNonFilterFiles { get; set; }

        public string HighlightedExtentions { get; set; }

        public FileDirectoryListbox(int width, int height) : base(width, height, new FileDirectoryListboxItem())
        {
            HighlightedExtentions = "";
        }

        private bool DisplayFolder(string folder)
        {
            if (System.IO.Directory.Exists(folder))
            {
                try
                {
                    List<object> newItems = new List<object>(20);
                    var dir = new System.IO.DirectoryInfo(folder);

                    if (dir.Parent != null && (!OnlyRootAndSubDirs || (OnlyRootAndSubDirs && System.IO.Path.GetFullPath(folder).ToLower() != System.IO.Path.GetFullPath(originalRootFolder).ToLower())))
                        newItems.Add(new FauxDirectory { Name = ".." });

                    foreach (var item in System.IO.Directory.GetDirectories(folder))
                        newItems.Add(new System.IO.DirectoryInfo(item));
                    var highlightExts = HighlightedExtentions.Trim(';').Split(';');
                    var filterExts = _extFilter.Trim(';').Split(';');

                    foreach (var filter in filterExts)
                    {
                        foreach (var item in System.IO.Directory.GetFiles(folder, filter))
                        {
                            var fileInfo = new System.IO.FileInfo(item);


                            if (highlightExts.Contains(fileInfo.Extension.ToLower()))
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

            if (selectedItem is System.IO.DirectoryInfo)
            {
                CurrentFolder = ((System.IO.DirectoryInfo)selectedItem).FullName;
                if (Items.Count > 0)
                    SelectedItem = Items[0];
            }
            else if (selectedItem is FauxDirectory)
            {
                if (((FauxDirectory)selectedItem).Name == "..")
                {
                    CurrentFolder = System.IO.Directory.GetParent(_currentFolder).FullName;
                    if (Items.Count > 0)
                        SelectedItem = Items[0];
                }
            }
            else if (selectedItem is System.IO.FileInfo)
            {

            }
        }
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

    internal class FileDirectoryListboxItem : ListBoxItemTheme
    {
        private Cell _directoryAppNormal = new Cell(Color.Purple, Color.Black);
        private Cell _directoryAppMouseOver = new Cell(Color.Purple, Color.Black);
        private Cell _directoryAppSelected = new Cell(new Color(255, 0, 255), Color.Black);
        private Cell _directoryAppSelectedOver = new Cell(new Color(255, 0, 255), Color.Black);
        private Cell _fileAppNormal = new Cell(Color.Gray, Color.Black);
        private Cell _fileAppMouseOver = new Cell(Color.Gray, Color.Black);
        private Cell _fileAppSelected = new Cell(Color.White, Color.Black);
        private Cell _fileAppSelectedOver = new Cell(Color.White, Color.Black);
        private Cell _highExtAppNormal = new Cell(ColorAnsi.Yellow, Color.Black);
        private Cell _highExtAppMouseOver = new Cell(ColorAnsi.Yellow, Color.Black);
        private Cell _highExtAppSelected = new Cell(Color.Yellow, Color.Black);
        private Cell _highExtAppSelectedOver = new Cell(Color.Yellow, Color.Black);

        public override void RefreshTheme(Colors themeColors)
        {
            base.RefreshTheme(themeColors);

            _directoryAppNormal.Background = themeColors.ControlBack;
            _directoryAppMouseOver.Background = themeColors.ControlBackLight;
            _directoryAppSelected.Background = themeColors.ControlBack;
            _directoryAppSelectedOver.Background = themeColors.ControlBackLight;

            _fileAppNormal.Background = themeColors.ControlBack;
            _fileAppMouseOver.Background = themeColors.ControlBackLight;
            _fileAppSelected.Background = themeColors.ControlBack;
            _fileAppSelectedOver.Background = themeColors.ControlBackLight;

            _highExtAppNormal.Background = themeColors.ControlBack;
            _highExtAppMouseOver.Background = themeColors.ControlBackLight;
            _highExtAppSelected.Background = themeColors.ControlBack;
            _highExtAppSelectedOver.Background = themeColors.ControlBackLight;
        }
        
        public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            Cell appearance;
            string displayString;

            if (item is System.IO.DirectoryInfo || item is FauxDirectory)
            {
                if (item is System.IO.DirectoryInfo info)
                    displayString = "<" + info.Name + ">";
                else
                    displayString = "<" + ((FauxDirectory)item).Name + ">";

                if (itemState.HasFlag(ControlStates.MouseOver) && itemState.HasFlag(ControlStates.Selected))
                    appearance = _directoryAppSelectedOver;
                else if (itemState.HasFlag(ControlStates.MouseOver))
                    appearance = _directoryAppMouseOver;
                else if (itemState.HasFlag(ControlStates.Selected))
                    appearance = _directoryAppSelected;
                else
                    appearance = _directoryAppNormal;
            }
            else if (item is System.IO.FileInfo info)
            {
                displayString = info.Name;

                if (itemState.HasFlag(ControlStates.MouseOver) && itemState.HasFlag(ControlStates.Selected))
                    appearance = _fileAppSelectedOver;
                else if (itemState.HasFlag(ControlStates.MouseOver))
                    appearance = _fileAppMouseOver;
                else if (itemState.HasFlag(ControlStates.Selected))
                    appearance = _fileAppSelected;
                else
                    appearance = _fileAppNormal;
            }
            else if (item is HighlightedExtFile extInfo)
            {
                displayString = extInfo.Name;

                if (itemState.HasFlag(ControlStates.MouseOver) && itemState.HasFlag(ControlStates.Selected))
                    appearance = _highExtAppSelectedOver;
                else if (itemState.HasFlag(ControlStates.MouseOver))
                    appearance = _highExtAppMouseOver;
                else if (itemState.HasFlag(ControlStates.Selected))
                    appearance = _highExtAppSelected;
                else
                    appearance = _highExtAppNormal;
            }
            else
            {
                appearance = GetStateAppearance(itemState);
                displayString = item.ToString();
            }

            surface.Print(area.X, area.Y, displayString.Align(SadConsole.HorizontalAlignment.Left, area.Width), appearance);
        }

    }
    #endregion
}
