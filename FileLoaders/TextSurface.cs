using System;
using SadConsole.Surfaces;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class BasicSurface : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "surface" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Single Surface";
            }
        }

        public object Load(string file)
        {
            return SadConsole.Surfaces.BasicSurface.Load(file);
        }

        public void Save(object surface, string file)
        {
            ((SadConsole.Surfaces.BasicSurface)surface).Save(file);
        }
    }
}
