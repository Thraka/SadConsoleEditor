using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    interface ITool
    {
        string Id { get; }

        string Title { get; }
    }
}
