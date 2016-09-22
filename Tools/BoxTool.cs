namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Game;

    class BoxTool : ITool
    {
        private AnimatedTextSurface animSinglePoint;
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
            animSinglePoint = new AnimatedTextSurface("single", 1, 1, Settings.Config.ScreenFont);
            var _frameSinglePoint = animSinglePoint.CreateFrame();
            _frameSinglePoint[0].GlyphIndex = 42;


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
            Brush = new SadConsole.Game.GameObject();
            Brush.Font = Settings.Config.ScreenFont;
            AnimatedTextSurface animation = new AnimatedTextSurface("single", 1, 1, Settings.Config.ScreenFont);
            animation.CreateFrame()[0].GlyphIndex = 42;
            Brush.Animations.Add(animation.Name, animation);
            Brush.Animation = animation;
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

            firstPoint = null;
            secondPoint = null;
        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = true;
            if (!firstPoint.HasValue)
            {
                Brush.Position = info.ConsoleLocation;
            }
            else
            {
                // Draw the line (erase old) to where the mouse is
                // create the animation frame
                AnimatedTextSurface animation = new AnimatedTextSurface("line", Math.Max(firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(firstPoint.Value.X, info.ConsoleLocation.X) + 1,
                                                                                Math.Max(firstPoint.Value.Y, info.ConsoleLocation.Y) - Math.Min(firstPoint.Value.Y, info.ConsoleLocation.Y) + 1,
                                                                                Settings.Config.ScreenFont);

                var frame = animation.CreateFrame();

                Point p1;

                if (firstPoint.Value.X > info.ConsoleLocation.X)
                {
                    if (firstPoint.Value.Y > info.ConsoleLocation.Y)
                        p1 = new Point(frame.Width - 1, frame.Height - 1);
                    else
                        p1 = new Point(frame.Width - 1, 0);
                }
                else
                {
                    if (firstPoint.Value.Y > info.ConsoleLocation.Y)
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
                boxShape.Location = new Point(0, 0);
                boxShape.Width = frame.Width;
                boxShape.Height = frame.Height;
                boxShape.Draw(new SurfaceEditor(frame));

                Brush.Animation = animation;
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
                    Point p1 = new Point(Math.Min(firstPoint.Value.X, secondPoint.Value.X), Math.Min(firstPoint.Value.Y, secondPoint.Value.Y));
                    //Point p2 = new Point(Math.Max(_firstPoint.Value.X, _secondPoint.Value.X), Math.Max(_firstPoint.Value.Y, _secondPoint.Value.Y));


                    boxShape.Location = p1;
                    boxShape.Draw(new SurfaceEditor(surface));

                    firstPoint = null;
                    secondPoint = null;

                    Brush.Animation = Brush.Animations["single"];

                    //surface.ResyncAllCellEffects();
                }
            }
            else if (info.RightClicked)
            {
                if (firstPoint.HasValue && !secondPoint.HasValue)
                {
                    firstPoint = null;
                    secondPoint = null;

                    Brush.Animation = Brush.Animations["single"];
                }
            }

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
            
        }
    }
}
