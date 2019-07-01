﻿namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Entities;
    using Console = SadConsole.Console;

    public class LineTool : ITool
    {
        private AnimatedConsole animSinglePoint;
        private SadConsole.Effects.Fade frameEffect;
        private Point? firstPoint;
        private Point secondPoint;
        private bool cancelled;

        private LineToolPanel _settingsPanel;

        public Entity Brush;

        public const string ID = "LINE";

        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Line"; }
        }

        public char Hotkey { get { return 'l'; } }


        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public LineTool()
        {
            animSinglePoint = new AnimatedConsole("single", 1, 1, Config.Program.ScreenFont) { [0] = { Glyph = 42 } };

            frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _settingsPanel = new LineToolPanel();

            ControlPanels = new CustomPanel[] { _settingsPanel };

            // 
            Brush = new Entity(1, 1, Config.Program.ScreenFont);
            Brush.Animations.Add(animSinglePoint.Name, animSinglePoint);
            Brush.Animation = animSinglePoint;
        }

        void ResetBox()
        {
            firstPoint = null;

            Brush.Animation = Brush.Animations["single"];
        }

        public void OnSelected()
        {
            RefreshTool();
            ResetBox();
            MainConsole.Instance.Brush = Brush;

            MainConsole.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            MainConsole.Instance.QuickSelectPane.IsVisible = true;
        }

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            MainConsole.Instance.QuickSelectPane.IsVisible = false;

            firstPoint = null;
        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            MainConsole.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
            Brush.Animation.IsDirty = true;
        }

        public void Update()
        {
        }

        public bool ProcessKeyboard(Keyboard info, Console surface)
        {
            return false;
        }

        public void ProcessMouse(MouseConsoleState info, Console surface, bool isInBounds)
        {
            if (cancelled)
            {
                // wait until left button is released...
                if (info.Mouse.LeftButtonDown)
                    return;
                else
                    cancelled = false;
            }

            if (info.Mouse.LeftButtonDown)
            {
                if (!firstPoint.HasValue)
                {
                    firstPoint = info.ConsoleCellPosition;
                    return;
                }
                else
                {
                    // Check for right click cancel.
                    if (info.Mouse.RightButtonDown)
                    {
                        cancelled = true;
                        firstPoint = null;
                        return;
                    }

                    if (secondPoint == info.ConsoleCellPosition)
                        return;

                    secondPoint = info.ConsoleCellPosition;
                    
                    // Draw the line (erase old) to where the mouse is
                    // create the animation frame
                    var animation = new AnimatedConsole("line", Math.Max(firstPoint.Value.X, secondPoint.X) - Math.Min(firstPoint.Value.X, secondPoint.X) + 1,
                                                                                    Math.Max(firstPoint.Value.Y, secondPoint.Y) - Math.Min(firstPoint.Value.Y, secondPoint.Y) + 1,
                                                                                    Config.Program.ScreenFont);

                    var frame = animation.CreateFrame();

                    Point p1;

                    if (firstPoint.Value.X > secondPoint.X)
                    {
                        if (firstPoint.Value.Y > secondPoint.Y)
                            p1 = Point.Zero;
                        else
                            p1 = new Point(0, frame.Height - 1);
                    }
                    else
                    {
                        if (firstPoint.Value.Y > secondPoint.Y)
                            p1 = new Point(frame.Width - 1, 0);
                        else
                            p1 = new Point(frame.Width - 1, frame.Height - 1);
                    }


                    animation.Center = p1;

                    var borderCell = new Cell(_settingsPanel.LineForeColor, _settingsPanel.LineBackColor, _settingsPanel.LineGlyph);
                    frame.DrawLine(firstPoint.Value, secondPoint, borderCell.Foreground, borderCell.Background, borderCell.Glyph);

                    Brush.Animation = animation;
                }
            }
            else if (firstPoint.HasValue)
            {
                // We let go outside of bounds
                if (!isInBounds)
                {
                    cancelled = true;
                    return;
                }

                var borderCell = new Cell(_settingsPanel.LineForeColor, _settingsPanel.LineBackColor, _settingsPanel.LineGlyph);

                if (info.Console is ScrollingConsole scrollingConsole)
                    surface.DrawLine(firstPoint.Value + scrollingConsole.ViewPort.Location, secondPoint + scrollingConsole.ViewPort.Location, borderCell.Foreground, borderCell.Background, borderCell.Glyph);
                else
                    surface.DrawLine(firstPoint.Value, secondPoint, borderCell.Foreground, borderCell.Background, borderCell.Glyph);

                firstPoint = null;

                Brush.Animation = Brush.Animations["single"];
            }
        }
    }
}
