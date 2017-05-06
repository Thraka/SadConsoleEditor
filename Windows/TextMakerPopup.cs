using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Windows
{
    class TextMakerPopup : SadConsole.Window
    {
        public TextMakerPopup() : base(Settings.Config.TextMakerSettings.WindowWidth, Settings.Config.TextMakerSettings.WindowHeight)
        {

        }
    }
}
