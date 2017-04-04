namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Surfaces;
    using SadConsole.Controls;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class LineTool : ITool
    {
        public SadConsole.GameHelpers.GameObject Brush;

        private SadConsole.Effects.Fade frameEffect;
        private Point? firstPoint;
        private Point? secondPoint;
        private SadConsole.Shapes.Line lineShape;
        private Cell lineCell;
        private Cell lineStyle;
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
            Brush = new SadConsole.GameHelpers.GameObject(1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            AnimatedSurface animation = new AnimatedSurface("single", 1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            animation.CreateFrame()[0].Glyph = 42;
            Brush.Animations.Add(animation.Name, animation);
            ResetLine();
        }

        private void SetAnimationLine(Point mousePosition)
        {
            // Draw the line (erase old) to where the mouse is
            // create the animation frame
            AnimatedSurface animation = new AnimatedSurface("line", Math.Max(firstPoint.Value.X, mousePosition.X) - Math.Min(firstPoint.Value.X, mousePosition.X) + 1,
                                                                            Math.Max(firstPoint.Value.Y, mousePosition.Y) - Math.Min(firstPoint.Value.Y, mousePosition.Y) + 1,
                                                                            SadConsoleEditor.Settings.Config.ScreenFont);


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

            //_lineStyle = new Cell(
            //                    MainScreen.Instance.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
            //                    MainScreen.Instance.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
            //                    MainScreen.Instance.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
            //_lineStyle.SpriteEffect = MainScreen.Instance.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            //_lineStyle.CopyAppearanceTo(_lineCell);

            lineShape = new SadConsole.Shapes.Line();
            lineShape.Cell = lineCell;
            lineShape.UseEndingCell = false;
            lineShape.UseStartingCell = false;
            lineShape.StartingLocation = p1;
            lineShape.EndingLocation = p2;
            lineShape.Draw(new SurfaceEditor(frame));

            settingsPanel.LineLength = frame.Width > frame.Height ? frame.Width : frame.Height;

            Brush.Animation = animation;
        }

        void ResetLine()
        {
            firstPoint = null;
            secondPoint = null;
            lineShape = null;

            Brush.Animation = Brush.Animations["single"];

            settingsPanel.LineLength = 0;
        }

        public void OnSelected()
        {
            RefreshTool();
            ResetLine();
            MainScreen.Instance.Brush = Brush;

            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = true;
		}

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = false;

            settingsPanel.LineLength = 0;
            firstPoint = null;
            secondPoint = null;
            lineShape = null;

        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
            lineStyle = new Cell(CharacterPickPanel.SharedInstance.SettingForeground,
                                              CharacterPickPanel.SharedInstance.SettingBackground,
                                              CharacterPickPanel.SharedInstance.SettingCharacter,
                                              CharacterPickPanel.SharedInstance.SettingMirrorEffect);

            lineStyle.CopyAppearanceTo(lineCell);
            Brush.Animation.IsDirty = true;
        }

        public void Update()
        {
        }

        public bool ProcessKeyboard(Keyboard info, ISurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseConsoleState info, ISurface surface)
        {
        }

        public void MouseEnterSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = true;

            if (!firstPoint.HasValue)
            {
                Brush.Position = info.ConsolePosition;
            }
            else
            {
                SetAnimationLine(info.ConsolePosition);
                
            }


            // TODO: Make this work. They push DOWN on the mouse, start the line from there, if they "Click" then go to mode where they click a second time
            // If they don't click and hold it down longer than click, pretend a second click happened and draw the line.
            if (info.Mouse.LeftClicked)
            {
                if (!firstPoint.HasValue)
                {
                    firstPoint = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);
                }
                else
                {
                    secondPoint = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);

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
            else if (info.Mouse.RightClicked)
            {
                if (firstPoint.HasValue && !secondPoint.HasValue)
                {
                    ResetLine();
                }
            }

        }

        
    }
}
