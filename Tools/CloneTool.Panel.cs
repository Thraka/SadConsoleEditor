using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsole.Input;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsoleEditor.Windows;

namespace SadConsoleEditor.Tools
{
    class CloneToolPanel : CustomPanel
    {
        private CloneState _state;
        private Button _reset;
        private Button _loadBrush;
        private Button _saveBrush;
        private Button _clone;
        private Button _clear;
        private Button _move;
        private CheckBox _skipEmptyColor;
        private CheckBox _altEmptyColorCheck;
        private Controls.ColorPresenter _altEmptyColor;

        private Func<CellSurface> _saveBrushHandler;
        private Action<CellSurface> _loadBrushHandler;

        private int _currentStepChar = 175;

        public bool SkipEmptyCells { get { return _skipEmptyColor.IsSelected; } }
        public bool UseAltEmptyColor { get { return _altEmptyColorCheck.IsSelected; } }
        public Color AltEmptyColor { get { return _altEmptyColor.SelectedColor; } }

        public CloneState State
        {
            get { return _state; }
            set
            {
                _state = value;

                _saveBrush.IsEnabled = value == CloneState.Selected;
                _clone.IsEnabled = value == CloneState.Selected;
                _clear.IsEnabled = value == CloneState.Selected;
                _move.IsEnabled = value == CloneState.Selected;
            } 
        }

        public enum CloneState
        {
            SelectingPoint1,
            SelectingPoint2,
            Selected,
            Clone,
            Clear,
            Move
        }

        public CloneToolPanel(Action<CellSurface> loadBrushHandler, Func<CellSurface> saveBrushHandler)
        {
            _reset = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _reset.Text = "Reset Steps";
            _reset.ButtonClicked += (o, e) => State = CloneState.SelectingPoint1;

            _loadBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _loadBrush.Text = "Import Brush";
            _loadBrush.ButtonClicked += _loadBrush_ButtonClicked;

            _saveBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveBrush.Text = "Export Brush";
            _saveBrush.ButtonClicked += _saveBrush_ButtonClicked;

            _clone = new Button(Consoles.ToolPane.PanelWidth, 1);
            _clone.Text = "Clone";
            _clone.ButtonClicked += clone_ButtonClicked;

            _clear = new Button(Consoles.ToolPane.PanelWidth, 1);
            _clear.Text = "Clear";
            _clear.ButtonClicked += clear_ButtonClicked;

            _move = new Button(Consoles.ToolPane.PanelWidth, 1);
            _move.Text = "Move";
            _move.ButtonClicked += move_ButtonClicked;

            _skipEmptyColor = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _skipEmptyColor.Text = "Skip Empty";

            _altEmptyColorCheck = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _altEmptyColorCheck.Text = "Use Alt. Empty";

            _altEmptyColor = new Controls.ColorPresenter("Alt. Empty Clr", Settings.Green, 18);
            _altEmptyColor.SelectedColor = Color.Black;

            Controls = new ControlBase[] { _reset, _loadBrush, _saveBrush, _clone, _clear, _move, _skipEmptyColor, _altEmptyColorCheck, _altEmptyColor };

            _loadBrushHandler = loadBrushHandler;
            _saveBrushHandler = saveBrushHandler;

            Title = "Clone";
            State = CloneState.SelectingPoint1;
        }

        private void move_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Move;
        }

        private void clear_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Clear;
        }

        private void clone_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Clone;
        }

        private void _saveBrush_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CellSurface), new Type[] { typeof(CellSurface) });
                    var stream = System.IO.File.OpenWrite(popup.SelectedFile);

                    serializer.WriteObject(stream, _saveBrushHandler());
                    stream.Dispose();


                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        private void _loadBrush_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    var fileObject = System.IO.File.OpenRead(popup.SelectedFile);
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CellSurface), new Type[] { typeof(CellSurface) });

                    var surface = serializer.ReadObject(fileObject) as CellSurface;

                    _loadBrushHandler(surface);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.FileFilter = "*.con;*.console;*.brush";
            popup.Show(true);
            popup.Center();
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            if (control == _saveBrush || control == _skipEmptyColor)
            {
                return 1;
            }

            return 0;
        }

        public override void Loaded()
        {
        }
    }
}
