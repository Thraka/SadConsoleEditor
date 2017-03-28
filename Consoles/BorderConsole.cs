using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Surfaces;
using SadConsole.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Consoles
{
    class BorderConsole: SadConsole.Console
    {
        public BorderConsole(int width, int height): base(width, height)
        {
            textSurface.Font = Settings.Config.ScreenFont;
            
            PrepBox();
        }

        // Todo: Tint console? Draw yellow dashes (line control?) for the console bounds
        public void PrepBox()
        {
            Clear();

            var box = Box.GetDefaultBox();
            box.Width = TextSurface.Width;
            box.Height = TextSurface.Height;
            box.Fill = false;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;
            box.Draw(this);

            //List<Cell> newAreaCells = new List<Cell>(TextSurface.Width * 2 + TextSurface.Height * 2);
            //List<Rectangle> newAreaRects = new List<Rectangle>(TextSurface.Width * 2 + TextSurface.Height * 2);

            //if (TextSurface.Width - 2 > EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth)
            //{
            //    for (int x = 1; x <= (TextSurface.Width - 2) / EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth; x++)
            //    {
            //        Line line = new Line();
            //        line.Cell = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, GlyphIndex = 124 };
            //        line.UseStartingCell = false;
            //        line.UseEndingCell = false;
            //        line.StartingLocation = new Point((x * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth), 1);
            //        line.EndingLocation = new Point((x * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsWidth), TextSurface.Height - 2);
            //        line.Draw(this);
            //    }
            //}

            //if (TextSurface.Height - 2 > EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight)
            //{
            //    for (int y = 1; y <= (TextSurface.Height - 2) / EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight; y++)
            //    {
            //        Line line = new Line();
            //        line.Cell = new Cell() { Foreground = Color.Yellow * 0.5f, Background = Color.Transparent, GlyphIndex = 45 };
            //        line.UseStartingCell = false;
            //        line.UseEndingCell = false;
            //        line.StartingLocation = new Point(1, (y * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight));
            //        line.EndingLocation = new Point(TextSurface.Width - 2, (y * EditorConsoleManager.Instance.SelectedEditor.Settings.BoundsHeight));
            //        line.Draw(this);
            //    }
            //}
        }
    }
}
