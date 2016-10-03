namespace SadConsoleEditor.Tools
{
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Game;
    using SadConsole.Input;
    using SadConsoleEditor.Panels;

    class TextTool : ITool
    {
        public const string ID = "TEXT";

        private bool writing;
        private Console tempConsole;
        private SadConsole.Effects.EffectsManager blinkManager;

        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Text"; }
        }
        public char Hotkey { get { return 't'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        private RecolorToolPanel settingsPanel;

        public GameObject Brush;

        public override string ToString()
        {
            return Title;
        }

        public TextTool()
        {
            settingsPanel = new RecolorToolPanel();

            ControlPanels = new CustomPanel[] { settingsPanel, CharacterPickPanel.SharedInstance };
            tempConsole = new Console(1, 1);
        }

        public void OnSelected()
        {
            Brush = new SadConsole.Game.GameObject(Settings.Config.ScreenFont);
            Brush.Animation = new AnimatedTextSurface("default", 1, 1);
            Brush.Animation.CreateFrame();
            Brush.IsVisible = false;

            blinkManager = new SadConsole.Effects.EffectsManager(Brush.Animation.Frames[0]);

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
            writing = false;
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
                                      CharacterPickPanel.SharedInstance.SettingBackground, 95);

            SadConsole.Effects.Blink blinkEffect = new SadConsole.Effects.Blink();
            blinkEffect.BlinkSpeed = 0.35f;
            blinkManager.SetEffect(Brush.Animation.Frames[0][0], blinkEffect);
        }

        public void Update()
        {
            blinkManager.UpdateEffects(SadConsole.Engine.GameTimeElapsedUpdate);
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            if (writing)
            {
                if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    writing = false;
                    Brush.IsVisible = false;
                    EditorConsoleManager.AllowKeyboardToMoveConsole = true;
                }
                else
                {
                    //tempConsole.TextSurface = (ITextSurfaceRendered)surface;
                    tempConsole.VirtualCursor.PrintAppearance = new CellAppearance(CharacterPickPanel.SharedInstance.SettingForeground, CharacterPickPanel.SharedInstance.SettingBackground);
                    tempConsole.ProcessKeyboard(info);
                    Brush.Position = tempConsole.VirtualCursor.Position;
                }

                return true;
            }

            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseEnterSurface(MouseInfo info, ITextSurface surface)
        {
            //Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            //Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            

            if (info.LeftClicked)
            {
                EditorConsoleManager.AllowKeyboardToMoveConsole = false;
                writing = true;

                tempConsole.TextSurface = (ITextSurfaceRendered)surface;
                tempConsole.VirtualCursor.Position = Brush.Position = info.ConsoleLocation;

                Brush.IsVisible = true;
            }


        }
    }
}

