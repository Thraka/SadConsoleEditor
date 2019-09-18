using System;
using System.Collections.Generic;
using System.Text;
using SadConsole;

namespace EntityPlugin
{
    public class AnimatedConsoleEditor : SadConsole.AnimatedConsole
    {
        public AnimatedConsoleEditor(string name, int width, int height, Font font) : base(name, width, height, font)
        {
        }

        public AnimatedConsoleEditor(AnimatedConsole baseConsole): base(baseConsole.Name, baseConsole.Width, baseConsole.Height)
        {
            FramesList = new List<CellSurface>(baseConsole.Frames);
            
        }

        public void InsertFrame(int index, CellSurface frame)
        {
            FramesList.Insert(index, frame);
        }

        public void RemoveFrame(CellSurface frame)
        {
            if (FramesList.Contains(frame))
                FramesList.Remove(frame);
        }
    }
}
