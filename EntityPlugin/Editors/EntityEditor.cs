﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SadConsole;
//using SadConsoleEditor.Panels;
using System.Linq;
using SadConsole.Input;
using SadConsoleEditor.Panels;
using SadConsoleEditor.Editors;
using SadConsoleEditor;
using Tools = SadConsoleEditor.Tools;

namespace EntityPlugin.Editors
{
    public class EntityEditorMetadata : IEditorMetadata
    {
        public string Id => "ENTITY";
        public string Title { get; set; } = "Entity/Animation";
        public string FilePath { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsSaved { get; set; }
        public SadConsoleEditor.FileLoaders.IFileLoader LastLoader { get; set; }
        public IEditor Create() => new EntityEditor();
    }

    public class EntityEditor : IEditor
    {
        private Tools.ITool[] _tools;
        private Tools.ITool selectedTool;
        private ToolsPanel toolsPanel;
        private Panels.AnimationsPanel _animationPanel;
        private Panels.AnimationFramesPanel _animationFramesPanel;
        private Panels.EntityNamePanel _entityNamePanel;

        private CustomPanel[] _panels;

        private ScrollingConsole _surface;
        private AnimatedConsole _animation;
        private SadConsole.Entities.Entity _entity;

        public ScrollingConsole Surface => _surface;

        public IEditor LinkedEditor { get; set; }

        public IEditorMetadata Metadata { get; set; } = new EntityEditorMetadata(); 

        public CustomPanel[] Panels => _panels;

        public int Width => _surface.Width;

        public int Height => _surface.Height;

        public string DocumentTitle { get; set; }

        public Tools.ITool SelectedTool
        {
            get => selectedTool;
            set => toolsPanel.ToolsListBox.SelectedItem = value;
        }

        public EntityEditor()
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

            _animationPanel = new Panels.AnimationsPanel((ac) =>
            {
                _animation = ac;
            });

            _animationFramesPanel = new Panels.AnimationFramesPanel((cs) =>
            {
                _surface.SetSurface(cs.Cells, cs.Width, cs.Height);
            });

            _entityNamePanel = new Panels.EntityNamePanel();

            //panels = new CustomPanel[] { layerManagementPanel, toolsPanel };
            _panels = new CustomPanel[] { _entityNamePanel, _animationFramesPanel, _animationPanel, toolsPanel };

            toolsPanel.ToolsListBox.SelectedItem = _tools[0];
        }

        public void Load(string file, SadConsoleEditor.FileLoaders.IFileLoader loader)
        {
            if (loader.Id == "ENTITY")
            {
                Reset();

                //var cellSurface = (SadConsole.CellSurface)loader.Load(file);
                //_surface = new ScrollingConsole(cellSurface.Width, cellSurface.Height, Config.Program.ScreenFont, new Rectangle(0, 0,
                //            Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, cellSurface.Width),
                //            Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, cellSurface.Height)), cellSurface.Cells);

                //_entity = new SadConsole.Entities.Entity()

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
        }

        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();
            int renderWidth = Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, height);

            _entity = new SadConsole.Entities.Entity(width, height, Config.Program.ScreenFont);
            _entity.DefaultForeground = foreground;
            _entity.DefaultBackground = background;
            _animation = _entity.Animation;
            _animation.DefaultForeground = foreground;
            _animation.DefaultBackground = background;
            _animation.Clear();
            _surface = new ScrollingConsole(width, height, Config.Program.ScreenFont, new Rectangle(0,0, renderWidth, renderHeight), _animation.Cells);
            _surface.DefaultForeground = foreground;
            _surface.DefaultBackground = background;

            _entityNamePanel.SetEntity(_entity);
            _animationPanel.SetEntity(_entity);
            _animationFramesPanel.SetAnimation(_animation);
            //_surface.FillWithRandomGarbage();
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
            Reset();
            int renderWidth = Math.Min(MainConsole.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainConsole.Instance.InnerEmptyBounds.Height, height);

            var oldSurface = _surface;

            if (renderWidth == width && renderHeight == height)
                _surface.Resize(width, height, false);
            else
                _surface.Resize(width, height, false, new Rectangle(0, 0, renderWidth, renderHeight));
            
            MainConsole.Instance.CenterEditor();
        }

        public bool Save(string file, SadConsoleEditor.FileLoaders.IFileLoader saver) =>
            saver.Save(_surface, file);

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
                List<CustomPanel> newPanels = new List<CustomPanel>() { _entityNamePanel, _animationFramesPanel, _animationPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                _panels = newPanels.ToArray();
                MainConsole.Instance.ToolsPane.RedrawPanels();
            }
        }
    }
}
