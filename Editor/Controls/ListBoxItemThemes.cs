using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Themes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsoleEditor.Controls
{
    class EditorsListBoxItem : ListBoxItemTheme
    {
        public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            var look = GetStateAppearance(itemState);
            string value = ((Editors.IEditorMetadata)item).Title;
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            surface.Print(area.Left, area.Top, value, look);
        }
    }

    class FileLoaderListBoxItem : ListBoxItemTheme
    {
        public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            var look = GetStateAppearance(itemState);
            string value = ((FileLoaders.IFileLoader)item).FileTypeName;
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            surface.Print(area.Left, area.Top, value, look);
        }
    }
}
