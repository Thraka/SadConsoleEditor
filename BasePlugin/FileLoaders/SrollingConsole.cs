using System;
using SadConsole;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    public class ScrollingConsole : IFileLoader
    {

        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "sconsole", "sconsolez" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Scrolling Con.";
            }
        }

        public string Id => "SCROLLING";

        public object Load(string file)
        {
            return SadConsole.Serializer.Load<SadConsole.ScrollingConsole>(file, file.EndsWith('z'));
        }

        public void Save(object surface, string file)
        {
            SadConsole.Serializer.Save<SadConsole.ScrollingConsole>((SadConsole.ScrollingConsole)surface, file, file.EndsWith('z'));
        }
    }
}
