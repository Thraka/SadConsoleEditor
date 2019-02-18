#if XNA
using Microsoft.Xna.Framework;
#endif

using System;
using SadConsole.Input;
using SadConsole.Themes;
using SadConsole.Controls;
using SadConsole;
using Console = SadConsole.Console;

namespace SadConsoleEditor.Controls
{
    class ColorPresenter : SadConsole.Controls.ControlBase
    {
        public class ThemeType: ThemeBase
        {
            /// <inheritdoc />
            public override void Attached(ControlBase control)
            {
                if (!(control is ColorPresenter presenter)) throw new Exception($"Theme can only be added to a {nameof(ColorPresenter)}");

                control.Surface = new CellSurface(control.Width, control.Height);
                control.Surface.DefaultForeground = presenter._selectedColor;
                control.Surface.DefaultBackground = Color.Transparent;
                control.Surface.Clear();
                base.Attached(control);
            }

            /// <inheritdoc />
            public override void UpdateAndDraw(ControlBase control, TimeSpan time)
            {
                if (!(control is ColorPresenter presenter)) return;

                if (!presenter.IsDirty) return;

                Cell appearance;

                if (Helpers.HasFlag(presenter.State, ControlStates.Disabled))
                    appearance = Disabled;

                //else if (Helpers.HasFlag(presenter.State, ControlStates.MouseLeftButtonDown) || Helpers.HasFlag(presenter.State, ControlStates.MouseRightButtonDown))
                //    appearance = MouseDown;

                //else if (Helpers.HasFlag(presenter.State, ControlStates.MouseOver))
                //    appearance = MouseOver;

                else if (Helpers.HasFlag(presenter.State, ControlStates.Focused))
                    appearance = Focused;

                else
                    appearance = Normal;

                var middle = (presenter.Height != 1 ? presenter.Height / 2 : 0);

                // Redraw the control
                presenter.Surface.Fill(
                    appearance.Foreground,
                    appearance.Background,
                    appearance.Glyph, null);

                presenter.Surface.Print(0, 0, presenter._title);

                presenter.Surface.Print(presenter.Surface.Width - 3, 0, "   ", Color.Black, presenter._selectedColor);
                if (presenter._character != 0)
                {
                    presenter.Surface.SetGlyph(presenter.Surface.Width - 2, 0, presenter._character);
                    presenter.Surface.SetForeground(presenter.Surface.Width - 2, 0, presenter._characterColor);
                }
                
                presenter.IsDirty = false;
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


        public event EventHandler ColorChanged;
        public event EventHandler RightClickedColor;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    ColorChanged?.Invoke(this, EventArgs.Empty);
                    IsDirty = true;
                }
            }
        }

        public string Title { get { return _title; } set { _title = value; IsDirty = true; } }

        public Color CharacterColor { get { return _characterColor; } set { _characterColor = value; IsDirty = true; } }
        public int Character { get { return _character; } set { _character = value; IsDirty = true; } }

        public bool DisableColorPicker { get; set; }
        public bool EnableCharacterPicker { get; set; }

        private Color _selectedColor;
        private string _title;
        private Windows.ColorPickerPopup _popup;
        private int _character;
        private Color _characterColor;

        
        public ColorPresenter(string title, Color defaultColor, int width): base(width, 1)
        {
            _selectedColor = defaultColor;
            Theme = new ThemeType();
            _title = title;
            _popup = new Windows.ColorPickerPopup();
            _popup.Closed += (o, e) =>
                {
                    if (_popup.DialogResult)
                        SelectedColor = _popup.SelectedColor;
                };
        }
        
        protected override void OnLeftMouseClicked(MouseConsoleState info)
        {
            if (!DisableColorPicker)
            {
                var location = this.TransformConsolePositionByControlPosition(info.CellPosition);
                if (location.X >= Width - 3)
                {
                    base.OnLeftMouseClicked(info);
                    _popup.SelectedColor = _selectedColor;
                    _popup.Show(true);
                }
            }
            else if (EnableCharacterPicker)
            {
                var location = this.TransformConsolePositionByControlPosition(info.CellPosition);
                if (location.X >= Width - 3)
                {
                    base.OnLeftMouseClicked(info);
                }
            }
        }

        protected override void OnRightMouseClicked(MouseConsoleState info)
        {
            var location = this.TransformConsolePositionByControlPosition(info.CellPosition);
            if (location.X >= Width - 3)
            {
                base.OnRightMouseClicked(info);
                RightClickedColor?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
