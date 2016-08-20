using SadConsole.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Input;
using SadConsole.Game;

namespace SadConsoleEditor.Panels
{
    class EntityNamePanel: CustomPanel
    {
        private InputBox nameBox;
        private DrawingSurface nameTitle;
        private GameObject entity;


        public EntityNamePanel()
        {
            Title = "Entity";

            nameTitle = new DrawingSurface(Consoles.ToolPane.PanelWidth, 1);
            nameTitle.Print(0, 0, "Name");

            nameBox = new InputBox(Consoles.ToolPane.PanelWidth);
            nameBox.TextChanged += NameBox_TextChanged;

            Controls = new ControlBase[] { nameTitle, nameBox };
        }

        public override void Loaded()
        {
        }

        public override void ProcessMouse(MouseInfo info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            return 0;//control == nameTitle ? 1 : 0;
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            if (entity != null)
                entity.Name = nameBox.Text;
        }

        public void SetEntity(GameObject entity)
        {
            this.entity = entity;

            if (entity == null)
            {
                nameBox.Text = "";
                nameBox.IsEnabled = false;
            }
            else
            {
                nameBox.Text = entity.Name;
                nameBox.IsEnabled = true;
            }

        }
    }
}
