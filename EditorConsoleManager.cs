using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using SadConsole.Input;

namespace SadConsoleEditor
{
    class CustomConsoleList : ConsoleList
    {
        public override void Render()
        {
            if (IsVisible)
            {
                var copyList = new List<IConsole>(_consoles);

                for (int i = 0; i < copyList.Count; i++)
                {
                    copyList[i].Render();

                    if (EditorConsoleManager.ActiveEditor != null && copyList[i] == EditorConsoleManager.ActiveEditor.RenderedConsole)
                    {
                        EditorConsoleManager.ActiveEditor.Render();

                        if (EditorConsoleManager.Brush != null && EditorConsoleManager.Brush.IsVisible)
                            EditorConsoleManager.Brush.Render();
                    }
                }
            }
        }

        public override void Update()
        {
            if (DoUpdate)
            {
                var copyList = new List<IConsole>(_consoles);

                for (int i = copyList.Count - 1; i >= 0; i--)
                {
                    copyList[i].Update();

                    if (EditorConsoleManager.ActiveEditor != null && copyList[i] == EditorConsoleManager.ActiveEditor.RenderedConsole)
                    {
                        EditorConsoleManager.ActiveEditor.Update();

                        if (EditorConsoleManager.Brush != null && EditorConsoleManager.Brush.IsVisible)
                            EditorConsoleManager.Brush.Update();
                    }
                }
            }
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            return EditorConsoleManager.ProcessKeyboard(info);
        }

        public override bool ProcessMouse(MouseInfo info)
        {
            return base.ProcessMouse(info);
            //return EditorConsoleManager.ProcessMouse(info);
        }
    }


    static class EditorConsoleManager
    {
        private static CustomConsoleList Consoles;
        public static Consoles.QuickSelectPane QuickSelectPane;
        private static Console topBarPane;
        private static Consoles.BorderConsole borderConsole;
        private static ControlsConsole scrollerContainer;

        public static Dictionary<Type, FileLoaders.IFileLoader[]> EditorFileTypes;
        public static Dictionary<string, Editors.Editors> Editors;

        public static List<Editors.IEditor> OpenEditors;
        private static string topBarLayerName = "None";
        private static string topBarToolName = "None";
        private static Point topBarMousePosition;

        public static SadConsole.Game.GameObject Brush;
        public static bool AllowKeyboardToMoveConsole;

        public static Editors.IEditor ActiveEditor { get; private set; }

        public static Consoles.ToolPane ToolsPane { get; private set; }

        public static SadConsole.Controls.ScrollBar ToolsPaneScroller { get; private set; }

        public static string LayerName
        {
            set
            {
                topBarLayerName = value;
                RefreshBackingPanel();
            }
        }

        public static string ToolName
        {
            set
            {
                topBarToolName = value;
                RefreshBackingPanel();
            }
        }

        public static Point SurfaceMouseLocation
        {
            set
            {
                topBarMousePosition = value;
                RefreshBackingPanel();
            }
        }

        public static void Initialize()
        {
            Consoles = new CustomConsoleList();

            // Hook the update event that happens each frame so we can trap keys and respond.
            SadConsole.Engine.ConsoleRenderStack = Consoles;
            SadConsole.Engine.ActiveConsole = Consoles;

            // Create the basic consoles
            QuickSelectPane = new SadConsoleEditor.Consoles.QuickSelectPane();
            QuickSelectPane.Redraw();
            QuickSelectPane.IsVisible = false;

            topBarPane = new SadConsole.Consoles.Console(Settings.Config.WindowWidth, 1);
            topBarPane.TextSurface.DefaultBackground = Settings.Color_MenuBack;
            topBarPane.Clear();
            topBarPane.MouseCanFocus = false;
            topBarPane.IsVisible = false;

            borderConsole = new SadConsoleEditor.Consoles.BorderConsole(10, 10);
            borderConsole.IsVisible = false;
            borderConsole.CanUseMouse = false;

            ToolsPane = new Consoles.ToolPane();
            ToolsPane.Position = new Point(Settings.Config.WindowWidth - ToolsPane.Width - 1, 1);
            ToolsPane.IsVisible = false;

            // Scroll bar for toolpane
            // Create scrollbar
            ToolsPaneScroller = SadConsole.Controls.ScrollBar.Create(System.Windows.Controls.Orientation.Vertical, Settings.Config.WindowHeight - 1);
            ToolsPaneScroller.Maximum = ToolsPane.TextSurface.Height - Settings.Config.WindowHeight;
            ToolsPaneScroller.ValueChanged += (o, e) =>
            {
                ToolsPane.TextSurface.RenderArea = new Rectangle(0, ToolsPaneScroller.Value, ToolsPane.Width, Settings.Config.WindowHeight);
            };
            scrollerContainer = new ControlsConsole(1, ToolsPaneScroller.Height);
            scrollerContainer.Add(ToolsPaneScroller);
            scrollerContainer.Position = new Point(Settings.Config.WindowWidth - 1, 1);
            scrollerContainer.IsVisible = false;
            scrollerContainer.MouseCanFocus = false;
            scrollerContainer.ProcessMouseWithoutFocus = true;

            // Add the consoles to the main console list
            Consoles.Add(QuickSelectPane);
            Consoles.Add(topBarPane);
            Consoles.Add(ToolsPane);
            Consoles.Add(scrollerContainer);

            // Setup the file types for base editors.
            EditorFileTypes = new Dictionary<Type, FileLoaders.IFileLoader[]>(3);
            OpenEditors = new List<SadConsoleEditor.Editors.IEditor>();
            //EditorFileTypes.Add(typeof(Editors.DrawingEditor), new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface() });

            // Add valid editors
            Editors = new Dictionary<string, SadConsoleEditor.Editors.Editors>();
            Editors.Add("Console Draw", SadConsoleEditor.Editors.Editors.Console);
            Editors.Add("Animated Game Object", SadConsoleEditor.Editors.Editors.GameObject);
            Editors.Add("Game Scene", SadConsoleEditor.Editors.Editors.Scene);
            Editors.Add("User Interface Console", SadConsoleEditor.Editors.Editors.GUI);

            // Show new window
            ShowStartup();
        }

        private static void ShowStartup()
        {
            Window.Prompt("Create new or open existing?", "New", "Open",
            (b) =>
            {
                if (b)
                {
                    Windows.NewConsolePopup popup = new Windows.NewConsolePopup();
                    popup.Center();
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight, popup.SettingForeground, popup.SettingBackground); };
                    popup.Show(true);
                }
                else
                {
                    Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
                    popup.Center();
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else LoadEditor(popup.SelectedFile, popup.SelectedLoader); };
                    popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredTextSurface(), new FileLoaders.TextSurface(), new FileLoaders.Scene(), new FileLoaders.GameObject() };
                    popup.Show(true);
                }

            });
        }


        private static void CreateNewEditor(Editors.Editors editorType, int width, int height, Color defaultForeground, Color defaultBackground)
        {
            Editors.IEditor editor = null;

            switch (editorType)
            {
                case SadConsoleEditor.Editors.Editors.Console:
                    editor = new Editors.LayeredConsoleEditor();
                    editor.New(defaultForeground, defaultBackground, width, height);
                    break;
                case SadConsoleEditor.Editors.Editors.GameObject:
                    editor = new Editors.GameObjectEditor();
                    editor.New(defaultForeground, defaultBackground, width, height);
                    break;
                case SadConsoleEditor.Editors.Editors.Scene:
                    editor = new Editors.SceneEditor();
                    editor.New(defaultForeground, defaultBackground, width, height);
                    break;
                case SadConsoleEditor.Editors.Editors.GUI:
                    break;
                default:
                    break;
            }

            if (editor != null)
            {
                AddEditor(editor, true);
            }

            topBarPane.IsVisible = true;
            ToolsPane.IsVisible = true;
            scrollerContainer.IsVisible = true;
        }

        private static void LoadEditor(string file, FileLoaders.IFileLoader loader)
        {
            Editors.IEditor editor = null;

            if (loader is FileLoaders.LayeredTextSurface || loader is FileLoaders.TextSurface)
            {
                editor = new Editors.LayeredConsoleEditor();
                AddEditor(editor, false);
                editor.Load(file, loader);
            }
            else if (loader is FileLoaders.GameObject)
            {
                editor = new Editors.GameObjectEditor();
                AddEditor(editor, false);
                editor.Load(file, loader);
            }
            else if (loader is FileLoaders.Scene)
            {
                editor = new Editors.SceneEditor();
                AddEditor(editor, false);
                editor.Load(file, loader);
            }
            if (editor != null)
                ChangeActiveEditor(editor);

            topBarPane.IsVisible = true;
            ToolsPane.IsVisible = true;
            scrollerContainer.IsVisible = true;
        }

        public static void ShowCloseConsolePopup()
        {
            Window.Prompt(new SadConsole.ColoredString("Are you sure? You will lose any unsaved changes."), "Yes", "No", (r) =>
            {
                if (r)
                    RemoveEditor(ActiveEditor);
            });
        }

        public static void ShowNewEditorPopup()
        {
            Windows.NewConsolePopup popup = new Windows.NewConsolePopup();
            popup.Center();
            popup.Closed += (s, e) => { if (popup.DialogResult) CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight, popup.SettingForeground, popup.SettingBackground); };
            popup.Show(true);
        }

        public static void ShowLoadEditorPopup()
        {
            Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.Closed += (s, e) => { if (popup.DialogResult) LoadEditor(popup.SelectedFile, popup.SelectedLoader); };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredTextSurface(), new FileLoaders.Scene(), new FileLoaders.GameObject() };
            popup.Show(true);
        }

        public static void AddEditor(Editors.IEditor editor, bool show)
        {
            OpenEditors.Add(editor);
            ToolsPane.PanelFiles.DocumentsListbox.Items.Add(editor);

            if (show)
                ChangeActiveEditor(editor);
        }

        public static void RemoveEditor(Editors.IEditor editor)
        {
            ToolsPane.PanelFiles.DocumentsListbox.Items.Remove(editor);
            OpenEditors.Remove(editor);
            editor.OnClosed();

            if (OpenEditors.Count == 0)
                ShowStartup();
            else
                ChangeActiveEditor(OpenEditors[0]);
        }

        public static void ChangeActiveEditor(Editors.IEditor editor)
        {
            AllowKeyboardToMoveConsole = true;

            if (ActiveEditor != null)
            {
                ActiveEditor.OnDeselected();
                Consoles.Remove(ActiveEditor.RenderedConsole);
            }

            if (OpenEditors.Contains(editor))
            {
                ActiveEditor = editor;
                CenterEditor();
                ToolsPane.RedrawPanels();
                ActiveEditor.OnSelected();

                Consoles.Insert(0, ActiveEditor.RenderedConsole);
                UpdateBorder(editor.Position);

                if (ToolsPane.PanelFiles.DocumentsListbox.SelectedItem != editor)
                    ToolsPane.PanelFiles.DocumentsListbox.SelectedItem = editor;
            }
        }

        public static void SaveEditor()
        {
            if (ActiveEditor != null)
                ActiveEditor.Save();
        }

        public static void UpdateBorder(Point position)
        {
            if (borderConsole.Width != ActiveEditor.Width + 2 || borderConsole.Height != ActiveEditor.Height + 2)
            {
                Consoles.Remove(borderConsole);
                borderConsole = new Consoles.BorderConsole(ActiveEditor.Width + 2, ActiveEditor.Height + 2);
            }

            if (!Consoles.Contains(borderConsole) && Consoles.Contains(ActiveEditor.RenderedConsole))
                Consoles.Insert(Consoles.IndexOf(ActiveEditor.RenderedConsole), borderConsole);

            borderConsole.Position = position - new Point(1, 1);
            borderConsole.IsVisible = true;
        }

        public static void CenterEditor()
        {
            Point position = new Point();

            var screenSize = SadConsole.Engine.GetScreenSizeInCells(Settings.Config.ScreenFont);

            if (ActiveEditor.Width < screenSize.X)
                position.X = ((screenSize.X - 20) / 2) - (ActiveEditor.Width / 2);
            else
                position.X = ((screenSize.X - 20) - ActiveEditor.Width) / 2;

            if (ActiveEditor.Height < screenSize.Y)
                position.Y = (screenSize.Y / 2) - (ActiveEditor.Height / 2);
            else
                position.Y = (screenSize.Y - ActiveEditor.Height) / 2;

            ActiveEditor.Move(position.X, position.Y);
        }

        private static void RefreshBackingPanel()
        {
            topBarPane.Clear();

            var text = new SadConsole.ColoredString("   X: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarMousePosition.X.ToString(), Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString(" Y: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarMousePosition.Y.ToString(), Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString("   Layer: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarLayerName, Settings.Appearance_TextValue) +
                       new SadConsole.ColoredString("   Tool: ", Settings.Appearance_Text) + new SadConsole.ColoredString(topBarToolName, Settings.Appearance_TextValue);

            topBarPane.Print(0, 0, text);
        }

        public static void UpdateBrush()
        {
            if (Brush != null)
            {
                Brush.RenderOffset = ActiveEditor.Position;
            }
        }

        public static bool ProcessKeyboard(KeyboardInfo info)
        {
            //var result = base.ProcessKeyboard(info);
            if (AllowKeyboardToMoveConsole)
            {
                bool shifted = info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

                var position = new Point(borderConsole.Position.X + 1, borderConsole.Position.Y + 1);
                bool movekeyPressed = false;
                if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    if (borderConsole.Position.X + borderConsole.Width - 1 != 0)
                    {
                        position.X -= 1;
                        movekeyPressed = true;
                    }
                }
                else if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    var max = ToolsPane.Position.TranslateFont(ToolsPane.TextSurface.Font, borderConsole.TextSurface.Font);
                    
                    
                    if (borderConsole.Position.X != max.X - 1)
                    {
                        position.X += 1;
                        movekeyPressed = true;
                    }
                }

                if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    if (borderConsole.Position.Y + borderConsole.Height - 2 != 0)
                    {
                        position.Y -= 1;
                        movekeyPressed = true;
                    }

                }
                else if (!shifted && info.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    if (QuickSelectPane.IsVisible)
                    {
                        if (borderConsole.Position.Y * borderConsole.TextSurface.Font.Size.Y + borderConsole.TextSurface.Font.Size.Y < QuickSelectPane.Position.Y)
                        {
                            position.Y += 1;
                            movekeyPressed = true;
                        }
                    }
                    else if (borderConsole.Position.Y != Settings.Config.WindowHeightAsScreenFont - 1)
                    {
                        position.Y += 1;
                        movekeyPressed = true;
                    }
                }

                if (movekeyPressed)
                {
                    ActiveEditor.Move(position.X, position.Y);
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
                        if (ToolsPane.ProcessKeyboard(info))
                        {
                            return true;
                        }
                        // Look for quick select F* keys
                        else if (QuickSelectPane.ProcessKeyboard(info))
                        {
                            return true;
                        }
                        else if (ActiveEditor != null)
                        {
                            return ActiveEditor.ProcessKeyboard(info);
                        }
                    }
                }

            }
            
            return false;
        }

        public static bool ProcessMouse(MouseInfo info)
        {
            //if (ActiveEditor != null && info.Console != ToolsPane && info.Console != ToolsPaneScroller)
            //{
            //    ActiveEditor.RenderedConsole.process
            //}
            return false;
        }
    }
}