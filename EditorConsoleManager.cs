using SadConsole.Consoles;
using System;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Windows;
using SadConsole.Input;

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
        private SadConsole.Controls.ScrollBar _toolsPaneScroller;

        public int EditingSurfaceWidth { get; private set; }
        public int EditingSurfaceHeight { get; private set; }

        
        public SadConsole.Entities.Entity Brush { get; private set; }
        public Consoles.ToolPane ToolPane { get; private set; }

       

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

            ToolPane.SelectedEditorChanged += (o, e) =>
                {
                    if (_oldEditor != null)
                    {
                        //_oldEditor.MouseEnter -= Editor_MouseEnter;
                        //_oldEditor.MouseExit -= Editor_MouseExit;
                        //_oldEditor.MouseMove -= Editor_MouseMove;
                    }

                    _oldEditor = ToolPane.SelectedEditor;

                    //_oldEditor.MouseEnter += Editor_MouseEnter;
                    //_oldEditor.MouseExit += Editor_MouseExit;
                    //_oldEditor.MouseMove += Editor_MouseMove;

                    UpdateBox();

                };

            ToolPane.FinishCreating();

            EditingSurfaceWidth = ToolPane.SelectedEditor.Width;
            EditingSurfaceHeight = ToolPane.SelectedEditor.Height;
        }

        public void UpdateBrush(SadConsole.Entities.Entity newBrushEntity)
        {
            Brush = newBrushEntity;
            CenterEditor();
        }

        public void UpdateBox()
        {
            if (_borderRenderer != null)
            {
                _borderRenderer.CellData.Resize(ToolPane.SelectedEditor.Width + 2, ToolPane.SelectedEditor.Height+ 2);
            }
            CenterEditor();
            EditingSurfaceWidth = ToolPane.SelectedEditor.Width;
            EditingSurfaceHeight = ToolPane.SelectedEditor.Height;
        }

        private void CenterEditor()
        {
            // Was in the middle of moving EditingContainer to this. Got to finish this. This should happen anytime the editing surface resizes.

            Point position = new Point();
            //this.ResetViewArea();

            var screenSize = SadConsole.Engine.GetScreenSizeInCells(EditorConsoleManager.Instance.ToolPane.Font);

            if (ToolPane.SelectedEditor.Width < screenSize.X)
                position.X = ((screenSize.X - 20) / 2) - (ToolPane.SelectedEditor.Width / 2);
            else
                position.X = ((screenSize.X - 20) - ToolPane.SelectedEditor.Width) / 2;

            if (ToolPane.SelectedEditor.Height < screenSize.Y)
                position.Y = (screenSize.Y / 2) - (ToolPane.SelectedEditor.Height / 2);
            else
                position.Y = (screenSize.Y - ToolPane.SelectedEditor.Height) / 2;

            ToolPane.SelectedEditor.Position(position.X, position.Y);
            _borderRenderer.Position = new Microsoft.Xna.Framework.Point(position.X - 1, position.Y - 1);
            Brush.PositionOffset = position;
        }

        public override void Update()
        {
            base.Update();

            ToolPane.SelectedEditor.Surface.Update();
            Brush.Update();

            ProcessKeyboard(SadConsole.Engine.Keyboard);
        }

        public override void Render()
        {
            
            ToolPane.SelectedEditor.Surface.Render();

            if (_borderRenderer != null)
                _borderRenderer.Render();

            Brush.Render();

            // Draw the border for the console around it.
            base.Render();
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            var result = base.ProcessMouse(info);

            ToolPane.SelectedEditor.ProcessMouse(info);

            return result;
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            var result = base.ProcessKeyboard(info);

            ToolPane.SelectedEditor.ProcessKeyboard(info);

            return result;
        }

        public void ShowResizeConsolePopup()
        {
            ResizeSurfacePopup popup = new ResizeSurfacePopup(EditingSurfaceWidth, EditingSurfaceHeight);
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

        public void ResizeEditingSurface(int width, int height)
        {
            EditingSurfaceWidth = width;
            EditingSurfaceHeight = height;

            ToolPane.SelectedEditor.Resize(width, height);
        }

        public void LoadSurface()
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    ToolPane.SelectedEditor.Load(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.PreferredExtensions = ToolPane.SelectedEditor.FileExtensions;
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
                    ToolPane.SelectedEditor.Save(popup.SelectedFile);
                }
            };
            popup.CurrentFolder = Environment.CurrentDirectory;
            popup.PreferredExtensions = ToolPane.SelectedEditor.FileExtensions;
            popup.Show(true);
            popup.Center();
        }
    }
}
