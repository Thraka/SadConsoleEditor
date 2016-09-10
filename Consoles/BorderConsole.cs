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
        public BorderRenderer(int width, int height): base(width, height)
        {
            textSurface.Font = Settings.Config.ScreenFont;
            Renderer = new CachedTextSurfaceRenderer(textSurface);
            
            PrepBox();
        }

        // Todo: Tint console? Draw yellow dashes (line control?) for the console bounds
        public void PrepBox()
        {
            Clear();

            var box = Box.GetDefaultBox();
            box.Width = textSurface.Width;
            box.Height = textSurface.Height;
            box.Fill = false;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;
            box.Draw(this);

            //List<Cell> newAreaCells = new List<Cell>(textSurface.Width * 2 + textSurface.Height * 2);
            //List<Rectangle> newAreaRects = new List<Rectangle>(textSurface.Width * 2 + textSurface.Height * 2);

            //if (textSurface.Width - 2 > EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth)
            //{
            //    for (int x = 1; x <= (textSurface.Width - 2) / EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth; x++)
            //    {
            //        Line line = new Line();
            //        line.CellAppearance = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, GlyphIndex = 124 };
            //        line.UseStartingCell = false;
            //        line.UseEndingCell = false;
            //        line.StartingLocation = new Point((x * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth), 1);
            //        line.EndingLocation = new Point((x * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth), textSurface.Height - 2);
            //        line.Draw(this);
            //    }
            //}

            //if (textSurface.Height - 2 > EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight)
            //{
            //    for (int y = 1; y <= (textSurface.Height - 2) / EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight; y++)
            //    {
            //        Line line = new Line();
            //        line.CellAppearance = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, GlyphIndex = 45 };
            //        line.UseStartingCell = false;
            //        line.UseEndingCell = false;
            //        line.StartingLocation = new Point(1, (y * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight));
            //        line.EndingLocation = new Point(textSurface.Width - 2, (y * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight));
            //        line.Draw(this);
            //    }
            //}

            ((CachedTextSurfaceRenderer)Renderer).Update(textSurface);
        }
    }
}
