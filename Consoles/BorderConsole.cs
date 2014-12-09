using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
using SadConsole.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Consoles
{
    class BorderRenderer: SadConsole.Consoles.Console
    {
        public BorderRenderer() { }

        protected override void OnResize()
        {
            base.OnResize();

            PrepBox();
        }
        // Todo: Tint console? Draw yellow dashes (line control?) for the console bounds
        public void PrepBox()
        {
            _cellData.Clear();

            var box = Box.GetDefaultBox();
            box.Width = _cellData.Width;
            box.Height = _cellData.Height;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;
            box.Draw(_cellData);

            List<Cell> newAreaCells = new List<Cell>(_cellData.Width * 2 + _cellData.Height * 2);
            List<Rectangle> newAreaRects = new List<Rectangle>(_cellData.Width * 2 + _cellData.Height * 2);

            if (_cellData.Width - 2 > Settings.BoundsWidth)
            {
                for (int x = 1; x <= (_cellData.Width - 2) / Settings.BoundsWidth; x++)
                {
                    Line line = new Line();
                    line.CellAppearance = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, CharacterIndex = 124 };
                    line.UseStartingCell = false;
                    line.UseEndingCell = false;
                    line.StartingLocation = new Point((x * Settings.BoundsWidth), 1);
                    line.EndingLocation = new Point((x * Settings.BoundsWidth), _cellData.Height - 2);
                    line.Draw(_cellData);
                }
            }

            if (_cellData.Height - 2 > Settings.BoundsHeight)
            {
                for (int y = 1; y <= (_cellData.Height - 2) / Settings.BoundsHeight; y++)
                {
                    Line line = new Line();
                    line.CellAppearance = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, CharacterIndex = 45 };
                    line.UseStartingCell = false;
                    line.UseEndingCell = false;
                    line.StartingLocation = new Point(1, (y * Settings.BoundsHeight));
                    line.EndingLocation = new Point(_cellData.Width - 2, (y * Settings.BoundsHeight));
                    line.Draw(_cellData);
                }
            }

            foreach (var cell in _cellData)
            {
                if (cell.CharacterIndex != 0)
                {
                    newAreaCells.Add(_renderAreaCells[cell.Index]);
                    newAreaRects.Add(_renderAreaRects[cell.Index]);
                }
            }

            

            _renderAreaCells = newAreaCells.ToArray();
            _renderAreaRects = newAreaRects.ToArray();
        }
    }
}
