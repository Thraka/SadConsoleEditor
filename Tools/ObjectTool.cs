namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using Panels;
    using System.Collections.Generic;
    using System.Linq;
    using SadConsole.GameHelpers;
    using SadConsole.Consoles;
    class ObjectTool : ITool
    {
        public const string ID = "OBJECT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Object"; }
        }
        public char Hotkey { get { return 'o'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        private EntityBrush _brush;
        private Panels.ObjectToolPanel _panel;
		private DisplayObjectToolPanel _mouseOverObjectPanel;

		private GameObject _currentGameObject;

        public ObjectTool()
        {
            _panel = new Panels.ObjectToolPanel();
			_mouseOverObjectPanel = new DisplayObjectToolPanel("Mouse Object");

			ControlPanels = new CustomPanel[] { _panel, _mouseOverObjectPanel };

            _brush = new EntityBrush();
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            if (_panel.SelectedObject != null)
            {
                _currentGameObject = _panel.SelectedObject;

                _brush.CurrentAnimation.Frames[0].Fill(_currentGameObject.Character.Foreground,
                                   _currentGameObject.Character.Background,
                                   _currentGameObject.Character.CharacterIndex, null,
                                   _currentGameObject.Character.SpriteEffect);
            }
            else
            {
                _currentGameObject = null;
                _brush.CurrentAnimation.Frames[0].Fill(Color.White,
                                   Color.Transparent,
                                   0, null);
            }

            _brush.IsVisible = false;
            if (EditorConsoleManager.Instance.SelectedEditor is Editors.GameScreenEditor)
            {
                var editor = (Editors.GameScreenEditor)EditorConsoleManager.Instance.SelectedEditor;
                editor.DisplayObjectLayer = true;
            }
        }


        public void OnDeselected()
        {
            if (EditorConsoleManager.Instance.SelectedEditor is Editors.GameScreenEditor)
            {
                var editor = (Editors.GameScreenEditor)EditorConsoleManager.Instance.SelectedEditor;
                editor.DisplayObjectLayer = false;
            }
        }

        public void RefreshTool()
        {
            if (_panel.SelectedObject != null)
            {
                _currentGameObject = _panel.SelectedObject;

                _brush.CurrentAnimation.Frames[0].Fill(_currentGameObject.Character.Foreground,
                                   _currentGameObject.Character.Background,
                                   _currentGameObject.Character.CharacterIndex, null,
                                   _currentGameObject.Character.SpriteEffect);
            }
            else
            {
                _currentGameObject = null;
                _brush.CurrentAnimation.Frames[0].Fill(Color.White,
                                   Color.Transparent,
                                   0, null);
            }
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
            _brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = true;
            _brush.Position = info.ConsoleLocation;

            if (EditorConsoleManager.Instance.SelectedEditor is Editors.GameScreenEditor)
            {
                var editor = (Editors.GameScreenEditor)EditorConsoleManager.Instance.SelectedEditor;
                var point = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
				bool isObjectUnderPoint = editor.SelectedGameObjects.ContainsKey(point);

				if (info.LeftClicked)
				{
					// Suck up the object
					if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
					{
						if (isObjectUnderPoint)
						{
							_currentGameObject = editor.SelectedGameObjects[point].Clone();

							_brush.CurrentAnimation.Frames[0].Fill(_currentGameObject.Character.Foreground,
								   _currentGameObject.Character.Background,
								   _currentGameObject.Character.CharacterIndex, null,
								   _currentGameObject.Character.SpriteEffect);
						}
					}

					else if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
					{
						if (isObjectUnderPoint)
						{
							_currentGameObject = editor.SelectedGameObjects[point].Clone();
							_panel.AddNewGameObject(_currentGameObject);
						}
					}

					// Delete the object
					else if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
					{
						if (isObjectUnderPoint)
							editor.SelectedGameObjects.Remove(point);

						editor.SyncObjectsToLayer();
					}
				}

				// Stamp object
				else if (info.LeftButtonDown)
				{
					// Delete the object
					if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
					{
						if (editor.SelectedGameObjects.ContainsKey(point))
							editor.SelectedGameObjects.Remove(point);

						editor.SyncObjectsToLayer();
					}

					else if (_currentGameObject != null &&
						!Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) &&
						!Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
					{
						var cell = surface[info.ConsoleLocation.X, info.ConsoleLocation.Y];

						if (editor.SelectedGameObjects.ContainsKey(point))
							editor.SelectedGameObjects.Remove(point);

						var gameObj = _currentGameObject.Clone();
						gameObj.Position = point;

						editor.SelectedGameObjects.Add(point, gameObj);
						editor.SyncObjectsToLayer();
					}
				}

				// Edit object
				else if (info.RightClicked)
				{
					if (isObjectUnderPoint)
					{
						Windows.EditObjectPopup popup = new Windows.EditObjectPopup(editor.SelectedGameObjects[point]);
						popup.Closed += (o, e) => { if (popup.DialogResult) editor.SyncObjectsToLayer(); };
						popup.Show(true);
					}
				}

				// Display info about object
				else if (isObjectUnderPoint)
				{
					_mouseOverObjectPanel.DisplayedObject = editor.SelectedGameObjects[point];
					EditorConsoleManager.Instance.ToolPane.RefreshControls();
                }
				else
				{
					_mouseOverObjectPanel.DisplayedObject = null;
					EditorConsoleManager.Instance.ToolPane.RefreshControls();
				}
			}
        }
    }
}
