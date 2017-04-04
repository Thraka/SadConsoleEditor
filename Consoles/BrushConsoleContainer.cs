using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Input;
using SadConsole.GameHelpers;

namespace SadConsoleEditor.Consoles
{
    class BrushConsoleContainer: SadConsole.ConsoleContainer
    {
        private GameObject brush;
        public GameObject Brush
        {
            get { return brush; }
            set { brush = value; Children.Clear(); if (brush != null) Children.Add(brush); }
        }

        public BrushConsoleContainer()
        {
            TextSurface = new SadConsole.Surfaces.BasicSurface(1, 1, Settings.Config.ScreenFont);
        }

        public override bool ProcessMouse(MouseConsoleState state)
        {
            if (Brush != null)
            {
                // Transform mouse state into screen font
                if (MainScreen.Instance.InnerEmptyBounds.Contains(state.WorldPosition))
                {
                    Brush.IsVisible = true;
                    Brush.Position = state.WorldPosition;
                }
                else
                    Brush.IsVisible = false;
            }

            return false;
        }
    }
}
