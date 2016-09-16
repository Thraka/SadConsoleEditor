using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;

namespace SadConsoleEditor.Editors
{
    class LayeredConsoleEditor : IEditor
    {
        private LayeredTextSurface textSurface;
        private Console consoleWrapper;

        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.Console; } }

        public string EditorTypeName { get { return "Console"; } }

        public int Height { get { return textSurface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public EditorSettings Settings { get; private set; }

        public int Width { get { return textSurface.Width; } }

        public LayeredConsoleEditor()
        {
            consoleWrapper = new Console(1, 1);
            consoleWrapper.Renderer = new LayeredTextRenderer();
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();

            // Create the new text surface
            textSurface = new LayeredTextSurface(width, height, 1);

            // Update metadata
            LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));
            textSurface.SetActiveLayer(0);

            // Set the text surface as the one we're displaying
            consoleWrapper.TextSurface = textSurface;

            // Update the border
            EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.TextSurface)
            {
                // Load the plain surface
                TextSurface surface = (TextSurface)loader.Load(file);

                // Load up a new layered text surface
                textSurface = new LayeredTextSurface(surface.Width, surface.Height, 1);

                // Setup metadata
                LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));

                // Use the loaded surface
                textSurface.ActiveLayer.Cells = surface.Cells;
                textSurface.SetActiveLayer(0);

                // Set the text surface as the one we're displaying
                consoleWrapper.TextSurface = textSurface;

                // Update the border
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }
        }

        public void Save(string file, FileLoaders.IFileLoader loader)
        {
        }

        public void Reset()
        {

        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);
            EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            //TODO: Offest brush
        }

        public void OnClosed()
        {
        }

        public void OnDeselected()
        {
        }

        public void OnSelected()
        {
        }

        public void Render()
        {
        }

        public void Update()
        {
        }
        
    }
}
