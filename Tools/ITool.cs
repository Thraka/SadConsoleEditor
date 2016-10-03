using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsoleEditor.Panels;
using SadConsole.Consoles;

namespace SadConsoleEditor.Tools
{
    interface ITool
    {
        string Id { get; }

        string Title { get; }

        char Hotkey { get; }

        void OnSelected();

        void OnDeselected();

        bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface);

        void ProcessMouse(MouseInfo info, ITextSurface surface);

        void MouseEnterSurface(MouseInfo info, ITextSurface surface);

        void MouseExitSurface(MouseInfo info, ITextSurface surface);

        void MouseMoveSurface(MouseInfo info, ITextSurface surface);

        void RefreshTool();

        void Update();

        CustomPanel[] ControlPanels { get; }
    }
}
