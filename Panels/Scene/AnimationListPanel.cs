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

        public AnimationListPanel()
        {
            Title = "Animations";
            AnimationList = new ListBox<AnimationListBoxItem>(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            AnimationList.SelectedItemChanged += AnimationList_SelectedItemChanged;
            AnimationList.HideBorder = true;
            AnimationList.CompareByReference = true;

            Controls = new ControlBase[] { AnimationList };
        }

        private void AnimationList_SelectedItemChanged(object sender, ListBox<AnimationListBoxItem>.SelectedItemEventArgs e)
        {
            if (AnimationList.SelectedItem != null)
            {
                var animation = (AnimatedTextSurface)AnimationList.SelectedItem;
                var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;
                
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

            var animations = ((Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor).SelectedEntity.Animations;

            if (animations.Count != 0)
            {
                foreach (var item in animations.Values)
                    AnimationList.Items.Add(item);

                AnimationList.SelectedItem = ((Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor).SelectedEntity.Animation;
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
