using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Surfaces;
using SadConsole.GameHelpers;

namespace SadConsoleEditor.Panels
{
    class GameObjectManagementPanel : CustomPanel
    {
        private ListBox<EntityListBoxItem> GameObjectList;
        private Button removeSelected;
        private Button moveSelectedUp;
        private Button moveSelectedDown;
        private Button renameLayer;
        private Button importGameObject;
        private CheckBox drawGameObjectsCheckbox;

        private DrawingSurface animationListTitle;
        ListBox<AnimationListBoxItem> animationsListBox;
        Button playAnimationButton;

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

        public GameObjectManagementPanel()
        {
            Title = "Entities";
            GameObjectList = new ListBox<EntityListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls, 4);
            GameObjectList.HideBorder = true;
            GameObjectList.SelectedItemChanged += GameObject_SelectedItemChanged;
            GameObjectList.CompareByReference = true;

            removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls);
            removeSelected.Text = "Remove";
            removeSelected.Click += RemoveSelected_Click;

            moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls);
            moveSelectedUp.Text = "Move Up";
            moveSelectedUp.Click += MoveSelectedUp_Click;

            moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls);
            moveSelectedDown.Text = "Move Down";
            moveSelectedDown.Click += MoveSelectedDown_Click;
            
            renameLayer = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls);
            renameLayer.Text = "Rename";
            renameLayer.Click += RenameEntity_Click;

            importGameObject = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls);
            importGameObject.Text = "Import File";
            importGameObject.Click += ImportEntity_Click;

            drawGameObjectsCheckbox = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls, 1);
            drawGameObjectsCheckbox.IsSelected = true;
            drawGameObjectsCheckbox.Text = "Draw Objects";

            animationListTitle = new DrawingSurface(Consoles.ToolPane.PanelWidthControls, 2);
            animationListTitle.Print(0, 0, "Animations", Settings.Green);

            animationsListBox = new ListBox<AnimationListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls, 4);
            animationsListBox.SelectedItemChanged += AnimationList_SelectedItemChanged;
            animationsListBox.HideBorder = true;
            animationsListBox.CompareByReference = true;

            playAnimationButton = new Button(Consoles.ToolPane.PanelWidthControls);
            playAnimationButton.Text = "Play Animation";
            playAnimationButton.Click += (o, e) => { if (animationsListBox.SelectedItem != null) ((AnimatedSurface)animationsListBox.SelectedItem).Restart(); };


            Controls = new ControlBase[] { GameObjectList, removeSelected, moveSelectedUp, moveSelectedDown, renameLayer, importGameObject, null, drawGameObjectsCheckbox, null, animationListTitle,animationsListBox, null, playAnimationButton };

            GameObject_SelectedItemChanged(null, null);
        }

        void ImportEntity_Click(object sender, EventArgs e)
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

        void RenameEntity_Click(object sender, EventArgs e)
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

        void MoveSelectedDown_Click(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Objects.IndexOf(entity);
            editor.Objects.Remove(entity);
            editor.Objects.Insert(index + 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void MoveSelectedUp_Click(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            int index = editor.Objects.IndexOf(entity);
            editor.Objects.Remove(entity);
            editor.Objects.Insert(index - 1, entity);
            RebuildListBox();
            GameObjectList.SelectedItem = entity;
        }

        void RemoveSelected_Click(object sender, EventArgs e)
        {
            var entity = (ResizableObject)GameObjectList.SelectedItem;
            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            editor.RemoveGameObject(entity);

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

                moveSelectedUp.IsEnabled = editor.Objects.IndexOf(entity) != 0;
                moveSelectedDown.IsEnabled = editor.Objects.IndexOf(entity) != editor.Objects.Count - 1;
                renameLayer.IsEnabled = true;
             
                editor.SelectedEntity = entity.GameObject;
            }
            else
            {
                moveSelectedDown.IsEnabled = false;
                moveSelectedUp.IsEnabled = false;
                renameLayer.IsEnabled = false;
            }

            removeSelected.IsEnabled = GameObjectList.Items.Count != 0;
            RebuildAnimationListBox();
        }

        private void AnimationList_SelectedItemChanged(object sender, ListBox<AnimationListBoxItem>.SelectedItemEventArgs e)
        {
            if (animationsListBox.SelectedItem != null)
            {
                var animation = (AnimatedSurface)animationsListBox.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;
                animation.CurrentFrameIndex = 0;
                editor.SelectedEntity.Animation = animation;

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

        public void RebuildAnimationListBox()
        {
            animationsListBox.Items.Clear();

            if (GameObjectList.SelectedItem != null)
            {
                var animations = ((ResizableObject)GameObjectList.SelectedItem).GameObject.Animations;

                if (animations.Count != 0)
                {
                    foreach (var item in animations.Values)
                        animationsListBox.Items.Add(item);

                    animationsListBox.SelectedItem = ((ResizableObject)GameObjectList.SelectedItem).GameObject.Animation;
                }
            }
        }

        public override void ProcessMouse(SadConsole.Input.MouseConsoleState info)
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
            public override void Draw(ISurface surface, Microsoft.Xna.Framework.Rectangle area)
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

        private class AnimationListBoxItem : ListBoxItem
        {
            public override void Draw(ISurface surface, Microsoft.Xna.Framework.Rectangle area)
            {
                string value = ((AnimatedSurface)Item).Name;

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
