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
        private Button _restartAnimation;

        public PreviewAnimationPopup(Animation animation) : base(animation.Width + 2, animation.Height + 4)
        {
            Font = Settings.Config.ScreenFont;
            _animation = animation;

            CloseOnESC = true;
            _entity = new Entity();
            _entity.Font = Settings.Config.ScreenFont;
            _entity.Position = new Point(1, 1);
            _entity.AddAnimation(animation);
            _entity.SetActiveAnimation(animation);
            animation.Restart();
            _entity.Start();

            _restartAnimation = new Button(animation.Width, 1);
            _restartAnimation.Text = "Restart";
            _restartAnimation.Position = new Point(1, CellData.Height - 2);
            _restartAnimation.ButtonClicked += (s, e) => _animation.Restart();
            Add(_restartAnimation);
        }

        protected override void OnAfterRender()
        {
            base.OnAfterRender();

            _entity.RenderToSurface(_cellData);

            // Draw bar
            for (int i = 1; i < CellData.Width - 1; i++)
            {
                SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(CellData[i, CellData.Height - 3]);
                CellData[i, CellData.Height - 3].CharacterIndex = 205;
            }

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(CellData[0, CellData.Height - 3]);
            CellData[0, CellData.Height - 3].CharacterIndex = 204;

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(CellData[CellData.Width - 1, CellData.Height - 3]);
            CellData[CellData.Width - 1, CellData.Height - 3].CharacterIndex = 185;
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Space))
                _animation.Restart();

            return base.ProcessKeyboard(info);
        }

        public override bool ProcessMouse(MouseInfo info)
        {
            base.ProcessMouse(info);

            if (info.RightClicked)
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
