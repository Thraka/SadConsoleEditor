using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Entities;
using SadConsole.Input;

namespace EntityPlugin.Windows
{
    public class PreviewAnimationPopup : Window
    {
        private Entity entity;
        private AnimatedConsole animation;
        private Button restartAnimation;

        public PreviewAnimationPopup(AnimatedConsole animation) : base(animation.Width + 2, animation.Height + 4, SadConsoleEditor.Config.Program.ScreenFont)
        {
            this.animation = animation;

            CloseOnEscKey = true;
            entity = new Entity(1, 1, SadConsoleEditor.Config.Program.ScreenFont);
            entity.Position = new Point(1, 1);
            entity.Animation = animation;
            animation.Restart();
            entity.Animation.Start();
            entity.Position = new Point(1);
            Children.Add(entity);

            restartAnimation = new Button(animation.Width, 1);
            restartAnimation.Text = "Restart";
            restartAnimation.Position = new Point(1, Height - 2);
            restartAnimation.Click += (s, e) => this.animation.Restart();

            Add(restartAnimation);
        }

        public override void Invalidate()
        {
            base.Invalidate();

            // Draw bar
            for (int i = 1; i < Width - 1; i++)
            {
                SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(this[i, Height - 3]);
                this[i, Height - 3].Glyph = 205;
            }

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(this[0, Height - 3]);
            this[0, Height - 3].Glyph = 204;

            SadConsole.Themes.Library.Default.WindowTheme.BorderStyle.CopyAppearanceTo(this[Width - 1, Height - 3]);
            this[Width - 1, Height - 3].Glyph = 185;
        }

        public override bool ProcessKeyboard(Keyboard info)
        {
            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Space))
                animation.Restart();

            return base.ProcessKeyboard(info);
        }

        public override bool ProcessMouse(MouseConsoleState info)
        {
            base.ProcessMouse(info);

            if (info.Mouse.RightClicked)
                Hide();

            return true;
        }

        public override void Show(bool modal)
        {
            Center();
            base.Show(modal);
        }
    }
}
