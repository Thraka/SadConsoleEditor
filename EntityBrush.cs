using SadConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SadConsole.Consoles;

namespace SadConsoleEditor
{
    class EntityBrush : Entity, IEntityBrush
    {
        public List<Entity> TopLayers;

        public EntityBrush(int width, int height): base(width, height)
        {
            this.Font = Settings.Config.ScreenFont;
            TopLayers = new List<Entity>();
        }

        public void SyncLayers()
        {
            foreach (var item in TopLayers)
            {
                item.Position = this.Position;
                item.IsVisible = this.IsVisible;
                item.PositionOffset = this.PositionOffset;
                item.Font = this.Font;
                item.CurrentAnimation.Center = this.CurrentAnimation.Center;
            }
        }

        protected override void OnPositionChanged(Point oldLocation)
        {
            base.OnPositionChanged(oldLocation);

            if (TopLayers != null)
                foreach (var item in TopLayers)
                    item.Position = this.Position;
        }

        public override void Render(ITextSurface surface, Matrix renderingMatrix)
        {
            base.Render(surface, renderingMatrix);

            foreach (var item in TopLayers)
                item.Render();
        }

        public override void Update()
        {
            base.Update();

            foreach (var item in TopLayers)
                item.Update();
        }
    }
}
