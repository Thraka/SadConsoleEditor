using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    interface ITool
    {
        string Id { get; }

        string Title { get; }

        void OnSelected();

        void OnDeselected();

        void ProcessKeyboard(KeyboardInfo info, CellSurface surface);

        void ProcessMouse(MouseInfo info, CellSurface surface);

        void RefreshTool();

        CustomPane[] ControlPanes { get; }
    }
}
