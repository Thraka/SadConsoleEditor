using System;
using SadConsole;
using System.Linq;

namespace SadConsoleEditor.FileLoaders
{
    class Animation : IFileLoader
    {
        public bool SupportsLoad { get { return true; } }

        public bool SupportsSave { get { return true; } }

        public string[] Extensions
        {
            get
            {
                return new string[] { "anim", "animation" };
            }
        }

        public string FileTypeName
        {
            get
            {
                return "Animation";
            }
        }

        public object Load(string file)
        {
            return AnimatedConsole.Load(file);
        }

        public void Save(object surface, string file)
        {
            ((AnimatedConsole)surface).Save(file);
        }
    }
}
