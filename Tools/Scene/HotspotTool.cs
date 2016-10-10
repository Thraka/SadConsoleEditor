namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Input;
    using Panels;
    using System.Collections.Generic;
    using System.Linq;
    using SadConsole.Consoles;
    using SadConsole.Game;

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

            Brush = new SadConsole.Game.GameObject(Settings.Config.ScreenFont);
            Brush.Animation = new AnimatedTextSurface("default", 1, 1);
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
        }


        public void OnDeselected()
        {
        }

        public void RefreshTool()
        {
            Settings.QuickEditor.TextSurface = Brush.Animation.CurrentFrame;
            var editor = EditorConsoleManager.ActiveEditor as Editors.SceneEditor;

            if (editor != null)
            {
                if (editor.HotspotPanel.SelectedObject != null)
                {
                    Settings.QuickEditor.SetCellAppearance(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance);
                    Settings.QuickEditor.SetGlyph(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance.GlyphIndex);
                }
                else
                {
                    Settings.QuickEditor.SetGlyph(0, 0, editor.HotspotPanel.SelectedObject.DebugAppearance.GlyphIndex, Color.White, Color.Transparent);
                }
            }
        }


        public void Update()
        {
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
            RefreshTool();
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = true;
            Brush.Position = info.ConsoleLocation;

            if (EditorConsoleManager.ActiveEditor is Editors.SceneEditor)
            {
                var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;
                var point = new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y);
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
                else if (info.LeftButtonDown)
                {
                    
                    // SHIFT+CTRL -- Delete any hotspot here
                    if ((Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                     && (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)))
                    {
                        foreach (var spots in editor.Hotspots)
                        {
                            spots.Positions.Remove(info.ConsoleLocation);
                        }
                    }

                    // SHIFT -- Delete only the select type
                    else if (Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Engine.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                    {
                        if (mouseSpot != null && mouseSpot == editor.HotspotPanel.SelectedObject)
                            mouseSpot.Positions.Remove(info.ConsoleLocation);
                    }

                    

                    // Normal -- Place
                    else
                    {
                        if (mouseSpot != null)
                        {
                            // Remove the spot that exists here
                            foreach (var spots in editor.Hotspots)
                            {
                                spots.Positions.Remove(info.ConsoleLocation);
                            }
                        }

                        // Place
                        if (editor.HotspotPanel.SelectedObject != null && !editor.HotspotPanel.SelectedObject.Contains(info.ConsoleLocation))
                        {
                            editor.HotspotPanel.SelectedObject.Positions.Add(info.ConsoleLocation);
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
