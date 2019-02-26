namespace SadConsoleEditor.Controls
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Controls;
    using SadConsole.Themes;
    using System;
    using Console = SadConsole.Console;

    public class CharacterPicker: SadConsole.Controls.ControlBase
    {
        public class ThemeType : ThemeBase
        {
            /// <inheritdoc />
            public override void Attached(ControlBase control)
            {
                if (!(control is CharacterPicker)) throw new Exception($"Theme can only be added to a {nameof(CharacterPicker)}");

                control.Surface = new CellSurface(control.Width, control.Height);
                control.Surface.Clear();
                base.Attached(control);
            }

            /// <inheritdoc />
            public override void UpdateAndDraw(ControlBase control, TimeSpan time)
            {
                if (!(control is CharacterPicker picker)) return;

                if (!control.IsDirty) return;

                Cell appearance;

                if (Helpers.HasFlag(control.State, ControlStates.Disabled))
                    appearance = Disabled;

                //else if (Helpers.HasFlag(presenter.State, ControlStates.MouseLeftButtonDown) || Helpers.HasFlag(presenter.State, ControlStates.MouseRightButtonDown))
                //    appearance = MouseDown;

                //else if (Helpers.HasFlag(presenter.State, ControlStates.MouseOver))
                //    appearance = MouseOver;

                else if (Helpers.HasFlag(control.State, ControlStates.Focused))
                    appearance = Focused;

                else
                    appearance = Normal;

                if (picker._newCharacterLocation != new Point(-1))
                {
                    control.Surface.SetEffect(picker._oldCharacterLocation.X, picker._oldCharacterLocation.Y, null);
                }
                
                control.Surface.Fill(picker._charForeground, picker._fill, 0, null);

                int i = 0;

                for (int y = 0; y < Config.Program.ScreenFont.Rows; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        control.Surface.SetGlyph(x, y, i);
                        control.Surface.SetMirror(x, y, picker._mirrorEffect);
                        i++;
                    }
                }

                control.Surface.SetForeground(picker._newCharacterLocation.X, picker._newCharacterLocation.Y, picker._selectedCharColor);
                control.Surface.SetEffect(picker._newCharacterLocation.X, picker._newCharacterLocation.Y, picker._selectedCharEffect);

                control.IsDirty = false;
            }

            /// <inheritdoc />
            public override ThemeBase Clone()
            {
                return new ThemeType()
                {
                    Colors = Colors?.Clone(),
                    Normal = Normal.Clone(),
                    Disabled = Disabled.Clone(),
                    MouseOver = MouseOver.Clone(),
                    MouseDown = MouseDown.Clone(),
                    Selected = Selected.Clone(),
                    Focused = Focused.Clone(),
                };
            }
        }


        private Color _charForeground;
        private Color _fill;
        private Color _selectedCharColor;
        Microsoft.Xna.Framework.Graphics.SpriteEffects _mirrorEffect;

        private SadConsole.Controls.DrawingSurface _characterSurface;
        private SadConsole.Effects.Fade _selectedCharEffect;
        private int _selectedChar;

        private Point _oldCharacterLocation = new Point(-1);
        private Point _newCharacterLocation = new Point(-1);

        public event EventHandler<SelectedCharacterEventArgs> SelectedCharacterChanged;
        public bool UseFullClick;
        
        public Microsoft.Xna.Framework.Graphics.SpriteEffects MirrorEffect
        {
            get { return _mirrorEffect; }
            set
            {
                _mirrorEffect = value;
                IsDirty = true;
            }
        }

        public int SelectedCharacter
        {
            get { return _selectedChar; }
            set
            {
                if (_selectedChar == value) return;

                var old = _selectedChar;
                _selectedChar = value;

                _oldCharacterLocation = old.ToPoint(16);
                _newCharacterLocation = value.ToPoint(16);

                SelectedCharacterChanged?.Invoke(this, new SelectedCharacterEventArgs(old, value));

                IsDirty = true;
            }
        }

        public CharacterPicker(Color foreground, Color fill, Color selectedCharacterColor, Font characterFont)
            : this(foreground, fill, selectedCharacterColor)
        {
            _characterSurface.AlternateFont = characterFont;
            Theme = new ThemeType();
        }
        public CharacterPicker(Color foreground, Color fill, Color selectedCharacterColor):base(16, Config.Program.ScreenFont.Rows)
        {
            this.UseMouse = true;
            Theme = new ThemeType();

            _selectedCharColor = selectedCharacterColor;
            _charForeground = foreground;
            _fill = fill;

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
        }
        
        protected override void OnMouseIn(SadConsole.Input.MouseConsoleState info)
        {
            var mousePosition = TransformConsolePositionByControlPosition(info.CellPosition);

            if (new Rectangle(0, 0, 16, Config.Program.ScreenFont.Rows).Contains(mousePosition) && info.Mouse.LeftButtonDown)
            {
                if (!UseFullClick)
                    SelectedCharacter = Surface[mousePosition.ToIndex(16)].Glyph;
            }

            base.OnMouseIn(info);
        }

        protected override void OnLeftMouseClicked(SadConsole.Input.MouseConsoleState info)
        {
            var mousePosition = TransformConsolePositionByControlPosition(info.CellPosition);

            if (new Rectangle(0, 0, 16, Config.Program.ScreenFont.Rows).Contains(mousePosition))
            {
                SelectedCharacter = Surface[mousePosition.ToIndex(16)].Glyph;
            }
            
            base.OnLeftMouseClicked(info);
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
