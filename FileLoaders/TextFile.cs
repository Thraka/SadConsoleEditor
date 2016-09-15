using System;
using SadConsole.Consoles;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class TextFile : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "txt" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "ASCII Text";
            }
        }

        public ITextSurfaceRendered Load(string file)
        {
            string[] text = System.IO.File.ReadAllLines(file);
            int maxLineWidth = text.Max(s => s.Length);
            
            SadConsole.Consoles.Console importConsole = new SadConsole.Consoles.Console(maxLineWidth, text.Length);
            importConsole.VirtualCursor.AutomaticallyShiftRowsUp = false;

            foreach (var line in text)
                importConsole.VirtualCursor.Print(line);

            return importConsole.TextSurface;
        }

        public void Save(ITextSurfaceRendered surface, string file)
        {
            SurfaceEditor editor = new SurfaceEditor(surface);
            string[] lines = new string[surface.Height];

            for (int y = 0; y < surface.Height; y++)
                lines[y] = editor.GetString(0, y, surface.Width);

            System.IO.File.WriteAllLines(file, lines);
        }
    }
}
