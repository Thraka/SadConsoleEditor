using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsole.Entities;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Input;

namespace SadConsoleEditor.Windows
{
    public class PreviewAnimationPopup : Window
    {
        private Entity _entity;
        private Animation _animation;

        public PreviewAnimationPopup(Animation animation) : base(animation.Width + 2, animation.Height + 2)
        {
            Font = Settings.Config.ScreenFont;
            _animation = animation;

            CloseOnESC = true;
            _entity = new Entity();
            _entity.Font = Settings.Config.ScreenFont;
            _entity.Position = new Point(1, 1);
            _entity.AddAnimation(animation);
            _entity.SetActiveAnimation(animation);
            _entity.Start();            
        }

        protected override void OnAfterRender()
        {
            base.OnAfterRender();

            _entity.RenderToSurface(_cellData);
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Space))
                _animation.Restart();

            return base.ProcessKeyboard(info);
        }

        public override bool ProcessMouse(MouseInfo info)
        {
            if (info.LeftClicked || info.RightClicked)
                Hide();

            return true;
        }

        public override void Show(bool modal)
        {
            Center();
            base.Show(modal);
        }

        public override void Update()
        {
            base.Update();

            _entity.Update();
        }
    }
}
