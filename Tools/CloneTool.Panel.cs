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
        private CheckBox _skipEmptyColor;
        private CheckBox _altEmptyColorCheck;
        private SadConsole.Controls.DrawingSurface _steps;
        private Controls.ColorPresenter _altEmptyColor;

        private int _currentStepChar = 175;
        private string _step1 = "Select Start";
        private string _step2 = "Select End";
        private string _step3 = "Accept Selection";
        private string _step4 = "Stamp Clone";

        public bool SkipEmptyCells { get { return _skipEmptyColor.IsSelected; } }
        public bool UseAltEmptyColor { get { return _altEmptyColorCheck.IsSelected; } }
        public Color AltEmptyColor { get { return _altEmptyColor.SelectedColor; } }

        public CloneState State
        {
            get { return _state; }
            set
            {
                _state = value;

                _steps.FillArea(new Rectangle(0, 0, 1, _steps.Height), Color.Green, Color.Transparent, 0, null);

                if (value == CloneState.SelectingPoint1)
                    _steps.SetCharacter(0, 3, _currentStepChar);
                else if (value == CloneState.SelectingPoint2)
                    _steps.SetCharacter(0, 4, _currentStepChar);
                else if (value == CloneState.MovingClone)
                    _steps.SetCharacter(0, 5, _currentStepChar);

                _saveBrush.IsEnabled = value == CloneState.MovingClone;
            } 
        }

        public enum CloneState
        {
            SelectingPoint1,
            SelectingPoint2,
            Selected,
            MovingClone
        }

        public CloneToolPanel(Action<CellSurface> loadBrushHandler, Func<CellSurface> saveBrushHandler)
        {
            _reset = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _steps = new DrawingSurface(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 6);
            _steps.Fill(Settings.Yellow, Color.Transparent, 0, null);

            _steps.Print(1, 0, "Click on surface ", Settings.Color_TextBright);
            _steps.Print(1, 1, "to do these steps", Settings.Color_TextBright);
            _steps.Print(2, 3, _step1);
            _steps.Print(2, 4, _step2);
            _steps.Print(2, 5, _step4);

            _reset.Text = "Reset Steps";
            _reset.ButtonClicked += (o, e) => State = CloneState.SelectingPoint1;

            _loadBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _loadBrush.Text = "Import Brush";
            _loadBrush.ButtonClicked += (o, e) =>
            {
                SelectFilePopup popup = new SelectFilePopup();
                popup.Closed += (o2, e2) =>
                {
                    if (popup.DialogResult)
                    {
                        var fileObject = System.IO.File.OpenRead(popup.SelectedFile);
                        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CellSurface), new Type[] { typeof(CellSurface) });

                        var surface = serializer.ReadObject(fileObject) as CellSurface;

                        loadBrushHandler(surface);
                    }
                };
                popup.CurrentFolder = Environment.CurrentDirectory;
                popup.FileFilter = "*.con;*.console;*.brush";
                popup.Show(true);
                popup.Center();
            };

            _saveBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _saveBrush.Text = "Export Brush";
            _saveBrush.ButtonClicked += (o, e) =>
            {
                SelectFilePopup popup = new SelectFilePopup();
                popup.Closed += (o2, e2) =>
                {
                    if (popup.DialogResult)
                    {
                        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CellSurface), new Type[] { typeof(CellSurface) });
                        var stream = System.IO.File.OpenWrite(popup.SelectedFile);
            
                        serializer.WriteObject(stream, saveBrushHandler());
                        stream.Dispose();

                        
                    }
                };
                popup.CurrentFolder = Environment.CurrentDirectory;
                popup.FileFilter = "*.con;*.console;*.brush";
                popup.SelectButtonText = "Save";
                popup.SkipFileExistCheck = true;
                popup.Show(true);
                popup.Center();
            };

            _skipEmptyColor = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _skipEmptyColor.Text = "Skip Empty";

            _altEmptyColorCheck = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth, 1);
            _altEmptyColorCheck.Text = "Use Alt. Empty";

            _altEmptyColor = new Controls.ColorPresenter("Alt. Empty Clr", Settings.Green, 18);
            _altEmptyColor.SelectedColor = Color.Black;

            Controls = new ControlBase[] { _reset, _loadBrush, _saveBrush, _steps, _skipEmptyColor, _altEmptyColorCheck, _altEmptyColor };
            
            Title = "Clone";
            State = CloneState.SelectingPoint1;
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            if (control == _steps || control == _saveBrush || control == _skipEmptyColor)
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
