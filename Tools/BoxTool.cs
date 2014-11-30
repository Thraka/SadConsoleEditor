namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;

    class BoxTool : ITool
    {
        private Entity _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Box _boxShape;
        private Cell _lineCell;
        private CellAppearance _lineStyle;

        public const string ID = "BOX";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Box"; }
        }

        public CustomPane[] ControlPanes { get; private set; }

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
            _animSinglePoint.Commit();


            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _lineCell = new Cell();

        }

        public void OnSelected()
        {
            _lineStyle = new CellAppearance(
                                    EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor,
                                    EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor,
                                    EditorConsoleManager.Instance.ToolPane.SelectedCharacter);
            _lineStyle.CopyAppearanceTo(_lineCell);

            _entity = new Entity();
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
            _lineStyle = new CellAppearance(
                                    EditorConsoleManager.Instance.ToolPane.CharacterForegroundColor,
                                    EditorConsoleManager.Instance.ToolPane.CharacterBackgroundColor,
                                    EditorConsoleManager.Instance.ToolPane.SelectedCharacter);
            _lineStyle.CopyAppearanceTo(_lineCell);
        }

        public void ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {

        }

        public void ProcessMouse(MouseInfo info, CellSurface surface)
        {

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
                Point p2;

                if (_firstPoint.Value.X > info.ConsoleLocation.X)
                {
                    if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
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
                    if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
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

                _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
                _boxShape.Location = p1;
                _boxShape.Width = p2.X - p1.X;
                _boxShape.Height = p2.Y - p1.Y;
                _boxShape.Draw(frame);

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
                }
                else
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);

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
    }
}
