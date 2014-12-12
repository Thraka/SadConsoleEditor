﻿#region Using Statements
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

        public static Point WindowSize { get; set; }

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            var sadConsoleComponent = new SadConsole.EngineGameComponent(this, () =>
            {
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Settings.xml");
                Point size = WindowSize = new Point(80, 50);

                if (doc.Root.Element("WindowSize") != null)
                {
                    size.X = int.Parse(doc.Root.Element("WindowSize").Attribute("width").Value);
                    size.Y = int.Parse(doc.Root.Element("WindowSize").Attribute("height").Value);
                    WindowSize = size;
                }
                if (doc.Root.Element("ConsoleBoundsMarker") != null)
                {
                    Settings.BoundsWidth = int.Parse(doc.Root.Element("ConsoleBoundsMarker").Attribute("width").Value);
                    Settings.BoundsHeight = int.Parse(doc.Root.Element("ConsoleBoundsMarker").Attribute("height").Value);
                }

                Settings.NewScreenWidth = int.Parse(doc.Root.Element("NewConsole").Attribute("width").Value);
                Settings.NewScreenHeight = int.Parse(doc.Root.Element("NewConsole").Attribute("height").Value);

                using (var stream = System.IO.File.OpenRead("EditorFont.font"))
                    SadConsole.Engine.DefaultFont = SadConsole.Serializer.Deserialize<SadConsole.Font>(stream);

                using (var stream = System.IO.File.OpenRead(doc.Root.Element("ScreenFont").Value))
                    Settings.ScreenFont = SadConsole.Serializer.Deserialize<SadConsole.Font>(stream);



                SadConsole.Engine.DefaultFont.ResizeGraphicsDeviceManager(graphics, size.X, size.Y, 0, 0);
                SadConsole.Engine.UseMouse = true;
                SadConsole.Engine.UseKeyboard = true;

                Settings.SetupThemes();

                SadConsole.Engine.ConsoleRenderStack = EditorConsoleManager.Instance;
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
