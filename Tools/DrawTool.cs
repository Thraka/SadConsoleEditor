using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    class PaintTool: ITool
    {
        public const string ID = "PENCIL";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Pencil"; }
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.Brush = new SadConsole.Entities.Entity();
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor, EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor, EditorConsoleManager.Instance.ToolPane.SelectedCharacter, null);
        }

        public void OnDeselected()
        {

        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            
        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            
        }
    }
}
