using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Input;
using SadConsole.Entities;
using SadConsoleEditor.Panels;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Windows;

namespace EntityPlugin.Panels
{
    class EntityNamePanel: CustomPanel
    {
        private Button setName;
        private DrawingSurface nameTitle;
        private Entity entity;


        public EntityNamePanel()
        {
            Title = "Game Object";

            nameTitle = new DrawingSurface(ToolPane.PanelWidth - 3, 2);
            nameTitle.OnDraw = PrintName;

            setName = new Button(3);
            setName.Text = "Set";

            setName.Click += (s, e) =>
            {
                RenamePopup rename = new RenamePopup(entity.Name);
                rename.Closed += (s2, e2) => { if (rename.DialogResult) entity.Name = rename.NewName; nameTitle.IsDirty = true; };
                rename.Center();
                rename.Show(true);
            };

            Controls = new ControlBase[] { setName, nameTitle };
        }

        private void PrintName(DrawingSurface drawing)
        {
            drawing.Surface.Clear();
            drawing.Surface.Print(0, 0, "Name", SadConsole.Themes.Library.Default.Colors.Green);
            drawing.Surface.Print(0, 1, entity.Name, SadConsole.Themes.Library.Default.Colors.Blue);
        }

        public override void Loaded()
        {
        }

        public override void ProcessMouse(MouseConsoleState info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            if (control == setName)
            {
                control.Position = new Microsoft.Xna.Framework.Point(ToolPane.PanelWidth - setName.Width - 1, control.Position.Y);
                return -1;
            }
            return 0;
        }

        public void SetEntity(Entity entity)
        {
            this.entity = entity;
            nameTitle.IsDirty = true;
            setName.IsEnabled = entity != null;
        }
    }
}
