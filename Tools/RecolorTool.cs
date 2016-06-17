namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Consoles;
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
        public char Hotkey { get { return 'r'; } }

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
            _brush = new EntityBrush(1, 1);
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            Settings.QuickEditor.TextSurface = _brush.CurrentAnimation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
            _brush.IsVisible = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;
		}

        public void OnDeselected()
        {
		}

        public void RefreshTool()
        {
            Settings.QuickEditor.TextSurface = _brush.CurrentAnimation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseEnterSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.Position = info.ConsoleLocation;
            _brush.IsVisible = true;

            if (info.LeftButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                if (!_settingsPanel.IgnoreForeground)
                    cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;

                if (!_settingsPanel.IgnoreBackground)
                    cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
            }
            else if (info.RightButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                if (!_settingsPanel.IgnoreForeground)
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground = cell.Foreground;

                if (!_settingsPanel.IgnoreBackground)
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground = cell.Background;
            }
        }
    }
}
