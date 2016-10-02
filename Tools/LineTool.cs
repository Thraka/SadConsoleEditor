namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Controls;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class LineTool : ITool
    {
        public SadConsole.Game.GameObject Brush;

        private SadConsole.Effects.Fade frameEffect;
        private Point? firstPoint;
        private Point? secondPoint;
        private SadConsole.Shapes.Line lineShape;
        private Cell lineCell;
        private CellAppearance lineStyle;
        private LineToolPanel settingsPanel;
        
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
            frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            lineCell = new Cell();

            settingsPanel = new LineToolPanel();
            ControlPanels = new CustomPanel[] { settingsPanel, CharacterPickPanel.SharedInstance };

            // Configure the animations
            Brush = new SadConsole.Game.GameObject();
            Brush.Font = Settings.Config.ScreenFont;
            AnimatedTextSurface animation = new AnimatedTextSurface("single", 1, 1, Settings.Config.ScreenFont);
            animation.CreateFrame()[0].GlyphIndex = 42;
            Brush.Animations.Add(animation.Name, animation);
            SetAnimationSingle();
        }

        private void SetAnimationLine(Point mousePosition)
        {
            // Draw the line (erase old) to where the mouse is
            // create the animation frame
            AnimatedTextSurface animation = new AnimatedTextSurface("line", Math.Max(firstPoint.Value.X, mousePosition.X) - Math.Min(firstPoint.Value.X, mousePosition.X) + 1,
                                                                            Math.Max(firstPoint.Value.Y, mousePosition.Y) - Math.Min(firstPoint.Value.Y, mousePosition.Y) + 1,
                                                                            Settings.Config.ScreenFont);


            var frame = animation.CreateFrame();

            Point p1;
            Point p2;

            if (firstPoint.Value.X > mousePosition.X)
            {
                if (firstPoint.Value.Y > mousePosition.Y)
                {
                    p1 = new Point(frame.Width - 1, frame.Height - 1);
                    p2 = new Point(0, 0);
                }
                else
                {
                    p1 = new Point(frame.Width - 1, 0);
                    p2 = new Point(0, frame.Height - 1);
                }
            }
            else
            {
                if (firstPoint.Value.Y > mousePosition.Y)
                {
                    p1 = new Point(0, frame.Height - 1);
                    p2 = new Point(frame.Width - 1, 0);
                }
                else
                {
                    p1 = new Point(0, 0);
                    p2 = new Point(frame.Width - 1, frame.Height - 1);
                }
            }

            animation.Center = p1;

            //_lineStyle = new CellAppearance(
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
            //_lineStyle.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            //_lineStyle.CopyAppearanceTo(_lineCell);

            lineShape = new SadConsole.Shapes.Line();
            lineShape.CellAppearance = lineCell;
            lineShape.UseEndingCell = false;
            lineShape.UseStartingCell = false;
            lineShape.StartingLocation = p1;
            lineShape.EndingLocation = p2;
            lineShape.Draw(new SurfaceEditor(frame));

            settingsPanel.LineLength = frame.Width > frame.Height ? frame.Width : frame.Height;

            Brush.Animation = animation;
        }

        private void SetAnimationSingle()
        {
            Brush.Animation = Brush.Animations["single"];
        }

        public void OnSelected()
        {
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

            settingsPanel.LineLength = 0;
            firstPoint = null;
            secondPoint = null;
            lineShape = null;

        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
            lineStyle = new CellAppearance(CharacterPickPanel.SharedInstance.SettingForeground,
                                              CharacterPickPanel.SharedInstance.SettingBackground,
                                              CharacterPickPanel.SharedInstance.SettingCharacter,
                                              CharacterPickPanel.SharedInstance.SettingMirrorEffect);

            lineStyle.CopyAppearanceTo(lineCell);
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

            if (!firstPoint.HasValue)
            {
                Brush.Position = info.ConsoleLocation;
            }
            else
            {
                SetAnimationLine(info.ConsoleLocation);
                
            }


            // TODO: Make this work. They push DOWN on the mouse, start the line from there, if they "Click" then go to mode where they click a second time
            // If they don't click and hold it down longer than click, pretend a second click happened and draw the line.
            if (info.LeftClicked)
            {
                if (!firstPoint.HasValue)
                {
                    firstPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                }
                else
                {
                    secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                    lineShape.StartingLocation = firstPoint.Value;
                    lineShape.EndingLocation = secondPoint.Value;
                    lineShape.Draw(new SurfaceEditor(surface));

                    firstPoint = null;
                    secondPoint = null;
                    lineShape = null;

                    Brush.Animation = Brush.Animations["single"];

                    //surface.ResyncAllCellEffects();
                    settingsPanel.LineLength = 0;
                }
            }
            else if (info.RightClicked)
            {
                if (firstPoint.HasValue && !secondPoint.HasValue)
                {
                    firstPoint = null;
                    secondPoint = null;
                    lineShape = null;

                    Brush.Animation = Brush.Animations["single"];

                    settingsPanel.LineLength = 0;
                }
            }

        }
    }
}
