using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsoleEditor.Panels;

namespace SadConsoleEditor.Tools
{
    interface ITool
    {
        string Id { get; }

        string Title { get; }

        char Hotkey { get; }

        void OnSelected();

        void OnDeselected();

        bool ProcessKeyboard(KeyboardInfo info, CellSurface surface);

        void ProcessMouse(MouseInfo info, CellSurface surface);

        void MouseEnterSurface(MouseInfo info, CellSurface surface);

        void MouseExitSurface(MouseInfo info, CellSurface surface);

        void MouseMoveSurface(MouseInfo info, CellSurface surface);

        void RefreshTool();

        CustomPanel[] ControlPanels { get; }
    }
}
