namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using Panels;
    using System.Collections.Generic;
    using System.Linq;
    using SadConsole.Surfaces;
    using SadConsole.GameHelpers;

    class HotspotTool : ITool
    {
        public const string ID = "HOTSPOT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Place/Remove Hotspots"; }
        }
        public char Hotkey { get { return 'h'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        public GameObject Brush;
        //private DisplayObjectToolPanel _mouseOverObjectPanel;

		private GameObject _currentGameObject;

        public HotspotTool()
        {
			//_mouseOverObjectPanel = new DisplayObjectToolPanel("Mouse Object");

            //ControlPanels = new CustomPanel[] { _panel, _mouseOverObjectPanel };
            ControlPanels = new CustomPanel[] { };

            Brush = new SadConsole.GameHelpers.GameObject(Settings.Config.ScreenFont);
            Brush.Animation = new AnimatedSurface("default", 1, 1);
            Brush.Animation.CreateFrame();
            Brush.IsVisible = false;
        }

        public override string ToString()
        {
            return Title;
        }

        public void OnSelected()
        {
            RefreshTool();
            EditorConsoleManager.Brush = Brush;
            EditorConsoleManager.UpdateBrush();

            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).ShowDarkLayer = true;
            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).HighlightType = Editors.SceneEditor.HighlightTypes.HotSpot;
        }


        public void OnDeselected()
        {
            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).ShowDarkLayer = false;
        }

        public void RefreshTool()
        {
            SadConsoleEditor.Settings.QuickEditor.BasicSurface = Brush.Animation.CurrentFrame;
            var editor = EditorConsoleManager.ActiveEditor as Editors.SceneEditor;

            if (editor != null)
            {
                if (editor.HotspotPanel.SelectedObject != null)
                {
                    SadConsoleEditor.Settings.QuickEditor.SetCell(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance);
                    SadConsoleEditor.Settings.QuickEditor.SetGlyph(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance.Glyph);
                }
                else
                {
                    //SadConsoleEditor.Settings.QuickEditor.SetGlyph(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance.Glyph, Color.White, Color.Transparent);
                }
            }
        }


        public void Update()
        {
        }

        public bool ProcessKeyboard(Keyboard info, ISurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseConsoleState info, ISurface surface)
        {
        }

        public void MouseEnterSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = true;
            RefreshTool();
        }

        public void MouseExitSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseConsoleState info, ISurface surface)
        {
            Brush.IsVisible = true;
            Brush.Position = info.ConsolePosition;

            if (EditorConsoleManager.ActiveEditor is Editors.SceneEditor)
            {
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;
                var point = new Point(info.ConsolePosition.X, info.ConsolePosition.Y);
                Hotspot mouseSpot = null;

                foreach (var spot in editor.Hotspots)
                {
                    // Spot under our mouse
                    if (spot.Contains(point))
                    {
                        mouseSpot = spot;
                        break;
                    }
                }

                if (info.RightClicked)
                {
                    // Suck up the object
                    if (mouseSpot != null)
                    {
                        editor.HotspotPanel.SelectedObject = mouseSpot;
                        RefreshTool();
                    }
                }

                // Stamp object
                else if (info.Mouse.LeftButtonDown)
                {
                    
                    // SHIFT+CTRL -- Delete any hotspot here
                    if ((Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                     && (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)))
                    {
                        foreach (var spots in editor.Hotspots)
                        {
                            spots.Positions.Remove(info.ConsolePosition);
                        }
                    }

                    // SHIFT -- Delete only the select type
                    else if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                    {
                        if (mouseSpot != null && mouseSpot == editor.HotspotPanel.SelectedObject)
                            mouseSpot.Positions.Remove(info.ConsolePosition);
                    }

                    

                    // Normal -- Place
                    else
                    {
                        if (mouseSpot != null)
                        {
                            // Remove the spot that exists here
                            foreach (var spots in editor.Hotspots)
                            {
                                spots.Positions.Remove(info.ConsolePosition);
                            }
                        }

                        // Place
                        if (editor.HotspotPanel.SelectedObject != null && !editor.HotspotPanel.SelectedObject.Contains(info.ConsolePosition))
                        {
                            editor.HotspotPanel.SelectedObject.Positions.Add(info.ConsolePosition);
                        }
                    }
                }
                
                // Display info about object
                else if (mouseSpot != null)
                {
                    //_mouseOverObjectPanel.DisplayedObject = editor.SelectedGameObjects[point];
                    //EditorConsoleManager.Instance.ToolPane.RefreshControls();
                }
                else
                {
                    //_mouseOverObjectPanel.DisplayedObject = null;
                    //EditorConsoleManager.Instance.ToolPane.RefreshControls();
                }
            }

        }
    }
}
