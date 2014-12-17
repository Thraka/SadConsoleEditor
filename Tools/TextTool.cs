namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;

    class TextTool : ITool
    {
        private bool writing;
        private Console tempConsole;
        private int _cursorCharacter = 95;

        public const string ID = "TEXT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Text"; }
        }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public TextTool()
        {
            tempConsole = new Console();
            tempConsole.CanUseKeyboard = true;
            tempConsole.VirtualCursor.AutomaticallyShiftRowsUp = false;
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
        }

        public void OnSelected()
        {
            SadConsole.Effects.Blink blinkEffect = new SadConsole.Effects.Blink();
            blinkEffect.BlinkSpeed = 0.35f;

            EditorConsoleManager.Instance.UpdateBrush(new EntityBrush());
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(Color.White, Color.Black, _cursorCharacter, blinkEffect);
            EditorConsoleManager.Instance.Brush.IsVisible = false;
            EditorConsoleManager.Instance.AllowKeyboardToMoveConsole = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;

        }

        public void OnDeselected()
        {
            writing = false;
        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.ToolPane.KeyboardHandler = null;
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            if (writing)
            {
                if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    writing = false;
                    EditorConsoleManager.Instance.Brush.IsVisible = false;
                }
                else
                {
                    tempConsole.CellData = surface;
                    tempConsole.VirtualCursor.PrintAppearance = new CellAppearance(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground);
                    tempConsole.ProcessKeyboard(info);
                    EditorConsoleManager.Instance.Brush.Position = tempConsole.VirtualCursor.Position;
                }
            }
        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            
        }

        public void MouseEnterSurface(MouseInfo info, CellSurface surface)
        {
            
        }

        public void MouseExitSurface(MouseInfo info, CellSurface surface)
        {
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            if (info.LeftClicked)
            {
                writing = true;

                tempConsole.CellData = surface;
                tempConsole.VirtualCursor.Position = EditorConsoleManager.Instance.Brush.Position = info.ConsoleLocation;

                EditorConsoleManager.Instance.Brush.IsVisible = true;
            }
        }
    }
}
