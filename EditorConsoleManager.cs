using SadConsole.Consoles;
using System;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Windows;
using SadConsole.Input;
using System.Collections.Generic;
using SadConsoleEditor.Editors;

namespace SadConsoleEditor
{
    class EditorConsoleManager: ConsoleList
    {
        private static EditorConsoleManager _instance;

        public static EditorConsoleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorConsoleManager();
                    _instance.FinishCreating();
                }

                return _instance;
            }
        }

        private Consoles.BorderRenderer _borderRenderer;
        private Editors.IEditor _oldEditor;
        public Dictionary<string, Editors.IEditor> Editors;
        public IEditor SelectedEditor { get; private set; }
        private SadConsole.Controls.ScrollBar _toolsPaneScroller;

        public int EditingSurfaceWidth { get { return SelectedEditor.Width; } }
        public int EditingSurfaceHeight { get { return SelectedEditor.Height; } }

        
        public SadConsole.Entities.Entity Brush { get; private set; }
        public Consoles.ToolPane ToolPane { get; private set; }

        public bool AllowKeyboardToMoveConsole { get; set; }

        public SadConsole.Font Font { get; set; }

        private ControlsConsole _backingPanel;

        private EditorConsoleManager()
        {
            Font = SadConsole.Engine.DefaultFont;

            _backingPanel = new ControlsConsole(Game1.WindowSize.X, 1);

            _backingPanel.CellData.DefaultBackground = Settings.Color_MenuBack;
            _backingPanel.CellData.Clear();

            _backingPanel.IsVisible = true;

            Color Green = new Color(165, 224, 45);
            Color Red = new Color(246, 38, 108);
            Color Blue = new Color(100, 217, 234);
            Color Grey = new Color(117, 111, 81);
            Color Yellow = new Color(226, 218, 110);
            Color Orange = new Color(251, 149, 31);

            //_backingPanel.CellData.Print(0, 0, "Test", Green);
            //_backingPanel.CellData.Print(5, 0, "Test", Red);
            //_backingPanel.CellData.Print(10, 0, "Test", Blue);
            //_backingPanel.CellData.Print(15, 0, "Test", Grey);
            //_backingPanel.CellData.Print(20, 0, "Test", Yellow);
            //_backingPanel.CellData.Print(25, 0, "Test", Orange);

            this.Add(_backingPanel);

            Editors = new Dictionary<string, SadConsoleEditor.Editors.IEditor>();
            Editors.Add(DrawingEditor.ID, new DrawingEditor());
            Editors.Add(ObjectEditor.ID, new ObjectEditor());
        }

        private void FinishCreating()
        {
            _borderRenderer = new Consoles.BorderRenderer();

            ToolPane = new Consoles.ToolPane();
            ToolPane.Position = new Point(_backingPanel.CellData.Width - ToolPane.CellData.Width - 1, 1);
            ToolPane.CellData.Resize(ToolPane.CellData.Width, ToolPane.CellData.Height * 2);
            ToolPane.ViewArea = new Rectangle(0,0,ToolPane.CellData.Width, Game1.WindowSize.Y);
            this.Add(ToolPane);

            _toolsPaneScroller = new SadConsole.Controls.ScrollBar(System.Windows.Controls.Orientation.Vertical, Game1.WindowSize.Y - 1);
            _toolsPaneScroller.Maximum = ToolPane.CellData.Height - Game1.WindowSize.Y;
            _toolsPaneScroller.ValueChanged += (o, e) =>
                {
                    ToolPane.ViewArea = new Rectangle(0, _toolsPaneScroller.Value, ToolPane.CellData.Width, Game1.WindowSize.Y);
                };
            var scrollerContainer = new ControlsConsole(1, _toolsPaneScroller.Height);
            scrollerContainer.Add(_toolsPaneScroller);
            scrollerContainer.Position = new Point(_backingPanel.CellData.Width - 1, 1);
            scrollerContainer.IsVisible = true;
            scrollerContainer.MouseCanFocus = false;
            scrollerContainer.ProcessMouseWithoutFocus = true;
            this.Add(scrollerContainer);

            ToolPane.FinishCreating();

            ChangeEditor(new DrawingEditor());
        }

        public void ChangeEditor(IEditor editor)
        {
            SelectedEditor = editor;
            ToolPane.SetupEditor();
            UpdateBox();
        }

        public void UpdateBrush(SadConsole.Entities.Entity newBrushEntity)
        {
            Brush = newBrushEntity;
            Brush.PositionOffset = SelectedEditor.GetPosition();
        }

        public void UpdateBox()
        {
            if (_borderRenderer != null)
            {
                _borderRenderer.CellData.Resize(EditingSurfaceWidth + 2, EditingSurfaceHeight + 2);
            }
            CenterEditor();
        }

        private void CenterEditor()
        {
            // Was in the middle of moving EditingContainer to this. Got to finish this. This should happen anytime the editing surface resizes.

            Point position = new Point();
            //this.ResetViewArea();

            var screenSize = SadConsole.Engine.GetScreenSizeInCells(EditorConsoleManager.Instance.ToolPane.Font);

            if (EditingSurfaceWidth < screenSize.X)
                position.X = ((screenSize.X - 20) / 2) - (EditingSurfaceWidth / 2);
            else
                position.X = ((screenSize.X - 20) - EditingSurfaceWidth) / 2;

            if (EditingSurfaceHeight < screenSize.Y)
                position.Y = (screenSize.Y / 2) - (EditingSurfaceHeight / 2);
            else
                position.Y = (screenSize.Y - EditingSurfaceHeight) / 2;

            SelectedEditor.Position(position.X, position.Y);
            _borderRenderer.Position = new Microsoft.Xna.Framework.Point(position.X - 1, position.Y - 1);
            Brush.PositionOffset = position;
        }

        public override void Update()
        {
            base.Update();

            SelectedEditor.Surface.Update();
            Brush.Update();

            ProcessKeyboard(SadConsole.Engine.Keyboard);
        }

        public override void Render()
        {
            
            SelectedEditor.Surface.Render();

            if (_borderRenderer != null)
                _borderRenderer.Render();

            Brush.Render();

            // Draw the border for the console around it.
            base.Render();
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            var result = base.ProcessMouse(info);

            SelectedEditor.ProcessMouse(info);

            return result;
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            //var result = base.ProcessKeyboard(info);
            if (AllowKeyboardToMoveConsole)
            {
                var position = new Point(_borderRenderer.Position.X + 1, _borderRenderer.Position.Y + 1);
                bool keyPressed = false;
                if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    if (_borderRenderer.Position.X + _borderRenderer.CellData.Width - 1 != 0)
                    {
                        position.X -= 1;
                        keyPressed = true;
                    }
                }
                else if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    if (_borderRenderer.Position.X != ToolPane.Position.X - 1)
                    {
                        position.X += 1;
                        keyPressed = true;
                    }
                }

                if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    if (_borderRenderer.Position.Y + _borderRenderer.CellData.Height - 2 != 0)
                    {
                        position.Y -= 1;
                        keyPressed = true;
                    }

                }
                else if (info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    if (_borderRenderer.Position.Y != Game1.WindowSize.Y - 1)
                    {
                        position.Y += 1;
                        keyPressed = true;
                    }
                }

                if (keyPressed)
                {
                    SelectedEditor.Position(position.X, position.Y);
                    _borderRenderer.Position = new Microsoft.Xna.Framework.Point(position.X - 1, position.Y - 1);
                    Brush.PositionOffset = position;
                }
            }

            SelectedEditor.ProcessKeyboard(info);

            return true;
        }

        public void ShowResizeConsolePopup()
        {
            var popup = new ResizeSurfacePopup(EditingSurfaceWidth, EditingSurfaceHeight);
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    ResizeEditingSurface(popup.SettingWidth, popup.SettingHeight);
                }
            };

            popup.Show(true);
            popup.Center();
        }

        public void ShowNewConsolePopup()
        {
            var popup = new NewConsolePopup();
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    SelectedEditor = popup.Editor;
                    SelectedEditor.Reset();
                    ResizeEditingSurface(popup.SettingWidth, popup.SettingHeight);
                    SelectedEditor.Surface.Clear(popup.SettingForeground, popup.SettingBackground);
                    ToolPane.SetupEditor();
                }
            };

            popup.Show(true);
            popup.Center();
        }

        public void ResizeEditingSurface(int width, int height)
        {
            SelectedEditor.Resize(width, height);
        }

        public void LoadSurface()
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    SelectedEditor.Load(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.PreferredExtensions = SelectedEditor.FileExtensions;
            popup.Show(true);
            popup.Center();
        }

        public void SaveSurface()
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    SelectedEditor.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.PreferredExtensions = SelectedEditor.FileExtensions;
            popup.Show(true);
            popup.Center();
        }
    }
}
