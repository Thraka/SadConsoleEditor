using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Consoles;
using SadConsole.Game;

namespace SadConsoleEditor.Panels
{
    class EntityManagementPanel : CustomPanel
    {
        private ListBox<EntityListBoxItem> _entities;
        private Button _removeSelected;
        private Button _moveSelectedUp;
        private Button _moveSelectedDown;
        private Button _renameLayer;
        private Button _addNewLayerFromFile;

        public EntityManagementPanel()
        {
            Title = "Entities";
            _entities = new ListBox<EntityListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _entities.HideBorder = true;
            _entities.SelectedItemChanged += _layers_SelectedItemChanged;
            _entities.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += _removeSelected_ButtonClicked;

            _moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedUp.Text = "Move Up";
            _moveSelectedUp.ButtonClicked += _moveSelectedUp_ButtonClicked;

            _moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedDown.Text = "Move Down";
            _moveSelectedDown.ButtonClicked += _moveSelectedDown_ButtonClicked;
            
            _renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameLayer.Text = "Rename";
            _renameLayer.ButtonClicked += _renameLayer_ButtonClicked;

            _addNewLayerFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewLayerFromFile.Text = "Import";
            _addNewLayerFromFile.ButtonClicked += _addNewLayerFromFile_ButtonClicked;

            Controls = new ControlBase[] { _entities, _removeSelected, _moveSelectedUp, _moveSelectedDown, _renameLayer, _addNewLayerFromFile };
        }
        
        void _addNewLayerFromFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    if (((Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor).LoadEntity(popup.SelectedFile))
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
            var entity = (GameObject)_entities.SelectedItem;
            RenamePopup popup = new RenamePopup(entity.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) entity.Name = popup.NewName; _entities.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void _moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (GameObject)_entities.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;

            int index = editor.Entities.IndexOf(entity);
            editor.Entities.Remove(entity);
            editor.Entities.Insert(index - 1, entity);
            RebuildListBox();
            _entities.SelectedItem = entity;
        }

        void _moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (GameObject)_entities.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;

            int index = editor.Entities.IndexOf(entity);
            editor.Entities.Remove(entity);
            editor.Entities.Insert(index + 1, entity);
            RebuildListBox();
            _entities.SelectedItem = entity;
        }

        void _removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (GameObject)_entities.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;

            editor.Entities.Remove(entity);

            RebuildListBox();
            _entities.SelectedItem = _entities.Items[0];
        }

        void _addNewLayer_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _entities.SelectedItem;
            EditorConsoleManager.Instance.SelectedEditor.AddNewLayer("New");
            RebuildListBox();
            _entities.SelectedItem = previouslySelected;
        }

        void _layers_SelectedItemChanged(object sender, ListBox<EntityListBoxItem>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _entities.Items.Count != 1;

            _moveSelectedUp.IsEnabled = true;
            _moveSelectedDown.IsEnabled = true;
            _renameLayer.IsEnabled = true;

            var entity = (GameObject)_entities.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;

            editor.SelectedEntity = entity;
        }

        void _toggleHideShow_IsSelectedChanged(object sender, EventArgs e)
        {
            var layer = (GameObject)_entities.SelectedItem;
        }

        public void RebuildListBox()
        {
            _entities.Items.Clear();

            var entities = ((Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor).Entities;

            if (entities.Count != 0)
            {
                foreach (var item in entities)
                    _entities.Items.Add(item);


                _entities.SelectedItem = _entities.Items[0];
            }
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _entities ? 1 : 0;
        }

        public override void Loaded()
        {
            var previouslySelected = _entities.SelectedItem;
            RebuildListBox();
            if (previouslySelected == null || !_entities.Items.Contains(previouslySelected))
                _entities.SelectedItem = _entities.Items[0];
            else
                _entities.SelectedItem = previouslySelected;
        }

        private class EntityListBoxItem : ListBoxItem
        {
            public override void Draw(ITextSurface surface, Microsoft.Xna.Framework.Rectangle area)
            {
                string value = ((GameObject)Item).Name;

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
