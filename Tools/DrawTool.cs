namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Input;
    using Panels;
    using SadConsole.Consoles;
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
            _brush = new EntityBrush(1, 1);
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            Settings.QuickEditor.TextSurface = _brush.Animation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, 
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);
            _brush.IsVisible = false;

			EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel, System.EventArgs.Empty);
			EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed += EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = true;
		}


        public void OnDeselected()
        {
			EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed -= EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = false;
		}

        public void RefreshTool()
        {
            Settings.QuickEditor.TextSurface = _brush.Animation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, 
                                      EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);
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
            _brush.IsVisible = true;
            _brush.Position = info.ConsoleLocation;

            if (info.LeftButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                cell.GlyphIndex = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter;
                cell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;
                cell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
                cell.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            }

            if (info.RightButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter = cell.GlyphIndex;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground = cell.Foreground;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground = cell.Background;
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect = cell.SpriteEffect;
            }
        }
    }
}
