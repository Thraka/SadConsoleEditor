using Microsoft.Xna.Framework;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.GameHelpers
{
    class GameObject
    {
        public string Name { get; set; }
        public CellAppearance Character { get; set; }
        public List<KeyValuePair<string, string>> Settings { get; set; }
        public Point Position { get; set; }

        public GameObject()
        {
            Settings = new List<KeyValuePair<string, string>>();
        }
        
    }
}
