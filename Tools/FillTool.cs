namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Consoles;
    class FillTool : ITool
    {
        public const string ID = "FILL";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Fill"; }
        }
        public char Hotkey { get { return 'f'; } }

        public CustomPanel[] ControlPanels { get; private set; }
        private EntityBrush _brush;

        public FillTool()
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
            Settings.QuickEditor.TextSurface = _brush.CurrentAnimation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);

			EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel, EventArgs.Empty);
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed += EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = true;
            _brush.IsVisible = false;
        }

		

		public void OnDeselected()
        {
			EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed -= EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = false;
		}

		public void RefreshTool()
        {
            Settings.QuickEditor.TextSurface = _brush.CurrentAnimation.Frames[0];
            Settings.QuickEditor.Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect);
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

            if (info.LeftClicked)
            {
                Cell cellToMatch = new Cell();
                Cell currentFillCell = new Cell();

                surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y).Copy(cellToMatch);
                cellToMatch.Effect = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y).Effect;

                currentFillCell.GlyphIndex = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter;
                currentFillCell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;
                currentFillCell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;
                currentFillCell.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
                
                Func<Cell, bool> isTargetCell = (c) =>
                {
                    bool effect = c.Effect == null && cellToMatch.Effect == null;

                    if (c.Effect != null && cellToMatch.Effect != null)
                        effect = c.Effect == cellToMatch.Effect;

                    if (c.GlyphIndex == 0 && cellToMatch.GlyphIndex == 0)
                        return c.Background == cellToMatch.Background;

                    return c.Foreground == cellToMatch.Foreground &&
                           c.Background == cellToMatch.Background &&
                           c.GlyphIndex == cellToMatch.GlyphIndex &&
                           c.SpriteEffect == cellToMatch.SpriteEffect &&
                           effect;
                };

                Action<Cell> fillCell = (c) =>
                {
                    currentFillCell.Copy(c);
                    //console.TextSurface.SetEffect(c, _currentFillCell.Effect);
                };

                System.Collections.Generic.List<Cell> cells = new System.Collections.Generic.List<Cell>(surface.Cells);

                Func<Cell, SadConsole.Algorithms.NodeConnections<Cell>> getConnectedCells = (c) =>
                {
                    Algorithms.NodeConnections<Cell> connections = new Algorithms.NodeConnections<Cell>();

                    Point position = TextSurface.GetPointFromIndex(cells.IndexOf(c), surface.Width);

                    connections.West = surface.IsValidCell(position.X - 1, position.Y) ? surface.GetCell(position.X - 1, position.Y) : null;
                    connections.East = surface.IsValidCell(position.X + 1, position.Y) ? surface.GetCell(position.X + 1, position.Y) : null;
                    connections.North = surface.IsValidCell(position.X, position.Y - 1) ? surface.GetCell(position.X, position.Y - 1) : null;
                    connections.South = surface.IsValidCell(position.X, position.Y + 1) ? surface.GetCell(position.X, position.Y + 1) : null;

                    return connections;
                };

                if (!isTargetCell(currentFillCell))
                    SadConsole.Algorithms.FloodFill<Cell>(surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y), isTargetCell, fillCell, getConnectedCells);
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
