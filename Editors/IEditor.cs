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
using SadConsoleEditor.FileLoaders;

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

        IEnumerable<IFileLoader> FileExtensionsLoad { get; }
        IEnumerable<IFileLoader> FileExtensionsSave { get; }

        ITextSurface Surface { get; }

        CustomPanel[] ControlPanels { get; }

        void ProcessKeyboard(KeyboardInfo info);

        void ProcessMouse(MouseInfo info);

        void Resize(int width, int height);

        void Position(int x, int y);

        Point GetPosition();

        void Render();

        void Update();

        void Load(string file, IFileLoader loader);

        void Save(string file, IFileLoader loader);

        void Reset();

        void RemoveLayer(int index);

        void MoveLayerUp(int index);

        void MoveLayerDown(int index);

        void AddNewLayer(string name);

        bool LoadLayer(string file);

        void SaveLayer(int index, string file);

        void SetActiveLayer(int index);

        void OnSelected();

        void OnDeselected();

        void OnClosed();
    }
}
