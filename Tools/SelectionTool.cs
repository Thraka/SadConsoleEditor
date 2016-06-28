namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class SelectionTool : ITool
    {
        private EntityBrush _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Box _boxShape;
        private SelectionToolPanel _panel;
        private SelectionToolAltPanel _altPanel;
        private SadConsole.Effects.Fade _pulseAnimation;
        private ITextSurface _previousSurface;

        private SelectionToolPanel.CloneState _previousState;

        public const string ID = "SELECT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Selection"; }
        }
        public char Hotkey { get { return 's'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public SelectionTool()
        {
            _animSinglePoint = new Animation("single", 1, 1);
            _animSinglePoint.Font = Engine.DefaultFont;
            var _frameSinglePoint = _animSinglePoint.CreateFrame();
            _frameSinglePoint[0].GlyphIndex = 42;


            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };


            _panel = new SelectionToolPanel(LoadBrush, SaveBrush);
            _panel.StateChangedHandler = PanelStateChanged;

            _altPanel = new SelectionToolAltPanel();

            ControlPanels = new CustomPanel[] { _panel, _altPanel };

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

        private void PanelStateChanged(SelectionToolPanel.CloneState state)
        {
            if (state == SelectionToolPanel.CloneState.SelectingPoint1)
            {
                _entity.TopLayers.Clear();
                _entity.IsVisible = false;
            }
            else if (state == SelectionToolPanel.CloneState.Move)
            {
                var animation = _entity.CurrentAnimation;
                ClearBrush(_entity.Position.X, _entity.Position.Y, _previousSurface);
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                _entity.Position += animation.Center;
                _entity.SyncLayers();
            }
            else if (state == SelectionToolPanel.CloneState.Clear)
            {
                var animation = _entity.CurrentAnimation;
                ClearBrush(_entity.Position.X, _entity.Position.Y, _previousSurface);
                _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            }
            else if (state == SelectionToolPanel.CloneState.Clone)
            {
                var animation = _entity.CurrentAnimation;
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                _entity.Position += animation.Center;
                _entity.SyncLayers();
            }
        }

        private TextSurface SaveBrush()
        {
            TextSurface newSurface = new TextSurface(_entity.CurrentAnimation.CurrentFrame.Width, 
                                                     _entity.CurrentAnimation.CurrentFrame.Height, Settings.Config.ScreenFont);

            TextSurface.Copy(_entity.CurrentAnimation.CurrentFrame, newSurface);

            return newSurface;
        }

        public void LoadBrush(TextSurface surface)
        {
            _panel.State = SelectionToolPanel.CloneState.Clone;

            // Copy data to new animation
            Animation cloneAnimation = new Animation("clone", surface.Width, surface.Height);
            var frame = cloneAnimation.CreateFrame();
            TextSurface.Copy(surface, frame);

            cloneAnimation.Center = new Point(cloneAnimation.Width / 2, cloneAnimation.Height / 2);

            _entity.AddAnimation(cloneAnimation);
            _entity.SetActiveAnimation("clone");
            _entity.Tint = new Color(0f, 0f, 0f, 0f);

            _entity.IsVisible = true;
            _entity.TopLayers.Clear();

            var topLayer = new Entity(surface.Width, surface.Height, Settings.Config.ScreenFont);
            _entity.TopLayers.Add(topLayer);
            var animation = new Animation("box", surface.Width, surface.Height);
            frame = animation.CreateFrame();
            _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
            _boxShape.Location = new Point(0, 0);
            _boxShape.Width = frame.Width;
            _boxShape.Height = frame.Height;
            _boxShape.Draw(new SurfaceEditor(frame));
            animation.Center = cloneAnimation.Center;

            topLayer.AddAnimation(animation);
            topLayer.SetActiveAnimation(animation.Name);
            topLayer.Tint = new Color(0f, 0f, 0f, 0.2f);
            //_tempAnimation.Center = cloneAnimation.Center;
            _entity.SyncLayers();
        }

        public void OnSelected()
        {
            if (_panel.State != SelectionToolPanel.CloneState.Clone && _panel.State != SelectionToolPanel.CloneState.Move)
            {
                _entity = new EntityBrush(1, 1);

                _entity.Font = Settings.Config.ScreenFont;
                _entity.IsVisible = true;

                _entity.AddAnimation(_animSinglePoint);
                _entity.SetActiveAnimation("single");

                EditorConsoleManager.Instance.UpdateBrush(_entity);

                _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            }
            else
                EditorConsoleManager.Instance.UpdateBrush(_entity);
        }

        public void OnDeselected()
        {
            //_panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
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
            _previousSurface = surface;
            
            if (_panel.State == SelectionToolPanel.CloneState.Clone || _panel.State == SelectionToolPanel.CloneState.Move)
            {
                _entity.Position = info.ConsoleLocation;
            }

            if (info.RightClicked)
            {
                _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            }

            if (info.LeftClicked)
            {
                if (_panel.State == SelectionToolPanel.CloneState.Clone)
                {
                    StampBrush(info.ConsoleLocation.X, info.ConsoleLocation.Y, surface);
                }
                else if (_panel.State == SelectionToolPanel.CloneState.Move)
                {
                    StampBrush(info.ConsoleLocation.X, info.ConsoleLocation.Y, surface);
                    _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
                }
            }
        }

        public void MouseEnterSurface(MouseInfo info, ITextSurface surface)
        {
            if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1 || _panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
            {
                _entity.IsVisible = true;
                _entity.SyncLayers();
            }
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1 || _panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
            {
                _entity.IsVisible = false;
                _entity.SyncLayers();
            }
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            _entity.IsVisible = true;
            _entity.SyncLayers();

            if (info.LeftClicked)
            {
                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1)
                {
                    _panel.State = SelectionToolPanel.CloneState.SelectingPoint2;
                    _entity.Tint = new Color(0f, 0f, 0f, 0.5f);
                }

                else if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    _panel.State = SelectionToolPanel.CloneState.Selected;

                    // Copy data to new animation
                    var _tempAnimation = _entity.GetAnimation("selection");
                    Animation cloneAnimation = new Animation("clone", _tempAnimation.Width, _tempAnimation.Height);
                    var frame = cloneAnimation.CreateFrame();
                    Point topLeftPoint = new Point(Math.Min(_firstPoint.Value.X, _secondPoint.Value.X), Math.Min(_firstPoint.Value.Y, _secondPoint.Value.Y));
                    TextSurface.Copy(surface, topLeftPoint.X, topLeftPoint.Y, cloneAnimation.Width, cloneAnimation.Height, frame, 0, 0);

                    if (_altPanel.SkipEmptyCells && _altPanel.UseAltEmptyColor)
                    {
                        foreach (var cell in frame)
                        {
                            if (cell.GlyphIndex == 0 && cell.Background == _altPanel.AltEmptyColor)
                                cell.Background = Color.Transparent;
                        }
                    }

                    cloneAnimation.Center = _tempAnimation.Center;

                    _entity.AddAnimation(cloneAnimation);
                    _entity.SetActiveAnimation("clone");
                    _entity.Tint = new Color(0f, 0f, 0f, 0f);

                    // Display the rect
                    _entity.TopLayers.Clear();
                    var topLayer = new Entity(1, 1, Settings.Config.ScreenFont);
                    _entity.TopLayers.Add(topLayer);
                    topLayer.AddAnimation(_tempAnimation);
                    topLayer.SetActiveAnimation(_tempAnimation.Name);
                    topLayer.Tint = new Color(0f, 0f, 0f, 0.35f);
                    topLayer.Position = _entity.Position;
                    _entity.SyncLayers();
                }

                else if (_panel.State == SelectionToolPanel.CloneState.Selected)
                {
                    
                }
                else if (_panel.State == SelectionToolPanel.CloneState.Clone)
                {
                    //StampBrush(info.ConsoleLocation.X, info.ConsoleLocation.Y, surface);
                }
                else if (_panel.State == SelectionToolPanel.CloneState.Clear)
                {
                    // Erase selected area
                }
                else if (_panel.State == SelectionToolPanel.CloneState.Move)
                {
                    // Move the selected cells
                }


            }
            else
            {
                
                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1)
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

                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
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

                    Settings.QuickEditor.TextSurface = frame;

                    _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
                    _boxShape.Location = new Point(0, 0);
                    _boxShape.Width = frame.Width;
                    _boxShape.Height = frame.Height;
                    _boxShape.Draw(Settings.QuickEditor);

                    //frame.SetEffect(frame, _pulseAnimation);

                    _entity.AddAnimation(animation);
                    _entity.SetActiveAnimation("selection");
                }

            }

            _previousState = _panel.State;
        }

        private void StampBrush(int consoleLocationX, int consoleLocationY, ITextSurface surface)
        {
            int destinationX = consoleLocationX - _entity.CurrentAnimation.Center.X;
            int destinationY = consoleLocationY - _entity.CurrentAnimation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            for (int curx = 0; curx < _entity.CurrentAnimation.Width; curx++)
            {
                for (int cury = 0; cury < _entity.CurrentAnimation.Height; cury++)
                {
                    if (_entity.CurrentAnimation.CurrentFrame.IsValidCell(curx, cury))
                    {
                        var sourceCell = _entity.CurrentAnimation.CurrentFrame.GetCell(curx, cury);

                        // Not working, breakpoint here to remind me.
                        if (_altPanel.SkipEmptyCells && sourceCell.GlyphIndex == 0 && (sourceCell.Background == Color.Transparent || (_altPanel.UseAltEmptyColor && sourceCell.Background == _altPanel.AltEmptyColor)))
                        {
                            destY++;
                            continue;
                        }

                        if (surface.IsValidCell(destX, destY))
                        {
                            var desCell = surface.GetCell(destX, destY);
                            sourceCell.CopyAppearanceTo(desCell);
                            //TODO: effects
                            //surface.SetEffect(desCell, sourceCell.Effect);
                        }
                    }
                    destY++;
                }
                destY = destinationY;
                destX++;
            }
        }

        private void ClearBrush(int consoleLocationX, int consoleLocationY, ITextSurface surface)
        {
            int destinationX = consoleLocationX - _entity.CurrentAnimation.Center.X;
            int destinationY = consoleLocationY - _entity.CurrentAnimation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            Settings.QuickEditor.TextSurface = surface;

            for (int curx = 0; curx < _entity.CurrentAnimation.CurrentFrame.Width; curx++)
            {
                for (int cury = 0; cury < _entity.CurrentAnimation.CurrentFrame.Height; cury++)
                {
                    if (_entity.CurrentAnimation.CurrentFrame.IsValidCell(curx, cury))
                    {
                        if (surface.IsValidCell(destX, destY))
                        {
                            Settings.QuickEditor.Clear(destX, destY);
                        }
                    }
                    destY++;
                }
                destY = destinationY;
                destX++;
            }
        }
    }
}
