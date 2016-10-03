namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Game;
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

        private RecolorToolPanel settingsPanel;

        public GameObject Brush;

        public override string ToString()
        {
            return Title;
        }

        public RecolorTool()
        {
            settingsPanel = new RecolorToolPanel();

            ControlPanels = new CustomPanel[] { settingsPanel, CharacterPickPanel.SharedInstance };
        }

        public void OnSelected()
        {
            Brush = new SadConsole.Game.GameObject(Settings.Config.ScreenFont);
            Brush.Animation = new AnimatedTextSurface("default", 1, 1);
            Brush.Animation.CreateFrame();
            Brush.IsVisible = false;
            RefreshTool();
            EditorConsoleManager.Brush = Brush;
            EditorConsoleManager.UpdateBrush();

            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            CharacterPickPanel.SharedInstance.HideCharacter = true;
        }

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            CharacterPickPanel.SharedInstance.HideCharacter = false;
        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
            Settings.QuickEditor.TextSurface = Brush.Animation.Frames[0];
            Settings.QuickEditor.Fill(CharacterPickPanel.SharedInstance.SettingForeground,
                                      CharacterPickPanel.SharedInstance.SettingBackground, 42);
        }

        public void Update()
        {
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
            Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.Position = info.ConsoleLocation;
            Brush.IsVisible = true;

            if (info.LeftButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                if (!settingsPanel.IgnoreForeground)
                    cell.Foreground = CharacterPickPanel.SharedInstance.SettingForeground;

                if (!settingsPanel.IgnoreBackground)
                    cell.Background = CharacterPickPanel.SharedInstance.SettingBackground;
            }
            else if (info.RightButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                if (!settingsPanel.IgnoreForeground)
                    CharacterPickPanel.SharedInstance.SettingForeground = cell.Foreground;

                if (!settingsPanel.IgnoreBackground)
                    CharacterPickPanel.SharedInstance.SettingBackground = cell.Background;
            }
        }
    }
}
