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

namespace SadConsoleEditor.Tools
{
    class CloneToolPanel : CustomPanel
    {
        private CloneState _state;
        private Button _reset;
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
                else if (value == CloneState.Selected)
                    _steps.SetCharacter(0, 5, _currentStepChar);
                else if (value == CloneState.MovingClone)
                    _steps.SetCharacter(0, 6, _currentStepChar);
            } 
        }

        public enum CloneState
        {
            SelectingPoint1,
            SelectingPoint2,
            Selected,
            MovingClone
        }

        public CloneToolPanel()
        {
            _reset = new Button(18, 1);
            _steps = new DrawingSurface(18, 7);
            _steps.Fill(Settings.Yellow, Color.Transparent, 0, null);

            _steps.Print(1, 0, "Click on surface ", Settings.Color_TextBright);
            _steps.Print(1, 1, "to do these steps", Settings.Color_TextBright);
            _steps.Print(2, 3, _step1);
            _steps.Print(2, 4, _step2);
            _steps.Print(2, 5, _step3);
            _steps.Print(2, 6, _step4);

            _reset.Text = "Reset Steps";
            _reset.ButtonClicked += (o, e) => State = CloneState.SelectingPoint1;

            _skipEmptyColor = new CheckBox(18, 1);
            _skipEmptyColor.Text = "Skip Empty";

            _altEmptyColorCheck = new CheckBox(18, 1);
            _altEmptyColorCheck.Text = "Use Alt. Empty";

            _altEmptyColor = new Controls.ColorPresenter("Alt. Empty Clr", Settings.Green, 18);
            _altEmptyColor.SelectedColor = Color.Black;

            Controls = new ControlBase[] { _reset, _steps, _skipEmptyColor, _altEmptyColorCheck, _altEmptyColor };
            
            Title = "Clone";
            State = CloneState.SelectingPoint1;
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            if (control == _steps || control == _reset || control == _skipEmptyColor)
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
