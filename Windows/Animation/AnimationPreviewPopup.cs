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
        private GameObject entity;
        private AnimatedTextSurface animation;
        private Button restartAnimation;

        public PreviewAnimationPopup(AnimatedTextSurface animation) : base(animation.Width + 2, animation.Height + 4)
        {
            textSurface.Font = Settings.Config.ScreenFont;
            this.animation = animation;

            CloseOnESC = true;
            entity = new GameObject(Settings.Config.ScreenFont);
            entity.Position = new Point(1, 1);
            entity.Animation = animation;
            animation.Restart();
            entity.Animation.Start();

            restartAnimation = new Button(animation.Width, 1);
            restartAnimation.Text = "Restart";
            restartAnimation.Position = new Point(1, textSurface.Height - 2);
            restartAnimation.ButtonClicked += (s, e) => this.animation.Restart();
            Add(restartAnimation);
        }

        protected override void OnAfterRender(SpriteBatch batch)
        {
            base.OnAfterRender(batch);

            entity.Animation.CurrentFrame.Copy(textSurface, entity.Position.X, entity.Position.Y);

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
                animation.Restart();

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

            entity.Update();
        }
    }
}
