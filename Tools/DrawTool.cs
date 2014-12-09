namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Input;

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

        public CustomPanel[] ControlPanels { get; private set; }

        public PaintTool()
        {
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(new SadConsole.Entities.Entity());
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null);
            EditorConsoleManager.Instance.Brush.IsVisible = false;

            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed += CommonCharacterPickerPanel_Changed;
        }

        void CommonCharacterPickerPanel_Changed(object sender, System.EventArgs e)
        {
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null);
        }

        public void OnDeselected()
        {
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed -= CommonCharacterPickerPanel_Changed;
        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null);
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            
        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {

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

            if (info.LeftButtonDown)
            {
                //if (info.Console == surface)
                {
                    var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];
                    cell.CharacterIndex = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter;
                    cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;
                    cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
                }
            }
        }
    }
}
