using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Editors
{
    public interface IEditor
    {
        string Title { get; }

        string Id { get; }

        string[] Tools { get; }
    }
}
