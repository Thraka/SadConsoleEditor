namespace SadConsoleEditor.Controls
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using System;
    using Console = SadConsole.Consoles.Console;

    class ColorPicker : SadConsole.Controls.ControlBase
    {
        private Color _selectedColor;
        private Color _selectedHue;
        private Point _selectedColorPosition;

        public event EventHandler SelectedColorChanged;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; RefreshColor(); if (SelectedColorChanged != null) SelectedColorChanged(this, EventArgs.Empty); }
        }

        public Color SelectedHue
        {
            get { return _selectedHue; }
            set { _selectedHue = value; IsDirty = true; GetSelectedColor(); }
        }

       
        public ColorPicker(): base()
        {
            this.Resize(40, 20);

            SelectedHue = Color.Green;
            
        }

        private void GetSelectedColor()
        {

        }

        private void RefreshColor()
        {
            
        }

        public override void Compose()
        {
            if (IsDirty)
            {
                Color[] colors = Color.White.LerpSteps(Color.Black, _height);
                Color[] colorsEnd = _selectedHue.LerpSteps(Color.Black, _height);

                for (int y = 0; y < _height; y++)
                {
                    this[0, y].Background = colors[y];
                    this[_width - 1, y].Background = colorsEnd[y];

                    this[0, y].Foreground = new Color(255 - colors[y].R, 255 - colors[y].G, 255 - colors[y].B);
                    this[_width - 1, y].Foreground = new Color(255 - colorsEnd[y].R, 255 - colorsEnd[y].G, 255 - colorsEnd[y].B);



                    Color[] rowColors = colors[y].LerpSteps(colorsEnd[y], _width);

                    for (int x = 1; x < _width - 1; x++)
                    {
                        this[x, y].Background = rowColors[x];
                        this[x, y].Foreground = new Color(255 - rowColors[x].R, 255 - rowColors[x].G, 255 - rowColors[x].B);
                    }
                }

                IsDirty = false;
            }
        }

        protected override void OnMouseIn(SadConsole.Input.MouseInfo info)
        {
            base.OnMouseIn(info);

            if (info.LeftButtonDown)
            {
                SelectedColor = info.Cell.ActualBackground;
                this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 0;
                _selectedColorPosition = info.ConsoleLocation - this.Position;
                this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 4;
                
            }
        }

        public override void DetermineAppearance()
        {
            
        }
    }
}
