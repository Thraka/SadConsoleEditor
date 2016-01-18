using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsoleEditor.Panels;

namespace SadConsoleEditor.Editors
{
    public interface IEditor
    {
        EditorSettings Settings { get; }

        event EventHandler<MouseEventArgs> MouseEnter;
        event EventHandler<MouseEventArgs> MouseExit;
        event EventHandler<MouseEventArgs> MouseMove;

        int Width { get; }
        int Height { get; }

        string Title { get; }

        string ShortName { get; }

        string Id { get; }

        string[] Tools { get; }

        string FileExtensionsLoad { get; }
        string FileExtensionsSave { get; }

        SadConsole.Consoles.LayeredConsole Surface { get; }

        CustomPanel[] ControlPanels { get; }

        void ProcessKeyboard(KeyboardInfo info);

        void ProcessMouse(MouseInfo info);

        void Resize(int width, int height);

        void Position(int x, int y);

        Point GetPosition();

        void Render();

        void Load(string file);

        void Save(string file);
        void Reset();

        void RemoveLayer(int index);

        void MoveLayerUp(int index);

        void MoveLayerDown(int index);

        void AddNewLayer(string name);

        bool LoadLayer(string file);

        void SaveLayer(int index, string file);

        void SetActiveLayer(int index);
    }
}
