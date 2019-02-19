using SadConsole;
using Console = SadConsole.Console;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using SadConsole.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SadConsoleEditor
{
    public class MainConsole : ContainerConsole
    {
        public static MainConsole Instance;

        public Dictionary<string, Editors.IEditorMetadata> EditorTypes;
        public Dictionary<string, FileLoaders.IFileLoader> FileLoaders;

        private Console topBarPane;
        public Consoles.ToolPane ToolsPane;
        public Consoles.QuickSelectPane QuickSelectPane;
        private Console _borderConsole;

        public List<Editors.IEditor> OpenEditors;

        public Rectangle InnerEmptyBounds;
        public Rectangle InnerEmptyBoundsPixels;

        private string topBarLayerName = "None";
        private string topBarToolName = "None";
        private Point topBarMousePosition;

        private SadConsole.Entities.Entity brush;

        public SadConsole.Entities.Entity Brush
        {
            get { return brush; }
            set
            {
                if (brush != null)
                    brush.Parent = null;

                brush = value;
                Children.Add(brush);

                //if (value != null)
                //    value.Parent = borderConsole;
            }
        }
        public bool AllowKeyboardToMoveConsole;
        public bool UseKeyboard;

        public Editors.IEditor ActiveEditor { get; private set; }

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

        public Point SurfaceMouseLocation
        {
            set
            {
                topBarMousePosition = value;
                RefreshBackingPanel();
            }
        }

        public MainConsole()
        {
            MainConsole.Instance = this;
            UseKeyboard = true;

            // Create the basic consoles
            QuickSelectPane = new SadConsoleEditor.Consoles.QuickSelectPane();
            QuickSelectPane.Redraw();
            QuickSelectPane.IsVisible = false;

            topBarPane = new SadConsole.Console(Config.Program.WindowWidth, 1);
            topBarPane.DefaultBackground = SadConsole.Themes.Library.Default.Colors.MenuBack;
            topBarPane.Clear();
            topBarPane.FocusOnMouseClick = false;
            topBarPane.IsVisible = false;

            ToolsPane = new Consoles.ToolPane();
            ToolsPane.Position = new Point(Config.Program.WindowWidth - ToolsPane.Width - 1, 1);
            ToolsPane.IsVisible = false;

            // Get the whitespace in the middle of the screen
            var boundsLocation = new Point(1, topBarPane.Height).TranslateFont(topBarPane.Font, Config.Program.ScreenFont);
            boundsLocation.Y += 1;

            InnerEmptyBounds = new Rectangle(boundsLocation,
                                    new Point(
                                        new Point(ToolsPane.Position.X, 0).TranslateFont(SadConsole.Global.FontDefault, Config.Program.ScreenFont).X - boundsLocation.X - 1,
                                        new Point(0, QuickSelectPane.Position.Y).PixelLocationToConsole(Config.Program.ScreenFont).Y - 1 - boundsLocation.Y));

            InnerEmptyBoundsPixels = new Rectangle(InnerEmptyBounds.Location.ConsoleLocationToPixel(Config.Program.ScreenFont), InnerEmptyBounds.Size.ConsoleLocationToPixel(Config.Program.ScreenFont));

            _borderConsole = new Console(InnerEmptyBounds.Width, InnerEmptyBounds.Height, Config.Program.ScreenFont);
            _borderConsole.Position = InnerEmptyBounds.Location;
            
            // Add the consoles to the main console list
            Children.Add(_borderConsole);
            Children.Add(QuickSelectPane);
            Children.Add(topBarPane);
            Children.Add(ToolsPane);

            // Setup the file types for base editors.
            OpenEditors = new List<SadConsoleEditor.Editors.IEditor>();

            // Load tools
            var collection = new ServiceCollection();
            var assemblies = new List<System.Reflection.Assembly>();

            // Scan plugins
            var pluginsDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Plugins");
            foreach (var dir in System.IO.Directory.GetDirectories(pluginsDir))
            {
                var dirName = System.IO.Path.GetFileName(dir);
                var pluginDll = System.IO.Path.Combine(dir, dirName + ".dll");
                if (System.IO.File.Exists(pluginDll))
                {
                    assemblies.Add(System.Reflection.Assembly.LoadFile(pluginDll));

                    var settingsFile = System.IO.Path.GetRelativePath(AppContext.BaseDirectory, System.IO.Path.Combine(dir, dirName + ".json"));
                    
                    if (!System.IO.File.Exists(settingsFile))
                        throw new Exception($"Plugin must have a settings file: {settingsFile}");

                    var settings = SadConsole.Serializer.Load<EditorSettings[]>(settingsFile, false);
                    foreach (var setting in settings)
                        Config.Program.AddSettings(setting.EditorId, setting);
                }
            }

            collection.Scan(scan =>
                scan.FromAssemblies(assemblies)
                    .AddClasses(classes => classes.AssignableTo<Tools.ITool>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()

                    .AddClasses(classes => classes.AssignableTo<SadConsoleEditor.Editors.IEditorMetadata>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()

                    .AddClasses(classes => classes.AssignableTo<SadConsoleEditor.FileLoaders.IFileLoader>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()

                    .AddClasses(classes => classes.AssignableTo<IPlugin>())
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                );

            // Create an instance of plugin types
            var provider = collection.BuildServiceProvider();
            var plugins = provider.GetServices<IPlugin>();
            var tools = provider.GetServices<Tools.ITool>();

            EditorTypes = provider.GetServices<SadConsoleEditor.Editors.IEditorMetadata>().ToDictionary(i => i.Id);
            FileLoaders = provider.GetServices<SadConsoleEditor.FileLoaders.IFileLoader>().ToDictionary(i => i.Id);

            foreach (var plugin in plugins)
                plugin.Register();

            foreach (var tool in tools)
                ToolsPane.RegisterTool(tool);

            IsExclusiveMouse = true;
            SadConsole.Global.FocusedConsoles.Set(this);
        }

        public void ShowStartup()
        {
            Window.Prompt("Create new or open existing?", "New", "Open",
            (b) =>
            {
                if (b)
                {
                    var popup = new Windows.NewConsolePopup();
                    popup.Center();
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight, popup.SettingForeground, popup.SettingBackground); };
                    popup.Show(true);
                }
                else
                {
                    Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
                    popup.Center();
                    popup.Closed += (s, e) => { if (!popup.DialogResult) ShowStartup(); else LoadEditor(popup.SelectedFile, popup.SelectedEditor, popup.SelectedLoader); };
                    popup.Show(true);
                }

            });
        }


        private void CreateNewEditor(SadConsoleEditor.Editors.IEditorMetadata editor, int width, int height, Color defaultForeground, Color defaultBackground)
        {
            var createdEditor = editor.Create();
            createdEditor.New(defaultForeground, defaultBackground, width, height);
            AddEditor(createdEditor, true);
            topBarPane.IsVisible = true;
            ToolsPane.IsVisible = true;
        } 

        private void LoadEditor(string file, Editors.IEditorMetadata editor, FileLoaders.IFileLoader loader)
        {
            var createdEditor = editor.Create();
            createdEditor.Load(file, loader);
            createdEditor.Metadata.FilePath = file;
            createdEditor.Metadata.IsLoaded = true;
            createdEditor.Metadata.Title = System.IO.Path.GetFileNameWithoutExtension(file);
            AddEditor(createdEditor, true);

            topBarPane.IsVisible = true;
            ToolsPane.IsVisible = true;
        }

        public void ShowCloseConsolePopup() =>
            Window.Prompt(new SadConsole.ColoredString("Are you sure? You will lose any unsaved changes."), "Yes", "No", (r) => { if (r) RemoveEditor(ActiveEditor); });

        public void ShowNewEditorPopup()
        {
            Windows.NewConsolePopup popup = new Windows.NewConsolePopup();
            popup.Center();
            popup.Closed += (s, e) => { if (popup.DialogResult) CreateNewEditor(popup.Editor, popup.SettingWidth, popup.SettingHeight, popup.SettingForeground, popup.SettingBackground); };
            popup.Show(true);
        }

        public void ShowLoadEditorPopup()
        {
            //Windows.SelectFilePopup popup = new Windows.SelectFilePopup();
            //popup.Center();
            //popup.Closed += (s, e) => { if (popup.DialogResult) LoadEditor(popup.SelectedFile, popup.SelectedLoader); };
            //popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredSurface(), new FileLoaders.SadConsole.Surfaces.Basic(), new FileLoaders.Scene(), new FileLoaders.Entity(), new FileLoaders.Ansi() };
            //popup.Show(true);
        }

        public void ShowResizeEditorPopup()
        {
            if (ActiveEditor != null)
            {
                //Windows.ResizeSurfacePopup popup = new Windows.ResizeSurfacePopup(ActiveEditor.Width, ActiveEditor.Height);
                //popup.Center();
                //popup.Closed += (s, e) =>
                //{
                //    if (popup.DialogResult)
                //    {
                //        ActiveEditor.Resize(popup.SettingWidth, popup.SettingHeight);
                //    }
                //};
                //popup.Show(true);
            }
        }

        public void AddEditor(Editors.IEditor editor, bool show)
        {
            OpenEditors.Add(editor);
            ToolsPane.PanelFiles.DocumentsListbox.Items.Add(editor);

            if (show)
                ChangeActiveEditor(editor);
        }

        public void RemoveEditor(Editors.IEditor editor)
        {
            ToolsPane.PanelFiles.DocumentsListbox.Items.Remove(editor);
            editor.OnClosed();
            OpenEditors.Remove(editor);

            if (OpenEditors.Count == 0)
                ShowStartup();
            else
                ChangeActiveEditor(OpenEditors[0]);
        }

        public void ChangeActiveEditor(Editors.IEditor editor)
        {
            AllowKeyboardToMoveConsole = true;

            if (ActiveEditor != null)
            {
                ActiveEditor.OnDeselected();
                ActiveEditor.Surface.Parent = null;
                _borderConsole.Clear();
            }

            if (OpenEditors.Contains(editor))
            {
                ActiveEditor = editor;
                ActiveEditor.OnSelected();
                Children.Add(ActiveEditor.Surface);
                ToolsPane.RedrawPanels();
                
                CenterEditor();

                Children.MoveToTop(brush);

                if (ToolsPane.PanelFiles.DocumentsListbox.SelectedItem != editor)
                    ToolsPane.PanelFiles.DocumentsListbox.SelectedItem = editor;
            }
        }

        public void SaveEditor()
        {
            if (ActiveEditor != null)
            {
                if (!ActiveEditor.Metadata.IsLoaded && !ActiveEditor.Metadata.IsSaved)
                    SaveAsEditor();
                else
                {
                    ActiveEditor.Save(ActiveEditor.Metadata.FilePath, ActiveEditor.Metadata.LastLoader);
                }
            }
        }
        public void SaveAsEditor()
        {
            var popup = new Windows.SelectFilePopup(ActiveEditor.Metadata);
            popup.Center();
            popup.SkipFileExistCheck = true;
            popup.Closed += (s, e) =>
            {
                if (popup.DialogResult)
                {
                    ActiveEditor.Save(popup.SelectedFile, popup.SelectedLoader);
                    ActiveEditor.Metadata.IsSaved = true;
                    ActiveEditor.Metadata.FilePath = popup.SelectedFile;
                    ActiveEditor.Metadata.LastLoader = popup.SelectedLoader;
                }
            };

            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void CenterEditor()
        {
            Point position = new Point();
            var editorBounds = InnerEmptyBounds;
            editorBounds.Inflate(-1, -1);

            if (ActiveEditor.Width > editorBounds.Width || ActiveEditor.Height > editorBounds.Height)
            {
                // Need scrolling console
                if (ActiveEditor.Width > editorBounds.Width && ActiveEditor.Height > editorBounds.Height)
                {
                    position = editorBounds.Location;
                    ActiveEditor.Surface.ViewPort = new Rectangle(0, 0, editorBounds.Width, editorBounds.Height);
                }
                else if (ActiveEditor.Width > editorBounds.Width)
                {
                    position = new Point(editorBounds.Location.X, (editorBounds.Height + editorBounds.Y - ActiveEditor.Height) / 2);
                    ActiveEditor.Surface.ViewPort = new Rectangle(0, 0, editorBounds.Width, ActiveEditor.Height);
                }
                else if (ActiveEditor.Height > editorBounds.Height)
                {
                    position = new Point((editorBounds.Width + editorBounds.X - ActiveEditor.Width) / 2, editorBounds.Location.Y);
                    ActiveEditor.Surface.ViewPort = new Rectangle(0, 0, ActiveEditor.Width, editorBounds.Height);
                }
            }
            else
            {
                // Center normal
                position = new Point((editorBounds.Width + editorBounds.X - ActiveEditor.Width) / 2, (editorBounds.Height + editorBounds.Y - ActiveEditor.Height) / 2);
            }

            if (position.X < editorBounds.Left)
                position.X = editorBounds.Left;

            if (position.Y < editorBounds.Top)
                position.Y = editorBounds.Top;

            ActiveEditor.Surface.Position = position;
            _borderConsole.Clear();
            _borderConsole.DrawBox(new Rectangle(ActiveEditor.Surface.Position.X - 1 - _borderConsole.Position.X, ActiveEditor.Surface.Position.Y - 1 - _borderConsole.Position.Y, ActiveEditor.Surface.ViewPort.Width + 2, ActiveEditor.Surface.ViewPort.Height + 2), new Cell(DefaultForeground, Color.Black, 177));
        }

        private void RefreshBackingPanel()
        {
            topBarPane.Clear();

            var text = new SadConsole.ColoredString("   X: ", SadConsole.Themes.Library.Default.Colors.Text, Color.Transparent)     + new SadConsole.ColoredString(topBarMousePosition.X.ToString(), SadConsole.Themes.Library.Default.Colors.TextBright, Color.Transparent) +
                       new SadConsole.ColoredString(" Y: ", SadConsole.Themes.Library.Default.Colors.Text, Color.Transparent)       + new SadConsole.ColoredString(topBarMousePosition.Y.ToString(), SadConsole.Themes.Library.Default.Colors.TextBright, Color.Transparent) +
                       new SadConsole.ColoredString("   Layer: ", SadConsole.Themes.Library.Default.Colors.Text, Color.Transparent) + new SadConsole.ColoredString(topBarLayerName, SadConsole.Themes.Library.Default.Colors.TextBright, Color.Transparent) +
                       new SadConsole.ColoredString("   Tool: ", SadConsole.Themes.Library.Default.Colors.Text, Color.Transparent)  + new SadConsole.ColoredString(topBarToolName, SadConsole.Themes.Library.Default.Colors.TextBright, Color.Transparent);
            text.IgnoreBackground = true;
            topBarPane.Print(0, 0, text);
        }

        public override bool ProcessMouse(MouseConsoleState state)
        {
            if (brush != null)
            {
                if (MainConsole.Instance.InnerEmptyBounds.Contains(state.WorldPosition))
                {
                    brush.IsVisible = true;
                    brush.Position = state.WorldPosition;
                    if (MainConsole.Instance.ActiveEditor != null)
                    {
                        state = new MouseConsoleState(MainConsole.Instance.ActiveEditor.Surface, state.Mouse);
                        SurfaceMouseLocation = state.ConsolePosition;
                        return MainConsole.Instance.ActiveEditor.ProcessMouse(state, true);
                    }
                }
                else
                {
                    brush.IsVisible = false;
                }
            }

            if (ToolsPane.ProcessMouse(state))
                return true;

            return false;
        }

        //private bool ProcessMouseForBrush(IConsole console, MouseConsoleState state)
        //{
        //    // This is not currently used. It may be in the future.
        //    return false;
        //}

        public override bool ProcessKeyboard(Keyboard info)
        {
            if (UseKeyboard)
            {
                bool movekeyPressed = false;
                //var position = new Point(borderConsole.Position.X + 1, borderConsole.Position.Y + 1);
                ////var result = base.ProcessKeyboard(info);
                //if (AllowKeyboardToMoveConsole && ActiveEditor != null && ActiveEditor.Surface != null)
                //{
                //    bool shifted = Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
                //    var oldRenderArea = ActiveEditor.Surface.RenderArea;

                //    if (!shifted && Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                //        ActiveEditor.Surface.RenderArea = new Rectangle(ActiveEditor.Surface.RenderArea.Left - 1, ActiveEditor.Surface.RenderArea.Top, InnerEmptyBounds.Width, InnerEmptyBounds.Height);

                //    else if (!shifted && Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                //        ActiveEditor.Surface.RenderArea = new Rectangle(ActiveEditor.Surface.RenderArea.Left + 1, ActiveEditor.Surface.RenderArea.Top, InnerEmptyBounds.Width, InnerEmptyBounds.Height);

                //    if (!shifted && Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                //        ActiveEditor.Surface.RenderArea = new Rectangle(ActiveEditor.Surface.RenderArea.Left, ActiveEditor.Surface.RenderArea.Top - 1, InnerEmptyBounds.Width, InnerEmptyBounds.Height);

                //    else if (!shifted && Global.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                //        ActiveEditor.Surface.RenderArea = new Rectangle(ActiveEditor.Surface.RenderArea.Left, ActiveEditor.Surface.RenderArea.Top + 1, InnerEmptyBounds.Width, InnerEmptyBounds.Height);

                //    movekeyPressed = oldRenderArea != ActiveEditor.Surface.RenderArea;

                //}

                if (!movekeyPressed)
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
                        if (ToolsPane.ProcessKeyboard(Global.KeyboardState))
                        {
                        }
                        // Look for quick select F* keys
                        else if (QuickSelectPane.ProcessKeyboard(Global.KeyboardState))
                        {
                        }
                        else if (ActiveEditor != null)
                        {
                            ActiveEditor.ProcessKeyboard(Global.KeyboardState);
                        }
                    }
                }
            }

            // Always return true so that the virtual cursor doesn't start working.
            return true;
        }


    }
}