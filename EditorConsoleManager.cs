using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

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

        Consoles.BorderRenderer _borderRenderer;
        SadConsoleEditor.Editors.IEditor _oldEditor;

        
        public SadConsole.Entities.Entity Brush { get; set; }
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

            _backingPanel.CellData.Print(0, 0, "Test", Green);
            _backingPanel.CellData.Print(5, 0, "Test", Red);
            _backingPanel.CellData.Print(10, 0, "Test", Blue);
            _backingPanel.CellData.Print(15, 0, "Test", Grey);
            _backingPanel.CellData.Print(20, 0, "Test", Yellow);
            _backingPanel.CellData.Print(25, 0, "Test", Orange);

            this.Add(_backingPanel);
        }

        private void FinishCreating()
        {
            _borderRenderer = new Consoles.BorderRenderer();

            ToolPane = new Consoles.ToolPane();
            ToolPane.Position = new Point(_backingPanel.CellData.Width - ToolPane.CellData.Width, 1);

            this.Add(ToolPane);


            ToolPane.SelectedEditorChanged += (o, e) =>
                {
                    if (_oldEditor != null)
                    {
                        _oldEditor.MouseEnter -= Editor_MouseEnter;
                        _oldEditor.MouseExit -= Editor_MouseExit;
                        _oldEditor.MouseMove -= Editor_MouseMove;
                    }

                    _oldEditor = ToolPane.SelectedEditor;

                    _oldEditor.MouseEnter += Editor_MouseEnter;
                    _oldEditor.MouseExit += Editor_MouseExit;
                    _oldEditor.MouseMove += Editor_MouseMove;

                    UpdateBox();
                };

            ToolPane.FinishCreating();

        }

        private void Editor_MouseEnter(object sender, SadConsole.Input.MouseEventArgs e)
        {
            Brush.IsVisible = true;
        }

        private void Editor_MouseExit(object sender, SadConsole.Input.MouseEventArgs e)
        {
            Brush.IsVisible = false;
        }

        private void Editor_MouseMove(object sender, SadConsole.Input.MouseEventArgs e)
        {
            Brush.Position = e.ConsoleLocation;
        }

        public void UpdateBox()
        {
            if (_borderRenderer != null)
            {
                _borderRenderer.CellData.Resize(ToolPane.SelectedEditor.Width + 2, ToolPane.SelectedEditor.Height+ 2);
            }
            CenterEditor();
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
            EditorConsoleManager.Instance.Brush.PositionOffset = position;
        }

        public override void Update()
        {
            base.Update();

            ToolPane.SelectedEditor.Surface.Update();
            Brush.Update();
        }

        public override void Render()
        {
            // Draw the border for the console around it.
            base.Render();

            ToolPane.SelectedEditor.Surface.Render();

            if (_borderRenderer != null)
                _borderRenderer.Render();

            EditorConsoleManager.Instance.Brush.Render();
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            var result = base.ProcessMouse(info);

            ToolPane.SelectedEditor.ProcessMouse(info);

            return result;
        }
    }
}
