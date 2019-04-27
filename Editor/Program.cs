using System;
using SadConsole;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole.Input;
using Console = SadConsole.Console;

namespace SadConsoleEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            //SadConsole.Settings.UnlimitedFPS = true;
            //SadConsole.Settings.UseHardwareFullScreen = true;
            SadConsole.Settings.UseDefaultExtendedFont = true;

            // Load our program settings
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ProgramSettings));
            using (var fileObject = System.IO.File.OpenRead("Settings.json"))
                Config.Program = serializer.ReadObject(fileObject) as ProgramSettings;

            // Setup the engine and creat the main window.
            SadConsole.Game.Create(Config.Program.WindowWidth, Config.Program.WindowHeight);
            //SadConsole.Engine.Initialize("IBM.font", 80, 25, (g) => { g.GraphicsDeviceManager.HardwareModeSwitch = false; g.Window.AllowUserResizing = true; });

            // Hook the start event so we can add consoles to the system.
            SadConsole.Game.OnInitialize = Init;

            // Start the game.
            SadConsole.Game.Instance.Run();

            //
            // Code here will not run until the game has shut down.
            //

            SadConsole.Game.Instance.Dispose();
        }

        private static void Init()
        {
            Config.Program.ScreenFont = Global.FontDefault;

            // Any setup
            if (Settings.UnlimitedFPS)
                SadConsole.Game.Instance.Components.Add(new SadConsole.Game.FPSCounterComponent(SadConsole.Game.Instance));

            SadConsole.Game.Instance.Window.Title = "SadConsole Editor - v" + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();
            SadConsole.Themes.Library.Default.WindowTheme.BorderLineStyle = CellSurface.ConnectedLineThick;

            //Global.MouseState.ProcessMouseWhenOffScreen = true;

            // We'll instead use our demo consoles that show various features of SadConsole.
            Global.CurrentScreen = new MainConsole();

            MainConsole.Instance.ShowStartup();
        }
    }
}