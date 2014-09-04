namespace SadConsoleEditor.Controls
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using System;
    using System.Linq;
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
            set
            {
                if (_selectedColor != value)
                {
                    SetClosestIndex(value);
                    _selectedColor = value;
                    if (SelectedColorChanged != null)
                        SelectedColorChanged(this, EventArgs.Empty);
                }
            }
        }

        private Color SelectedColorSafe
        {
            get { return _selectedColor; }
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    if (SelectedColorChanged != null)
                        SelectedColorChanged(this, EventArgs.Empty);
                }
            }
        }

        public Color SelectedHue
        {
            get { return _selectedHue; }
            set { _selectedHue = value; IsDirty = true; Compose(); ResetSelectedColor(); }
        }

       
        public ColorPicker(int width, int height, Color hue): base()
        {
            this.Resize(width, height);
            SelectedHue = hue;
            Compose();

            _selectedColorPosition = new Point(0, 0);
            ResetSelectedColor();
            this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 4;
        }

        private void ResetSelectedColor()
        {
            SelectedColorSafe = this[_selectedColorPosition.X, _selectedColorPosition.Y].Background;
        }

        private void SetClosestIndex(Color color)
        {
            ColorMine.ColorSpaces.Rgb rgbColorStop = new ColorMine.ColorSpaces.Rgb() { R = color.R, G = color.G, B = color.B };
            Tuple<Color, double, int>[] colorWeights = new Tuple<Color, double, int>[Cells.Length];

            // Create a color weight for every cell compared to the color stop
            for (int x = 0; x < Cells.Length; x++)
            {
                ColorMine.ColorSpaces.Rgb rgbColor = new ColorMine.ColorSpaces.Rgb() { R = this[x].Background.R, G = this[x].Background.G, B = this[x].Background.B };
                ColorMine.ColorSpaces.Cmy cmyColor = rgbColor.To<ColorMine.ColorSpaces.Cmy>();

                colorWeights[x] = new Tuple<Color, double, int>(this[x].Background, rgbColorStop.Compare(cmyColor, new ColorMine.ColorSpaces.Comparisons.Cie1976Comparison()), x);

            }

            var foundColor = colorWeights.OrderBy(t => t.Item2).First();

            this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 0;
            _selectedColorPosition = this[foundColor.Item3].Position;
            this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 4;

            this.IsDirty = true;
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

            if (Parent.CapturedControl == null)
            {
                if (info.LeftButtonDown)
                {
                    var location = this.TransformConsolePositionByControlPosition(info);
                    this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 0;
                    _selectedColorPosition = location;
                    SelectedColorSafe = this[_selectedColorPosition.X, _selectedColorPosition.Y].Background;
                    this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 4;
                    IsDirty = true;

                    Parent.CaptureControl(this);
                }
            }
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            if (Parent.CapturedControl == this)
            {
                if (info.LeftButtonDown == false)
                    Parent.ReleaseControl();
                else
                {
                    var location = this.TransformConsolePositionByControlPosition(info);

                    //if (info.ConsoleLocation.X >= Position.X && info.ConsoleLocation.X < Position.X + Width)
                    if (location.X >= -6 && location.X <= _width + 5 && location.Y > -4 && location.Y < _height + 3)
                    {
                        this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 0;
                        _selectedColorPosition = new Point(MathHelper.Clamp(location.X, 0, _width - 1), MathHelper.Clamp(location.Y, 0, _height - 1));
                        SelectedColorSafe = this[_selectedColorPosition.X, _selectedColorPosition.Y].Background;
                        this[_selectedColorPosition.X, _selectedColorPosition.Y].CharacterIndex = 4;
                    }

                    IsDirty = true;
                }
            }

            return base.ProcessMouse(info);
        }

        public override void DetermineAppearance()
        {
            
        }
    }
}
