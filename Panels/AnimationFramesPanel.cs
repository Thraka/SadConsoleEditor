using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Controls;
using SadConsoleEditor.Windows;
using SadConsole;
using SadConsole.Entities;
using SadConsoleEditor.Editors;

namespace SadConsoleEditor.Panels
{
    class AnimationFramesPanel : CustomPanel
    {
        private Button _removeSelected;
        private Button _moveSelectedUp;
        private Button _moveSelectedDown;
        private Button _addNewFrame;
        private Button _addNewFrameFromFile;
        private Button _saveFrameToFile;

        private DrawingSurface _framesCounterBox;
        private Button _nextFrame;
        private Button _previousFrame;

        private Action<Frame> _frameChangeCallback;
        private Animation _currentAnimation;
        private Frame _selectedFrame;

        public AnimationFramesPanel(Action<Frame> frameChangeCallback)
        {
            Title = "Frames";

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += removeSelected_ButtonClicked;

            _moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedUp.Text = "Move Up";
            _moveSelectedUp.ButtonClicked += moveSelectedUp_ButtonClicked;

            _moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedDown.Text = "Move Down";
            _moveSelectedDown.ButtonClicked += moveSelectedDown_ButtonClicked;

            _addNewFrame = new Button(Consoles.ToolPane.PanelWidth, 1);
            _addNewFrame.Text = "Add New";
            _addNewFrame.ButtonClicked += addNewFrame_ButtonClicked;

            _addNewFrameFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewFrameFromFile.Text = "Load From File";
            _addNewFrameFromFile.ButtonClicked += addNewFrameFromFile_ButtonClicked;

            _saveFrameToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveFrameToFile.Text = "Save Frame to File";
            _saveFrameToFile.ButtonClicked += saveFrameToFile_ButtonClicked;


            // Frames area
            _framesCounterBox = new DrawingSurface(Consoles.ToolPane.PanelWidth, 1);

            _nextFrame = new Button(4, 1);
            _nextFrame.Text = ">>";
            _nextFrame.ButtonClicked += nextFrame_ButtonClicked;

            _previousFrame = new Button(4, 1);
            _previousFrame.Text = "<<";
            _previousFrame.ButtonClicked += previousFrame_ButtonClicked;

            _frameChangeCallback = frameChangeCallback;
            
            Controls = new ControlBase[] { _framesCounterBox, _previousFrame, _nextFrame, _removeSelected, _addNewFrame, _moveSelectedUp, _moveSelectedDown, _addNewFrameFromFile, _saveFrameToFile};
        }

        public void TryNextFrame()
        {
            if (_nextFrame.IsEnabled)
                nextFrame_ButtonClicked(null, EventArgs.Empty);
        }

        public void TryPreviousFrame()
        {
            if (_previousFrame.IsEnabled)
                previousFrame_ButtonClicked(null, EventArgs.Empty);
        }

        public void SetAnimation(Animation animation)
        {
            _currentAnimation = animation;

            _selectedFrame = _currentAnimation.Frames[0];

            EnableDisableControls(0);
            DrawFrameCount();

            _frameChangeCallback(_selectedFrame);
        }

        private void nextFrame_ButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = _currentAnimation.Frames.IndexOf(_selectedFrame) + 1;

            _selectedFrame = _currentAnimation.Frames[currentIndex];

            EnableDisableControls(currentIndex);

            _frameChangeCallback(_selectedFrame);

            EditorConsoleManager.Instance.ToolPane.RefreshControls();
        }

        private void previousFrame_ButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = _currentAnimation.Frames.IndexOf(_selectedFrame) - 1;

            _selectedFrame = _currentAnimation.Frames[currentIndex];

            EnableDisableControls(currentIndex);

            _frameChangeCallback(_selectedFrame);

            EditorConsoleManager.Instance.ToolPane.RefreshControls();
        }
        private void EnableDisableControls(int currentIndex)
        {
            _previousFrame.IsEnabled = currentIndex != 0;
            _nextFrame.IsEnabled = currentIndex != _currentAnimation.Frames.Count - 1;

            _removeSelected.IsEnabled = _currentAnimation.Frames.Count != 1;
            _moveSelectedUp.IsEnabled = _currentAnimation.Frames.Count != 1 && currentIndex != _currentAnimation.Frames.Count - 1;
            _moveSelectedDown.IsEnabled = _currentAnimation.Frames.Count != 1 && currentIndex != 0;
            _removeSelected.IsEnabled = _currentAnimation.Frames.Count != 1;
        }

        void saveFrameToFile_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    _selectedFrame.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush;*.frame";
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
                    if (System.IO.File.Exists(popup.SelectedFile))
                    {
                        var surface = Frame.Load(popup.SelectedFile);

                        if (surface.Width != _currentAnimation.Width || surface.Height != _currentAnimation.Height)
                        {
                            var newFrame = _currentAnimation.CreateFrame();
                            surface.Copy(newFrame);
                        }
                        else
                        {
                            var newFrame = _currentAnimation.CreateFrame();
                            surface.Copy(newFrame);
                        }

                        EnableDisableControls(0);
                        DrawFrameCount();
                    }
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush;*.frame";
            popup.Show(true);
            popup.Center();
        }

        void moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var index = _currentAnimation.Frames.IndexOf(_selectedFrame);
            _currentAnimation.Frames.Remove(_selectedFrame);
            _currentAnimation.Frames.Insert(index - 1, _selectedFrame);

            EnableDisableControls(_currentAnimation.Frames.IndexOf(_selectedFrame));
            DrawFrameCount();
        }

        void moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var index = _currentAnimation.Frames.IndexOf(_selectedFrame);
            _currentAnimation.Frames.Remove(_selectedFrame);
            _currentAnimation.Frames.Insert(index + 1, _selectedFrame);

            EnableDisableControls(_currentAnimation.Frames.IndexOf(_selectedFrame));
            DrawFrameCount();
        }

        void removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            _currentAnimation.Frames.Remove(_selectedFrame);
            _selectedFrame = _currentAnimation.Frames[0];

            EnableDisableControls(0);
            DrawFrameCount();
        }

        void addNewFrame_ButtonClicked(object sender, EventArgs e)
        {
            var frame = _currentAnimation.CreateFrame();
            frame.Fill(Settings.Config.EntityEditor.DefaultForeground, Settings.Config.EntityEditor.DefaultBackground, 0, null);
            EnableDisableControls(_currentAnimation.Frames.IndexOf(_selectedFrame));
            DrawFrameCount();
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        private void DrawFrameCount()
        {
            ColoredString frameNumber = new ColoredString((_currentAnimation.Frames.IndexOf(_selectedFrame) + 1).ToString(), Settings.Blue, Settings.Color_MenuBack, null);
            ColoredString frameSep = new ColoredString(" \\ ", Settings.Grey, Settings.Color_MenuBack, null);
            ColoredString frameMax = new ColoredString(_currentAnimation.Frames.Count.ToString(), Settings.Blue, Settings.Color_MenuBack, null);
            _framesCounterBox.Print(0, 0, frameNumber + frameSep + frameMax);
        }

        public override int Redraw(ControlBase control)
        {
            if (control == _framesCounterBox)
            {
                DrawFrameCount();
            }
            else if (control == _nextFrame)
            {
                _nextFrame.Position = new Microsoft.Xna.Framework.Point(_previousFrame.Position.X + _previousFrame.Width + 1, _previousFrame.Position.Y);
                return 0;
            }

            else if (control == _moveSelectedDown || control == _addNewFrame)
                return 1;

            return 0;
        }

        public override void Loaded()
        {
            
        }
    }
}
