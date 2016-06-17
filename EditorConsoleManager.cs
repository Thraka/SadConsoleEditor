using SadConsole.Consoles;
using System;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Windows;
using SadConsole.Input;
using System.Collections.Generic;
using SadConsoleEditor.Editors;
using System.Linq;

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
        private SelectFilePopup _fileDialogPopup;
        public Dictionary<string, Editors.IEditor> Editors;
        public IEditor SelectedEditor { get; private set; }
        private Action<object, EventArgs> _popupCallback;
        private SadConsole.Controls.ScrollBar _toolsPaneScroller;
        private Dictionary<SadConsole.Controls.RadioButton, IEditor> _documentButtons = new Dictionary<SadConsole.Controls.RadioButton, IEditor>();
        private Dictionary<IEditor, string> _editorsLastSelectedTool = new Dictionary<IEditor, string>();


        public int EditingSurfaceWidth { get { return SelectedEditor.Width; } }
        public int EditingSurfaceHeight { get { return SelectedEditor.Height; } }


        public IEntityBrush Brush { get; private set; }
        public Consoles.ToolPane ToolPane { get; private set; }

		public Consoles.QuickSelectPane QuickSelectPane { get; private set; }

		public bool AllowKeyboardToMoveConsole { get; set; }

        private Point topBarMousePosition;
        private string topBarLayerName = "None";
        private string topBarToolName = "None";

        public Point SurfaceMouseLocation
        {
            set
            {
                topBarMousePosition = value;
                RefreshBackingPanel();
            }
        }

        public string LayerName
        {
            set
            {
                topBarLayerName = value;
                RefreshBackingPanel();
            }
        }

        public string ToolName
        {
            set
            {
                topBarToolName = value;
                RefreshBackingPanel();
            }
        }

        private ControlsConsole _backingPanel;

        private void RefreshBackingPanel()
        {
            _backingPanel.Clear();

            var text = new SadConsole.ColoredString("   X: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarMousePosition.X.ToString(), Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString(" Y: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarMousePosition.Y.ToString(), Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString("   Layer: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarLayerName, Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString("   Tool: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarToolName, Settings.Appearance_TextValue);

            _backingPanel.Print(0, 0, text);
        }

        private EditorConsoleManager()
        {
            _backingPanel = new ControlsConsole(Settings.Config.WindowWidth, 2);

            _backingPanel.TextSurface.DefaultBackground = Settings.Color_MenuBack;
            _backingPanel.Clear();
            _backingPanel.ProcessMouseWithoutFocus = true;

            _backingPanel.IsVisible = true;
            SurfaceMouseLocation = new Point(0, 0);
            Color Green = new Color(165, 224, 45);
            Color Red = new Color(246, 38, 108);
            Color Blue = new Color(100, 217, 234);
            Color Grey = new Color(117, 111, 81);
            Color Yellow = new Color(226, 218, 110);
            Color Orange = new Color(251, 149, 31);

            //_backingPanel.TextSurface.Print(0, 0, "Test", Green);
            //_backingPanel.TextSurface.Print(5, 0, "Test", Red);
            //_backingPanel.TextSurface.Print(10, 0, "Test", Blue);
            //_backingPanel.TextSurface.Print(15, 0, "Test", Grey);
            //_backingPanel.TextSurface.Print(20, 0, "Test", Yellow);
            //_backingPanel.TextSurface.Print(25, 0, "Test", Orange);

            this.Add(_backingPanel);

            _fileDialogPopup = new SelectFilePopup();
            _fileDialogPopup.Closed += (o, e) =>
            {
                if (_popupCallback != null)
                    _popupCallback(o, e);
            };
            _fileDialogPopup.CurrentFolder = Environment.CurrentDirectory;
        }

        private void FinishCreating()
        {
            ToolPane = new Consoles.ToolPane();
            ToolPane.Position = new Point(_backingPanel.TextSurface.Width - ToolPane.TextSurface.Width - 1, 2);
            //ToolPane.TextSurface.Resize(ToolPane.TextSurface.Width, ToolPane.TextSurface.Height * 2);
            ToolPane.TextSurface.RenderArea = new Rectangle(0, 0, ToolPane.TextSurface.Width, Settings.Config.WindowHeight - 2);
            this.Add(ToolPane);

            _toolsPaneScroller = SadConsole.Controls.ScrollBar.Create(System.Windows.Controls.Orientation.Vertical, Settings.Config.WindowHeight - 1);
            _toolsPaneScroller.Maximum = ToolPane.TextSurface.Height - Settings.Config.WindowHeight;
            _toolsPaneScroller.ValueChanged += (o, e) =>
                {
                    ToolPane.TextSurface.RenderArea = new Rectangle(0, _toolsPaneScroller.Value, ToolPane.TextSurface.Width, Settings.Config.WindowHeight);
                };
            var scrollerContainer = new ControlsConsole(1, _toolsPaneScroller.Height);
            scrollerContainer.Add(_toolsPaneScroller);
            scrollerContainer.Position = new Point(_backingPanel.TextSurface.Width - 1, 1);
            scrollerContainer.IsVisible = true;
            scrollerContainer.MouseCanFocus = false;
            scrollerContainer.ProcessMouseWithoutFocus = true;
            this.Add(scrollerContainer);

			QuickSelectPane = new Consoles.QuickSelectPane();
			QuickSelectPane.Position = new Point(0, Settings.Config.WindowHeight - QuickSelectPane.TextSurface.Height);
			QuickSelectPane.Redraw();
			QuickSelectPane.IsVisible = true;
			this.Add(QuickSelectPane);

			ToolPane.FinishCreating();

            Editors = new Dictionary<string, SadConsoleEditor.Editors.IEditor>();
            
            ChangeEditor(new DrawingEditor());
            //_borderRenderer = new Consoles.BorderRenderer(1, 1);

            Editors.Add(DrawingEditor.ID, new DrawingEditor());
            //Editors.Add(GameScreenEditor.ID, new GameScreenEditor());
            //Editors.Add(EntityEditor.ID, new EntityEditor());
            //Editors.Add(SceneEditor.ID, new SceneEditor());

        }

        public void ChangeEditor(IEditor editor)
        {
            if (SelectedEditor != null)
            {
                if (_editorsLastSelectedTool.ContainsKey(SelectedEditor))
                    _editorsLastSelectedTool.Remove(SelectedEditor);

                _editorsLastSelectedTool.Add(SelectedEditor, ToolPane.SelectedTool.Id);
            }

            SelectedEditor = editor;
            ToolPane.SetupEditor();
            UpdateBox();

            if (_editorsLastSelectedTool.ContainsKey(SelectedEditor))
            {
                string id = _editorsLastSelectedTool[SelectedEditor];

                foreach (var tool in ToolPane.ToolsPanel.ToolsListBox.Items.Cast<Tools.ITool>())
                {
                    if (tool.Id == id)
                    {
                        ToolPane.SelectedTool = tool;
                        break;
                    }
                }
            }
        }

        public void ScrollToolbox(int scrollValueChanged)
        {
            _toolsPaneScroller.Value += scrollValueChanged / 20;
        }

        public void UpdateBrush(IEntityBrush newBrushEntity)
        {
            Brush = newBrushEntity;
            Brush.PositionOffset = SelectedEditor.GetPosition();
        }

        public void UpdateBox()
        {
            //if (_borderRenderer != null)
                _borderRenderer = new Consoles.BorderRenderer(EditingSurfaceWidth + 2, EditingSurfaceHeight + 2);

            CenterEditor();
        }

        private void CenterEditor()
        {
            // Was in the middle of moving EditingContainer to this. Got to finish this. This should happen anytime the editing surface resizes.

            Point position = new Point();

            var screenSize = SadConsole.Engine.GetScreenSizeInCells(Settings.Config.ScreenFont);

            if (EditingSurfaceWidth < screenSize.X)
                position.X = ((screenSize.X - 20) / 2) - (EditingSurfaceWidth / 2);
            else
                position.X = ((screenSize.X - 20) - EditingSurfaceWidth) / 2;

            if (EditingSurfaceHeight < screenSize.Y)
                position.Y = (screenSize.Y / 2) - (EditingSurfaceHeight / 2);
            else
                position.Y = (screenSize.Y - EditingSurfaceHeight) / 2;

            SelectedEditor.Position(position.X, position.Y);
            _borderRenderer.Position = new Point(position.X - 1, position.Y - 1);
            Brush.PositionOffset = position;
        }

        public override void Update()
        {
            base.Update();

            SelectedEditor.Surface.Update();
            Brush.Update();

            //ProcessKeyboard(SadConsole.Engine.Keyboard);
        }

        public override void Render()
        {
            
            SelectedEditor.Render();

            if (_borderRenderer != null)
                _borderRenderer.Render();

            Brush.Render();

            // Draw the border for the console around it.
            base.Render();
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            var result = base.ProcessMouse(info);

            

            if (!ToolPane.IsMouseOver && !_backingPanel.IsMouseOver)
                SelectedEditor.ProcessMouse(info);
            

            return result;
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            //var result = base.ProcessKeyboard(info);
            if (AllowKeyboardToMoveConsole)
            {
				bool shifted = info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

				var position = new Point(_borderRenderer.Position.X + 1, _borderRenderer.Position.Y + 1);
                bool movekeyPressed = false;
                if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    if (_borderRenderer.Position.X + _borderRenderer.TextSurface.Width - 1 != 0)
                    {
                        position.X -= 1;
                        movekeyPressed = true;
                    }
                }
                else if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    if (_borderRenderer.Position.X != ToolPane.Position.X - 1)
                    {
                        position.X += 1;
                        movekeyPressed = true;
                    }
                }

                if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    if (_borderRenderer.Position.Y + _borderRenderer.TextSurface.Height - 2 != 0)
                    {
                        position.Y -= 1;
                        movekeyPressed = true;
                    }

                }
                else if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    if (_borderRenderer.Position.Y != Settings.Config.WindowHeight - 1)
                    {
                        position.Y += 1;
                        movekeyPressed = true;
                    }
                }
                
                if (movekeyPressed)
                {
                    SelectedEditor.Position(position.X, position.Y);
                    _borderRenderer.Position = new Microsoft.Xna.Framework.Point(position.X - 1, position.Y - 1);
                    Brush.PositionOffset = position;
                }
                else
                {
					//if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Subtract))
					//{
					//	SelectedEditor.Surface.ResizeCells(SelectedEditor.Surface.CellSize.X / 2, SelectedEditor.Surface.CellSize.Y / 2);
					//}
					//else if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Add))
					//{
					//	SelectedEditor.Surface.ResizeCells(SelectedEditor.Surface.CellSize.X * 2, SelectedEditor.Surface.CellSize.Y * 2);
					//}
					//else
					{
						// Look for tool hotkeys
						ToolPane.ProcessKeyboard(info);

						// Look for quick select F* keys
						QuickSelectPane.ProcessKeyboard(info);
					}
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

        public void ShowCloseConsolePopup()
        {
            Window.Prompt(new SadConsole.ColoredString("Are you sure? You will lose any unsaved changes."), "Yes", "No", (r) =>
            {
                if (r)
                    CloseDocument(SelectedEditor);
            });
        }

        public void ShowNewConsolePopup(bool allowCancel)
        {
            var popup = new NewConsolePopup();
            popup.AllowCancel = allowCancel;
            popup.Closed += (o, e) =>
            {
                if (popup.DialogResult)
                {
                    IEditor editor;

                    //if (popup.Editor.Id == EntityEditor.ID)
                    //    editor = new EntityEditor();
                    //else if (popup.Editor.Id == DrawingEditor.ID)
                    //    editor = new DrawingEditor();
                    //else if (popup.Editor.Id == SceneEditor.ID)
                    //    editor = new SceneEditor();
                    //else
                    //    editor = new GameScreenEditor();

                    editor = new DrawingEditor();


                    editor.Resize(popup.SettingWidth, popup.SettingHeight);
                    editor.Surface.Fill(popup.SettingForeground, popup.SettingBackground, 0, null);
                    AddDocument(editor);
                }
            };

            popup.Show(true);
            popup.Center();
        }

        public void ResizeEditingSurface(int width, int height)
        {
            SelectedEditor.Resize(width, height);
        }

        private void LoadSurfaceAction(object sender, EventArgs e)
        {
            if (_fileDialogPopup.DialogResult)
            {
                SelectedEditor.Load(_fileDialogPopup.SelectedFile);
                //ToolPane.LayersPanel.RebuildListBox();
            }
        }

        private void SaveSurfaceAction(object sender, EventArgs e)
        {
            if (_fileDialogPopup.DialogResult)
            {
                SelectedEditor.Save(_fileDialogPopup.SelectedFile);
            }
        }

        public void LoadSurface()
        {
            _popupCallback = LoadSurfaceAction;
            _fileDialogPopup.FileFilter = SelectedEditor.FileExtensionsLoad;
            _fileDialogPopup.SelectButtonText = "Open";
            _fileDialogPopup.Show(true);
            _fileDialogPopup.Center();
        }

        public void SaveSurface()
        {
            _popupCallback = SaveSurfaceAction;
            _fileDialogPopup.SelectButtonText = "Save";
            _fileDialogPopup.FileFilter = SelectedEditor.FileExtensionsSave;
            _fileDialogPopup.SkipFileExistCheck = true;
            _fileDialogPopup.Show(true);
            _fileDialogPopup.Center();
        }


        public void AddDocument(IEditor editor)
        {
            var button = new SadConsole.Controls.RadioButton(editor.ShortName.Length + 6, 1) { Text = editor.ShortName };
            button.IsSelectedChanged += DocumentButtonClick;
            _documentButtons.Add(button, editor);
            _backingPanel.Add(button);
            ArrangeDocumentButtons();
            button.IsSelected = true;
        }

        public void CloseDocument(IEditor editor)
        {
            SadConsole.Controls.RadioButton button = null;
            
            foreach (var item in _documentButtons.Keys)
            {
                if (_documentButtons[item] == editor)
                {
                    button = item;
                    break;
                }
            }

            if (button != null)
            {
                _backingPanel.Remove(button);
                _documentButtons.Remove(button);

                if (_documentButtons.Count == 0)
                    ShowNewConsolePopup(false);
                else
                {
                    ArrangeDocumentButtons();

                    foreach (var item in _documentButtons.Keys)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void DocumentButtonClick(object sender, EventArgs e)
        {
            var button = (SadConsole.Controls.RadioButton)sender;
            if (button.IsSelected)
                ChangeEditor(_documentButtons[button]);
        }

        private void ArrangeDocumentButtons()
        {
            int positionx = 0;
            foreach (var item in _documentButtons.Keys)
            {
                item.Position = new Point(positionx, 1);
                positionx += item.Width + 2;
            }

        }
    }
}
