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
        private CheckBox _skipTransparent;
        private SadConsole.Controls.DrawingSurface _steps;
        private Controls.ColorPresenter _test;

        private int _currentStepChar = 175;
        private string _step1 = "Select Start";
        private string _step2 = "Select End";
        private string _step3 = "Accept Selection";
        private string _step4 = "Stamp Clone";

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
            _steps.Fill(Settings.Color_Text, Color.Transparent, 0, null);

            _steps.Print(1, 0, "Click on surface ", Settings.Color_TextBright);
            _steps.Print(1, 1, "to do these steps", Settings.Color_TextBright);
            _steps.Print(2, 3, _step1);
            _steps.Print(2, 4, _step2);
            _steps.Print(2, 5, _step3);
            _steps.Print(2, 6, _step4);

            _reset.Text = "Restart";
            _reset.ButtonClicked += (o, e) => State = CloneState.SelectingPoint1;

            _skipTransparent = new CheckBox(18, 1);
            _skipTransparent.Text = "Skip Transparent";

            _test = new Controls.ColorPresenter("Fill", Settings.Color_Text, 18);
            _test.SelectedColor = Color.Black;

            Controls = new ControlBase[] { _steps, _reset, _skipTransparent, _test };
            
            Title = "Clone";
            State = CloneState.SelectingPoint1;
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            if (control == _steps)
            {
                return 1;
            }

            return 0;
        }

        public override void Loaded(CellSurface surface)
        {
        }

        public override void Unloaded(CellSurface surface)
        {
        }
    }
}
