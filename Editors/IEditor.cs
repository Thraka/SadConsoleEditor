using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Surfaces;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SadConsoleEditor.Panels;
using SadConsoleEditor.FileLoaders;
using SadConsoleEditor.Panels;

namespace SadConsoleEditor.Editors
{
    public enum Editors
    {
        Console,
        GameObject,
        Scene,
        GUI
    }

    public interface IEditor
    {
        Editors EditorType { get; }

        string EditorTypeName { get; }

        string Title { get; set; }

        CustomPanel[] Panels { get; }

        int Width { get; }

        int Height { get; }

        string DocumentTitle { get; }

        Point Position { get; }

        SadConsole.Console RenderedConsole { get; }

        void Render();

        void Update();

        bool ProcessKeyboard(Keyboard info);

        void Move(int x, int y);

        void New(Color foreground, Color background, int width, int height);

        void Load(string file, IFileLoader loader);

        void Save();

        void Resize(int width, int height);

        void Reset();

        void OnSelected();

        void OnDeselected();

        void OnClosed();
    }
}
