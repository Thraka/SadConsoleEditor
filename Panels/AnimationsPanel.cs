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
        private Button _addNewAnimation;
        private Button _renameAnimation;
        private Button _addNewAnimationFromFile;
        private Button _saveAnimationToFile;

        private SadConsole.Entities.Entity _entity;

        private Action<Animation> _animationChangeCallback;

        public AnimationsPanel(Action<Animation> animationChangeCallback)
        {
            Title = "Animations";
            _animations = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _animations.HideBorder = true;
            _animations.SelectedItemChanged += _animations_SelectedItemChanged;
            _animations.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += _removeAnimation_ButtonClicked;

            _addNewAnimation = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewAnimation.Text = "Add New";
            _addNewAnimation.ButtonClicked += _addNewAnimation_ButtonClicked;

            _renameAnimation = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameAnimation.Text = "Rename";
            _renameAnimation.ButtonClicked += _renameAnimation_ButtonClicked;

            _addNewAnimationFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewAnimationFromFile.Text = "Load From File";
            _addNewAnimationFromFile.ButtonClicked += _addNewAnimationFromFile_ButtonClicked;

            _saveAnimationToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveAnimationToFile.Text = "Save Anim. to File";
            _saveAnimationToFile.ButtonClicked += _saveAnimationToFile_ButtonClicked;

            _animationChangeCallback = animationChangeCallback;

            Controls = new ControlBase[] { _animations, _removeSelected, _addNewAnimation, _renameAnimation, _addNewAnimationFromFile, _saveAnimationToFile };
        }

        void _saveAnimationToFile_ButtonClicked(object sender, EventArgs e)
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

                        _entity.AddAnimation(animation);

                        RebuildListBox();

                    }
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.anim;*.animation";
            popup.Show(true);
            popup.Center();
        }

        void _renameAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;
            RenamePopup popup = new RenamePopup(animation.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) animation.Name = popup.NewName; _animations.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void _removeAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;

            if (animation.Name == "default")
            {
                SadConsole.Consoles.Window.Message(new ColoredString("You cannot delete the default animation"), "Close");
            }
            else
            {
                _entity.RemoveAnimation(animation);
                RebuildListBox();
                _animations.SelectedItem = _animations.Items[0];
            }
        }

        void _addNewAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _animations.SelectedItem;
            var animation = new Animation("New", 10, 10);
            animation.CreateFrame();
            _entity.AddAnimation(animation);
            RebuildListBox();
            _animations.SelectedItem = previouslySelected;
        }

        void _animations_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _animations.Items.Count != 1;

            _renameAnimation.IsEnabled = true;

            if (_animations.SelectedItem != null)
            {
                var animation = (Animation)_animations.SelectedItem;

                _removeSelected.IsEnabled = _animations.Items.Count != 1;

                _animationChangeCallback(animation);
            }
        }


        public void RebuildListBox()
        {
            _animations.Items.Clear();

            foreach (var item in _entity.Animations)
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

        public void SetEntity(Entity entity)
        {
            _entity = entity;
            RebuildListBox();
        }
    }
}
