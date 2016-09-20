using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Consoles;

namespace SadConsoleEditor.Panels
{
    class LayersPanel : CustomPanel
    {
        private ListBox<LayerListBoxItem> _layers;
        private Button removeSelected;
        private Button moveSelectedUp;
        private Button moveSelectedDown;
        private Button addNewLayer;
        private Button renameLayer;
        private Button addNewLayerFromFile;
        private Button saveLayerToFile;
        private CheckBox toggleHideShow;

        private LayeredTextSurface surface;

        public LayersPanel()
        {
            Title = "Layers";
            _layers = new ListBox<LayerListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _layers.HideBorder = true;
            _layers.SelectedItemChanged += _layers_SelectedItemChanged;
            _layers.CompareByReference = true;

            removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            removeSelected.Text = "Remove";
            removeSelected.ButtonClicked += _removeSelected_ButtonClicked;

            moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedUp.Text = "Move Up";
            moveSelectedUp.ButtonClicked += _moveSelectedUp_ButtonClicked;

            moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedDown.Text = "Move Down";
            moveSelectedDown.ButtonClicked += _moveSelectedDown_ButtonClicked;

            toggleHideShow = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            toggleHideShow.Text = "Show/Hide";
            toggleHideShow.TextAlignment = System.Windows.HorizontalAlignment.Center;
            toggleHideShow.IsSelectedChanged += _toggleHideShow_IsSelectedChanged;

            addNewLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            addNewLayer.Text = "Add New";
            addNewLayer.ButtonClicked += _addNewLayer_ButtonClicked;

            renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            renameLayer.Text = "Rename";
            renameLayer.ButtonClicked += _renameLayer_ButtonClicked;

            addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            addNewLayerFromFile.Text = "Load From File";
            addNewLayerFromFile.ButtonClicked += _addNewLayerFromFile_ButtonClicked;

            saveLayerToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            saveLayerToFile.Text = "Save Layer to File";
            saveLayerToFile.ButtonClicked += saveLayerToFile_ButtonClicked;

            Controls = new ControlBase[] { _layers, toggleHideShow, removeSelected, moveSelectedUp, moveSelectedDown, addNewLayer, renameLayer, addNewLayerFromFile, saveLayerToFile };
        }

        public void SetLayeredTextSurface(LayeredTextSurface surface)
        {
            this.surface = surface;

            // Do updates
            RebuildListBox();
        }

        void saveLayerToFile_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    TextSurface newSurface = new TextSurface(surface.Width, surface.Height, layer.Cells);
                    newSurface.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface() };
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        void _addNewLayerFromFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    TextSurface newSurface = TextSurface.Load(popup.SelectedFile);

                    if (newSurface.Width != surface.Width || newSurface.Height != surface.Height)
                    {
                        var newLayer = surface.Add();
                        LayerMetadata.Create("Loaded", true, true, true, newLayer);
                        var tempSurface = new TextSurface(surface.Width, surface.Height,
                                                          newLayer.Cells, surface.Font);
                        newSurface.Copy(tempSurface);
                        newLayer.Cells = tempSurface.Cells;
                    }
                    else
                    {
                        var layer = surface.Add();
                        LayerMetadata.Create("Loaded", true, true, true, layer);
                        layer.Cells = newSurface.Cells;

                    }

                    RebuildListBox();
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            //popup.FileFilter = "*.con;*.console;*.brush";
            popup.Show(true);
            popup.Center();
        }

        void _renameLayer_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
            var meta = (LayerMetadata)layer.Metadata;
            RenamePopup popup = new RenamePopup(meta.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) meta.Name = popup.NewName; _layers.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void _moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
            surface.Move(layer, layer.Index - 1);
            RebuildListBox();
            _layers.SelectedItem = layer;
        }

        void _moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
            surface.Move(layer, layer.Index + 1);
            RebuildListBox();
            _layers.SelectedItem = layer;
        }

        void _removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
            surface.Remove(layer);
            RebuildListBox();
            _layers.SelectedItem = _layers.Items[0];
        }

        void _addNewLayer_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _layers.SelectedItem;
            LayerMetadata.Create("new", true, true, true, surface.Add());
            RebuildListBox();
            _layers.SelectedItem = previouslySelected;
        }

        void _layers_SelectedItemChanged(object sender, ListBox<LayerListBoxItem>.SelectedItemEventArgs e)
        {
            removeSelected.IsEnabled = _layers.Items.Count != 1;

            moveSelectedUp.IsEnabled = true;
            moveSelectedDown.IsEnabled = true;
            renameLayer.IsEnabled = true;

            if (_layers.SelectedItem != null)
            {
                var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
                var meta = (LayerMetadata)layer.Metadata;

                moveSelectedUp.IsEnabled = meta.IsMoveable && _layers.Items.Count != 1 && layer.Index != _layers.Items.Count - 1;
                moveSelectedDown.IsEnabled = meta.IsMoveable && _layers.Items.Count != 1 && layer.Index != 0;
                removeSelected.IsEnabled = meta.IsRemoveable && _layers.Items.Count != 1;
                renameLayer.IsEnabled = meta.IsRenamable;

                toggleHideShow.IsSelected = layer.IsVisible;

                surface.SetActiveLayer(layer.Index);
                EditorConsoleManager.LayerName = meta.Name;
            }
            else
                EditorConsoleManager.LayerName = "None";
        }

        void _toggleHideShow_IsSelectedChanged(object sender, EventArgs e)
        {
            var layer = (LayeredTextSurface.Layer)_layers.SelectedItem;
            layer.IsVisible = toggleHideShow.IsSelected;
            layer.IsVisible = toggleHideShow.IsSelected;
        }

        public void RebuildListBox()
        {
            _layers.Items.Clear();

            for (int i = surface.LayerCount - 1; i >= 0; i--)
                _layers.Items.Add(surface.GetLayer(i));

            _layers.SelectedItem = _layers.Items[0];
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _layers || control == toggleHideShow ? 1 : 0;
        }

        public override void Loaded()
        {
            var previouslySelected = _layers.SelectedItem;
            RebuildListBox();
            if (previouslySelected == null || !_layers.Items.Contains(previouslySelected))
                _layers.SelectedItem = _layers.Items[0];
            else
                _layers.SelectedItem = previouslySelected;
        }

        private class LayerListBoxItem : ListBoxItem
        {
            public override void Draw(ITextSurface surface, Microsoft.Xna.Framework.Rectangle area)
            {
                string value = ((LayerMetadata)((LayeredTextSurface.Layer)Item).Metadata).Name;

                if (value.Length < area.Width)
                    value += new string(' ', area.Width - value.Length);
                else if (value.Length > area.Width)
                    value = value.Substring(0, area.Width);
                var editor = new SurfaceEditor(surface);
                editor.Print(area.X, area.Y, value, _currentAppearance);
                _isDirty = false;
            }
        }
    }
}
