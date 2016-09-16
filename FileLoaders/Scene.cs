using System;
using SadConsole.Consoles;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class Scene : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "scene", "layers" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Scene";
            }
        }

        public ITextSurfaceRendered Load(string file)
        {
            if (System.IO.Path.GetExtension(file) == ".scene")
            {

            }

            return SadConsole.Consoles.LayeredTextSurface.Load(file, typeof(LayerMetadata));
        }

        public void Save(ITextSurfaceRendered surface, string file)
        {
            if (surface is SadConsole.Consoles.LayeredTextSurface)
                ((SadConsole.Consoles.LayeredTextSurface)surface).Save(file, typeof(LayerMetadata));
        }
    }
}
