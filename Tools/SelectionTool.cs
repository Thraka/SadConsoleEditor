namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using SadConsole.Game;

    class LayeredGameObject : GameObject
    {
        public GameObject SelectedSurface;
        public bool ShowSelectedSurface;


        public LayeredGameObject()
        {
            SelectedSurface = new GameObject(Settings.Config.ScreenFont);
        }

        public override void Render()
        {
            if (IsVisible)
            {
                if (repositionRects)
                {
                    if (ShowSelectedSurface)
                        renderer.Render(SelectedSurface, NoMatrix);
                    renderer.Render(this, NoMatrix);
                }
                else
                {
                    if (ShowSelectedSurface)
                        renderer.Render(SelectedSurface, position + renderOffset - animation.Center, usePixelPositioning);
                    renderer.Render(this, position + renderOffset - animation.Center, usePixelPositioning);
                }
            }
        }

        public override void Update()
        {
            Animation.Update();
        }

        protected override void OnPositionChanged(Point oldLocation)
        {
            SelectedSurface.Position = Position;
            base.OnPositionChanged(oldLocation);
        }
    }

    class SelectionTool : ITool
    {
        private const string AnimationSingle = "single";
        private const string AnimationSelection = "selection";

        public LayeredGameObject Brush;

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
            Brush = new LayeredGameObject();
            Brush.Font = Settings.Config.ScreenFont;
            

            var animation = new AnimatedTextSurface(AnimationSingle, 1, 1, Settings.Config.ScreenFont);
            animation.CreateFrame()[0].GlyphIndex = 42;
            Brush.Animations.Add(animation.Name, animation);

            animation = new AnimatedTextSurface(AnimationSelection, 1, 1, Settings.Config.ScreenFont);
            Brush.Animations.Add(animation.Name, animation);

            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };


            _panel = new SelectionToolPanel(LoadBrush, SaveBrush);
            _panel.StateChangedHandler = PanelStateChanged;
            _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;

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
                Brush.ShowSelectedSurface = false;
                Brush.IsVisible = false;
                Brush.Animation = Brush.Animations[AnimationSingle];
            }
            else if (state == SelectionToolPanel.CloneState.Move)
            {
                var animation = Brush.Animation;
                Brush.ShowSelectedSurface = false;
                ClearBrush(Brush.Position.X, Brush.Position.Y, _previousSurface);
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                Brush.Position += animation.Center;

            }
            else if (state == SelectionToolPanel.CloneState.Clear)
            {
                var animation = Brush.Animation;
                Brush.ShowSelectedSurface = false;
                ClearBrush(Brush.Position.X, Brush.Position.Y, _previousSurface);
                _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            }
            else if (state == SelectionToolPanel.CloneState.Clone)
            {
                var animation = Brush.Animation;
                Brush.ShowSelectedSurface = true;
                animation.Center = new Point(animation.Width / 2, animation.Height / 2);
                Brush.Position += animation.Center;
            }
        }

        private TextSurface SaveBrush()
        {
            TextSurface newSurface = new TextSurface(Brush.SelectedSurface.Animation.CurrentFrame.Width,
                                                     Brush.SelectedSurface.Animation.CurrentFrame.Height, Settings.Config.ScreenFont);

            Brush.SelectedSurface.Animation.CurrentFrame.Copy(newSurface);

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

            Brush.SelectedSurface.Animation = cloneAnimation;
            //Brush.Animation.Tint = new Color(0f, 0f, 0f, 0f);

            Brush.IsVisible = true;

            MakeBoxAnimation(surface.Width, surface.Height, cloneAnimation.Center);
        }

        private void MakeBoxAnimation(int width, int height, Point center)
        {
            AnimatedTextSurface animation;

            if (Brush.Animations.ContainsKey(AnimationSelection))
            {
                animation = Brush.Animations[AnimationSelection];

                if (animation.Width == width && animation.Height == height && animation.Center == center)
                {
                    Brush.Animation = animation;
                    return;
                }
            }
            
            animation = new AnimatedTextSurface(AnimationSelection, width, height, Settings.Config.ScreenFont);
            Settings.QuickEditor.TextSurface = animation.CreateFrame();

            _boxShape = SadConsole.Shapes.Box.GetDefaultBox();
            _boxShape.Location = new Point(0, 0);
            _boxShape.Width = width;
            _boxShape.Height = height;
            _boxShape.Draw(Settings.QuickEditor);

            //frame.SetEffect(frame, _pulseAnimation);

            Brush.Animations[animation.Name] = animation;
            Brush.Animation = animation;
        }

        public void OnSelected()
        {
            Brush.IsVisible = true;
            Brush.Animation = Brush.Animations["single"];

            EditorConsoleManager.Brush = Brush;
            EditorConsoleManager.UpdateBrush();

            _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;


            //if (_panel.State != SelectionToolPanel.CloneState.Clone && _panel.State != SelectionToolPanel.CloneState.Move)
            //{
            //    Brush.IsVisible = true;
            //    Brush.Animation = Brush.Animations["single"];

            //    EditorConsoleManager.Brush = Brush;
            //    EditorConsoleManager.UpdateBrush();

            //    _panel.State = SelectionToolPanel.CloneState.SelectingPoint1;
            //}
            //else
            //{
            //    EditorConsoleManager.Brush = Brush;
            //    EditorConsoleManager.UpdateBrush();
            //}
        }

        public void OnDeselected()
        {
            Brush.IsVisible = false;
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
                Brush.Position = info.ConsoleLocation;
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
                Brush.IsVisible = true;
                //_entity.SyncLayers();
            }
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1 || _panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
            {
                Brush.IsVisible = true;
                //_entity.SyncLayers();
            }
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = true;
            //_entity.SyncLayers();

            if (info.LeftClicked)
            {
                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint1)
                {
                    _panel.State = SelectionToolPanel.CloneState.SelectingPoint2;
                    Brush.Animation.Tint = new Color(0f, 0f, 0f, 0.5f);
                }

                else if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
                {
                    _secondPoint = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                    _panel.State = SelectionToolPanel.CloneState.Selected;
                    
                    // Copy data to new animation
                    
                    AnimatedTextSurface cloneAnimation = new AnimatedTextSurface("clone", Brush.Width, Brush.Height, Settings.Config.ScreenFont);
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

                    cloneAnimation.Center = Brush.Animation.Center;

                    Brush.SelectedSurface.Animation = cloneAnimation;
                    //Brush.Animations[cloneAnimation.Name] = cloneAnimation;
                    //Brush.Animation = cloneAnimation;
                    //Brush.Animation.Tint = new Color(0f, 0f, 0f, 0f);

                    //// Display the rect
                    //var topLayer = new GameObject(Settings.Config.ScreenFont);
                    //Brush.UnderAnimation = topLayer;
                    //topLayer.Animations[_tempAnimation.Name] = _tempAnimation;
                    //topLayer.Animation = _tempAnimation;
                    //topLayer.Animation.Tint = new Color(0f, 0f, 0f, 0.35f);
                    //topLayer.Position = Brush.Position;
                    ////_entity.SyncLayers();
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
                    Brush.Position = info.ConsoleLocation;
                    _firstPoint = Brush.Position;

                    // State was reset and we didn't know about it
                    if (_previousState != _panel.State || Brush.Animation.Name != AnimationSingle)
                    {
                        Brush.Animation = Brush.Animations[AnimationSingle];
                        Brush.Animation.Tint = new Color(0f, 0f, 0f, 0f);
                    }
                }

                if (_panel.State == SelectionToolPanel.CloneState.SelectingPoint2)
                {
                    int width = Math.Max(_firstPoint.Value.X, info.ConsoleLocation.X) - Math.Min(_firstPoint.Value.X, info.ConsoleLocation.X) + 1;
                    int height = Math.Max(_firstPoint.Value.Y, info.ConsoleLocation.Y) - Math.Min(_firstPoint.Value.Y, info.ConsoleLocation.Y) + 1;

                    Point p1;

                    if (_firstPoint.Value.X > info.ConsoleLocation.X)
                    {
                        if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
                            p1 = new Point(width - 1, height - 1);
                        else
                            p1 = new Point(width - 1, 0);
                    }
                    else
                    {
                        if (_firstPoint.Value.Y > info.ConsoleLocation.Y)
                            p1 = new Point(0, height - 1);
                        else
                            p1 = new Point(0, 0);
                    }

                    MakeBoxAnimation(width, height, p1);
                }

            }

            _previousState = _panel.State;
        }

        private void StampBrush(int consoleLocationX, int consoleLocationY, ITextSurface surface)
        {
            int destinationX = consoleLocationX - Brush.SelectedSurface.Animation.Center.X;
            int destinationY = consoleLocationY - Brush.SelectedSurface.Animation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            for (int curx = 0; curx < Brush.SelectedSurface.Animation.Width; curx++)
            {
                for (int cury = 0; cury < Brush.SelectedSurface.Animation.Height; cury++)
                {
                    if (Brush.SelectedSurface.Animation.CurrentFrame.IsValidCell(curx, cury))
                    {
                        var sourceCell = Brush.SelectedSurface.Animation.CurrentFrame.GetCell(curx, cury);

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
            int destinationX = consoleLocationX - Brush.SelectedSurface.Animation.Center.X;
            int destinationY = consoleLocationY - Brush.SelectedSurface.Animation.Center.Y;
            int destX = destinationX;
            int destY = destinationY;

            Settings.QuickEditor.TextSurface = surface;

            for (int curx = 0; curx < Brush.SelectedSurface.Animation.CurrentFrame.Width; curx++)
            {
                for (int cury = 0; cury < Brush.SelectedSurface.Animation.CurrentFrame.Height; cury++)
                {
                    if (Brush.SelectedSurface.Animation.CurrentFrame.IsValidCell(curx, cury))
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
