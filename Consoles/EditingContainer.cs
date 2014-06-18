using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Consoles
{
    class EditingContainer : SadConsole.Consoles.Console
    {
        SadConsole.Consoles.CellsRenderer _borderRenderer;

        public EditingContainer()
        {
            CanUseMouse = true;
            CanUseKeyboard = true;
            MoveToFrontOnMouseFocus = false;
            UpdateBox();
        }

        public override void Render()
        {
            // Draw the border for the console around it.

            base.Render();
            _borderRenderer.Render();
        }

        protected override void OnPositionChanged(Microsoft.Xna.Framework.Point oldLocation)
        {
            if (_borderRenderer != null)
                _borderRenderer.Position = new Microsoft.Xna.Framework.Point(this.Position.X - 1, this.Position.Y - 1);
        }

        protected override void OnCellDataChanged(SadConsole.CellSurface oldCells, SadConsole.CellSurface newCells)
        {
            // TODO: Build a new CellsRenderer that only draws cells that are in the cellsurface.
            // Then draw the box, and remove the cells from the cellsurface.
            UpdateBox();
        }

        protected override void OnResize()
        {
            UpdateBox();

            CenterEditor();
        }

        private void UpdateBox()
        {
            _borderRenderer = new SadConsole.Consoles.CellsRenderer(new SadConsole.CellSurface(CellData.Width + 2, CellData.Height + 2), new Microsoft.Xna.Framework.Graphics.SpriteBatch(SadConsole.Engine.Device));
            var box = SadConsole.Shapes.Box.GetDefaultBox();
            box.Width = _borderRenderer.CellData.Width;
            box.Height = _borderRenderer.CellData.Height;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;
            box.Draw(_borderRenderer.CellData);

            foreach (var cell in _borderRenderer.CellData)
            {
                cell.IsVisible = cell.CharacterIndex != 0;
            }
            _borderRenderer.Position = new Microsoft.Xna.Framework.Point(this.Position.X - 1, this.Position.Y - 1);
        }

        public void CenterEditor()
        {
            Point position = this.Position;
            this.ResetViewArea();
            
            var screenSize = SadConsole.Engine.GetScreenSizeInCells(this.Font);

            if (this.CellData.Width < screenSize.X)
                position.X = ((screenSize.X - 20) / 2) - (this.CellData.Width / 2);
            else
                position.X = ((screenSize.X - 20) - this.CellData.Width) / 2;

            if (this.CellData.Height < screenSize.Y)
                position.Y = (screenSize.Y / 2) - (this.CellData.Height / 2);
            else
                position.Y = (screenSize.Y - this.CellData.Height) / 2;

            this.Position = position;
        }

        //private class AbsoluteCellSurface : CellSurface
        //{
        //    public AbsoluteCellSurface(int width, int height)
        //        : base(width, height)
        //    {

        //    }


        //}
    }
}
