namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using Panels;

    class ObjectTool : ITool
    {
        public const string ID = "OBJECT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Object"; }
        }

        public CustomPanel[] ControlPanels { get; private set; }

        private EntityBrush _brush;
        private Panels.ObjectToolPanel _panel;

        public ObjectTool()
        {
            _panel = new Panels.ObjectToolPanel();
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel, _panel };

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
            _brush.IsVisible = true;
            _brush.Position = info.ConsoleLocation;

            if (info.LeftButtonDown)
            {
                var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];

                CellAppearance appearance = new CellAppearance(
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter,
                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);

                if (EditorConsoleManager.Instance.SelectedEditor is Editors.GameScreenEditor)
                {
                    var editor = (Editors.GameScreenEditor)EditorConsoleManager.Instance.SelectedEditor;
                    var point = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                    if (editor.GameObjects.ContainsKey(point))
                    {
                        editor.GameObjects.Remove(point);
                    }

                    var gameObj = new GameHelpers.GameObject() { Character = appearance, Position = point };
                    gameObj.Name = _panel.SettingName;

                    editor.GameObjects.Add(point, new GameHelpers.GameObject() { Character = appearance, Position = point });
                }
                

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
