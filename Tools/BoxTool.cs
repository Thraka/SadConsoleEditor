namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Surfaces;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.GameHelpers;

    class BoxTool : ITool
    {
        private AnimatedSurface animSinglePoint;
        private SadConsole.Effects.Fade frameEffect;
        private Point? firstPoint;
        private Point? secondPoint;
        private SadConsole.Shapes.Box boxShape;

        private BoxToolPanel _settingsPanel;

        public GameObject Brush;

        public const string ID = "BOX";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Box"; }
        }

        public char Hotkey { get { return 'b'; } }


        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public BoxTool()
        {
            animSinglePoint = new AnimatedSurface("single", 1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            var _frameSinglePoint = animSinglePoint.CreateFrame();
            _frameSinglePoint[0].Glyph = 42;


            frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _settingsPanel = new BoxToolPanel();

            ControlPanels = new CustomPanel[] { _settingsPanel };

            // 
            Brush = new SadConsole.GameHelpers.GameObject(1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            AnimatedSurface animation = new AnimatedSurface("single", 1, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            animation.CreateFrame()[0].Glyph = 42;
            Brush.Animations.Add(animation.Name, animation);
            Brush.Animation = animation;
        }

        void ResetBox()
        {
            firstPoint = null;
            secondPoint = null;

            Brush.Animation = Brush.Animations["single"];
        }


        public void OnSelected()
        {
            RefreshTool();
            ResetBox();
            MainScreen.Instance.Brush = Brush;

            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = true;
        }

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            MainScreen.Instance.QuickSelectPane.IsVisible = false;

            firstPoint = null;
            secondPoint = null;
        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            MainScreen.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
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
            if (!firstPoint.HasValue)
            {
                Brush.Position = info.ConsolePosition;
            }
            else
            {
                // Draw the line (erase old) to where the mouse is
                // create the animation frame
                AnimatedSurface animation = new AnimatedSurface("line", Math.Max(firstPoint.Value.X, info.ConsolePosition.X) - Math.Min(firstPoint.Value.X, info.ConsolePosition.X) + 1,
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

                animation.Center = p1;

                boxShape = SadConsole.Shapes.Box.GetDefaultBox();

                if (_settingsPanel.UseCharacterBorder)
                    boxShape.LeftSideCharacter = boxShape.RightSideCharacter =
                    boxShape.TopLeftCharacter = boxShape.TopRightCharacter = boxShape.TopSideCharacter =
                    boxShape.BottomLeftCharacter = boxShape.BottomRightCharacter = boxShape.BottomSideCharacter =
                    _settingsPanel.BorderCharacter;

                boxShape.Foreground = _settingsPanel.LineForeColor;
                boxShape.FillColor = _settingsPanel.FillColor;
                boxShape.Fill = _settingsPanel.UseFill;
                boxShape.BorderBackground = _settingsPanel.LineBackColor;
                boxShape.Position = new Point(0, 0);
                boxShape.Width = frame.Width;
                boxShape.Height = frame.Height;
                boxShape.Draw(new SurfaceEditor(frame));

                Brush.Animation = animation;
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
                    Point p1 = new Point(Math.Min(firstPoint.Value.X, secondPoint.Value.X), Math.Min(firstPoint.Value.Y, secondPoint.Value.Y));
                    //Point p2 = new Point(Math.Max(_firstPoint.Value.X, _secondPoint.Value.X), Math.Max(_firstPoint.Value.Y, _secondPoint.Value.Y));


                    boxShape.Position = p1;
                    boxShape.Draw(new SurfaceEditor(surface));

                    firstPoint = null;
                    secondPoint = null;

                    Brush.Animation = Brush.Animations["single"];

                    //surface.ResyncAllCellEffects();
                }
            }
            else if (info.Mouse.RightClicked)
            {
                if (firstPoint.HasValue && !secondPoint.HasValue)
                {
                    firstPoint = null;
                    secondPoint = null;

                    Brush.Animation = Brush.Animations["single"];
                }
            }

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
            
        }
    }
}
