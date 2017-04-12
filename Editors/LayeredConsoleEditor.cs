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
using SadConsoleEditor.FileLoaders;
using SadConsole.Renderers;

namespace SadConsoleEditor.Editors
{
    class LayeredConsoleEditor : IEditor
    {
        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;
        private ToolsPanel toolsPanel;

        private CustomPanel[] panels;
        private LayersPanel layerManagementPanel;

        private SadConsole.Renderers.LayeredSurfaceRenderer renderer;
        private SadConsole.Surfaces.LayeredSurface surface;

        public ISurface Surface => surface;

        public ISurfaceRenderer Renderer => renderer;

        public Editors EditorType => Editors.Console;

        public string EditorTypeName => "Console";

        public string Title { get; set; }

        public CustomPanel[] Panels => panels;

        public int Width => surface.Width;

        public int Height => surface.Height;

        public string DocumentTitle { get; set; }

        public LayeredConsoleEditor()
        {
            renderer = new LayeredSurfaceRenderer();

            layerManagementPanel = new LayersPanel();
            layerManagementPanel.IsCollapsed = true;

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

            toolsPanel = new ToolsPanel();
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.TextTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.CircleTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.RecolorTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.FillTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.BoxTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SelectionTool.ID]);

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;
            toolsPanel.ToolsListBox.SelectedItem = tools[Tools.PaintTool.ID];

            panels = new CustomPanel[] { layerManagementPanel, toolsPanel };

            
        }

        public void Load(string file, IFileLoader loader)
        {
            throw new NotImplementedException();
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();

            int renderWidth = Math.Max(MainScreen.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Max(MainScreen.Instance.InnerEmptyBounds.Height, height);

            surface = new SadConsole.Surfaces.LayeredSurface(width, height, SadConsoleEditor.Settings.Config.ScreenFont, 1);
            
            if (renderWidth != width || renderHeight != height)
                surface.RenderArea = new Rectangle(0, 0, renderWidth, renderHeight);

            LayerMetadata.Create("Root", true, false, true, surface.ActiveLayer);

            var editor = new SurfaceEditor(surface);
            editor.FillWithRandomGarbage();

            layerManagementPanel.SetLayeredSurface(surface);
            layerManagementPanel.IsCollapsed = true;
        }

        public void OnClosed()
        {
            
        }

        public void OnDeselected()
        {
            
        }

        public void OnSelected()
        {
            
        }

        public bool ProcessKeyboard(Keyboard info)
        {
            return false;
        }

        public bool ProcessMouse(SadConsole.Input.MouseConsoleState info, bool isInBounds)
        {
            toolsPanel.SelectedTool?.ProcessMouse(info, surface, isInBounds);
            return false;
        }

        public void Render()
        {
            
        }

        public void Reset()
        {
            
        }

        public void Resize(int width, int height)
        {
            
        }

        public void Save()
        {
            
        }

        public void Update()
        {
            
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
    }
}
