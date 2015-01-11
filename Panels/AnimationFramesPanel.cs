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
        private ListBox<Controls.ListBoxItemFrame> _frames;
        private Button _removeSelected;
        private Button _moveSelectedDown;
        private Button _moveSelectedUp;
        private Button _addNewFrame;
        private Button _addNewFrameFromFile;
        private Button _saveFrameToFile;

        private Action<Frame> _frameChangeCallback;
        private Animation _currentAnimation;

        public AnimationFramesPanel(Action<Frame> frameChangeCallback)
        {
            Title = "Frames";
            _frames = new ListBox<Controls.ListBoxItemFrame>(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 4);
            _frames.HideBorder = true;
            _frames.SelectedItemChanged += frames_SelectedItemChanged;
            _frames.CompareByReference = true;

            _removeSelected = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _removeSelected.Text = "Remove";
            _removeSelected.ButtonClicked += removeSelected_ButtonClicked;

            _moveSelectedDown = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedDown.Text = "Move Down";
            _moveSelectedDown.ButtonClicked += moveSelectedDown_ButtonClicked;

            _moveSelectedUp = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _moveSelectedUp.Text = "Move Up";
            _moveSelectedUp.ButtonClicked += moveSelectedUp_ButtonClicked;

            _addNewFrame = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewFrame.Text = "Add New";
            _addNewFrame.ButtonClicked += addNewFrame_ButtonClicked;

            _addNewFrameFromFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _addNewFrameFromFile.Text = "Load From File";
            _addNewFrameFromFile.ButtonClicked += addNewFrameFromFile_ButtonClicked;

            _saveFrameToFile = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveFrameToFile.Text = "Save Layer to File";
            _saveFrameToFile.ButtonClicked += saveFrameToFile_ButtonClicked;

            _frameChangeCallback = frameChangeCallback;

            Controls = new ControlBase[] { _frames, _removeSelected, _moveSelectedDown, _moveSelectedUp, _addNewFrame, _addNewFrameFromFile, _saveFrameToFile };
        }

        public void SetAnimation(Animation animation)
        {
            _currentAnimation = animation;
            RebuildListBox();
        }

        void saveFrameToFile_ButtonClicked(object sender, EventArgs e)
        {
            var frame = (AnimationEditor.FrameWrapper)_frames.SelectedItem;

            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    frame.Frame.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
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
                        var surface = CellSurface.Load(popup.SelectedFile);

                        if (surface.Width != _currentAnimation.Width || surface.Height != _currentAnimation.Height)
                        {
                            var newFrame = _currentAnimation.CreateFrame();
                            surface.Copy(newFrame);
                        }
                        else
                        {
                            EditorConsoleManager.Instance.SelectedEditor.Surface.AddLayer(surface);
                            var newFrame = _currentAnimation.CreateFrame();
                            surface.Copy(newFrame);
                        }

                        RebuildListBox();
                    }
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
            popup.Show(true);
            popup.Center();
        }

        void moveSelectedUp_ButtonClicked(object sender, EventArgs e)
        {
            var frame = (AnimationEditor.FrameWrapper)_frames.SelectedItem;

            var index = _currentAnimation.Frames.IndexOf(frame.Frame);
            _currentAnimation.Frames.Remove(frame.Frame);
            _currentAnimation.Frames.Insert(index - 1, frame.Frame);

            RebuildListBox();
            _frames.SelectedItem = frame;
        }

        void moveSelectedDown_ButtonClicked(object sender, EventArgs e)
        {
            var frame = (AnimationEditor.FrameWrapper)_frames.SelectedItem;
            var index = _currentAnimation.Frames.IndexOf(frame.Frame);
            _currentAnimation.Frames.Remove(frame.Frame);
            _currentAnimation.Frames.Insert(index + 1, frame.Frame);

            RebuildListBox();
            _frames.SelectedItem = frame;
        }

        void removeSelected_ButtonClicked(object sender, EventArgs e)
        {
            var frame = (AnimationEditor.FrameWrapper)_frames.SelectedItem;
            _currentAnimation.Frames.Remove(frame.Frame);
            RebuildListBox();
            _frames.SelectedItem = _frames.Items[0];
        }

        void addNewFrame_ButtonClicked(object sender, EventArgs e)
        {
            var previouslySelected = _frames.SelectedItem;
            _currentAnimation.CreateFrame();
            RebuildListBox();
            _frames.SelectedItem = _frames.Items[_frames.Items.Count - 1];
        }

        void frames_SelectedItemChanged(object sender, ListBox<Controls.ListBoxItemFrame>.SelectedItemEventArgs e)
        {
            _removeSelected.IsEnabled = _frames.Items.Count != 1;

            _moveSelectedDown.IsEnabled = true;
            _moveSelectedUp.IsEnabled = true;

            if (_frames.SelectedItem != null)
            {
                var frame = (AnimationEditor.FrameWrapper)_frames.SelectedItem;
                
                _moveSelectedDown.IsEnabled = _frames.Items.Count != 1 && _frames.SelectedIndex != _frames.Items.Count - 1;
                _moveSelectedUp.IsEnabled = _frames.Items.Count != 1 && _frames.SelectedIndex != 0;
                _removeSelected.IsEnabled = _frames.Items.Count != 1;

                _frameChangeCallback(frame.Frame);
            }
        }

        public void RebuildListBox()
        {
            _frames.Items.Clear();

            for (int i = 0; i < _currentAnimation.Frames.Count; i++)
                _frames.Items.Add(new AnimationEditor.FrameWrapper() { CurrentIndex = i + 1, Frame = _currentAnimation.Frames[i] });

            _frames.SelectedItem = _frames.Items[0];
        }

        public override void ProcessMouse(SadConsole.Input.MouseInfo info)
        {
        }

        public override int Redraw(SadConsole.Controls.ControlBase control)
        {
            return control == _frames ? 1 : 0;
        }

        public override void Loaded()
        {
            var previouslySelected = _frames.SelectedItem;
            RebuildListBox();
            if (previouslySelected == null || !_frames.Items.Contains(previouslySelected))
                _frames.SelectedItem = _frames.Items[0];
            else
                _frames.SelectedItem = previouslySelected;
        }
    }
}
