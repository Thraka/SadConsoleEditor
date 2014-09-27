using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
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

        public void PrepBox()
        {
            _cellData.Clear();

            var box = SadConsole.Shapes.Box.GetDefaultBox();
            box.Width = _cellData.Width;
            box.Height = _cellData.Height;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;
            box.Draw(_cellData);

            List<Cell> newAreaCells = new List<Cell>(_cellData.Width * 2 + _cellData.Height * 2);
            List<Rectangle> newAreaRects = new List<Rectangle>(_cellData.Width * 2 + _cellData.Height * 2);

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
