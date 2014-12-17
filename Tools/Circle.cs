namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;

    class CircleTool : ITool
    {
        private EntityBrush _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Circle _circleShape;
        private SadConsole.Shapes.Ellipse _ellipseShape;

        private CircleToolPanel _settingsPanel;
        private CellAppearance _borderAppearance;

        public const string ID = "CIRCLE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Circle"; }
        }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public CircleTool()
        {
            _animSinglePoint = new Animation("single", 1, 1);
            _animSinglePoint.Font = Engine.DefaultFont;
            var _frameSinglePoint = _animSinglePoint.CreateFrame();
            _frameSinglePoint[0].CharacterIndex = 42;
            _animSinglePoint.Commit();


            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _settingsPanel = new CircleToolPanel();

            ControlPanels = new CustomPanel[] { _settingsPanel, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel};
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
        }

        public void RefreshTool()
        {
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {

        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {
            _entity.IsVisible = true;
            if (!_firstPoint.HasValue)
            {
                _entity.Position = info.ConsoleLocation;

                _settingsPanel.CircleWidth = 0;
                _settingsPanel.CircleHeight = 0;
            }
            else
            {
                Animation animation;
                // Draw the line (erase old) to where the mouse is
                // create the animation frame
                animation = new Animation("line", Math.Max(_firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(_firstPoint.Value.X, info.ConsoleLocation.X) + 1,
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

                _settingsPanel.CircleWidth = frame.Width;
                _settingsPanel.CircleHeight = frame.Height;

                animation.Center = p1;

                _ellipseShape = new SadConsole.Shapes.Ellipse();
                _ellipseShape.BorderAppearance = _borderAppearance;
                _ellipseShape.EndingPoint = new Point(frame.Width - 1, frame.Height - 1);
                _ellipseShape.Draw(frame);

                animation.Commit();
                _entity.SetActiveAnimation("line");
            }


            // TODO: Make this work. They push DOWN on the mouse, start the line from there, if they "Click" then go to mode where they click a second time
            // If they don't click and hold it down longer than click, pretend a second click happened and draw the line.
            if (info.LeftClicked)
            {
                if (!_firstPoint.HasValue)
                {
                    _firstPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    _borderAppearance = new CellAppearance(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                                           EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
                                                           EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
                }
                else
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    Point p1 = new Point(Math.Min(_firstPoint.Value.X, _secondPoint.Value.X), Math.Min(_firstPoint.Value.Y, _secondPoint.Value.Y));
                    Point p2 = new Point(Math.Max(_firstPoint.Value.X, _secondPoint.Value.X), Math.Max(_firstPoint.Value.Y, _secondPoint.Value.Y));

                    _ellipseShape.StartingPoint = p1;
                    _ellipseShape.EndingPoint = p2;
                    _ellipseShape.Draw(surface);

                    _entity.SetActiveAnimation("single");
                    _entity.Position = _secondPoint.Value;


                    _firstPoint = null;
                    _secondPoint = null;

                    //surface.ResyncAllCellEffects();
                }
            }
            else if (info.RightClicked)
            {
                if (_firstPoint.HasValue && !_secondPoint.HasValue)
                {
                    _firstPoint = null;
                    _secondPoint = null;

                    _settingsPanel.CircleWidth = 0;
                    _settingsPanel.CircleHeight = 0;

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
            if (!_firstPoint.HasValue)
                _entity.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, CellSurface surface)
        {
            

        }
    }
}
