namespace SadConsoleEditor.Controls
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using System;
    using Console = SadConsole.Consoles.Console;

    class CharacterPicker: SadConsole.Controls.ControlBase
    {
        private Color _charForeground;
        private Color _fill;
        private Color _selectedCharColor;

        private SadConsole.Controls.DrawingSurface _characterSurface;
        private SadConsole.Effects.Fade _selectedCharEffect;
        private int _selectedChar;

        public event EventHandler<SelectedCharacterEventArgs> SelectedCharacterChanged; 

        public int SelectedCharacter
        {
            get { return _selectedChar; }
            set
            {
                int old = _selectedChar;
                _selectedChar = value;

                var oldLocation = CellSurface.GetPointFromIndex(old, 16);
                var newLocation = CellSurface.GetPointFromIndex(value, 16);

                this.SetForeground(oldLocation.X, oldLocation.Y, _charForeground);
                this.SetForeground(newLocation.X, newLocation.Y, _selectedCharColor);
                this.SetEffect(this[oldLocation.X, oldLocation.Y], null);
                this.SetEffect(this[newLocation.X, newLocation.Y], _selectedCharEffect);
                
                if (SelectedCharacterChanged != null)
                    SelectedCharacterChanged(this, new SelectedCharacterEventArgs(old, value));
            }
        }

        public CharacterPicker(Color foreground, Color fill, Color selectedCharacterColor, Font characterFont)
            : this(foreground, fill, selectedCharacterColor)
        {
            _characterSurface.AlternateFont = characterFont;
        }
        public CharacterPicker(Color foreground, Color fill, Color selectedCharacterColor)
        {
            this.DefaultForeground = _charForeground = foreground;
            this.DefaultBackground = _fill = fill;
            this.Clear();
            this.Resize(16, 16);

            this.CanUseMouse = true;

            _selectedCharColor = selectedCharacterColor;

            //_characterSurface = new SadConsole.Controls.DrawingSurface(16, 16);
            //_characterSurface.DefaultBackground = fill;
            //_characterSurface.DefaultForeground = foreground;
            //_characterSurface.Clear();

            _selectedCharEffect = new SadConsole.Effects.Fade()
            {
                FadeBackground = true,
                UseCellBackground = false,
                DestinationBackground = new ColorGradient(_fill, _selectedCharColor * 0.8f),
                FadeDuration = 2d,
                CloneOnApply = false,
                AutoReverse = true,
                Repeat = true,
            };

            SelectedCharacter = 1;

            this.AlternateFont = Settings.ScreenFont;
        }

        public override void Compose()
        {
            int i = 0;

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    this.SetCharacter(x, y, i);
                    i++;
                }
            }
        }

        protected override void OnMouseIn(SadConsole.Input.MouseInfo info)
        {
            var mousePosition = TransformConsolePositionByControlPosition(info);

            if (new Rectangle(0, 0, 16, 16).Contains(mousePosition) && info.LeftButtonDown)
            {
                SelectedCharacter = this[mousePosition.ToIndex(16)].CharacterIndex;
            }

            base.OnMouseIn(info);
        }

        protected override void OnLeftMouseClicked(SadConsole.Input.MouseInfo info)
        {
            var mousePosition = TransformConsolePositionByControlPosition(info);

            if (new Rectangle(0, 0, 16, 16).Contains(mousePosition))
            {
                SelectedCharacter = this[mousePosition.ToIndex(16)].CharacterIndex;
            }
            
            base.OnLeftMouseClicked(info);
        }

        public override void DetermineAppearance()
        {
            
        }

        public class SelectedCharacterEventArgs: EventArgs
        {
            public int NewCharacter;
            public int OldCharacter;

            public SelectedCharacterEventArgs(int oldCharacter, int newCharacter)
            {
                NewCharacter = newCharacter;
                OldCharacter = oldCharacter;
            }
        }
    }
}
