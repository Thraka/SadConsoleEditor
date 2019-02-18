using System;
using SadConsole;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Tools;
using SadConsoleEditor.Panels;
using SadConsole.Input;

namespace SadConsoleEditor.Consoles
{
    public class ToolPane : ContainerConsole
    {
        private ControlsConsole ToolsConsole;
        private ControlsConsole ScrollerConsole;

        public static int PanelWidth;
        public static int PanelWidthControls;

        public SadConsole.Controls.ScrollBar ToolsPaneScroller { get; private set; }

        private List<Tuple<CustomPanel, int>> _hotSpots;
        public Dictionary<string, ITool> Tools;

        public FilesPanel PanelFiles;

        public new int Width { get { return ToolsConsole.Width; } }
        
        internal ToolPane()
        {
            ToolsConsole = new ControlsConsole(Config.Program.ToolPaneWidth - 1, Config.Program.WindowHeight * 3);
            //ToolsConsole.MouseHandler = ProcessMouse;
            ToolsConsole.UseKeyboard = false;

            // Create scrollbar
            ToolsPaneScroller = new SadConsole.Controls.ScrollBar(SadConsole.Orientation.Vertical, Config.Program.WindowHeight - 1);
            ToolsPaneScroller.Maximum = ToolsConsole.Height - Config.Program.WindowHeight;
            ToolsPaneScroller.ValueChanged += (o, e) =>
            {
                ToolsConsole.ViewPort = new Rectangle(0, ToolsPaneScroller.Value, ToolsConsole.Width, Config.Program.WindowHeight);
            };

            ScrollerConsole = new ControlsConsole(1, Config.Program.WindowHeight - 1);
            ScrollerConsole.Add(ToolsPaneScroller);
            ScrollerConsole.Position = new Point(Width, 0);
            ScrollerConsole.IsVisible = true;
            ScrollerConsole.FocusOnMouseClick = false;

            

            PanelWidth = Config.Program.ToolPaneWidth - 1;
            PanelWidthControls = PanelWidth - 2;

            Tools = new Dictionary<string, ITool>();
            
            _hotSpots = new List<Tuple<CustomPanel, int>>();

            ToolsConsole.ViewPort = new Rectangle(0, 0, ToolsConsole.Width, Config.Program.WindowHeight - 1);

            // Create panels
            PanelFiles = new FilesPanel();

            Children.Add(ToolsConsole);
            Children.Add(ScrollerConsole);
        }

        internal void RegisterTool(ITool tool)
        {
            Tools.Add(tool.Id, tool);
        }

        public ITool GetTool(string id)
        {
            if (Tools.ContainsKey(id))
                return Tools[id];
            else
                throw new Exception($"Tool is not registered: {id}");
        }

        public IEnumerable<ITool> GetTools(params string[] ids)
        {
            foreach (var id in ids)
            {
                if (Tools.ContainsKey(id))
                    yield return Tools[id];
            }
        }

        public void RedrawPanels()
        {
            int activeRow = 0;
            ToolsConsole.Clear();
            ToolsConsole.RemoveAll();
            _hotSpots.Clear();

            char open = (char)31;
            char closed = (char)16;

            List<CustomPanel> allPanels = new List<CustomPanel>() { PanelFiles };

            // Custom panels from the selected editor
            if (MainConsole.Instance.ActiveEditor != null)
                if (MainConsole.Instance.ActiveEditor.Panels != null && MainConsole.Instance.ActiveEditor.Panels.Length != 0)
                    allPanels.AddRange(MainConsole.Instance.ActiveEditor.Panels);

            // Custom panels from the selected tool
            //if (PanelTools.SelectedTool.ControlPanels != null && PanelTools.SelectedTool.ControlPanels.Length != 0)
            //    allPanels.AddRange(PanelTools.SelectedTool.ControlPanels);

            // Display all panels needed
            if (allPanels.Count != 0)
            {
                foreach (var pane in allPanels)
                {
                    if (pane.IsVisible)
                    {
                        pane.Loaded();
                        _hotSpots.Add(new Tuple<CustomPanel, int>(pane, activeRow));
                        if (pane.IsCollapsed == false)
                        {
                            ToolsConsole.Print(1, activeRow++, open + " " + pane.Title);
                            ToolsConsole.Print(0, activeRow++, new string((char)196, ToolsConsole.Width));

                            foreach (var control in pane.Controls)
                            {
                                if (control != null)
                                {
                                    if (control.IsVisible)
                                    {
                                        ToolsConsole.Add(control);
                                        control.Position = new Point(1, activeRow);
                                        activeRow += pane.Redraw(control) + control.Height;
                                    }
                                }
                                else
                                    activeRow++;
                            }

                            activeRow += 1;
                        }
                        else
                            ToolsConsole.Print(1, activeRow++, closed + " " + pane.Title);
                    }
                }
            }

            int scrollAbility = activeRow + 1 - Config.Program.WindowHeight;

            if (scrollAbility <= 0)
            {
                ToolsPaneScroller.Value = 0;
                ToolsPaneScroller.IsEnabled = false;
                ToolsPaneScroller.Maximum = 0;
            }
            else
            {
                ToolsPaneScroller.Maximum = scrollAbility;
                ToolsPaneScroller.IsEnabled = true;
            }
        }
        
        public override bool ProcessMouse(MouseConsoleState state)
        {
            state = new MouseConsoleState(ToolsConsole, state.Mouse);
            var scrollerState = new MouseConsoleState(ScrollerConsole, state.Mouse);

            if (state.IsOnConsole)
            {
                if (state.Mouse.ScrollWheelValueChange != 0)
                {
                    if (ToolsPaneScroller.IsEnabled)
                        ToolsPaneScroller.Value += state.Mouse.ScrollWheelValueChange / 20;

                    return true;
                }

                foreach (var item in _hotSpots)
                {
                    if (item.Item2 == state.CellPosition.Y)
                    {
                        if (state.Mouse.LeftClicked)
                        {
                            item.Item1.IsCollapsed = !item.Item1.IsCollapsed;
                            RedrawPanels();
                            return true;
                        }
                    }
                }

                return ToolsConsole.ProcessMouse(state);
            }

            if (scrollerState.IsOnConsole)
            {
                return ScrollerConsole.ProcessMouse(scrollerState);
            }

            return false;
        }

        public bool ProcessKeyboard(Keyboard info)
        {
            return false;
        }
    }
}
