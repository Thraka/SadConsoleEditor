using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SadConsoleEditor
{
    static class EditorConsoleManager
    {
        private static ConsoleList Consoles;
        private static Consoles.QuickSelectPane quickSelectPane;
        private static Console topBarPane;
        private static Consoles.BorderConsole borderConsole;

        public static Dictionary<Type, FileLoaders.IFileLoader[]> EditorFileTypes;
        public static Dictionary<string, Editors.Editors> Editors;

        public static List<Editors.IEditor> OpenEditors;
        private static string topBarLayerName = "None";
        private static string topBarToolName = "None";
        private static Point topBarMousePosition;

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
            Consoles = new ConsoleList();

            // Hook the update event that happens each frame so we can trap keys and respond.
            SadConsole.Engine.ConsoleRenderStack = Consoles;
            SadConsole.Engine.ActiveConsole = Consoles;
            SadConsole.Engine.EngineUpdated += Engine_EngineUpdated;
            SadConsole.Engine.EngineDrawFrame += Engine_EngineDrawFrame;

            // Create the basic consoles
            quickSelectPane = new SadConsoleEditor.Consoles.QuickSelectPane();
            quickSelectPane.Position = new Point(0, Settings.Config.WindowHeight - quickSelectPane.TextSurface.Height);
            quickSelectPane.Redraw();
            quickSelectPane.IsVisible = true;

            topBarPane = new SadConsole.Consoles.Console(Settings.Config.WindowWidth, 1);
            topBarPane.TextSurface.DefaultBackground = Settings.Color_MenuBack;
            topBarPane.Clear();
            topBarPane.MouseCanFocus = false;

            borderConsole = new SadConsoleEditor.Consoles.BorderConsole(10, 10);
            borderConsole.IsVisible = false;

            ToolsPane = new Consoles.ToolPane();
            ToolsPane.Position = new Point(Settings.Config.WindowWidth - ToolsPane.Width - 1, 1);

            // Scroll bar for toolpane
            // Create scrollbar
            ToolsPaneScroller = SadConsole.Controls.ScrollBar.Create(System.Windows.Controls.Orientation.Vertical, Settings.Config.WindowHeight - 1);
            ToolsPaneScroller.Maximum = ToolsPane.TextSurface.Height - Settings.Config.WindowHeight;
            ToolsPaneScroller.ValueChanged += (o, e) =>
            {
                ToolsPane.TextSurface.RenderArea = new Rectangle(0, ToolsPaneScroller.Value, ToolsPane.Width, Settings.Config.WindowHeight);
            };
            var scrollerContainer = new ControlsConsole(1, ToolsPaneScroller.Height);
            scrollerContainer.Add(ToolsPaneScroller);
            scrollerContainer.Position = new Point(Settings.Config.WindowWidth - 1, 1);
            scrollerContainer.IsVisible = true;
            scrollerContainer.MouseCanFocus = false;
            scrollerContainer.ProcessMouseWithoutFocus = true;

            // Add the consoles to the main console list
            Consoles.Add(topBarPane);
            Consoles.Add(quickSelectPane);
            Consoles.Add(ToolsPane);
            Consoles.Add(scrollerContainer);
            Consoles.Add(borderConsole);

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
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight); };
                    popup.Show(true);
                }
                else
                {
                    Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
                    popup.Center();
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else LoadEditor(popup.SelectedFile, popup.SelectedLoader); };
                    popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredTextSurface(), new FileLoaders.TextSurface(), new FileLoaders.Scene(), new FileLoaders.Entity() };
                    popup.Show(true);
                }

            });
        }

        
        private static void CreateNewEditor(Editors.Editors editorType, int width, int height)
        {
            Editors.IEditor editor = null;

            switch (editorType)
            {
                case SadConsoleEditor.Editors.Editors.Console:
                    editor = new Editors.LayeredConsoleEditor();
                    editor.New(Color.White, Color.Transparent, width, height);
                    break;
                case SadConsoleEditor.Editors.Editors.GameObject:
                    break;
                case SadConsoleEditor.Editors.Editors.Scene:
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
        }

        private static void LoadEditor(string file, FileLoaders.IFileLoader loader)
        {
            Editors.IEditor editor = null;

            if (loader is FileLoaders.LayeredTextSurface || loader is FileLoaders.TextSurface)
            {
                editor = new Editors.LayeredConsoleEditor();
                editor.Load(file, loader);
            }
            else if (loader is FileLoaders.Entity)
            {

            }
            else if (loader is FileLoaders.Scene)
            {

            }

            if (editor != null)
                AddEditor(editor, true);
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
            popup.Closed += (s, e) => { if (popup.DialogResult) CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight); };
            popup.Show(true);
        }

        public static void ShowLoadEditorPopup()
        {
            Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.Closed += (s, e) => { if (popup.DialogResult) LoadEditor(popup.SelectedFile, popup.SelectedLoader); };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredTextSurface(), new FileLoaders.Scene(), new FileLoaders.Entity() };
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
            if (ActiveEditor != null)
            {
                ActiveEditor.OnDeselected();
                Consoles.Remove(ActiveEditor.RenderedConsole);
            }

            if (OpenEditors.Contains(editor))
            {
                ActiveEditor = editor;
                CenterEditor();
                UpdateBorder(editor.Position);
                ToolsPane.RedrawPanels();
                ActiveEditor.OnSelected();
                Consoles.Add(ActiveEditor.RenderedConsole);

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
                Consoles.Add(borderConsole);
            }
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

        private static void Engine_EngineDrawFrame(object sender, EventArgs e)
        {
            
        }

        private static void Engine_EngineUpdated(object sender, EventArgs e)
        {

        }
    }
}