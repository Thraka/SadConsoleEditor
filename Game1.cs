#region Using Statements
using System;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.Xml.Linq;
#endregion

namespace SadConsoleEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "SadConsole Editor - v" + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();

            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ProgramSettings));
            using (var fileObject = System.IO.File.OpenRead("Settings.json"))
                Settings.Config = serializer.ReadObject(fileObject) as ProgramSettings;
            
            var sadConsoleComponent = new SadConsole.EngineGameComponent(this, graphics, Settings.Config.ProgramFontFile, Settings.Config.WindowWidth, Settings.Config.WindowHeight, () =>
            {
                // Load settings 
                var font = SadConsole.Engine.LoadFont(Settings.Config.ScreenFontFile);
                Settings.Config.ScreenFont = font.GetFont(SadConsole.Font.FontSizes.One);
                
                SadConsole.Engine.UseMouse = true;
                SadConsole.Engine.UseKeyboard = true;

                Settings.SetupThemes();

                Settings.QuickEditor = new SadConsole.Consoles.SurfaceEditor(new SadConsole.Consoles.TextSurface(10, 10, SadConsole.Engine.DefaultFont));

                SadConsole.Engine.ConsoleRenderStack = EditorConsoleManager.Instance;
                SadConsole.Engine.ActiveConsole = EditorConsoleManager.Instance;

                graphics.ApplyChanges();
                EditorConsoleManager.Instance.ShowNewConsolePopup(false);
            });

            Components.Add(sadConsoleComponent);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //SadConsole.Engine.Update(gameTime, this.IsActive);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Settings.Color_WorkAreaBack);

            //SadConsole.Engine.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
