using System;
using SadConsole.Surfaces;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class LayeredSurface : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "console" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Layered Surface";
            }
        }

        public object Load(string file)
        {
            return SadConsole.Surfaces.LayeredSurface.Load(file, typeof(LayerMetadata));
        }

        public void Save(object surface, string file)
        {
            ((SadConsole.Surfaces.LayeredSurface)surface).Save(file, typeof(LayerMetadata));
        }
    }
}
