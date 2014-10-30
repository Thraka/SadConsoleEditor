using SadConsole;
using SadConsole.Consoles;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Editors
{
    public interface IEditor
    {
        EventHandler<MouseEventArgs> MouseEnter { get; set; }
        EventHandler<MouseEventArgs> MouseExit { get; set; }
        EventHandler<MouseEventArgs> MouseMove { get; set; }

        int Width { get; }
        int Height { get; }

        string Title { get; }

        string Id { get; }

        string[] Tools { get; }

        string FileExtensions { get; }

        SadConsole.Consoles.Console Surface { get; }

        CustomPane[] ControlPanes { get; }

        void ProcessKeyboard(KeyboardInfo info);

        void ProcessMouse(MouseInfo info);

        void Resize(int width, int height);

        void Position(int x, int y);

        void Load(string file);

        void Save(string file);
    }
}
