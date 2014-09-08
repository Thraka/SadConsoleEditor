using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Tools
{
    class PaintTool: ITool
    {
        public const string ID = "PAINT";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Paint"; }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
