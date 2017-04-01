namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Surfaces;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.GameHelpers;

    class CircleTool : ITool
    {
        public GameObject Brush;
        private SadConsole.Effects.Fade frameEffect;
        private Point? firstPoint;
        private Point? secondPoint;
        private SadConsole.Shapes.Circle circleShape;
        private SadConsole.Shapes.Ellipse ellipseShape;

        private CircleToolPanel settingsPanel;
        private Cell borderAppearance;

        public const string ID = "CIRCLE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Circle"; }
        }
        public char Hotkey { get { return 'c'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public CircleTool()
        {
            frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            // Configure the animations
            Brush = new SadConsole.GameHelpers.GameObject(1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            AnimatedSurface animation = new AnimatedSurface("single", 1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            animation.CreateFrame()[0].Glyph = 42;
            Brush.Animations.Add(animation.Name, animation);

            settingsPanel = new CircleToolPanel();
            ControlPanels = new CustomPanel[] { settingsPanel, CharacterPickPanel.SharedInstance };
            ResetCircle();
        }


        void ResetCircle()
        {
            firstPoint = null;
            secondPoint = null;

            settingsPanel.CircleWidth = 0;
            settingsPanel.CircleHeight = 0;

            Brush.Animation = Brush.Animations["single"];
        }

        public void OnSelected()
        {
            RefreshTool();
            ResetCircle();
            MainScreen.Instance.Brush = Brush;
            MainScreen.Instance.UpdateBrush();

            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = true;
        }

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = false;

            settingsPanel.CircleHeight = 0;
            settingsPanel.CircleWidth = 0;
            firstPoint = null;
            secondPoint = null;
            circleShape = null;

        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }


        public void RefreshTool()
        {
            borderAppearance = new Cell(CharacterPickPanel.SharedInstance.SettingForeground,
                                   CharacterPickPanel.SharedInstance.SettingBackground,
                                   CharacterPickPanel.SharedInstance.SettingCharacter,
                                   CharacterPickPanel.SharedInstance.SettingMirrorEffect);
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
            if (!firstPoint.HasValue)
                Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = true;

            if (!firstPoint.HasValue)
            {
                Brush.Position = info.ConsolePosition;

                settingsPanel.CircleWidth = 0;
                settingsPanel.CircleHeight = 0;
            }
            else
            {
                AnimatedSurface animation;
                // Draw the line (erase old) to where the mouse is
                // create the animation frame
                animation = new AnimatedSurface("line", Math.Max(firstPoint.Value.X, info.ConsolePosition.X) - Math.Min(firstPoint.Value.X, info.ConsolePosition.X) + 1,
                                                            Math.Max(firstPoint.Value.Y, info.ConsolePosition.Y) - Math.Min(firstPoint.Value.Y, info.ConsolePosition.Y) + 1,
                                                            SadConsoleEditor.Settings.Config.ScreenFont);

                var frame = animation.CreateFrame();

                Point p1;

                if (firstPoint.Value.X > info.ConsolePosition.X)
                {
                    if (firstPoint.Value.Y > info.ConsolePosition.Y)
                        p1 = new Point(frame.Width - 1, frame.Height - 1);
                    else
                        p1 = new Point(frame.Width - 1, 0);
                }
                else
                {
                    if (firstPoint.Value.Y > info.ConsolePosition.Y)
                        p1 = new Point(0, frame.Height - 1);
                    else
                        p1 = new Point(0, 0);
                }

                settingsPanel.CircleWidth = frame.Width;
                settingsPanel.CircleHeight = frame.Height;

                animation.Center = p1;

                SadConsoleEditor.Settings.QuickEditor.TextSurface = frame;

                ellipseShape = new SadConsole.Shapes.Ellipse();
                ellipseShape.BorderAppearance = borderAppearance;
                ellipseShape.EndingPoint = new Point(frame.Width - 1, frame.Height - 1);
                ellipseShape.Draw(SadConsoleEditor.Settings.QuickEditor);

                Brush.Animation = animation;
            }


            // TODO: Make this work. They push DOWN on the mouse, start the line from there, if they "Click" then go to mode where they click a second time
            // If they don't click and hold it down longer than click, pretend a second click happened and draw the line.
            if (info.Mouse.LeftClicked)
            {
                if (!firstPoint.HasValue)
                {
                    firstPoint = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);
                    RefreshTool();
                }
                else
                {
                    secondPoint = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);
                    Point p1 = new Point(Math.Min(firstPoint.Value.X, secondPoint.Value.X), Math.Min(firstPoint.Value.Y, secondPoint.Value.Y));
                    Point p2 = new Point(Math.Max(firstPoint.Value.X, secondPoint.Value.X), Math.Max(firstPoint.Value.Y, secondPoint.Value.Y));

                    SadConsoleEditor.Settings.QuickEditor.TextSurface = surface;

                    ellipseShape.StartingPoint = p1;
                    ellipseShape.EndingPoint = p2;
                    ellipseShape.Draw(SadConsoleEditor.Settings.QuickEditor);

                    Brush.Animation = Brush.Animations["single"];
                    Brush.Position = secondPoint.Value;


                    firstPoint = null;
                    secondPoint = null;

                    //surface.ResyncAllCellEffects();
                }
            }
            else if (info.Mouse.RightClicked)
            {
                if (firstPoint.HasValue && !secondPoint.HasValue)
                {
                    firstPoint = null;
                    secondPoint = null;

                    settingsPanel.CircleWidth = 0;
                    settingsPanel.CircleHeight = 0;

                    Brush.Animation = Brush.Animations["single"];
                    Brush.Position = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);
                }
            }
        }
    }
}
