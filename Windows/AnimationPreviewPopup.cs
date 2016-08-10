using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Input;
using Microsoft.Xna.Framework.Graphics;
using SadConsole.Game;

namespace SadConsoleEditor.Windows
{
    public class PreviewAnimationPopup : Window
    {
        private GameObject _entity;
        private AnimatedTextSurface _animation;
        private Button _restartAnimation;

        public PreviewAnimationPopup(AnimatedTextSurface animation) : base(animation.Width + 2, animation.Height + 4)
        {
            textSurface.Font = Settings.Config.ScreenFont;
            _animation = animation;

            CloseOnESC = true;
            _entity = new GameObject(Settings.Config.ScreenFont);
            _entity.Position = new Point(1, 1);
            _entity.Animation = animation;
            animation.Restart();
            _entity.Animation.Start();

            _restartAnimation = new Button(animation.Width, 1);
            _restartAnimation.Text = "Restart";
            _restartAnimation.Position = new Point(1, textSurface.Height - 2);
            _restartAnimation.ButtonClicked += (s, e) => _animation.Restart();
            Add(_restartAnimation);
        }

        protected override void OnAfterRender(SpriteBatch batch)
        {
            base.OnAfterRender(batch);

            _entity.Animation.CurrentFrame.Copy(textSurface, _entity.Position.X, _entity.Position.Y);

            // Draw bar
            for (int i = 1; i < textSurface.Width - 1; i++)
            {
                SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(textSurface.GetCell(i, textSurface.Height - 3));
                textSurface.GetCell(i, textSurface.Height - 3).GlyphIndex = 205;
            }

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(textSurface.GetCell(0, textSurface.Height - 3));
            textSurface.GetCell(0, textSurface.Height - 3).GlyphIndex = 204;

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(textSurface.GetCell(textSurface.Width - 1, textSurface.Height - 3));
            textSurface.GetCell(textSurface.Width - 1, textSurface.Height - 3).GlyphIndex = 185;
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
