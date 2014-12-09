namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using System;

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

        public CustomPanel[] ControlPanels { get; private set; }

        public FillTool()
        {
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(new SadConsole.Entities.Entity());
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null);
            EditorConsoleManager.Instance.Brush.IsVisible = false;
        }

        public void OnDeselected()
        {

        }

        public void RefreshTool()
        {
            EditorConsoleManager.Instance.Brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter, null);
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {

        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            
        }

        public void MouseEnterSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            EditorConsoleManager.Instance.Brush.Position = info.ConsoleLocation;

            if (info.LeftClicked)
            {
                Cell cellToMatch = new Cell();
                Cell currentFillCell = new Cell();

                surface[info.ConsoleLocation.X, info.ConsoleLocation.Y].Copy(cellToMatch);
                cellToMatch.Effect = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y].Effect;

                currentFillCell.CharacterIndex = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter;
                currentFillCell.Foreground = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground;
                currentFillCell.Background = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground;

                Func<Cell, bool> isTargetCell = (c) =>
                {
                    bool effect = c.Effect == null && cellToMatch.Effect == null;

                    if (c.Effect != null && cellToMatch.Effect != null)
                        effect = c.Effect == cellToMatch.Effect;

                    if (c.CharacterIndex == 0 && cellToMatch.CharacterIndex == 0)
                        return c.Background == cellToMatch.Background;

                    return c.Foreground == cellToMatch.Foreground &&
                           c.Background == cellToMatch.Background &&
                           c.CharacterIndex == cellToMatch.CharacterIndex &&
                           effect;
                };

                Action<Cell> fillCell = (c) =>
                {
                    currentFillCell.Copy(c);
                    //console.CellData.SetEffect(c, _currentFillCell.Effect);
                };

                Func<Cell, SadConsole.Algorithms.NodeConnections<Cell>> getConnectedCells = (c) =>
                {
                    Algorithms.NodeConnections<Cell> connections = new Algorithms.NodeConnections<Cell>();

                    Point position = c.Position;

                    connections.West = surface.IsValidCell(position.X - 1, position.Y) ? surface[position.X - 1, position.Y] : null;
                    connections.East = surface.IsValidCell(position.X + 1, position.Y) ? surface[position.X + 1, position.Y] : null;
                    connections.North = surface.IsValidCell(position.X, position.Y - 1) ? surface[position.X, position.Y - 1] : null;
                    connections.South = surface.IsValidCell(position.X, position.Y + 1) ? surface[position.X, position.Y + 1] : null;

                    return connections;
                };

                if (!isTargetCell(currentFillCell))
                    SadConsole.Algorithms.FloodFill<Cell>(surface[info.ConsoleLocation.X, info.ConsoleLocation.Y], isTargetCell, fillCell, getConnectedCells);
            }
        }

    }
}
