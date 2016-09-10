using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Game;

namespace SadConsoleEditor
{
    class EntityBrush : GameObject, IEntityBrush
    {
        public List<GameObject> TopLayers;

        public EntityBrush(int width, int height): base(Settings.Config.ScreenFont)
        {
            Animation = new AnimatedTextSurface("defualt", width, height, Settings.Config.ScreenFont);
            Animation.CreateFrame();
            TopLayers = new List<GameObject>();
        }

        public void SyncLayers()
        {
            foreach (var item in TopLayers)
            {
                item.Position = this.Position;
                item.IsVisible = this.IsVisible;
                item.RenderOffset = this.RenderOffset;
                item.Animation.Font = Settings.Config.ScreenFont;
                item.Animation.Center = this.Animation.Center;
            }
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
            base.Render();

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
