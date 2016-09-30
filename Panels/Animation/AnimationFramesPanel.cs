using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsoleEditor.Editors;
using SadConsole.Consoles;

namespace SadConsoleEditor.Panels
{
    class AnimationFramesPanel : CustomPanel
    {
        private Button removeSelected;
        private Button moveSelectedUp;
        private Button moveSelectedDown;
        private Button addNewFrame;
        private Button addNewFrameFromFile;
        private Button saveFrameToFile;
        private Button clonePreviousFrame;

        private DrawingSurface framesCounterBox;
        private Button nextFrame;
        private Button previousFrame;

        private Action<TextSurfaceBasic> frameChangeCallback;
        private AnimatedTextSurface currentAnimation;
        private TextSurfaceBasic selectedFrame;

        public AnimationFramesPanel(Action<TextSurfaceBasic> frameChangeCallback)
        {
            Title = "Frames";

            removeSelected = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            removeSelected.Text = "Remove";
            removeSelected.ButtonClicked += removeSelected_ButtonClicked;

            moveSelectedUp = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedUp.Text = "Move Up";
            moveSelectedUp.ButtonClicked += moveSelectedUp_ButtonClicked;

            moveSelectedDown = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            moveSelectedDown.Text = "Move Down";
            moveSelectedDown.ButtonClicked += moveSelectedDown_ButtonClicked;

            addNewFrame = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            addNewFrame.Text = "Add New";
            addNewFrame.ButtonClicked += addNewFrame_ButtonClicked;

            addNewFrameFromFile = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            addNewFrameFromFile.Text = "Load From File";
            addNewFrameFromFile.ButtonClicked += addNewFrameFromFile_ButtonClicked;

            saveFrameToFile = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            saveFrameToFile.Text = "Save Frame to File";
            saveFrameToFile.ButtonClicked += saveFrameToFile_ButtonClicked;

            clonePreviousFrame = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            clonePreviousFrame.Text = "Copy prev. frame";
            clonePreviousFrame.ButtonClicked += clonePreviousFrame_ButtonClicked;

            // Frames area
            framesCounterBox = new DrawingSurface(Consoles.ToolPane.PanelWidth - 2, 1);

            nextFrame = new Button(4, 1);
            nextFrame.Text = ">>";
            nextFrame.ShowEnds = false;
            nextFrame.ButtonClicked += nextFrame_ButtonClicked;

            previousFrame = new Button(4, 1);
            previousFrame.Text = "<<";
            previousFrame.ShowEnds = false;
            previousFrame.ButtonClicked += previousFrame_ButtonClicked;

            this.frameChangeCallback = frameChangeCallback;
            
            Controls = new ControlBase[] { framesCounterBox, previousFrame, nextFrame, removeSelected, addNewFrame, clonePreviousFrame, moveSelectedUp, moveSelectedDown, addNewFrameFromFile, saveFrameToFile};
        }

        

        public void TryNextFrame()
        {
            if (nextFrame.IsEnabled)
                nextFrame_ButtonClicked(null, EventArgs.Empty);
        }

        public void TryPreviousFrame()
        {
            if (previousFrame.IsEnabled)
                previousFrame_ButtonClicked(null, EventArgs.Empty);
        }

        public void SetAnimation(AnimatedTextSurface animation)
        {
            currentAnimation = animation;

            selectedFrame = currentAnimation.Frames[0];

            EnableDisableControls(0);
            DrawFrameCount();

            frameChangeCallback(selectedFrame);
        }

        private void nextFrame_ButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = currentAnimation.Frames.IndexOf(selectedFrame) + 1;

            selectedFrame = currentAnimation.Frames[currentIndex];

            EnableDisableControls(currentIndex);

            frameChangeCallback(selectedFrame);

            EditorConsoleManager.ToolsPane.RedrawPanels();
        }
        private void clonePreviousFrame_ButtonClicked(object sender, EventArgs e)
        {
            var prevIndex = currentAnimation.Frames.IndexOf(selectedFrame) - 1;

            var prevFrame = currentAnimation.Frames[prevIndex];

            prevFrame.Copy(selectedFrame);
        }

        private void previousFrame_ButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = currentAnimation.Frames.IndexOf(selectedFrame) - 1;

            selectedFrame = currentAnimation.Frames[currentIndex];

            EnableDisableControls(currentIndex);

            frameChangeCallback(selectedFrame);

            EditorConsoleManager.ToolsPane.RedrawPanels();
        }
        private void EnableDisableControls(int currentIndex)
        {
            previousFrame.IsEnabled = currentIndex != 0;
            nextFrame.IsEnabled = currentIndex != currentAnimation.Frames.Count - 1;

            removeSelected.IsEnabled = currentAnimation.Frames.Count != 1;
            moveSelectedUp.IsEnabled = currentAnimation.Frames.Count != 1 && currentIndex != currentAnimation.Frames.Count - 1;
            moveSelectedDown.IsEnabled = currentAnimation.Frames.Count != 1 && currentIndex != 0;
            removeSelected.IsEnabled = currentAnimation.Frames.Count != 1;
            clonePreviousFrame.IsEnabled = currentIndex != 0;
        }

        void saveFrameToFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    TextSurface surface = new TextSurface(selectedFrame.Width, selectedFrame.Height, selectedFrame.Cells, Settings.Config.ScreenFont);
                    surface.DefaultForeground = selectedFrame.DefaultForeground;
                    surface.DefaultBackground = selectedFrame.DefaultBackground;
                    
                    popup.SelectedLoader.Save(surface, popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface(), new FileLoaders.TextFile() };
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        void addNewFrameFromFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    var surface = (ITextSurfaceRendered)popup.SelectedLoader.Load(popup.SelectedFile);
                    var newFrame = currentAnimation.CreateFrame();

                    surface.Copy(newFrame);

                    EnableDisableControls(0);
                    DrawFrameCount();
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface(), new FileLoaders.TextFile() };
            popup.Show(true);
            popup.Center();
        }

        void moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var index = currentAnimation.Frames.IndexOf(selectedFrame);
            currentAnimation.Frames.Remove(selectedFrame);
            currentAnimation.Frames.Insert(index - 1, selectedFrame);

            EnableDisableControls(currentAnimation.Frames.IndexOf(selectedFrame));
            DrawFrameCount();
        }

        void moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var index = currentAnimation.Frames.IndexOf(selectedFrame);
            currentAnimation.Frames.Remove(selectedFrame);
            currentAnimation.Frames.Insert(index + 1, selectedFrame);

            EnableDisableControls(currentAnimation.Frames.IndexOf(selectedFrame));
            DrawFrameCount();
        }

        void removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            currentAnimation.Frames.Remove(selectedFrame);
            selectedFrame = currentAnimation.Frames[0];

            EnableDisableControls(0);
            DrawFrameCount();
        }

        void addNewFrame_ButtonClicked(object sender, EventArgs e)
        {
            var frame = currentAnimation.CreateFrame();
            Settings.QuickEditor.TextSurface = frame;
            Settings.QuickEditor.Fill(Settings.Config.EntityEditor.DefaultForeground, Settings.Config.EntityEditor.DefaultBackground, 0, null);
            EnableDisableControls(currentAnimation.Frames.IndexOf(selectedFrame));
            DrawFrameCount();
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        private void DrawFrameCount()
        {
            ColoredString frameNumber = new ColoredString((currentAnimation.Frames.IndexOf(selectedFrame) + 1).ToString(), Settings.Blue, Settings.Color_MenuBack);
            ColoredString frameSep = new ColoredString(" \\ ", Settings.Grey, Settings.Color_MenuBack);
            ColoredString frameMax = new ColoredString(currentAnimation.Frames.Count.ToString(), Settings.Blue, Settings.Color_MenuBack);
            framesCounterBox.Fill(Settings.Blue, Settings.Color_MenuBack, 0, null);
            framesCounterBox.Print(0, 0, frameNumber + frameSep + frameMax);
        }

        public override int Redraw(ControlBase control)
        {
            if (control == framesCounterBox)
            {
                DrawFrameCount();
            }
            else if (control == nextFrame)
            {
                nextFrame.Position = new Microsoft.Xna.Framework.Point(previousFrame.Position.X + previousFrame.Width + 1, previousFrame.Position.Y);
                return 0;
            }

            else if (control == moveSelectedDown || control == addNewFrame)
                return 1;

            return 0;
        }

        public override void Loaded()
        {
            
        }
    }
}
