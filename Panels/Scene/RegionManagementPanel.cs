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
        private Button editSettings;
        private Controls.ColorPresenter zoneColorPresenter;
        private CheckBox drawZonesCheckbox;
        private DrawingSurface _propertySurface;

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

            editSettings = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            editSettings.Text = "Edit Settings";
            editSettings.ButtonClicked += EditSettings_ButtonClicked;

            zoneColorPresenter = new SadConsoleEditor.Controls.ColorPresenter("Selected Zone Color", Settings.Green, SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2);
            zoneColorPresenter.SelectedColor = Color.Aqua;
            zoneColorPresenter.IsEnabled = false;
            zoneColorPresenter.ColorChanged += ZoneColorPresenter_ColorChanged;

            drawZonesCheckbox = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            drawZonesCheckbox.IsSelected = true;
            drawZonesCheckbox.Text = "Draw zones";

            Controls = new ControlBase[] { GameObjectList, removeSelected, moveSelectedUp, moveSelectedDown, renameLayer, editSettings, null, importGameObject, null, zoneColorPresenter, null, drawZonesCheckbox };

            GameObject_SelectedItemChanged(null, null);
        }

        private void EditSettings_ButtonClicked(object sender, EventArgs e)
        {
            var zone = ((ResizableObject<Zone>)GameObjectList.SelectedItem).Data;
            Windows.KeyValueEditPopup popup = new Windows.KeyValueEditPopup(zone.Settings);
            popup.Closed += (o, e2) =>
            {
                if (popup.DialogResult)
                {
                    //_objectTypesListbox.GetContainer(_objectTypesListbox.SelectedItem).IsDirty = true;
                    zone.Settings = popup.SettingsDictionary;
                    RebuildProperties(zone);
                }
            };
            popup.Show(true);
        }

        private void RebuildControls()
        {
            Controls = new ControlBase[] { GameObjectList, removeSelected, moveSelectedUp, moveSelectedDown, renameLayer, editSettings, null, importGameObject, null, zoneColorPresenter, null, drawZonesCheckbox, null, _propertySurface };
            EditorConsoleManager.ToolsPane.RedrawPanels();
        }

        private void ZoneColorPresenter_ColorChanged(object sender, EventArgs e)
        {
            var entity = GameObjectList.SelectedItem as ResizableObject<Zone>;

            if (entity != null)
            {
                entity.Recolor(zoneColorPresenter.SelectedColor);
                entity.Data.DebugAppearance.Background = zoneColorPresenter.SelectedColor;
            }
        }

        void ImportEntity_ButtonClicked(object sender, EventArgs e)
        {
            (EditorConsoleManager.ActiveEditor as Editors.SceneEditor)?.LoadZone(new Zone() {
                                                                                    Area = new Rectangle(1, 1, 10, 10),
                                                                                    DebugAppearance = new CellAppearance(Color.White, Color.White.GetRandomColor(SadConsole.Engine.Random), 0),
                                                                                    Title = "Zone" });
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
            var entity = (ResizableObject<Zone>)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Zones.IndexOf(entity);
            editor.Zones.Remove(entity);
            editor.Zones.Insert(index + 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void MoveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject<Zone>)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Zones.IndexOf(entity);
            editor.Zones.Remove(entity);
            editor.Zones.Insert(index - 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void RemoveSelected_ButtonClicked(object sender, EventArgs e)
        {
            var entity = (ResizableObject<Zone>)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            editor.Zones.Remove(entity);

            RebuildListBox();

            if (GameObjectList.Items.Count != 0)
                GameObjectList.SelectedItem = GameObjectList.Items[0];
        }

        void RebuildProperties(Zone zone)
        {

            if (zone.Settings.Count == 0)
            {
                _propertySurface = null;
                editSettings.IsEnabled = false;
            }
            else
            {
                _propertySurface = new DrawingSurface(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, zone.Settings.Count);
                editSettings.IsEnabled = true;
            }

            int y = 0;
            foreach (var setting in zone.Settings)
            {
                _propertySurface.Print(0, y, setting.Key.Length > 18 ? setting.Key.Substring(0, 18) : setting.Key, Settings.Yellow);
                _propertySurface.Print(1, y + 1, setting.Value.Length > 17 ? setting.Value.Substring(0, 17) : setting.Value, Settings.Grey);

                y += 2;
            }

            RebuildControls();
        }

        void GameObject_SelectedItemChanged(object sender, ListBox<EntityListBoxItem>.SelectedItemEventArgs e)
        {
            if (GameObjectList.SelectedItem != null)
            {
                var entity = (ResizableObject<Zone>)GameObjectList.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

                moveSelectedUp.IsEnabled = editor.Zones.IndexOf(entity) != 0;
                moveSelectedDown.IsEnabled = editor.Zones.IndexOf(entity) != editor.Zones.Count - 1;
                renameLayer.IsEnabled = true;
             
                editor.SelectedEntity = entity.GameObject;
                zoneColorPresenter.IsEnabled = true;
                zoneColorPresenter.SelectedColor = entity.GameObject.Animation.CurrentFrame[0].Background;

                RebuildProperties(entity.Data);

            }
            else
            {
                editSettings.IsEnabled = false;
                zoneColorPresenter.IsEnabled = false;
                moveSelectedDown.IsEnabled = false;
                moveSelectedUp.IsEnabled = false;
                renameLayer.IsEnabled = false;
                _propertySurface = null;
            }

            removeSelected.IsEnabled = GameObjectList.Items.Count != 0;

            RebuildControls();
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
