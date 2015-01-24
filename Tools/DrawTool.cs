namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Input;
    using Panels;

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
        public char Hotkey { get { return 'p'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        private EntityBrush _brush;

        public PaintTool()
        {
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
            _brush = new EntityBrush();
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            _brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);
            _brush.IsVisible = false;
        }


        public void OnDeselected()
        {
        }

        public void RefreshTool()
        {
            _brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);
        }

        public bool ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            return false;
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
            _brush.IsVisible = true;
            _brush.Position = info.ConsoleLocation;

            if (info.LeftButtonDown)
            {
                var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];
                cell.CharacterIndex = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter;
                cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;
                cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
                cell.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            }

            if (info.RightButtonDown)
            {
                var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];

                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter = cell.CharacterIndex;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground = cell.Foreground;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground = cell.Background;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect = cell.SpriteEffect;
            }
        }
    }
}
