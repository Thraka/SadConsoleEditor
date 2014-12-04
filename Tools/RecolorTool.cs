namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Input;

    class RecolorTool : ITool
    {
        public const string ID = "RECOLOR";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Recolor"; }
        }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(new SadConsole.Entities.Entity());
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor, EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor, 42, null);
            EditorConsoleManager.Instance.Brush.IsVisible = false;

            
        }

        public void OnDeselected()
        {

        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor, EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor, 42, null);
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {

        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            if (info.LeftButtonDown)
            {
                //if (info.Console == surface)
                {
                    var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];
                    cell.CharacterIndex = EditorConsoleManager.Instance.ToolPane.SelectedCharacter;
                    cell.Foreground = EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor;
                    cell.Background = EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor;
                }
            }
        }

        public void MouseEnterSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.Position = info.ConsoleLocation;
        }
    }
}
