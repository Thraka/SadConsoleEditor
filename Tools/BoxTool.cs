namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class BoxTool : ITool
    {
        private EntityBrush _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Box _boxShape;

        private BoxToolPanel _settingsPanel;

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
            _animSinglePoint = new Animation("single", 1, 1);
            _animSinglePoint.Font = Engine.DefaultFont;
            var _frameSinglePoint = _animSinglePoint.CreateFrame();
            _frameSinglePoint[0].CharacterIndex = 42;


            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _settingsPanel = new BoxToolPanel();

            ControlPanels = new CustomPanel[] { _settingsPanel };
        }

        public void OnSelected()
        {
            _entity = new EntityBrush();
            _entity.IsVisible = false;

            _entity.AddAnimation(_animSinglePoint);
            _entity.SetActiveAnimation("single");

            EditorConsoleManager.Instance.UpdateBrush(_entity);
        }

        public void OnDeselected()
        {
            _firstPoint = null;
            _secondPoint = null;
        }

        public void RefreshTool()
        {
        }

        public bool ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            _entity.IsVisible = true;
            if (!_firstPoint.HasValue)
            {
                _entity.Position = info.ConsoleLocation;
            }
            else
            {
                // Draw the line (erase old) to where the mouse is
                // create the animation frame
                Animation animation = new Animation("line", Math.Max(_firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(_firstPoint.Value.X, info.ConsoleLocation.X) + 1,
                                                            Math.Max(_firstPoint.Value.Y, info.ConsoleLocation.Y) - Math.Min(_firstPoint.Value.Y, info.ConsoleLocation.Y) + 1);

                _entity.AddAnimation(animation);

                var frame = animation.CreateFrame();

                Point p1;

                if (_firstPoint.Value.X > info.ConsoleLocation.X)
                {
                    if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
                        p1 = new Point(frame.Width - 1, frame.Height - 1);
                    else
                        p1 = new Point(frame.Width - 1, 0);
                }
                else
                {
                    if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
                        p1 = new Point(0, frame.Height - 1);
                    else
                        p1 = new Point(0, 0);
                }

                animation.Center = p1;

                _boxShape = SadConsole.Shapes.Box.GetDefaultBox();

                if (_settingsPanel.UseCharacterBorder)
                    _boxShape.LeftSideCharacter = _boxShape.RightSideCharacter =
                    _boxShape.TopLeftCharacter = _boxShape.TopRightCharacter = _boxShape.TopSideCharacter =
                    _boxShape.BottomLeftCharacter = _boxShape.BottomRightCharacter = _boxShape.BottomSideCharacter =
                    _settingsPanel.BorderCharacter;

                _boxShape.Foreground = _settingsPanel.LineForeColor;
                _boxShape.FillColor = _settingsPanel.FillColor;
                _boxShape.Fill = _settingsPanel.UseFill;
                _boxShape.BorderBackground = _settingsPanel.LineBackColor;
                _boxShape.Location = new Point(0, 0);
                _boxShape.Width = frame.Width;
                _boxShape.Height = frame.Height;
                _boxShape.Draw(frame);

                _entity.SetActiveAnimation("line");
            }


            // TODO: Make this work. They push DOWN on the mouse, start the line from there, if they "Click" then go to mode where they click a second time
            // If they don't click and hold it down longer than click, pretend a second click happened and draw the line.
            if (info.LeftClicked)
            {
                if (!_firstPoint.HasValue)
                {
                    _firstPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                }
                else
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    Point p1 = new Point(Math.Min(_firstPoint.Value.X, _secondPoint.Value.X), Math.Min(_firstPoint.Value.Y, _secondPoint.Value.Y));
                    //Point p2 = new Point(Math.Max(_firstPoint.Value.X, _secondPoint.Value.X), Math.Max(_firstPoint.Value.Y, _secondPoint.Value.Y));


                    _boxShape.Location = p1;
                    _boxShape.Draw(surface);

                    _firstPoint = null;
                    _secondPoint = null;


                    _entity.SetActiveAnimation("single");

                    //surface.ResyncAllCellEffects();
                }
            }
            else if (info.RightClicked)
            {
                if (_firstPoint.HasValue && !_secondPoint.HasValue)
                {
                    _firstPoint = null;
                    _secondPoint = null;

                    _entity.SetActiveAnimation("single");
                }
            }
        }

        public void MouseEnterSurface(MouseInfo info, CellSurface surface)
        {
            _entity.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, CellSurface surface)
        {
            _entity.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            

        }
    }
}
