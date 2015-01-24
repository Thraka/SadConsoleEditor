#region Using Statements
using System;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
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

            var sadConsoleComponent = new SadConsole.EngineGameComponent(this, () =>
            {
                // Load settings 
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ProgramSettings));
                using (var fileObject = System.IO.File.OpenRead("Settings.json"))
                    Settings.Config = serializer.ReadObject(fileObject) as ProgramSettings;

                using (var stream = System.IO.File.OpenRead(Settings.Config.ProgramFontFile))
                    SadConsole.Engine.DefaultFont = SadConsole.Serializer.Deserialize<SadConsole.Font>(stream);

                using (var stream = System.IO.File.OpenRead(Settings.Config.ScreenFontFile))
                    Settings.Config.ScreenFont = SadConsole.Serializer.Deserialize<SadConsole.Font>(stream);

                //if (System.IO.File.Exists("Settings.json"))
                //    System.IO.File.Delete("Settings.json");

                //var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ProgramSettings));

                //using (var stream = System.IO.File.OpenWrite("Settings.json"))
                //    serializer.WriteObject(stream, Settings.Config);

                //Settings.Config. = "Cheepicus12.font";


                SadConsole.Engine.DefaultFont.ResizeGraphicsDeviceManager(graphics, Settings.Config.WindowWidth, Settings.Config.WindowHeight, 0, 0);
                SadConsole.Engine.UseMouse = true;
                SadConsole.Engine.UseKeyboard = true;

                Settings.SetupThemes();

                SadConsole.Engine.ConsoleRenderStack = EditorConsoleManager.Instance;
                SadConsole.Engine.ActiveConsole = EditorConsoleManager.Instance;

                EditorConsoleManager.Instance.ShowNewConsolePopup();
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
