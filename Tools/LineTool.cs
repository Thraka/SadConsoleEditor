namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Controls;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;

    class LineTool : ITool
    {
        private EntityBrush _entity;
        private Animation _animSinglePoint;
        private SadConsole.Effects.Fade _frameEffect;
        private Point? _firstPoint;
        private Point? _secondPoint;
        private SadConsole.Shapes.Line _lineShape;
        private Cell _lineCell;
        private CellAppearance _lineStyle;

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

            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel };
        }

        public void OnSelected()
        {
            _lineStyle = new CellAppearance(
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
            _lineStyle.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            _lineStyle.CopyAppearanceTo(_lineCell);

            _entity = new EntityBrush();
            _entity.IsVisible = false;

            _entity.AddAnimation(_animSinglePoint);
            _entity.SetActiveAnimation("single");

            EditorConsoleManager.Instance.UpdateBrush(_entity);

			EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel, System.EventArgs.Empty);
			EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed += EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = true;
		}

        public void OnDeselected()
        {
			EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.Changed -= EditorConsoleManager.Instance.QuickSelectPane.CommonCharacterPickerPanel_ChangedHandler;
			EditorConsoleManager.Instance.QuickSelectPane.IsVisible = false;
		}

        public void RefreshTool()
        {
            _lineStyle = new CellAppearance(
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
            _lineStyle.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
            _lineStyle.CopyAppearanceTo(_lineCell);
        }

        public bool ProcessKeyboard(KeyboardInfo info, CellSurface surface)
        {
            return false;
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

                _lineStyle = new CellAppearance(
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground,
                                    EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingCharacter);
                _lineStyle.SpriteEffect = EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingMirrorEffect;
                _lineStyle.CopyAppearanceTo(_lineCell);

                _lineShape = new SadConsole.Shapes.Line();
                _lineShape.CellAppearance = _lineCell;
                _lineShape.UseEndingCell = false;
                _lineShape.UseStartingCell = false;
                _lineShape.StartingLocation = p1;
                _lineShape.EndingLocation = p2;
                _lineShape.Draw(frame);

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

                    _lineShape.StartingLocation = _firstPoint.Value;
                    _lineShape.EndingLocation = _secondPoint.Value;
                    _lineShape.Draw(surface);

                    _firstPoint = null;
                    _secondPoint = null;
                    _lineShape = null;

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
                    _lineShape = null;

                    _entity.SetActiveAnimation("single");
                }
            }

        }
    }
}
