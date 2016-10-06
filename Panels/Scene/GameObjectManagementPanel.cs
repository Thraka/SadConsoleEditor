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
        private ListBox<EntityListBoxItem> GameObjectList;
        private Button removeSelected;
        private Button moveSelectedUp;
        private Button moveSelectedDown;
        private Button renameLayer;
        private Button importGameObject;
        private CheckBox drawGameObjectsCheckbox;

        public bool DrawObjects
        {
            get { return drawGameObjectsCheckbox.IsSelected; }
            set { drawGameObjectsCheckbox.IsSelected = value; }
        }


        public ResizableObject SelectedGameObject
        {
            get { return GameObjectList.SelectedItem as ResizableObject; }
            set { GameObjectList.SelectedItem = value; }
        }

        public EntityManagementPanel()
        {
            Title = "Entities";
            GameObjectList = new ListBox<EntityListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 4);
            GameObjectList.HideBorder = true;
            GameObjectList.SelectedItemChanged += GameObject_SelectedItemChanged;
            GameObjectList.CompareByReference = true;

            removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            removeSelected.Text = "Remove";
            removeSelected.ButtonClicked += RemoveSelected_ButtonClicked;

            moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedUp.Text = "Move Up";
            moveSelectedUp.ButtonClicked += MoveSelectedUp_ButtonClicked;

            moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedDown.Text = "Move Down";
            moveSelectedDown.ButtonClicked += MoveSelectedDown_ButtonClicked;
            
            renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            renameLayer.Text = "Rename";
            renameLayer.ButtonClicked += RenameEntity_ButtonClicked;

            importGameObject = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            importGameObject.Text = "Import File";
            importGameObject.ButtonClicked += ImportEntity_ButtonClicked;

            drawGameObjectsCheckbox = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            drawGameObjectsCheckbox.IsSelected = true;
            drawGameObjectsCheckbox.Text = "Draw Objects";

            Controls = new ControlBase[] { GameObjectList, removeSelected, moveSelectedUp, moveSelectedDown, renameLayer, importGameObject, null, drawGameObjectsCheckbox };

            GameObject_SelectedItemChanged(null, null);
        }

        void ImportEntity_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    var entity = (GameObject)popup.SelectedLoader.Load(popup.SelectedFile);
                    entity.Position = new Microsoft.Xna.Framework.Point(0, 0);
                    //entity.RenderOffset = (EditorConsoleManager.ActiveEditor as Editors.SceneEditor).Position;
                    (EditorConsoleManager.ActiveEditor as Editors.SceneEditor)?.LoadEntity(entity);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.GameObject() };
            popup.Show(true);
            popup.Center();
        }

        void RenameEntity_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            RenamePopup popup = new RenamePopup(entity.Name);
            popup.Closed += (o, e2) =>
            {
                if (popup.DialogResult)
                {
                    var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;
                    editor.RenameGameObject(entity, popup.NewName);
                }

                GameObjectList.IsDirty = true;
            };
            popup.Show(true);
            popup.Center();
        }

        void MoveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Objects.IndexOf(entity);
            editor.Objects.Remove(entity);
            editor.Objects.Insert(index + 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void MoveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Objects.IndexOf(entity);
            editor.Objects.Remove(entity);
            editor.Objects.Insert(index - 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void RemoveSelected_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            editor.Objects.Remove(entity);

            RebuildListBox();
            GameObjectList.SelectedItem = GameObjectList.Items[0];
        }

        void GameObject_SelectedItemChanged(object sender, ListBox<EntityListBoxItem>.SelectedItemEventArgs e)
        {
            if (GameObjectList.SelectedItem != null)
            {
                var entity = (ResizableObject)GameObjectList.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

                removeSelected.IsEnabled = GameObjectList.Items.Count != 1;

                moveSelectedUp.IsEnabled = editor.Objects.IndexOf(entity) != 0;
                moveSelectedDown.IsEnabled = editor.Objects.IndexOf(entity) != editor.Objects.Count - 1;
                renameLayer.IsEnabled = true;
             
                editor.SelectedEntity = entity.GameObject;
            }
            else
            {
                removeSelected.IsEnabled = false;
                moveSelectedDown.IsEnabled = false;
                moveSelectedUp.IsEnabled = false;
                renameLayer.IsEnabled = false;
            }
        }

        public void RebuildListBox()
        {
            GameObjectList.Items.Clear();

            if (EditorConsoleManager.ActiveEditor is Editors.SceneEditor)
            {
                var entities = ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).Objects;

                if (entities.Count != 0)
                {
                    foreach (var item in entities)
                        GameObjectList.Items.Add(item);


                    GameObjectList.SelectedItem = GameObjectList.Items[0];
                }
            }
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == GameObjectList ? 1 : 0;
        }

        public override void Loaded()
        {
            var previouslySelected = GameObjectList.SelectedItem;
            RebuildListBox();
            if (GameObjectList.Items.Count != 0)
            {
                if (previouslySelected == null || !GameObjectList.Items.Contains(previouslySelected))
                    GameObjectList.SelectedItem = GameObjectList.Items[0];
                else
                    GameObjectList.SelectedItem = previouslySelected;
            }
        }

        private class EntityListBoxItem : ListBoxItem
        {
            public override void Draw(ITextSurface surface, Microsoft.Xna.Framework.Rectangle area)
            {
                string value = ((ResizableObject)Item).Name;

                if (string.IsNullOrEmpty(value))
                    value = "<no name>";

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
