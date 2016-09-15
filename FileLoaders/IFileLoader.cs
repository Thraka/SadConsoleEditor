using SadConsole.Consoles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsoleEditor.FileLoaders
{
    public interface IFileLoader
    {
        bool SupportsLoad { get; }
        bool SupportsSave { get; }
        string FileTypeName { get; }
        string[] Extensions { get; }
        ITextSurfaceRendered Load(string file);
        void Save(ITextSurfaceRendered surface, string file);
    }
}
