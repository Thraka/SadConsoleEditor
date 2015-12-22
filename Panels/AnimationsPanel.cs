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
        private Button _changeSpeedButton;
        private Button _setCenterButton;
        private Button _setBoundingBoxButton;
        private CheckBox _repeatCheck;
        private DrawingSurface _animationSpeedLabel;
        private Button _playPreview;

        private SadConsole.Entities.Entity _entity;

        private Action<Animation> _animationChangeCallback;
        private Action<CustomTool> _invokeCustomToolCallback;

        public enum CustomTool
        {
            Center,
            CollisionBox,
            None
        }

        public AnimationsPanel(Action<Animation> animationChangeCallback)
        {
            Title = "Animations";
            _animations = new ListBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _animations.HideBorder = true;
            _animations.SelectedItemChanged += animations_SelectedItemChanged;
            _animations.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += removeAnimation_ButtonClicked;

            _addNewAnimation = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewAnimation.Text = "Add New";
            _addNewAnimation.ButtonClicked += addNewAnimation_ButtonClicked;

            _renameAnimation = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _renameAnimation.Text = "Rename";
            _renameAnimation.ButtonClicked += renameAnimation_ButtonClicked;

            _addNewAnimationFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewAnimationFromFile.Text = "Load From File";
            _addNewAnimationFromFile.ButtonClicked += addNewAnimationFromFile_ButtonClicked;

            _saveAnimationToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveAnimationToFile.Text = "Save Anim. to File";
            _saveAnimationToFile.ButtonClicked += saveAnimationToFile_ButtonClicked;

            _changeSpeedButton = new Button(3, 1);
            _changeSpeedButton.Text = "Set";
            _changeSpeedButton.ButtonClicked += changeSpeedButton_ButtonClicked; ;

            _setCenterButton = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _setCenterButton.Text = "Set Center";
            _setCenterButton.ButtonClicked += (s, e) => _invokeCustomToolCallback(CustomTool.Center);

            _setBoundingBoxButton = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _setBoundingBoxButton.Text = "Set Collision";
            _setBoundingBoxButton.ButtonClicked += (s, e) => _invokeCustomToolCallback(CustomTool.CollisionBox);

            _animationSpeedLabel = new DrawingSurface(13, 1);

            _repeatCheck = new CheckBox(Consoles.ToolPane.PanelWidth, 1);
            _repeatCheck.Text = "Repeat";
            _repeatCheck.IsSelectedChanged += repeatCheck_IsSelectedChanged;

            _playPreview = new Button(Consoles.ToolPane.PanelWidth, 1);
            _playPreview.Text = "Play Preview";
            _playPreview.ButtonClicked += playPreview_ButtonClicked; ;

            _animationChangeCallback = animationChangeCallback;
            //_invokeCustomToolCallback = invokeCustomToolCallback;

            Controls = new ControlBase[] { _animations, _removeSelected, _addNewAnimation, _renameAnimation, _addNewAnimationFromFile, _saveAnimationToFile, _playPreview, _animationSpeedLabel, _changeSpeedButton, _repeatCheck };
        }

        private void repeatCheck_IsSelectedChanged(object sender, EventArgs e)
        {
            ((Animation)_animations.SelectedItem).Repeat = _repeatCheck.IsSelected;
        }

        private void changeSpeedButton_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;
            AnimationSpeedPopup popup = new AnimationSpeedPopup(animation.AnimationDuration);
            popup.Closed += (s2, e2) =>
            {
                if (popup.DialogResult)
                {
                    animation.AnimationDuration = popup.NewSpeed;
                    _animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                    _animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack, null) + new ColoredString(((Animation)_animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack, null));
                }
            };
            popup.Center();
            popup.Show(true);
        }

        void saveAnimationToFile_ButtonClicked(object sender, EventArgs e)
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

        void addNewAnimationFromFile_ButtonClicked(object sender, EventArgs e)
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

        void renameAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (Animation)_animations.SelectedItem;
            RenamePopup popup = new RenamePopup(animation.Name);
            popup.Closed += (o, e2) => { if (popup.DialogResult) animation.Name = popup.NewName; _animations.IsDirty = true; };
            popup.Show(true);
            popup.Center();
        }

        void removeAnimation_ButtonClicked(object sender, EventArgs e)
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

        void addNewAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _animations.SelectedItem;
            var animation = new Animation("New", 10, 10);
            animation.CreateFrame();
            _entity.AddAnimation(animation);
            RebuildListBox();
            _animations.SelectedItem = previouslySelected;
        }

        void animations_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _animations.Items.Count != 1;

            _renameAnimation.IsEnabled = true;

            if (_animations.SelectedItem != null)
            {
                var animation = (Animation)_animations.SelectedItem;

                _removeSelected.IsEnabled = _animations.Items.Count != 1;

                _repeatCheck.IsSelected = animation.Repeat;
                _animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                _animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack, null) + new ColoredString(((Animation)_animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack, null));

                _animationChangeCallback(animation);
            }
        }

        private void playPreview_ButtonClicked(object sender, EventArgs e)
        {
            PreviewAnimationPopup popup = new PreviewAnimationPopup((Animation)_animations.SelectedItem);
            popup.Center();
            popup.Show(true);
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
            if (control == _changeSpeedButton)
            {
                _animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                _animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack, null) + new ColoredString(((Animation)_animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack, null));
                _changeSpeedButton.Position = new Microsoft.Xna.Framework.Point(Consoles.ToolPane.PanelWidth - _changeSpeedButton.Width + 1, _animationSpeedLabel.Position.Y);
                return 0;
            }
            else if (control == _animations || control == _saveAnimationToFile || control == _playPreview || control == _setCenterButton || control == _setBoundingBoxButton)
                return 1;
            else
                return 0;
        }

        public override void Loaded()
        {
            //var previouslySelected = _animations.SelectedItem;
            //RebuildListBox();
            //if (previouslySelected == null || !_animations.Items.Contains(previouslySelected))
            //    _animations.SelectedItem = _animations.Items[0];
            //else
            //    _animations.SelectedItem = previouslySelected;
        }

        public void SetEntity(Entity entity)
        {
            _entity = entity;
            RebuildListBox();
        }
    }
}
