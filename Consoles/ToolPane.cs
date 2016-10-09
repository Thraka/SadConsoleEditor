using System;
using SadConsole;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using Microsoft.Xna.Framework;
using SadConsoleEditor.Editors;
using SadConsoleEditor.Tools;
using SadConsoleEditor.Windows;
using SadConsole.Controls;
using SadConsoleEditor.Controls;
using SadConsoleEditor.Panels;
using SadConsole.Input;

namespace SadConsoleEditor.Consoles
{
    class ToolPane : ControlsConsole
    {
        public static int PanelWidth;
        public static int PanelWidthControls;

        private List<Tuple<CustomPanel, int>> _hotSpots;
        private Dictionary<string, ITool> _tools;

        public FilesPanel PanelFiles;
        //public ToolsPanel PanelTools;
        
        public ToolPane() : base(Settings.Config.ToolPaneWidth - 1, Settings.Config.WindowHeight * 2)
        {
            PanelWidth = Settings.Config.ToolPaneWidth - 1;
            PanelWidthControls = PanelWidth - 2;

            _tools = new Dictionary<string, ITool>();

            CanUseKeyboard = false;
            ProcessMouseWithoutFocus = true;

            textSurface.DefaultBackground = Settings.Color_MenuBack;
            textSurface.DefaultForeground = Settings.Color_TitleText;
            Clear();

            _hotSpots = new List<Tuple<CustomPanel, int>>();

            // Create tools
            _tools.Add(PaintTool.ID, new PaintTool());
            textSurface.RenderArea = new Rectangle(0, 0, Width, Settings.Config.WindowHeight - 1);

            // Create panels
            PanelFiles = new FilesPanel();
            //PanelTools = new ToolsPanel();
        }
                

        public void RedrawPanels()
        {
            int activeRow = 0;
            Clear();
            RemoveAll();
            _hotSpots.Clear();

            char open = (char)31;
            char closed = (char)16;

            List<CustomPanel> allPanels = new List<CustomPanel>() { PanelFiles };

            // Custom panels from the selected editor
            if (EditorConsoleManager.ActiveEditor != null)
                if (EditorConsoleManager.ActiveEditor.Panels != null && EditorConsoleManager.ActiveEditor.Panels.Length != 0)
                    allPanels.AddRange(EditorConsoleManager.ActiveEditor.Panels);

            // Custom panels from the selected tool
            //if (SelectedTool.ControlPanels != null && SelectedTool.ControlPanels.Length != 0)
            //    allPanels.AddRange(SelectedTool.ControlPanels);

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
                            Print(1, activeRow++, open + " " + pane.Title);
                            Print(0, activeRow++, new string((char)196, textSurface.Width));

                            foreach (var control in pane.Controls)
                            {
                                if (control != null)
                                {
                                    if (control.IsVisible)
                                    {
                                        Add(control);
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
                            Print(1, activeRow++, closed + " " + pane.Title);
                    }
                }
            }
        }

        public override bool ProcessMouse(SadConsole.Input.MouseInfo info)
        {
            base.ProcessMouse(info);

            if (_isMouseOver)
            {
                if (info.ScrollWheelValueChange != 0)
                {
                    EditorConsoleManager.ToolsPaneScroller.Value += info.ScrollWheelValueChange / 20;
                    return true;
                }

                foreach (var item in _hotSpots)
                {
                    if (item.Item2 == info.ConsoleLocation.Y)
                    {
                        if (info.LeftClicked)
                        {
                            item.Item1.IsCollapsed = !item.Item1.IsCollapsed;
                            RedrawPanels();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}
