namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using SadConsoleEditor.Panels;

    class TextTool : ITool
    {
        private bool writing;
        private Console tempConsole;
        private int _cursorCharacter = 95;

        private EntityBrush _brush;

        public const string ID = "TEXT";
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

        public override string ToString()
        {
            return Title;
        }

        public TextTool()
        {
            tempConsole = new Console(1, 1);
            tempConsole.CanUseKeyboard = true;
            tempConsole.VirtualCursor.AutomaticallyShiftRowsUp = false;
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
        }

        public void OnSelected()
        {
            SadConsole.Effects.Blink blinkEffect = new SadConsole.Effects.Blink();
            blinkEffect.BlinkSpeed = 0.35f;
            _brush = new EntityBrush(1, 1);
            Settings.QuickEditor.TextSurface = _brush.CurrentAnimation.Frames[0];
            Settings.QuickEditor.Fill(Color.White, Color.Black, _cursorCharacter, blinkEffect);
            _brush.IsVisible = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;
            EditorConsoleManager.Instance.UpdateBrush(_brush);
        }

        public void OnDeselected()
        {
            writing = false;
        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.ToolPane.KeyboardHandler = null;
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            if (writing)
            {
                if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    writing = false;
                    _brush.IsVisible = false;
                    EditorConsoleManager.Instance.AllowKeyboardToMoveConsole = true;
                }
                else
                {
                    tempConsole.TextSurface = surface;
                    tempConsole.VirtualCursor.PrintAppearance = new CellAppearance(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground);
                    tempConsole.ProcessKeyboard(info);
                    _brush.Position = tempConsole.VirtualCursor.Position;
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
            
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            if (info.LeftClicked)
            {
                EditorConsoleManager.Instance.AllowKeyboardToMoveConsole = false;
                writing = true;

                tempConsole.TextSurface = surface;
                tempConsole.VirtualCursor.Position = _brush.Position = info.ConsoleLocation;

                _brush.IsVisible = true;
            }
        }
    }
}
