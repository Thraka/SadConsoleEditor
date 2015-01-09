using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Entities;

namespace SadConsoleEditor.Panels
{
    class AnimationsPanel : CustomPanel
    {
        private ListBox _animations;
        private Button _removeSelected;
        private Button _addNewLayer;
        private Button _renameLayer;
        private Button _addNewLayerFromFile;
        private Button _saveLayerToFile;

        public SadConsole.Entities.Entity Entity;

        public AnimationsPanel()
        {
            Title = "Animations";
            _animations = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _animations.HideBorder = true;
            _animations.SelectedItemChanged += _layers_SelectedItemChanged;
            _animations.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += _removeSelected_ButtonClicked;

            _addNewLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayer.Text = "Add New";
            _addNewLayer.ButtonClicked += _addNewLayer_ButtonClicked;

            _renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameLayer.Text = "Rename";
            _renameLayer.ButtonClicked += _renameLayer_ButtonClicked;

            _addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayerFromFile.Text = "Load From File";
            _addNewLayerFromFile.ButtonClicked += _addNewAnimationFromFile_ButtonClicked;

            _saveLayerToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveLayerToFile.Text = "Save Anim. to File";
            _saveLayerToFile.ButtonClicked += _saveLayerToFile_ButtonClicked;

            Controls = new ControlBase[] { _animations, _removeSelected, _addNewLayer, _renameLayer, _addNewLayerFromFile, _saveLayerToFile };
        }

        void _saveLayerToFile_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    animation.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.anim;*.animation;";
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        void _addNewAnimationFromFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    if (System.IO.File.Exists(popup.SelectedFile))
                    {
                        var animation = Animation.Load(popup.SelectedFile);

                        Entity.AddAnimation(animation);

                        RebuildListBox();

                    }
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.anim;*.animation";
            popup.Show(true);
            popup.Center();
        }

        void _renameLayer_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;
            var layer = (Consoles.LayeredConsole.Metadata)_animations.SelectedItem;
            RenamePopup popup = new RenamePopup(layer.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) layer.Name = popup.NewName; _animations.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void _removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            var layer = (Consoles.LayeredConsole.Metadata)_animations.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.Surface.RemoveLayer(layer.Index);
            RebuildListBox();
            _animations.SelectedItem = _animations.Items[0];
        }

        void _addNewLayer_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _animations.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.Surface.AddLayer("New");
            RebuildListBox();
            _animations.SelectedItem = previouslySelected;
        }

        void _layers_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _animations.Items.Count != 1;

            _renameLayer.IsEnabled = true;

            if (_animations.SelectedItem != null)
            {
                var layer = (Consoles.LayeredConsole.Metadata)_animations.SelectedItem;

                _removeSelected.IsEnabled = _animations.Items.Count != 1;

                EditorConsoleManager.Instance.SelectedEditor.Surface.SetActiveLayer(layer.Index);
            }
        }


        public void RebuildListBox()
        {
            _animations.Items.Clear();

            foreach (var item in Entity.Animations)
                _animations.Items.Add(item);

            _animations.SelectedItem = _animations.Items[0];
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _animations ? 1 : 0;
        }

        public override void Loaded()
        {
            var previouslySelected = _animations.SelectedItem;
            RebuildListBox();
            if (previouslySelected == null || !_animations.Items.Contains(previouslySelected))
                _animations.SelectedItem = _animations.Items[0];
            else
                _animations.SelectedItem = previouslySelected;
        }
    }
}
