using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Game;
using SadConsole.Consoles;

namespace SadConsoleEditor.Panels
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

        private GameObject entity;

        private Action<AnimatedTextSurface> animationChangeCallback;
        private Action<CustomTool> invokeCustomToolCallback;

        public enum CustomTool
        {
            Center,
            CollisionBox,
            None
        }

        public AnimationsPanel(Action<AnimatedTextSurface> animationChangeCallback)
        {
            Title = "Animations";
            animations = new ListBox(Consoles.ToolPane.PanelWidthControls, 4);
            animations.HideBorder = true;
            animations.SelectedItemChanged += animations_SelectedItemChanged;
            animations.CompareByReference = true;

            removeSelected = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            removeSelected.Text = "Remove";
            removeSelected.ButtonClicked += removeAnimation_ButtonClicked;

            addNewAnimation = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            addNewAnimation.Text = "Add New";
            addNewAnimation.ButtonClicked += addNewAnimation_ButtonClicked;

            renameAnimation = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            renameAnimation.Text = "Rename";
            renameAnimation.ButtonClicked += renameAnimation_ButtonClicked;

            addNewAnimationFromFile = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            addNewAnimationFromFile.Text = "Import Anim.";
            addNewAnimationFromFile.ButtonClicked += addNewAnimationFromFile_ButtonClicked;

            saveAnimationToFile = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            saveAnimationToFile.Text = "Export Anim.";
            saveAnimationToFile.ButtonClicked += saveAnimationToFile_ButtonClicked;

            changeSpeedButton = new Button(3, 1);
            changeSpeedButton.ShowEnds = false;
            changeSpeedButton.Text = "Set";
            changeSpeedButton.ButtonClicked += changeSpeedButton_ButtonClicked;

            cloneSelectedAnimationButton = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            cloneSelectedAnimationButton.Text = "Clone Sel. Anim";
            cloneSelectedAnimationButton.ButtonClicked += cloneSelectedAnimation_ButtonClicked;

            reverseAnimationButton = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            reverseAnimationButton.Text = "Reverse Animation";
            reverseAnimationButton.ButtonClicked += reverseAnimation_ButtonClicked; ;

            setCenterButton = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            setCenterButton.Text = "Set Center";
            setCenterButton.ButtonClicked += (s, e) => invokeCustomToolCallback(CustomTool.Center);

            setBoundingBoxButton = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            setBoundingBoxButton.Text = "Set Collision";
            setBoundingBoxButton.ButtonClicked += (s, e) => invokeCustomToolCallback(CustomTool.CollisionBox);

            animationSpeedLabel = new DrawingSurface(Consoles.ToolPane.PanelWidthControls - changeSpeedButton.Width, 1);

            repeatCheck = new CheckBox(Consoles.ToolPane.PanelWidthControls, 1);
            repeatCheck.Text = "Repeat";
            repeatCheck.IsSelectedChanged += repeatCheck_IsSelectedChanged;

            playPreview = new Button(Consoles.ToolPane.PanelWidthControls, 1);
            playPreview.Text = "Play Preview";
            playPreview.ButtonClicked += playPreview_ButtonClicked; ;

            this.animationChangeCallback = animationChangeCallback;
            //_invokeCustomToolCallback = invokeCustomToolCallback;

            Controls = new ControlBase[] { animations, null, removeSelected, addNewAnimation, renameAnimation, cloneSelectedAnimationButton, null, addNewAnimationFromFile, saveAnimationToFile, null, playPreview, null, animationSpeedLabel, changeSpeedButton, repeatCheck, null, reverseAnimationButton };
        }

        private void reverseAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (AnimatedTextSurface)animations.SelectedItem;
            animation.Frames.Reverse();
            animations_SelectedItemChanged(this, null);
        }

        private void cloneSelectedAnimation_ButtonClicked(object sender, EventArgs e)
        {
            RenamePopup popup = new RenamePopup("clone");
            popup.Closed += (o, e2) =>
            {
                if (popup.DialogResult)
                {
                    var animation = (AnimatedTextSurface)animations.SelectedItem;
                    var newAnimation = new AnimatedTextSurface(popup.NewName, animation.Width, animation.Height, Settings.Config.ScreenFont);

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
            ((AnimatedTextSurface)animations.SelectedItem).Repeat = repeatCheck.IsSelected;
        }

        private void changeSpeedButton_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (AnimatedTextSurface)animations.SelectedItem;
            AnimationSpeedPopup popup = new AnimationSpeedPopup(animation.AnimationDuration);
            popup.Closed += (s2, e2) =>
            {
                if (popup.DialogResult)
                {
                    animation.AnimationDuration = popup.NewSpeed;
                    animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                    animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack) + new ColoredString(((AnimatedTextSurface)animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack));
                }
            };
            popup.Center();
            popup.Show(true);
        }

        void saveAnimationToFile_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (AnimatedTextSurface)animations.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    popup.SelectedLoader.Save(animation, popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.Animation() };
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
                    var animation = (AnimatedTextSurface)popup.SelectedLoader.Load(popup.SelectedFile);

                    entity.Animations[animation.Name] = animation;

                    RebuildListBox();
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.Animation() };
            popup.Show(true);
            popup.Center();
        }

        void renameAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (AnimatedTextSurface)animations.SelectedItem;
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

        void removeAnimation_ButtonClicked(object sender, EventArgs e)
        {
            var animation = (AnimatedTextSurface)animations.SelectedItem;

            if (animation.Name == "default")
            {
                SadConsole.Consoles.Window.Message(new ColoredString("You cannot delete the default animation"), "Close");
            }
            else
            {
                entity.Animations.Remove(animation.Name);
                RebuildListBox();
                animations.SelectedItem = animations.Items[0];
            }
        }

        void addNewAnimation_ButtonClicked(object sender, EventArgs e)
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
                        var previouslySelected = (AnimatedTextSurface)animations.SelectedItem;
                        var animation = new AnimatedTextSurface(newName, previouslySelected.Width, previouslySelected.Height, Settings.Config.ScreenFont);
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

        void animations_SelectedItemChanged(object sender, ListBox<ListBoxItem>.SelectedItemEventArgs e)
        {
            removeSelected.IsEnabled = animations.Items.Count != 1;

            renameAnimation.IsEnabled = true;

            if (animations.SelectedItem != null)
            {
                var animation = (AnimatedTextSurface)animations.SelectedItem;

                removeSelected.IsEnabled = animations.Items.Count != 1;

                repeatCheck.IsSelected = animation.Repeat;
                animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack) + new ColoredString(((AnimatedTextSurface)animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack));

                animationChangeCallback(animation);
            }
        }

        private void playPreview_ButtonClicked(object sender, EventArgs e)
        {
            PreviewAnimationPopup popup = new PreviewAnimationPopup((AnimatedTextSurface)animations.SelectedItem);
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

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            if (control == changeSpeedButton)
            {
                animationSpeedLabel.Fill(Settings.Green, Settings.Color_MenuBack, 0, null);
                animationSpeedLabel.Print(0, 0, new ColoredString("Speed: ", Settings.Green, Settings.Color_MenuBack) + new ColoredString(((AnimatedTextSurface)animations.SelectedItem).AnimationDuration.ToString(), Settings.Blue, Settings.Color_MenuBack));
                changeSpeedButton.Position = new Microsoft.Xna.Framework.Point(Consoles.ToolPane.PanelWidth - changeSpeedButton.Width - 1, animationSpeedLabel.Position.Y);
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

        public void SetEntity(GameObject entity)
        {
            this.entity = entity;
            RebuildListBox();
        }
    }
}
