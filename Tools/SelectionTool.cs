namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Game;

    class SelectionTool : ITool
    {
        private EntityBrush _entity;
        private AnimatedTextSurface _animSinglePoint;
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
            _animSinglePoint = new AnimatedTextSurface("single", 1, 1, Settings.Config.ScreenFont);
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
                var animation = _entity.Animation;
                ClearBrush(_entity.Position.X, _entity.Position.Y, _previousSurface);
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                _entity.Position += animation.Center;
                _entity.SyncLayers();
            }
            else if (state == SelectionToolPanel.CloneState.Clear)
            {
                var animation = _entity.Animation;
                ClearBrush(_entity.Position.X, _entity.Position.Y, _previousSurface);
                _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            }
            else if (state == SelectionToolPanel.CloneState.Clone)
            {
                var animation = _entity.Animation;
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                _entity.Position += animation.Center;
                _entity.SyncLayers();
            }
        }

        private TextSurface SaveBrush()
        {
            TextSurface newSurface = new TextSurface(_entity.Animation.CurrentFrame.Width, 
                                                     _entity.Animation.CurrentFrame.Height, Settings.Config.ScreenFont);

            _entity.Animation.CurrentFrame.Copy(newSurface);

            return newSurface;
        }

        public void LoadBrush(TextSurface surface)
        {
            _panel.State = SelectionToolPanel.CloneState.Clone;

            // Copy data to new animation
            var cloneAnimation = new AnimatedTextSurface("clone", surface.Width, surface.Height, Settings.Config.ScreenFont);
            var frame = cloneAnimation.CreateFrame();
            surface.Copy(frame);

            cloneAnimation.Center = new Point(cloneAnimation.Width / 2, cloneAnimation.Height / 2);

            _entity.Animations[cloneAnimation.Name] = cloneAnimation;
            _entity.Animation = cloneAnimation;
            _entity.Animation.Tint = new Color(0f, 0f, 0f, 0f);

            _entity.IsVisible = true;
            _entity.TopLayers.Clear();

            var topLayer = new GameObject(Settings.Config.ScreenFont);
            _entity.TopLayers.Add(topLayer);
            var animation = new AnimatedTextSurface("box", surface.Width, surface.Height, Settings.Config.ScreenFont);
            frame = animation.CreateFrame();
            _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
            _boxShape.Location = new Point(0, 0);
            _boxShape.Width = frame.Width;
            _boxShape.Height = frame.Height;
            _boxShape.Draw(new SurfaceEditor(frame));
            animation.Center = cloneAnimation.Center;

            topLayer.Animations[animation.Name] = animation;
            topLayer.Animation = animation;
            topLayer.Animation.Tint = new Color(0f, 0f, 0f, 0.2f);
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

                _entity.Animations[_animSinglePoint.Name] = _animSinglePoint;
                _entity.Animation = _animSinglePoint;

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
                    _entity.Animation.Tint = new Color(0f, 0f, 0f, 0.5f);
                }

                else if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    _panel.State = SelectionToolPanel.CloneState.Selected;

                    // Copy data to new animation
                    var _tempAnimation = _entity.Animations["selection"];
                    AnimatedTextSurface cloneAnimation = new AnimatedTextSurface("clone", _tempAnimation.Width, _tempAnimation.Height, Settings.Config.ScreenFont);
                    var frame = cloneAnimation.CreateFrame();
                    Point topLeftPoint = new Point(Math.Min(_firstPoint.Value.X, _secondPoint.Value.X), Math.Min(_firstPoint.Value.Y, _secondPoint.Value.Y));
                    surface.Copy(topLeftPoint.X, topLeftPoint.Y, cloneAnimation.Width, cloneAnimation.Height, frame, 0, 0);

                    if (_altPanel.SkipEmptyCells && _altPanel.UseAltEmptyColor)
                    {
                        foreach (var cell in frame.Cells)
                        {
                            if (cell.GlyphIndex == 0 && cell.Background == _altPanel.AltEmptyColor)
                                cell.Background = Color.Transparent;
                        }
                    }

                    cloneAnimation.Center = _tempAnimation.Center;

                    _entity.Animations[cloneAnimation.Name] = cloneAnimation;
                    _entity.Animation = cloneAnimation;
                    _entity.Animation.Tint = new Color(0f, 0f, 0f, 0f);

                    // Display the rect
                    _entity.TopLayers.Clear();
                    var topLayer = new GameObject(Settings.Config.ScreenFont);
                    _entity.TopLayers.Add(topLayer);
                    topLayer.Animations[_tempAnimation.Name] = _tempAnimation;
                    topLayer.Animation = _tempAnimation;
                    topLayer.Animation.Tint = new Color(0f, 0f, 0f, 0.35f);
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
                        _entity.Animation = _entity.Animations["single"];
                        _entity.Animation.Tint = new Color(0f, 0f, 0f, 0f);
                    }
                }

                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
                {

                    var animation = new AnimatedTextSurface("selection", Math.Max(_firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(_firstPoint.Value.X, info.ConsoleLocation.X) + 1,
                                                                         Math.Max(_firstPoint.Value.Y, info.ConsoleLocation.Y) - Math.Min(_firstPoint.Value.Y, info.ConsoleLocation.Y) + 1,
                                                                         Settings.Config.ScreenFont);

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

                    if (_entity.Animations.ContainsKey("selection"))
                    {
                        var _tempAnimation = _entity.Animations["selection"];
                        if (_tempAnimation.Center == p1 && _tempAnimation.Width == animation.Width && _tempAnimation.Height == animation.Height)
                        {
                            return;
                        }
                    }


                    animation.Center = p1;

                    Settings.QuickEditor.TextSurface = frame;

                    _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
                    _boxShape.Location = new Point(0, 0);
                    _boxShape.Width = frame.Width;
                    _boxShape.Height = frame.Height;
                    _boxShape.Draw(Settings.QuickEditor);

                    //frame.SetEffect(frame, _pulseAnimation);

                    _entity.Animations[animation.Name] = animation;
                    _entity.Animation = animation;
                }

            }

            _previousState = _panel.State;
        }

        private void StampBrush(int consoleLocationX, int consoleLocationY, ITextSurface surface)
        {
            int destinationX = consoleLocationX - _entity.Animation.Center.X;
            int destinationY = consoleLocationY - _entity.Animation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            for (int curx = 0; curx < _entity.Animation.Width; curx++)
            {
                for (int cury = 0; cury < _entity.Animation.Height; cury++)
                {
                    if (_entity.Animation.CurrentFrame.IsValidCell(curx, cury))
                    {
                        var sourceCell = _entity.Animation.CurrentFrame.GetCell(curx, cury);

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
            int destinationX = consoleLocationX - _entity.Animation.Center.X;
            int destinationY = consoleLocationY - _entity.Animation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            Settings.QuickEditor.TextSurface = surface;

            for (int curx = 0; curx < _entity.Animation.CurrentFrame.Width; curx++)
            {
                for (int cury = 0; cury < _entity.Animation.CurrentFrame.Height; cury++)
                {
                    if (_entity.Animation.CurrentFrame.IsValidCell(curx, cury))
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
