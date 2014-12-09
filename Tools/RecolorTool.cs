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

        private RecolorToolPanel _settingsPanel;

        public override string ToString()
        {
            return Title;
        }

        public RecolorTool()
        {
            _settingsPanel = new RecolorToolPanel();

            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel, _settingsPanel };
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(new SadConsole.Entities.Entity());
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
            EditorConsoleManager.Instance.Brush.IsVisible = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;
        }

        public void OnDeselected()
        {

        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
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

                    if (!_settingsPanel.IgnoreForeground)
                        cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;

                    if (!_settingsPanel.IgnoreBackground)
                        cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
                }
            }
        }
    }
}
