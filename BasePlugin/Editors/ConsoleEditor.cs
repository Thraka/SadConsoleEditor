using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SadConsole;
using Console = SadConsole.Console;
//using SadConsoleEditor.Panels;
using System.Linq;
using SadConsole.Input;
using SadConsole.Renderers;
using SadConsoleEditor.Panels;

namespace SadConsoleEditor.Editors
{
    public class ConsoleEditorMetadata : IEditorMetadata
    {
        public string Id => "CONSOLE";
        public string Title { get; set; } = "Basic Console";
        public string FilePath { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsSaved { get; set; }
        public FileLoaders.IFileLoader LastLoader { get; set; }
        public IEditor Create() => new ConsoleEditor();
    }

    public class ConsoleEditor : IEditor
    {
        private Tools.ITool[] _tools;
        private Tools.ITool selectedTool;
        private ToolsPanel toolsPanel;

        private CustomPanel[] _panels;

        private ScrollingConsole _surface;

        public ScrollingConsole Surface => _surface;

        public IEditor LinkedEditor { get; set; }

        public IEditorMetadata Metadata { get; set; } = new ConsoleEditorMetadata(); 

        public CustomPanel[] Panels => _panels;

        public int Width => _surface.Width;

        public int Height => _surface.Height;

        public string DocumentTitle { get; set; }

        public Tools.ITool SelectedTool
        {
            get { return selectedTool; }
            set
            {
                toolsPanel.ToolsListBox.SelectedItem = value;
            }
        }

        public ConsoleEditor()
        {
            // Fill tools
            var settings = Config.Program.GetSettings(Metadata.Id);

            _tools = MainConsole.Instance.ToolsPane.GetTools(settings.Tools).ToArray();

            toolsPanel = new ToolsPanel();

            foreach (var tool in _tools)
            {
                toolsPanel.ToolsListBox.Items.Add(tool);
            }

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            //panels = new CustomPanel[] { layerManagementPanel, toolsPanel };
            _panels = new CustomPanel[] { toolsPanel };

            toolsPanel.ToolsListBox.SelectedItem = _tools[0];
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.CellSurface)
            {
                Reset();

                var cellSurface = (SadConsole.CellSurface)loader.Load(file);
                _surface = new ScrollingConsole(cellSurface.Width, cellSurface.Height, new Rectangle(0, 0,
                            Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, cellSurface.Width),
                            Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, cellSurface.Height)));

                //layerManagementPanel.SetLayeredSurface(surface);
            }
            //else if (loader is FileLoaders.SadConsole.Surfaces.Basic)
            //{
            //    Reset();

            //    var loadedSurface = (SadConsole.Surfaces.SadConsole.Surfaces.Basic)loader.Load(file);
            //    surface = new SadConsole.Surfaces.Layered(loadedSurface.Width, loadedSurface.Height, Config.Program.ScreenFont, 
            //                    new Rectangle(0, 0, Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, loadedSurface.RenderArea.Width),
            //                                        Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, loadedSurface.RenderArea.Height)),
            //                    1);
            //    loadedSurface.Copy(surface);
            //    LayerMetadata.Create("root", true, false, true, surface.ActiveLayer);
            //    layerManagementPanel.SetLayeredSurface(surface);

            //    Title = System.IO.Path.GetFileName(file);
            //}
            //else if (loader is FileLoaders.Ansi)
            //{
            //    Reset();

            //    var loadedSurface = (SadConsole.Surfaces.NoDrawSurface)loader.Load(file);
            //    surface = new SadConsole.Surfaces.Layered(loadedSurface.Width, loadedSurface.Height, Config.Program.ScreenFont,
            //                    new Rectangle(0, 0, Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, loadedSurface.RenderArea.Width),
            //                                        Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, loadedSurface.RenderArea.Height)),
            //                    1);
            //    loadedSurface.Copy(surface);
            //    LayerMetadata.Create("root", true, false, true, surface.ActiveLayer);
            //    layerManagementPanel.SetLayeredSurface(surface);
            //}

            Metadata.Title = System.IO.Path.GetFileName(file);
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();
            int renderWidth = Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, height);

            _surface = new SadConsole.ScrollingConsole(width, height, SadConsoleEditor.Config.Program.ScreenFont, new Rectangle(0,0, renderWidth, renderHeight));

            //LayerMetadata.Create("Root", true, false, true, surface.ActiveLayer);

            //layerManagementPanel.SetLayeredSurface(surface);
            //layerManagementPanel.IsCollapsed = true;

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
                SelectedTool = _tools[0];
            else
            {
                selectedTool.OnSelected();
                MainConsole.Instance.ToolsPane.RedrawPanels();
            }
        }

        public bool ProcessKeyboard(Keyboard info)
        {
            if (!toolsPanel.SelectedTool.ProcessKeyboard(info, _surface))
            {
                var keys = info.KeysReleased.Select(k => k.Character).ToList();

                foreach (var item in _tools)
                {
                    if (keys.Contains(item.Hotkey))
                    {
                        toolsPanel.ToolsListBox.SelectedItem = item;
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public bool ProcessMouse(SadConsole.Input.MouseConsoleState info, bool isInBounds)
        {
            toolsPanel.SelectedTool?.ProcessMouse(info, _surface, isInBounds);
            return false;
        }

        public void Draw()
        {
        }

        public void Reset()
        {
            
        }

        public void Resize(int width, int height)
        {
            //Reset();
            //int renderWidth = Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, width);
            //int renderHeight = Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, height);

            //var oldSurface = surface;
            //surface = new SadConsole.Surfaces.Layered(width, height, SadConsoleEditor.Config.Program.ScreenFont, new Rectangle(0, 0, renderWidth, renderHeight), 1);

            //for (var index = 0; index < oldSurface.LayerCount; index++)
            //{
            //    oldSurface.SetActiveLayer(index);
            //    surface.SetActiveLayer(index);
            //    oldSurface.Copy(surface);
            //    surface.GetLayer(index).Metadata = oldSurface.GetLayer(index).Metadata;
            //}
            
            //layerManagementPanel.SetLayeredSurface(surface);

            //MainConsole.Instance.RefreshBorder();
        }

        public void Save(string file, FileLoaders.IFileLoader saver)
        {
            saver.Save(_surface, file);
        }

        public void Update()
        {
            toolsPanel.SelectedTool?.Update();
        }

        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {
                selectedTool = tool;
                List<CustomPanel> newPanels = new List<CustomPanel>() { toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                _panels = newPanels.ToArray();
                MainConsole.Instance.ToolsPane.RedrawPanels();
            }
        }
    }
}
