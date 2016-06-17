using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor
{
    public interface IEntityBrush
    {
        Point PositionOffset { get; set; }

        void Render();

        void Update();
    }
}
