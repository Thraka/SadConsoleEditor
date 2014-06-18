using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SadConsoleEditor
{
    class EditorConsoleManager: ConsoleList
    {
        public EditorConsoleManager()
        {
            ControlsConsole console = new ControlsConsole(Game1.WindowSize.X, 1);

            console.CellData.DefaultBackground = Settings.Color_MenuBack;
            console.CellData.Clear();

            console.IsVisible = true;

            Color Green = new Color(165, 224, 45);
            Color Red = new Color(246, 38, 108);
            Color Blue = new Color(100, 217, 234);
            Color Grey = new Color(117, 111, 81);
            Color Yellow = new Color(226, 218, 110);
            Color Orange = new Color(251, 149, 31);

            console.CellData.Print(0, 0, "Test", Green);
            console.CellData.Print(5, 0, "Test", Red);
            console.CellData.Print(10, 0, "Test", Blue);
            console.CellData.Print(15, 0, "Test", Grey);
            console.CellData.Print(20, 0, "Test", Yellow);
            console.CellData.Print(25, 0, "Test", Orange);

            this.Add(console);

            var editContainer = new Consoles.EditingContainer();
            editContainer.CellData = new SadConsole.CellSurface(10, 10);
            editContainer.CellData.DefaultBackground = Color.Black;
            editContainer.CellData.Clear();
            editContainer.CenterEditor();
            this.Add(editContainer);

            var toolsPane = new Consoles.ToolPane();
            toolsPane.Position = new Point(console.CellData.Width - toolsPane.CellData.Width, 1);
            this.Add(toolsPane);
        }
    }
}
