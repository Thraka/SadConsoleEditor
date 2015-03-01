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
        private ListBox _layers;
        private Button _removeSelected;
        private Button _moveSelectedUp;
        private Button _moveSelectedDown;
        private Button _addNewLayer;
        private Button _renameLayer;
        private Button _addNewLayerFromFile;
        private Button _saveLayerToFile;
        private CheckBox _toggleHideShow;

        public LayersPanel()
        {
            Title = "Layers";
            _layers = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _layers.HideBorder = true;
            _layers.SelectedItemChanged += _layers_SelectedItemChanged;
            _layers.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += _removeSelected_ButtonClicked;

            _moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedUp.Text = "Move Up";
            _moveSelectedUp.ButtonClicked += _moveSelectedUp_ButtonClicked;

            _moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedDown.Text = "Move Down";
            _moveSelectedDown.ButtonClicked += _moveSelectedDown_ButtonClicked;

            _toggleHideShow = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _toggleHideShow.Text = "Show/Hide";
            _toggleHideShow.TextAlignment = System.Windows.HorizontalAlignment.Center;
            _toggleHideShow.IsSelectedChanged += _toggleHideShow_IsSelectedChanged;

            _addNewLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayer.Text = "Add New";
            _addNewLayer.ButtonClicked += _addNewLayer_ButtonClicked;

            _renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameLayer.Text = "Rename";
            _renameLayer.ButtonClicked += _renameLayer_ButtonClicked;

            _addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayerFromFile.Text = "Load From File";
            _addNewLayerFromFile.ButtonClicked += _addNewLayerFromFile_ButtonClicked;

            _saveLayerToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveLayerToFile.Text = "Save Layer to File";
            _saveLayerToFile.ButtonClicked += _saveLayerToFile_ButtonClicked;

            Controls = new ControlBase[] { _layers, _toggleHideShow, _removeSelected, _moveSelectedUp, _moveSelectedDown, _addNewLayer, _renameLayer, _addNewLayerFromFile, _saveLayerToFile };
        }

        void _saveLayerToFile_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    EditorConsoleManager.Instance.SelectedEditor.SaveLayer(layer.Index, popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
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
                    if (EditorConsoleManager.Instance.SelectedEditor.LoadLayer(popup.SelectedFile))
                    {
                            RebuildListBox();
                    }
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
            popup.Show(true);
            popup.Center();
        }

        void _renameLayer_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;
            RenamePopup popup = new RenamePopup(layer.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) layer.Name = popup.NewName; _layers.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void _moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.MoveLayerDown(layer.Index);
            RebuildListBox();
            _layers.SelectedItem = layer;
        }

        void _moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.MoveLayerUp(layer.Index);
            RebuildListBox();
            _layers.SelectedItem = layer;
        }

        void _removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.RemoveLayer(layer.Index);
            RebuildListBox();
            _layers.SelectedItem = _layers.Items[0];
        }

        void _addNewLayer_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _layers.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.AddNewLayer("New");
            RebuildListBox();
            _layers.SelectedItem = previouslySelected;
        }

        void _layers_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _layers.Items.Count != 1;

            _moveSelectedUp.IsEnabled = true;
            _moveSelectedDown.IsEnabled = true;
            _renameLayer.IsEnabled = true;

            if (_layers.SelectedItem != null)
            {
                var layer = (LayeredConsole.Metadata)_layers.SelectedItem;

                _moveSelectedUp.IsEnabled = layer.IsMoveable && _layers.Items.Count != 1 && layer.Index != _layers.Items.Count - 1;
                _moveSelectedDown.IsEnabled = layer.IsMoveable && _layers.Items.Count != 1 && layer.Index != 0;
                _removeSelected.IsEnabled = layer.IsRemoveable && _layers.Items.Count != 1;
                _renameLayer.IsEnabled = layer.IsRenamable;

                _toggleHideShow.IsSelected = layer.IsVisible;

                EditorConsoleManager.Instance.SelectedEditor.SetActiveLayer(layer.Index);
            }
        }

        void _toggleHideShow_IsSelectedChanged(object sender, EventArgs e)
        {
            var layer = (LayeredConsole.Metadata)_layers.SelectedItem;

            EditorConsoleManager.Instance.SelectedEditor.Surface[layer.Index].IsVisible = _toggleHideShow.IsSelected;
            layer.IsVisible = _toggleHideShow.IsSelected;
        }

        public void RebuildListBox()
        {
            _layers.Items.Clear();

            for (int i = EditorConsoleManager.Instance.SelectedEditor.Surface.Layers - 1; i >= 0 ; i--)
                _layers.Items.Add(EditorConsoleManager.Instance.SelectedEditor.Surface.GetLayerMetadata(i));

            _layers.SelectedItem = _layers.Items[0];
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _layers || control == _toggleHideShow ? 1 : 0;
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
    }
}
