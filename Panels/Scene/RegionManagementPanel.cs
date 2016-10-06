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
using Microsoft.Xna.Framework;

namespace SadConsoleEditor.Panels
{
    class RegionManagementPanel : CustomPanel
    {
        private ListBox<EntityListBoxItem> GameObjectList;
        private Button removeSelected;
        private Button moveSelectedUp;
        private Button moveSelectedDown;
        private Button renameLayer;
        private Button importGameObject;
        private Controls.ColorPresenter zoneColorPresenter;
        private CheckBox drawZonesCheckbox;

        public bool DrawZones
        {
            get { return drawZonesCheckbox.IsSelected; }
            set { drawZonesCheckbox.IsSelected = value; }
        }

        public ResizableObject SelectedGameObject
        {
            get { return GameObjectList.SelectedItem as ResizableObject; }
            set { GameObjectList.SelectedItem = value; }
        }

        public RegionManagementPanel()
        {
            Title = "Zones";
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
            importGameObject.Text = "Add New";
            importGameObject.ButtonClicked += ImportEntity_ButtonClicked;

            zoneColorPresenter = new SadConsoleEditor.Controls.ColorPresenter("Selected Zone Color", Settings.Green, SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2);
            zoneColorPresenter.SelectedColor = Color.Aqua;
            zoneColorPresenter.IsEnabled = false;
            zoneColorPresenter.ColorChanged += ZoneColorPresenter_ColorChanged;

            drawZonesCheckbox = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            drawZonesCheckbox.IsSelected = true;
            drawZonesCheckbox.Text = "Draw zones";

            Controls = new ControlBase[] { GameObjectList, removeSelected, moveSelectedUp, moveSelectedDown, renameLayer, importGameObject, null, zoneColorPresenter, null, drawZonesCheckbox };

            GameObject_SelectedItemChanged(null, null);
        }

        private void ZoneColorPresenter_ColorChanged(object sender, EventArgs e)
        {
            var entity = GameObjectList.SelectedItem as ResizableObject;

            if (entity != null)
            {
                entity.Recolor(zoneColorPresenter.SelectedColor);
            }
        }

        void ImportEntity_ButtonClicked(object sender, EventArgs e)
        {
            (EditorConsoleManager.ActiveEditor as Editors.SceneEditor)?.LoadZone(new Zone() { Area = new Rectangle(1, 1, 10, 10), DebugColor = Color.Aqua, Title = "Zone" });
        }

        void RenameEntity_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            RenamePopup popup = new RenamePopup(entity.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) entity.Name = popup.NewName; GameObjectList.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void MoveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Zones.IndexOf(entity);
            editor.Zones.Remove(entity);
            editor.Zones.Insert(index + 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void MoveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Zones.IndexOf(entity);
            editor.Zones.Remove(entity);
            editor.Zones.Insert(index - 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void RemoveSelected_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            editor.Zones.Remove(entity);

            RebuildListBox();

            if (GameObjectList.Items.Count != 0)
                GameObjectList.SelectedItem = GameObjectList.Items[0];
        }

        void GameObject_SelectedItemChanged(object sender, ListBox<EntityListBoxItem>.SelectedItemEventArgs e)
        {
            if (GameObjectList.SelectedItem != null)
            {
                var entity = (ResizableObject)GameObjectList.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

                moveSelectedUp.IsEnabled = editor.Zones.IndexOf(entity) != 0;
                moveSelectedDown.IsEnabled = editor.Zones.IndexOf(entity) != editor.Zones.Count - 1;
                renameLayer.IsEnabled = true;
             
                editor.SelectedEntity = entity.GameObject;
                zoneColorPresenter.IsEnabled = true;
                zoneColorPresenter.SelectedColor = entity.GameObject.Animation.CurrentFrame[0].Background;
            }
            else
            {
                zoneColorPresenter.IsEnabled = false;
                moveSelectedDown.IsEnabled = false;
                moveSelectedUp.IsEnabled = false;
                renameLayer.IsEnabled = false;
            }

            removeSelected.IsEnabled = GameObjectList.Items.Count != 0;
        }

        public void RebuildListBox()
        {
            GameObjectList.Items.Clear();

            if (EditorConsoleManager.ActiveEditor is Editors.SceneEditor)
            {
                var entities = ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).Zones;

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
