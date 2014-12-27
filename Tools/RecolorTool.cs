namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Input;
    using SadConsoleEditor.Panels;

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

        private EntityBrush _brush;

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
            _brush = new EntityBrush();
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            _brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
            _brush.IsVisible = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;
        }

        public void OnDeselected()
        {

        }

        public void RefreshTool()
        {
            _brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {

        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            
        }

        public void MouseEnterSurface(MouseInfo info, CellSurface surface)
        {
            _brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, CellSurface surface)
        {
            _brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            _brush.Position = info.ConsoleLocation;
            _brush.IsVisible = true;

            if (info.LeftButtonDown)
            {
                var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];

                if (!_settingsPanel.IgnoreForeground)
                    cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;

                if (!_settingsPanel.IgnoreBackground)
                    cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
            }
            else if (info.RightButtonDown)
            {
                var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];

                if (!_settingsPanel.IgnoreForeground)
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground = cell.Foreground;

                if (!_settingsPanel.IgnoreBackground)
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground = cell.Background;
            }
        }
    }
}
