#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace SadConsoleEditor
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Load our program settings
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ProgramSettings));
            using (var fileObject = System.IO.File.OpenRead("Settings.json"))
                Settings.Config = serializer.ReadObject(fileObject) as ProgramSettings;


            // Setup the engine and creat the main window.
            SadConsole.Engine.Initialize(Settings.Config.ProgramFontFile, Settings.Config.WindowWidth, Settings.Config.WindowHeight);

            // Hook the start event so we can add consoles to the system.
            SadConsole.Engine.EngineStart += Engine_EngineStart;
            
            // Start the game.
            SadConsole.Engine.Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Engine_EngineStart(object sender, EventArgs e)
        {
            SadConsole.Engine.MonoGameInstance.Window.Title = "SadConsole Editor - v" + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();

            
            // Load screen font
            var font = SadConsole.Engine.LoadFont(Settings.Config.ScreenFontFile);
            Settings.Config.ScreenFont = font.GetFont(SadConsole.Font.FontSizes.One);

            // Setup GUI themes
            Settings.SetupThemes();

            // Helper editor for any text surface
            Settings.QuickEditor = new SadConsole.Consoles.SurfaceEditor(new SadConsole.Consoles.TextSurface(10, 10, SadConsole.Engine.DefaultFont));

            // Start
            SadConsole.Libraries.GameHelpers.Initialize();

            // Setup system to run
            EditorConsoleManager.Initialize();
        }
    }
#endif
}
