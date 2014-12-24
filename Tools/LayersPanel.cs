using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;

namespace SadConsoleEditor.Tools
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
        private CheckBox _toggleHideShow;

        public LayersPanel()
        {
            Title = "Layers";
            _layers = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _layers.HideBorder = true;
            _layers.SelectedItemChanged += (o, e) =>
                {
                    _removeSelected.IsEnabled = _layers.Items.Count != 1;

                    _moveSelectedUp.IsEnabled = true;
                    _moveSelectedDown.IsEnabled = true;
                    _renameLayer.IsEnabled = true;

                    if (_layers.SelectedItem != null)
                    {
                        var layer = (Consoles.LayeredConsole.Metadata)_layers.SelectedItem;

                        _moveSelectedUp.IsEnabled = layer.IsMovable;
                        _moveSelectedDown.IsEnabled = layer.IsMovable;
                        _removeSelected.IsEnabled = layer.IsRemoveable;
                        _renameLayer.IsEnabled = layer.IsRenamable;
                    }
                };

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";

            _moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedUp.Text = "Move Up";

            _moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedDown.Text = "Move Down";

            _toggleHideShow = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _toggleHideShow.Text = "Show/Hide";
            _toggleHideShow.TextAlignment = System.Windows.HorizontalAlignment.Center;

            _addNewLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayer.Text = "Add New";

            _renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameLayer.Text = "Rename";

            _addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayerFromFile.Text = "Load From File";

            Controls = new ControlBase[] { _layers, _toggleHideShow, _removeSelected, _moveSelectedUp, _moveSelectedDown, _addNewLayer, _renameLayer, _addNewLayerFromFile };
        }

        public void RebuildListBox()
        {
            _layers.Items.Clear();

            for (int i = 0; i < EditorConsoleManager.Instance.SelectedEditor.Surface.Layers; i++)
                _layers.Items.Add(EditorConsoleManager.Instance.SelectedEditor.Surface.GetLayerMetadata(i));
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
            RebuildListBox();
            _layers.SelectedItem = _layers.Items[0];
        }
    }
}
