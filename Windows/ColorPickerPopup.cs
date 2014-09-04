using Microsoft.Xna.Framework;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Windows
{
    class ColorPickerPopup: SadConsole.Consoles.Window
    {
        private ColorBar _barR;
        private ColorBar _barG;
        private ColorBar _barB;
        private HueBar _barH;
        private ColorPicker _picker;
        private Color _selectedColor;
        
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    _barH.SelectedColor = _selectedColor;
                    _picker.SelectedColor = _selectedColor;
                }
            }
        }

        public ColorPickerPopup(): base(50, 30)
        {
            _picker = new Controls.ColorPicker(_cellData.Width - 18, 19, Color.YellowGreen) { Position = new Point(1, 1) };
            _picker.SelectedColorChanged += _picker_SelectedColorChanged;
            Add(_picker);

            _barH = new HueBar(base.ViewArea.Width - 2);

            _barH.Position = new Point(1, 21);
            _barH.ColorChanged += _barH_ColorChanged;
            Add(_barH);

            _barR = new ColorBar(base.ViewArea.Width - 2);

            _barR.StartingColor = Color.Black;
            _barR.EndingColor = Color.Red;
            _barR.Position = new Point(1, 23);
            _barR.ColorChanged += bar_ColorChanged;
            Add(_barR);

            _barG = new ColorBar(base.ViewArea.Width - 2);

            _barG.StartingColor = Color.Black;
            _barG.EndingColor = new Color(0, 255, 0);
            _barG.Position = new Point(1, 25);
            _barG.ColorChanged += bar_ColorChanged;
            Add(_barG);

            _barB = new ColorBar(base.ViewArea.Width - 2);

            _barB.StartingColor = Color.Black;
            _barB.EndingColor = Color.Blue;
            _barB.Position = new Point(1, 27);
            _barB.ColorChanged += bar_ColorChanged;
            Add(_barB);


            _selectedColor = _picker.SelectedColor;
            this.Title = "Select Color";
        }

        void _picker_SelectedColorChanged(object sender, EventArgs e)
        {
            _selectedColor = _picker.SelectedColor;
            Redraw();
        }

        void _barH_ColorChanged(object sender, EventArgs e)
        {
            _barR.SelectedColor = new Color(_barH.SelectedColor.R, 0, 0);
            _barG.SelectedColor = new Color(0, _barH.SelectedColor.G, 0);
            _barB.SelectedColor = new Color(0, 0, _barH.SelectedColor.B);
        }

        void bar_ColorChanged(object sender, EventArgs e)
        {
            _picker.SelectedHue = new Color(_barR.SelectedColor.R, _barG.SelectedColor.G, _barB.SelectedColor.B);
        }

        public override void Redraw()
        {
            base.Redraw();

            _cellData[0, 20].CharacterIndex = 199;
            _cellData[_cellData.Width - 1, 20].CharacterIndex = 182;

            for (int x = 1; x < _cellData.Width - 1; x++)
            {
                _cellData[x, 20].CharacterIndex = 196;
            }

            _cellData.Print(_cellData.Width - 16, 1, "Selected Color");

            SadConsole.Shapes.Box box = new SadConsole.Shapes.Box();
            box.Fill = true;
            box.FillColor = SelectedColor;
            box.Width = 12;
            box.Height = 3;
            box.Location = new Point(_cellData.Width - 15, 2);

            box.Draw(_cellData);
        }
        
    }
}
