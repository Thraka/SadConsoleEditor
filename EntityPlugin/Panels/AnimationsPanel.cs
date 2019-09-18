using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Entities;
using SadConsoleEditor.Panels;
using SadConsoleEditor.Consoles;

namespace EntityPlugin.Panels
{
    class AnimationsPanel : CustomPanel
    {
        private ListBox animations;
        private Button removeSelected;
        private Button addNewAnimation;
        private Button renameAnimation;
        private Button addNewAnimationFromFile;
        private Button saveAnimationToFile;
        private Button changeSpeedButton;
        private Button setCenterButton;
        private Button setBoundingBoxButton;
        private Button cloneSelectedAnimationButton;
        private Button reverseAnimationButton;
        private CheckBox repeatCheck;
        private DrawingSurface animationSpeedLabel;
        private Button playPreview;

        private Entity entity;

        private Action<AnimatedConsole> animationChangeCallback;
        private Action<CustomTool> invokeCustomToolCallback;

        private float _animationSpeed;

        public enum CustomTool
        {
            Center,
            CollisionBox,
            None
        }

        public AnimationsPanel(Action<AnimatedConsole> animationChangeCallback)
        {
            Title = "Animations";
            animations = new ListBox(ToolPane.PanelWidthControls, 4);
            animations.SelectedItemChanged += animations_SelectedItemChanged;
            animations.CompareByReference = true;

            removeSelected = new Button(ToolPane.PanelWidthControls);
            removeSelected.Text = "Remove";
            removeSelected.Click += removeAnimation_Click;

            addNewAnimation = new Button(ToolPane.PanelWidthControls);
            addNewAnimation.Text = "Add New";
            addNewAnimation.Click += addNewAnimation_Click;

            renameAnimation = new Button(ToolPane.PanelWidthControls);
            renameAnimation.Text = "Rename";
            renameAnimation.Click += renameAnimation_Click;

            addNewAnimationFromFile = new Button(ToolPane.PanelWidthControls);
            addNewAnimationFromFile.Text = "Import Anim.";
            addNewAnimationFromFile.Click += addNewAnimationFromFile_Click;

            saveAnimationToFile = new Button(ToolPane.PanelWidthControls);
            saveAnimationToFile.Text = "Export Anim.";
            saveAnimationToFile.Click += saveAnimationToFile_Click;

            changeSpeedButton = new Button(3);
            changeSpeedButton.Text = "Set";
            changeSpeedButton.Click += changeSpeedButton_Click;

            cloneSelectedAnimationButton = new Button(ToolPane.PanelWidthControls);
            cloneSelectedAnimationButton.Text = "Clone Sel. Anim";
            cloneSelectedAnimationButton.Click += cloneSelectedAnimation_Click;

            reverseAnimationButton = new Button(ToolPane.PanelWidthControls);
            reverseAnimationButton.Text = "Reverse Animation";
            reverseAnimationButton.Click += reverseAnimation_Click; ;

            setCenterButton = new Button(ToolPane.PanelWidthControls);
            setCenterButton.Text = "Set Center";
            setCenterButton.Click += (s, e) => invokeCustomToolCallback(CustomTool.Center);

            setBoundingBoxButton = new Button(ToolPane.PanelWidthControls);
            setBoundingBoxButton.Text = "Set Collision";
            setBoundingBoxButton.Click += (s, e) => invokeCustomToolCallback(CustomTool.CollisionBox);

            animationSpeedLabel = new DrawingSurface(ToolPane.PanelWidthControls - changeSpeedButton.Width, 1);
            animationSpeedLabel.OnDraw = (label) =>
            {
                label.Surface.Fill(SadConsole.Themes.Library.Default.Colors.Green, label.Theme.Colors.ControlBack, 0, null);
                label.Surface.Print(0, 0, new ColoredString("Speed: ", SadConsole.Themes.Library.Default.Colors.Green, label.Theme.Colors.ControlBack) + new ColoredString(((AnimatedConsole)animations.SelectedItem).AnimationDuration.ToString(), SadConsole.Themes.Library.Default.Colors.Blue, label.Theme.Colors.ControlBack));
            };

            repeatCheck = new CheckBox(ToolPane.PanelWidthControls, 1);
            repeatCheck.Text = "Repeat";
            repeatCheck.IsSelectedChanged += repeatCheck_IsSelectedChanged;

            playPreview = new Button(ToolPane.PanelWidthControls);
            playPreview.Text = "Play Preview";
            playPreview.Click += playPreview_Click; ;

            this.animationChangeCallback = animationChangeCallback;
            //_invokeCustomToolCallback = invokeCustomToolCallback;

            Controls = new ControlBase[] { animations, null, removeSelected, addNewAnimation, renameAnimation, cloneSelectedAnimationButton, null, addNewAnimationFromFile, saveAnimationToFile, null, playPreview, null, animationSpeedLabel, changeSpeedButton, repeatCheck, null, reverseAnimationButton };
        }

        private void reverseAnimation_Click(object sender, EventArgs e)
        {
            var animation = (AnimatedConsole)animations.SelectedItem;
            animation.Frames.Reverse();
            animations_SelectedItemChanged(this, null);
        }

        private void cloneSelectedAnimation_Click(object sender, EventArgs e)
        {
            RenamePopup popup = new RenamePopup("clone");
            popup.Closed += (o, e2) =>
            {
                if (popup.DialogResult)
                {
                    var animation = (AnimatedConsole)animations.SelectedItem;
                    var newAnimation = new AnimatedConsole(popup.NewName, animation.Width, animation.Height, SadConsoleEditor.Config.Program.ScreenFont);

                    foreach (var frame in animation.Frames)
                    {
                        var newFrame = newAnimation.CreateFrame();
                        frame.Copy(newFrame);
                    }

                    newAnimation.CurrentFrameIndex = 0;

                    entity.Animations[newAnimation.Name] = newAnimation;
                    RebuildListBox();
                }
            };
            popup.Show(true);
            popup.Center();
        }

        private void repeatCheck_IsSelectedChanged(object sender, EventArgs e)
        {
            ((AnimatedConsole)animations.SelectedItem).Repeat = repeatCheck.IsSelected;
        }

        private void changeSpeedButton_Click(object sender, EventArgs e)
        {
            var animation = (AnimatedConsole)animations.SelectedItem;
            Windows.AnimationSpeedPopup popup = new Windows.AnimationSpeedPopup(animation.AnimationDuration);
            popup.Closed += (s2, e2) =>
            {
                if (popup.DialogResult)
                {
                    animation.AnimationDuration = popup.NewSpeed;
                    animationSpeedLabel.IsDirty = true;
                }
            };
            popup.Center();
            popup.Show(true);
        }

        void saveAnimationToFile_Click(object sender, EventArgs e)
        {
            var animation = (AnimatedConsole)animations.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup(new[] { "ANIMATION" });
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    popup.SelectedLoader.Save(animation, popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        void addNewAnimationFromFile_Click(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup(new[] { "ANIMATION" });
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    var animation = (AnimatedConsole)popup.SelectedLoader.Load(popup.SelectedFile);

                    entity.Animations[animation.Name] = animation;

                    RebuildListBox();
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.Show(true);
            popup.Center();
        }

        void renameAnimation_Click(object sender, EventArgs e)
        {
            var animation = (AnimatedConsole)animations.SelectedItem;
            RenamePopup popup = new RenamePopup(animation.Name);
            popup.Closed += (o, e2) => 
            {
                if (popup.DialogResult)
                {
                    entity.Animations.Remove(animation.Name);
                    animation.Name = popup.NewName;
                    entity.Animations[animation.Name] = animation;
                    animations.IsDirty = true;
                }
            };
            popup.Show(true);
            popup.Center();
        }

        void removeAnimation_Click(object sender, EventArgs e)
        {
            var animation = (AnimatedConsole)animations.SelectedItem;

            if (animation.Name == "default")
            {
                SadConsole.Window.Message(new ColoredString("You cannot delete the default animation"), "Close");
            }
            else
            {
                entity.Animations.Remove(animation.Name);
                RebuildListBox();
                animations.SelectedItem = animations.Items[0];
            }
        }

        void addNewAnimation_Click(object sender, EventArgs e)
        {
            RenamePopup popup = new RenamePopup("", "Animation Name");
            popup.Closed += (o, e2) =>
            {

                if (popup.DialogResult)
                {
                    string newName = popup.NewName.Trim();
                    var keys = entity.Animations.Keys.Select(k => k.ToLower()).ToList();

                    if (keys.Contains(newName.ToLower()))
                    {
                        Window.Message("Name must be unique", "Close");
                    }
                    else if (string.IsNullOrEmpty(newName))
                    {
                        Window.Message("Name cannot be blank", "Close");
                    }
                    else
                    {
                        var previouslySelected = (AnimatedConsole)animations.SelectedItem;
                        var animation = new AnimatedConsole(newName, previouslySelected.Width, previouslySelected.Height, SadConsoleEditor.Config.Program.ScreenFont);
                        animation.CreateFrame();
                        animation.AnimationDuration = 1;
                        entity.Animations[animation.Name] = animation;
                        RebuildListBox();
                        animations.SelectedItem = animation;
                    }
                }
            };
            popup.Show(true);
            popup.Center();
        }

        void animations_SelectedItemChanged(object sender, ListBox.SelectedItemEventArgs e)
        {
            removeSelected.IsEnabled = animations.Items.Count != 1;

            renameAnimation.IsEnabled = true;

            if (animations.SelectedItem != null)
            {
                var animation = (AnimatedConsole)animations.SelectedItem;

                removeSelected.IsEnabled = animations.Items.Count != 1;

                repeatCheck.IsSelected = animation.Repeat;
                animationSpeedLabel.IsDirty = true;

                animationChangeCallback(animation);
            }
        }

        private void playPreview_Click(object sender, EventArgs e)
        {
            Windows.PreviewAnimationPopup popup = new Windows.PreviewAnimationPopup((AnimatedConsoleEditor)animations.SelectedItem);
            popup.Center();
            popup.Show(true);
        }

        public void RebuildListBox()
        {
            animations.Items.Clear();

            foreach (var item in entity.Animations)
            {
                animations.Items.Add(item.Value);

                if (item.Value == entity.Animation)
                    animations.SelectedItem = item.Value;
            }

            if (animations.SelectedItem == null)
                animations.SelectedItem = animations.Items[0];
        }

        public override void ProcessMouse(SadConsole.Input.MouseConsoleState info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == changeSpeedButton)
            {
                animationSpeedLabel.IsDirty = true;
                changeSpeedButton.Position = new Microsoft.Xna.Framework.Point(ToolPane.PanelWidth - changeSpeedButton.Width - 1, animationSpeedLabel.Position.Y);
            }

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
            this.entity = entity;
            RebuildListBox();
        }
    }
}
