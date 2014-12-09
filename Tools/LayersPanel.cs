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
        private Button _moveSelected;
        private Button _addNewLayer;
        private Button _renameLayer;
        private Button _addNewLayerFromFile;

        public LayersPanel()
        {
            Title = "Layers";
            _layers = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 5);
            _layers.HideBorder = true;
            _layers.SelectedItemChanged += (o, e) =>
                {
                    _removeSelected.IsEnabled = _layers.Items.Count != 1;
                };

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";

            _moveSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelected.Text = "Move Position";

            _addNewLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayer.Text = "Add New";

            _renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameLayer.Text = "Rename";

            _addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayerFromFile.Text = "Load From File";

            Controls = new ControlBase[] { _layers, _removeSelected, _moveSelected, _addNewLayer, _renameLayer, _addNewLayerFromFile };
        }

        public void RebuildListBox()
        {
            _layers.Items.Clear();

            for (int i = 0; i < EditorConsoleManager.Instance.SelectedEditor.Surface.Layers; i++)
                _layers.Items.Add(EditorConsoleManager.Instance.SelectedEditor.Surface.GetLayerName(i));
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _layers ? 1 : 0;
        }

        public override void Loaded()
        {
            RebuildListBox();
            _layers.SelectedItem = _layers.Items[0];
        }
    }
}
