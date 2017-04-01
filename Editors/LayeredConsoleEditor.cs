using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SadConsole.Surfaces;
using Console = SadConsole.Console;
using SadConsoleEditor.Panels;
using System.Linq;
using SadConsole.Input;
using SadConsole;

namespace SadConsoleEditor.Editors
{
    class LayeredConsoleEditor : IEditor
    {
        private LayeredSurface surface;
        private Console consoleWrapper;
        private CustomPanel[] panels;
        private LayersPanel layerManagementPanel;
        private ToolsPanel toolsPanel;
        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;

        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.Console; } }

        public string Title { get; set; }

        public string EditorTypeName { get { return "Console"; } }

        public int Height { get { return surface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public int Width { get { return surface.Width; } }

        public CustomPanel[] Panels { get { return panels; } }

        public Console RenderedConsole { get { return consoleWrapper; } }

        private Tools.ITool SelectedTool
        {
            get { return selectedTool; }
            set
            {
                toolsPanel.ToolsListBox.SelectedItem = value;
            }
        }

        public LayeredConsoleEditor()
        {
            consoleWrapper = new Console(1, 1);
            consoleWrapper.Renderer = new SadConsole.Renderers.LayeredSurfaceRenderer();
            consoleWrapper.MouseHandler = ProcessMouse;
            consoleWrapper.UseKeyboard = false;

            consoleWrapper.MouseMove += (o, e) => { toolsPanel.SelectedTool?.MouseMoveSurface(e.MouseState, surface); };
            consoleWrapper.MouseEnter += (o, e) => { toolsPanel.SelectedTool?.MouseEnterSurface(e.MouseState, surface); };
            consoleWrapper.MouseExit += (o, e) => { toolsPanel.SelectedTool?.MouseExitSurface(e.MouseState, surface); };

            layerManagementPanel = new LayersPanel() { IsCollapsed = true };
            toolsPanel = new ToolsPanel();

            // Fill tools
            tools = new Dictionary<string, Tools.ITool>();
            tools.Add(Tools.PaintTool.ID, new Tools.PaintTool());
            tools.Add(Tools.LineTool.ID, new Tools.LineTool());
            tools.Add(Tools.TextTool.ID, new Tools.TextTool());
            tools.Add(Tools.CircleTool.ID, new Tools.CircleTool());
            tools.Add(Tools.RecolorTool.ID, new Tools.RecolorTool());
            tools.Add(Tools.FillTool.ID, new Tools.FillTool());
            tools.Add(Tools.BoxTool.ID, new Tools.BoxTool());
            tools.Add(Tools.SelectionTool.ID, new Tools.SelectionTool());

            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.TextTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.CircleTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.RecolorTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.FillTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.BoxTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SelectionTool.ID]);

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            panels = new CustomPanel[] { layerManagementPanel, toolsPanel };
        }

        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox<SadConsole.Controls.ListBoxItem>.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {
                selectedTool = tool;
                List<CustomPanel> newPanels = new List<CustomPanel>() { layerManagementPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                panels = newPanels.ToArray();
                MainScreen.Instance.ToolsPane.RedrawPanels();
            }
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();

            // Create the new text surface
            surface = new LayeredSurface(width, height, 1);

            // Update metadata
            LayerMetadata.Create("main", false, false, true, surface.GetLayer(0));
            surface.SetActiveLayer(0);
            surface.Font = Settings.Config.ScreenFont;

            // Update the layer management panel
            layerManagementPanel.SetLayeredSurface(surface);

            // Set the text surface as the one we're displaying
            consoleWrapper.TextSurface = surface;

            // Update the border
            if (MainScreen.Instance.ActiveEditor == this)
                MainScreen.Instance.UpdateBorder(consoleWrapper.Position);
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.BasicSurface)
            {
                // Load the plain surface
                BasicSurface surface = (BasicSurface)loader.Load(file);

                // Load up a new layered text surface
                this.surface = new LayeredSurface(surface.Width, surface.Height, 1);

                // Setup metadata
                LayerMetadata.Create("main", false, false, true, this.surface.GetLayer(0));

                // Use the loaded surface
                this.surface.ActiveLayer.Cells = surface.Cells;
                this.surface.SetActiveLayer(0);

                // Set the text surface as the one we're displaying
                consoleWrapper.TextSurface = this.surface;

                // Update the border
                if (MainScreen.Instance.ActiveEditor == this)
                    MainScreen.Instance.UpdateBorder(consoleWrapper.Position);
            }
            else if (loader is FileLoaders.LayeredSurface)
            {
                surface = (LayeredSurface)loader.Load(file);
                consoleWrapper.TextSurface = surface;

                if (MainScreen.Instance.ActiveEditor == this)
                    MainScreen.Instance.UpdateBorder(consoleWrapper.Position);
            }

            surface.Font = Settings.Config.ScreenFont;
            Title = System.IO.Path.GetFileName(file);

            // Update the layer management panel
            layerManagementPanel.SetLayeredSurface(surface);
        }

        public void Save()
        {
            var popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.SkipFileExistCheck = true;
            popup.Closed += (s, e) =>
            {
                if (popup.DialogResult)
                    popup.SelectedLoader.Save(surface, popup.SelectedFile);

            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredSurface() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Resize(int width, int height)
        {
            var oldSurface = surface;
            var newSurface = new LayeredSurface(width, height, Settings.Config.ScreenFont, oldSurface.LayerCount);

            for (int i = 0; i < oldSurface.LayerCount; i++)
            {
                var oldLayer = oldSurface.GetLayer(i);
                var newLayer = newSurface.GetLayer(i);
                oldSurface.SetActiveLayer(i);
                newSurface.SetActiveLayer(i);
                oldSurface.Copy(newSurface);
                newLayer.Metadata = oldLayer.Metadata;
                newLayer.IsVisible = oldLayer.IsVisible;
            }

            consoleWrapper.TextSurface = surface = newSurface;
            layerManagementPanel.SetLayeredSurface(surface);
            toolsPanel.SelectedTool = toolsPanel.SelectedTool;

            if (MainScreen.Instance.ActiveEditor == this)
            {
                MainScreen.Instance.CenterEditor();
                MainScreen.Instance.UpdateBorder(consoleWrapper.Position);
            }
        }

        public void Reset()
        {

        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (MainScreen.Instance.ActiveEditor == this)
                MainScreen.Instance.UpdateBorder(consoleWrapper.Position);

            MainScreen.Instance.UpdateBrush();
        }

        public void OnClosed()
        {
        }

        public void OnDeselected()
        {
        }

        public void OnSelected()
        {
            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
            {
                var oldTool = selectedTool;
                SelectedTool = null;
                SelectedTool = selectedTool;
            }
        }

        public void Render()
        {
        }

        public void Update()
        {
            selectedTool.Update();
        }

        //public bool ProcessKeyboard(IConsole console, SadConsole.Input.Keyboard info)
        //{
            
        //    //MainScreen.Instance.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.BasicSurface);
        //    return false;
        //}

        public bool ProcessKeyboard(Keyboard info)
        {
            if (!toolsPanel.SelectedTool.ProcessKeyboard(info, surface))
            {
                var keys = info.KeysReleased.Select(k => k.Character).ToList();

                foreach (var item in tools.Values)
                {
                    if (keys.Contains(item.Hotkey))
                    {
                        SelectedTool = item;
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public bool ProcessMouse(IConsole console, SadConsole.Input.MouseConsoleState info)
        {
            consoleWrapper.MouseHandler = null;
            consoleWrapper.UseMouse = true;
            consoleWrapper.ProcessMouse(info);
            consoleWrapper.MouseHandler = ProcessMouse;

            toolsPanel.SelectedTool?.ProcessMouse(info, surface);

            if (consoleWrapper.IsMouseOver)
            {
                MainScreen.Instance.SurfaceMouseLocation = info.ConsolePosition;
                return true;
            }
            else
                MainScreen.Instance.SurfaceMouseLocation = Point.Zero;

            consoleWrapper.UseMouse = false;
            return false;
        }
    }
}
