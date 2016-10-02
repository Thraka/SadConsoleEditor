using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using SadConsoleEditor.Panels;
using System.Linq;
using SadConsole.Input;

namespace SadConsoleEditor.Editors
{
    class LayeredConsoleEditor : IEditor
    {
        private LayeredTextSurface textSurface;
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

        public int Height { get { return textSurface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public int Width { get { return textSurface.Width; } }

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
            consoleWrapper.Renderer = new LayeredTextRenderer();
            consoleWrapper.MouseHandler = ProcessMouse;
            consoleWrapper.CanUseKeyboard = false;

            consoleWrapper.MouseMove += (o, e) => { toolsPanel.SelectedTool?.MouseMoveSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseEnter += (o, e) => { toolsPanel.SelectedTool?.MouseEnterSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseExit += (o, e) => { toolsPanel.SelectedTool?.MouseExitSurface(e.OriginalMouseInfo, textSurface); };

            layerManagementPanel = new LayersPanel();
            toolsPanel = new ToolsPanel();

            // Fill tools
            tools = new Dictionary<string, Tools.ITool>();
            tools.Add(Tools.PaintTool.ID, new Tools.PaintTool());
            tools.Add(Tools.LineTool.ID, new Tools.LineTool());
            tools.Add(Tools.CircleTool.ID, new Tools.CircleTool());
            tools.Add(Tools.RecolorTool.ID, new Tools.RecolorTool());
            tools.Add(Tools.FillTool.ID, new Tools.FillTool());
            tools.Add(Tools.BoxTool.ID, new Tools.BoxTool());
            tools.Add(Tools.SelectionTool.ID, new Tools.SelectionTool());

            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
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

                List<CustomPanel> newPanels = new List<CustomPanel>() { layerManagementPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                panels = newPanels.ToArray();
                EditorConsoleManager.ToolsPane.RedrawPanels();
            }
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();

            // Create the new text surface
            textSurface = new LayeredTextSurface(width, height, 1);

            // Update metadata
            LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));
            textSurface.SetActiveLayer(0);
            textSurface.Font = Settings.Config.ScreenFont;

            // Update the layer management panel
            layerManagementPanel.SetLayeredTextSurface(textSurface);

            // Set the text surface as the one we're displaying
            consoleWrapper.TextSurface = textSurface;

            // Update the border
            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.TextSurface)
            {
                // Load the plain surface
                TextSurface surface = (TextSurface)loader.Load(file);

                // Load up a new layered text surface
                textSurface = new LayeredTextSurface(surface.Width, surface.Height, 1);

                // Setup metadata
                LayerMetadata.Create("main", false, false, true, textSurface.GetLayer(0));

                // Use the loaded surface
                textSurface.ActiveLayer.Cells = surface.Cells;
                textSurface.SetActiveLayer(0);
                
                // Set the text surface as the one we're displaying
                consoleWrapper.TextSurface = textSurface;

                // Update the border
                if (EditorConsoleManager.ActiveEditor == this)
                    EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }
            else if (loader is FileLoaders.LayeredTextSurface)
            {
                textSurface = (LayeredTextSurface)loader.Load(file);
                consoleWrapper.TextSurface = textSurface;

                if (EditorConsoleManager.ActiveEditor == this)
                    EditorConsoleManager.UpdateBorder(consoleWrapper.Position);
            }

            textSurface.Font = Settings.Config.ScreenFont;
            Title = System.IO.Path.GetFileName(file);

            // Update the layer management panel
            layerManagementPanel.SetLayeredTextSurface(textSurface);
        }

        public void Save()
        {
            var popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.SkipFileExistCheck = true;
            popup.Closed += (s, e) =>
            {
                if (popup.DialogResult)
                    popup.SelectedLoader.Save(textSurface, popup.SelectedFile);

            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.LayeredTextSurface() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Reset()
        {

        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            EditorConsoleManager.UpdateBrush();
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
                SelectedTool = selectedTool;
        }

        public void Render()
        {
        }

        public void Update()
        {
        }

        public bool ProcessKeyboard(IConsole console, SadConsole.Input.KeyboardInfo info)
        {
            //EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);
            return false;
        }

        public bool ProcessKeyboard(KeyboardInfo info)
        {
            return false;
        }

        public bool ProcessMouse(IConsole console, SadConsole.Input.MouseInfo info)
        {
            consoleWrapper.MouseHandler = null;
            consoleWrapper.CanUseMouse = true;
            consoleWrapper.ProcessMouse(info);
            consoleWrapper.MouseHandler = ProcessMouse;

            toolsPanel.SelectedTool?.ProcessMouse(info, textSurface);

            if (consoleWrapper.IsMouseOver)
            {
                EditorConsoleManager.SurfaceMouseLocation = info.ConsoleLocation;
                return true;
            }
            else
                EditorConsoleManager.SurfaceMouseLocation = Point.Zero;

            consoleWrapper.CanUseMouse = false;
            return false;
        }


        public override string ToString()
        {
            return this.ToString();
        }
        
    }
}
