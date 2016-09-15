using System;
using SadConsole.Consoles;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class Entity : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "entity", "object" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Game Object";
            }
        }

        public ITextSurfaceRendered Load(string file)
        {
            if (System.IO.Path.GetExtension(file) == ".scene")
            {

            }

            return SadConsole.Game.GameObject.Load(file);
        }

        public void Save(ITextSurfaceRendered surface, string file)
        {
            if (surface is LayeredTextSurface)
                ((LayeredTextSurface)surface).Save(file, typeof(LayerMetadata));
        }
    }
}
