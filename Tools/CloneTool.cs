namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;

    class CloneTool : ITool
    {
        private Entity _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Box _boxShape;
        private CloneToolPanel _panel;
        private SadConsole.Effects.Fade _pulseAnimation;

        private SadConsoleEditor.Tools.CloneToolPanel.CloneState _previousState;

        public const string ID = "CLONE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Clone"; }
        }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public CloneTool()
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


            _panel = new CloneToolPanel();
            ControlPanels = new CustomPanel[] { _panel };

            _pulseAnimation = new SadConsole.Effects.Fade()
            {
                FadeBackground = true,
                UseCellBackground = false,
                DestinationBackground = Color.Transparent,
                FadeDuration = 2d,
                CloneOnApply = false,
                AutoReverse = true,
                Repeat = true,
            };
        }

        public void OnSelected()
        {


            _entity = new Entity();
            _entity.IsVisible = false;

            _entity.AddAnimation(_animSinglePoint);
            _entity.SetActiveAnimation("single");

            EditorConsoleManager.Instance.UpdateBrush(_entity);

            EditorConsoleManager.Instance.ToolPane.ShowBackground = false;
            EditorConsoleManager.Instance.ToolPane.ShowForeground = false;
            EditorConsoleManager.Instance.ToolPane.ShowCharacterList = false;

            _panel.State = CloneToolPanel.CloneState.SelectingPoint1;
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
            if (info.LeftClicked)
            {
                if (_panel.State == CloneToolPanel.CloneState.SelectingPoint1)
                {
                    _panel.State = CloneToolPanel.CloneState.SelectingPoint2;
                    _entity.Tint = new Color(0f, 0f, 0f, 0.5f);
                }

                else if (_panel.State == CloneToolPanel.CloneState.SelectingPoint2)
                {
                    _panel.State = CloneToolPanel.CloneState.Selected;
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                }

                else if (_panel.State == CloneToolPanel.CloneState.Selected)
                {
                    _panel.State = CloneToolPanel.CloneState.MovingClone;

                    // Copy data to new animation
                    var _tempAnimation = _entity.GetAnimation("selection");
                    Animation cloneAnimation = new Animation("clone", _tempAnimation.Width, _tempAnimation.Height);
                    var frame = cloneAnimation.CreateFrame();
                    Point p1 = new Point(Math.Min(_firstPoint.Value.X, _secondPoint.Value.X), Math.Min(_firstPoint.Value.Y, _secondPoint.Value.Y));
                    surface.Copy(p1.X, p1.Y, cloneAnimation.Width, cloneAnimation.Height, frame, 0, 0);

                    if (_panel.SkipEmptyCells && _panel.UseAltEmptyColor)
                    {
                        foreach (var cell in frame)
                        {
                            if (cell.CharacterIndex == 0 && cell.Background == _panel.AltEmptyColor)
                                cell.Background = Color.Transparent;
                        }
                    }

                    cloneAnimation.Center = new Point(cloneAnimation.Width / 2, cloneAnimation.Height / 2);
                    cloneAnimation.Commit();

                    _entity.AddAnimation(cloneAnimation);
                    _entity.SetActiveAnimation("clone");
                    _entity.Tint = new Color(0f, 0f, 0f, 0f);
                }
                else if (_panel.State == CloneToolPanel.CloneState.MovingClone)
                {
                    // STAMP
                    // TODO: Add an option in Copy to skip empty and transparent cells. Pass in _panel.SkipEmptyCells
                    _entity.CellData.Copy(0, 0, _entity.CellData.Width, _entity.CellData.Height, surface, info.ConsoleLocation.X - _entity.CurrentAnimation.Center.X, info.ConsoleLocation.Y - _entity.CurrentAnimation.Center.Y);
                }
            }
            else
            {
                if (_panel.State == CloneToolPanel.CloneState.MovingClone)
                    _entity.Position = info.ConsoleLocation;

                if (_panel.State == CloneToolPanel.CloneState.SelectingPoint1)
                {
                    _entity.Position = info.ConsoleLocation;
                    _firstPoint = _entity.Position;

                    // State was reset and we didn't know about it
                    if (_previousState != _panel.State)
                    {
                        _entity.SetActiveAnimation("single");
                        _entity.Tint = new Color(0f, 0f, 0f, 0f);
                    }
                }

                if (_panel.State == CloneToolPanel.CloneState.SelectingPoint2)
                {

                    Animation animation = new Animation("selection", Math.Max(_firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(_firstPoint.Value.X, info.ConsoleLocation.X) + 1,
                                                                 Math.Max(_firstPoint.Value.Y, info.ConsoleLocation.Y) - Math.Min(_firstPoint.Value.Y, info.ConsoleLocation.Y) + 1);

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

                    var _tempAnimation = _entity.GetAnimation("selection");
                    if (_tempAnimation != null && _tempAnimation.Center == p1 && _tempAnimation.Width == animation.Width && _tempAnimation.Height == animation.Height)
                    {
                        return;
                    }


                    animation.Center = p1;

                    _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
                    _boxShape.Location = new Point(0, 0);
                    _boxShape.Width = frame.Width;
                    _boxShape.Height = frame.Height;
                    _boxShape.Draw(frame);

                    //frame.SetEffect(frame, _pulseAnimation);
                    animation.Commit();

                    _entity.AddAnimation(animation);
                    _entity.SetActiveAnimation("selection");
                }

            }

            _previousState = _panel.State;
        }
    }
}
