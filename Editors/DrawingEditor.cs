using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SadConsoleEditor.Tools;

namespace SadConsoleEditor.Editors
{
    class DrawingEditor: IEditor
    {
        public const string ID = "DRAW";

        public string Id { get { return ID; } }

        public string Title { get { return "Drawing"; } }


        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID };
            }
        }

        public override string ToString()
        {
            return Title;
        }

    }
}
