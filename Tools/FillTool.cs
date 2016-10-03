namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Consoles;
    using SadConsole.Game;

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

        public GameObject Brush;

        public FillTool()
        {
            ControlPanels = new CustomPanel[] { Panels.CharacterPickPanel.SharedInstance };
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
            Brush.Position = info.ConsoleLocation;
            Brush.IsVisible = true;

            if (info.LeftClicked)
            {
                Cell cellToMatch = new Cell();
                Cell currentFillCell = new Cell();

                surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y).Copy(cellToMatch);
                cellToMatch.Effect = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y).Effect;

                currentFillCell.GlyphIndex = CharacterPickPanel.SharedInstance.SettingCharacter;
                currentFillCell.Foreground = CharacterPickPanel.SharedInstance.SettingForeground;
                currentFillCell.Background = CharacterPickPanel.SharedInstance.SettingBackground;
                currentFillCell.SpriteEffect = CharacterPickPanel.SharedInstance.SettingMirrorEffect;
                
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

                CharacterPickPanel.SharedInstance.SettingCharacter = cell.GlyphIndex;
                CharacterPickPanel.SharedInstance.SettingForeground = cell.Foreground;
                CharacterPickPanel.SharedInstance.SettingBackground = cell.Background;
                CharacterPickPanel.SharedInstance.SettingMirrorEffect = cell.SpriteEffect;
            }
        }

    }
}
