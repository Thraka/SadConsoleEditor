using SadConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SadConsoleEditor
{
    class EntityBrush : Entity, IEntityBrush
    {
        public List<EntityBrush> TopLayers;

        public EntityBrush()
        {
            this.Font = Settings.ScreenFont;
            TopLayers = new List<EntityBrush>();
        }

        public void SyncEntities()
        {
            foreach (var item in TopLayers)
                item.Position = this.Position;
        }

        protected override void OnPositionChanged(Point oldLocation)
        {
            base.OnPositionChanged(oldLocation);

            if (TopLayers != null)
                foreach (var item in TopLayers)
                    item.Position = this.Position;
        }

        public override void Render()
        {
            foreach (var item in TopLayers)
            {
                item.Render();
            }

            base.Render();
        }

        public override void Update()
        {
            foreach (var item in TopLayers)
            {
                item.Update();
            }

            base.Update();
        }
    }
}
