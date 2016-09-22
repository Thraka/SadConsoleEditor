namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Controls;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class LineTool : ITool
    {
        public SadConsole.Game.GameObject Brush;

        private AnimatedTextSurface _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Line _lineShape;
        private Cell _lineCell;
        private CellAppearance _lineStyle;
        private LineToolPanel _settingsPanel;

        public const string ID = "LINE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Line"; }
        }
        public char Hotkey { get { return 'l'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public LineTool()
        {
            _frameEffect = new SadConsole.Effects.Fade()
            {
                UseCellBackground = true,
                FadeForeground = true,
                FadeDuration = 1f,
                AutoReverse = true
            };

            _lineCell = new Cell();

            _settingsPanel = new LineToolPanel();


            // Configure the animations
            Brush = new SadConsole.Game.GameObject();
            Brush.Font = Settings.Config.ScreenFont;
            AnimatedTextSurface animation = new AnimatedTextSurface("single", 1, 1, Settings.Config.ScreenFont);
            animation.CreateFrame()[0].GlyphIndex = 42;
            Brush.Animations.Add(animation.Name, animation);
            SetAnimationSingle();

            ControlPanels = new CustomPanel[] { _settingsPanel, CharacterPickPanel.SharedInstance };
        }

        private void SetAnimationLine(Point mousePosition)
        {
            // Draw the line (erase old) to where the mouse is
            // create the animation frame
            AnimatedTextSurface animation = new AnimatedTextSurface("line", Math.Max(_firstPoint.Value.X, mousePosition.X) - Math.Min(_firstPoint.Value.X, mousePosition.X) + 1,
                                                                            Math.Max(_firstPoint.Value.Y, mousePosition.Y) - Math.Min(_firstPoint.Value.Y, mousePosition.Y) + 1,
                                                                            Settings.Config.ScreenFont);


            var frame = animation.CreateFrame();

            Point p1;
            Point p2;

            if (_firstPoint.Value.X > mousePosition.X)
            {
                if (_firstPoint.Value.Y > mousePosition.Y)
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
                if (_firstPoint.Value.Y > mousePosition.Y)
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

            //_lineStyle = new CellAppearance(
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
            //                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
            //_lineStyle.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            //_lineStyle.CopyAppearanceTo(_lineCell);

            _lineShape = new SadConsole.Shapes.Line();
            _lineShape.CellAppearance = _lineCell;
            _lineShape.UseEndingCell = false;
            _lineShape.UseStartingCell = false;
            _lineShape.StartingLocation = p1;
            _lineShape.EndingLocation = p2;
            _lineShape.Draw(new SurfaceEditor(frame));

            _settingsPanel.LineLength = frame.Width > frame.Height ? frame.Width : frame.Height;

            Brush.Animation = animation;
        }

        private void SetAnimationSingle()
        {
            Brush.Animation = Brush.Animations["single"];
        }

        public void OnSelected()
        {
            RefreshTool();
            EditorConsoleManager.Brush = Brush;
            EditorConsoleManager.UpdateBrush();

            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(CharacterPickPanel.SharedInstance, System.EventArgs.Empty);
            CharacterPickPanel.SharedInstance.Changed += CharPanelChanged;
            EditorConsoleManager.QuickSelectPane.IsVisible = true;


            _lineStyle = new CellAppearance(CharacterPickPanel.SharedInstance.SettingForeground,
                                              CharacterPickPanel.SharedInstance.SettingBackground,
                                              CharacterPickPanel.SharedInstance.SettingCharacter,
                                              CharacterPickPanel.SharedInstance.SettingMirrorEffect);


            _lineStyle.CopyAppearanceTo(_lineCell);
            
		}

        public void OnDeselected()
        {
            CharacterPickPanel.SharedInstance.Changed -= CharPanelChanged;
            EditorConsoleManager.QuickSelectPane.IsVisible = false;

            _settingsPanel.LineLength = 0;
            _firstPoint = null;
            _secondPoint = null;
            _lineShape = null;

        }

        private void CharPanelChanged(object sender, System.EventArgs e)
        {
            EditorConsoleManager.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(sender, e);
            RefreshTool();
        }

        public void RefreshTool()
        {
            _lineStyle = new CellAppearance(CharacterPickPanel.SharedInstance.SettingForeground,
                                              CharacterPickPanel.SharedInstance.SettingBackground,
                                              CharacterPickPanel.SharedInstance.SettingCharacter,
                                              CharacterPickPanel.SharedInstance.SettingMirrorEffect);

            _lineStyle.CopyAppearanceTo(_lineCell);
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
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
            Brush.IsVisible = true;

            if (!_firstPoint.HasValue)
            {
                Brush.Position = info.ConsoleLocation;
            }
            else
            {
                SetAnimationLine(info.ConsoleLocation);
                
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

                    _lineShape.StartingLocation = _firstPoint.Value;
                    _lineShape.EndingLocation = _secondPoint.Value;
                    _lineShape.Draw(new SurfaceEditor(surface));

                    _firstPoint = null;
                    _secondPoint = null;
                    _lineShape = null;

                    Brush.Animation = Brush.Animations["single"];

                    //surface.ResyncAllCellEffects();
                    _settingsPanel.LineLength = 0;
                }
            }
            else if (info.RightClicked)
            {
                if (_firstPoint.HasValue && !_secondPoint.HasValue)
                {
                    _firstPoint = null;
                    _secondPoint = null;
                    _lineShape = null;

                    Brush.Animation = Brush.Animations["single"];

                    _settingsPanel.LineLength = 0;
                }
            }

        }
    }
}
