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

        //private EntityBrush _brush;
        public SadConsole.Game.GameObject Brush;

        public PaintTool()
        {
            ControlPanels = new CustomPanel[] { CharacterPickPanel.SharedInstance };
        }

        public override string ToString()
        {
            return Title;
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
            EditorConsoleManager.QuickSelectPane.IsVisible = true;
        }
        
        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            EditorConsoleManager.QuickSelectPane.IsVisible = false;
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
                                      CharacterPickPanel.SharedInstance.SettingBackground,
                                      CharacterPickPanel.SharedInstance.SettingCharacter,
                                      CharacterPickPanel.SharedInstance.SettingMirrorEffect);
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
            Brush.IsVisible = true;
            Brush.Position = info.ConsoleLocation;

            if (info.LeftButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                cell.GlyphIndex = CharacterPickPanel.SharedInstance.SettingCharacter;
                cell.Foreground = CharacterPickPanel.SharedInstance.SettingForeground;
                cell.Background = CharacterPickPanel.SharedInstance.SettingBackground;
                cell.SpriteEffect = CharacterPickPanel.SharedInstance.SettingMirrorEffect;
            }

            if (info.RightButtonDown)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                CharacterPickPanel.SharedInstance.SettingCharacter = cell.GlyphIndex;
                CharacterPickPanel.SharedInstance.SettingForeground = cell.Foreground;
                CharacterPickPanel.SharedInstance.SettingBackground = cell.Background;
                CharacterPickPanel.SharedInstance.SettingMirrorEffect = cell.SpriteEffect;
            }
        }
    }
}
