namespace SadConsoleEditor.Controls
{
    using Microsoft.Xna.Framework;
    using System;
    using SadConsole.Input;

    class ColorPresenter : SadConsole.Controls.ControlBase
    {
        public event EventHandler ColorChanged;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;

                    if (ColorChanged != null)
                        ColorChanged(this, EventArgs.Empty);
                }
            }
        }

        private Color _selectedColor;
        private string _title;
        private Windows.ColorPickerPopup _popup;
        
        public ColorPresenter(string title, Color foreground, int width)
        {
            Resize(width, 2);
            DefaultForeground = foreground;
            Clear();
            _title = title;
            _popup = new Windows.ColorPickerPopup();
            _popup.Closed += (o, e) =>
                {
                    if (_popup.DialogResult)
                        SelectedColor = _popup.SelectedColor;
                };
        }

        public override void Compose()
        {
            Clear();

            Print(0, 0, _title);

            Print(_width - 3, 0, "   ", Color.Black, _selectedColor);
        }

        public override void DetermineAppearance()
        {

        }

        protected override void OnLeftMouseClicked(MouseInfo info)
        {
            base.OnLeftMouseClicked(info);

            var location = this.TransformConsolePositionByControlPosition(info);
            if (location.X >= _width - 3)
            {
                _popup.SelectedColor = _selectedColor;
                _popup.Show(true);
            }
        }
    }
}
