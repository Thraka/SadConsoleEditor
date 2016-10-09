using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Controls;
using SadConsole.Input;
using SadConsole.Consoles;
using SadConsole.Game;

namespace SadConsoleEditor.Panels.Scene
{
    class AnimationListPanel : CustomPanel
    {
        ListBox<AnimationListBoxItem> AnimationList;
        Button playAnimationButton;
        

        public AnimationListPanel()
        {
            Title = "Animations";
            AnimationList = new ListBox<AnimationListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidthControls, 4);
            AnimationList.SelectedItemChanged += AnimationList_SelectedItemChanged;
            AnimationList.HideBorder = true;
            AnimationList.CompareByReference = true;

            playAnimationButton = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            playAnimationButton.Text = "Play Animation";
            playAnimationButton.ButtonClicked += (o, e) => { if (AnimationList.SelectedItem != null) ((AnimatedTextSurface)AnimationList.SelectedItem).Restart(); };

            Controls = new ControlBase[] { AnimationList, null, playAnimationButton };
        }

        private void AnimationList_SelectedItemChanged(object sender, ListBox<AnimationListBoxItem>.SelectedItemEventArgs e)
        {
            if (AnimationList.SelectedItem != null)
            {
                var animation = (AnimatedTextSurface)AnimationList.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;
                animation.CurrentFrameIndex = 0;
                editor.SelectedEntity.Animation = animation;
                
            }
        }

        public override void Loaded()
        {

        }

        public override void ProcessMouse(MouseInfo info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            return 0;
        }

        public void RebuildListBox()
        {
            AnimationList.Items.Clear();

            if (((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).SelectedEntity != null)
            {
                var animations = ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).SelectedEntity.Animations;

                if (animations.Count != 0)
                {
                    foreach (var item in animations.Values)
                        AnimationList.Items.Add(item);

                    AnimationList.SelectedItem = ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).SelectedEntity.Animation;
                }
            }
        }

        private class AnimationListBoxItem : ListBoxItem
        {
            public override void Draw(ITextSurface surface, Microsoft.Xna.Framework.Rectangle area)
            {
                string value = ((AnimatedTextSurface)Item).Name;

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
