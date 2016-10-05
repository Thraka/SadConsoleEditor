using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Game;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsoleEditor
{
    class ResizableObject
    {
        private GameObject overlay;
        private GameObject gameObject;
        private ObjectType objectType;
        private Point renderOffset;

        private bool isSelected;

        public ResizeRules Rules;


        public GameObject GameObject { get { return gameObject; } }

        public GameObject Overlay { get { return overlay; } }

        public ObjectType Type { get { return objectType; } }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; ProcessOverlay(); }
        }

        public ResizableObject(ObjectType objectType, GameObject gameObject)
        {
            this.objectType = objectType;
            this.gameObject = gameObject;
            
            renderOffset = gameObject.RenderOffset;

            Rules = ResizeRules.Get(objectType);
            ProcessOverlay();
        }

        public Point RenderOffset
        {
            get { return renderOffset; }
            set { renderOffset = value; ProcessOverlay(); }
        }

        public string Name
        {
            get { return gameObject.Name; }
            set { gameObject.Name = value; if (objectType == ObjectType.Zone) DrawZone(); }
        }
        

        public void ResizeObject(int width, int height, int? positionX = null, int? positionY = null)
        {
            if (objectType == ObjectType.Zone)
            {
                if (gameObject.Width != width || gameObject.Height != height)
                {
                    var animation = gameObject.Animation;
                    var backColor = animation.CurrentFrame[0].Background;

                    var newAnimation = new AnimatedTextSurface(gameObject.Animation.Name, width, height);
                    Settings.QuickEditor.TextSurface = newAnimation.CreateFrame();
                    Settings.QuickEditor.Fill(Color.White, backColor, 0);
                    Settings.QuickEditor.Print(0, 0, Name, Color.DarkGray);

                    gameObject.Animation = newAnimation;
                    gameObject.Update();

                    gameObject.Position = new Point(positionX ?? gameObject.Position.X, positionY ?? gameObject.Position.Y);

                    ProcessOverlay();
                }
            }
        }

        public void Recolor(Color color)
        {
            Settings.QuickEditor.TextSurface = gameObject.Animation.CurrentFrame;
            Settings.QuickEditor.Fill(Color.White, color, 0);
            Settings.QuickEditor.Print(0, 0, Name, Color.DarkGray);
        }

        public void DrawZone()
        {
            Settings.QuickEditor.TextSurface = gameObject.Animation.CurrentFrame;
            Settings.QuickEditor.Fill(Color.White, gameObject.Animation.CurrentFrame[0].Background, 0);
            Settings.QuickEditor.Print(0, 0, Name, Color.DarkGray);
        }

        public void Render()
        {
            gameObject.Render();

            if (isSelected)
                overlay.Render();
        }

        public void ProcessOverlay()
        {
            overlay = new GameObject(Settings.Config.ScreenFont);
            overlay.RenderOffset = gameObject.RenderOffset = renderOffset;
            overlay.Position = gameObject.Position - gameObject.Animation.Center - new Point(1);

            overlay.Animation = new AnimatedTextSurface("default", gameObject.Animation.Width + 2, gameObject.Animation.Height + 2, Settings.Config.ScreenFont);
            var frame = overlay.Animation.CreateFrame();

            var box = SadConsole.Shapes.Box.GetDefaultBox();
            box.Width = overlay.Animation.Width;
            box.Height = overlay.Animation.Height;
            box.Fill = false;
            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;

            var centers = new Point(box.Width / 2, box.Height / 2);
            
            Settings.QuickEditor.TextSurface = frame;

            box.Draw(Settings.QuickEditor);

            if (Rules.AllowLeftRight && Rules.AllowTopBottom)
            {
                Settings.QuickEditor.SetGlyph(0, 0, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, 0, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, Settings.QuickEditor.Height - 1, 254);
                Settings.QuickEditor.SetGlyph(0, Settings.QuickEditor.Height - 1, 254);
            }

            if (Rules.AllowLeftRight)
            {
                Settings.QuickEditor.SetGlyph(0, centers.Y, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, centers.Y, 254);
            }

            if (Rules.AllowTopBottom)
            {
                Settings.QuickEditor.SetGlyph(centers.X, 0, 254);
                Settings.QuickEditor.SetGlyph(centers.X, Settings.QuickEditor.Height - 1, 254);
            }

            if (objectType == ObjectType.GameObject)
            {
                Settings.QuickEditor.SetGlyph(gameObject.Animation.Center.X + 1, gameObject.Animation.Center.Y + 1, '*');
                Settings.QuickEditor.SetBackground(gameObject.Animation.Center.X + 1, gameObject.Animation.Center.Y + 1, Color.Black);
                overlay.Animation.Tint = Color.Black * 0.3f;

                //EditorConsoleManager.Brush = overlay;
                //EditorConsoleManager.UpdateBrush();
            }
        }




        public enum ObjectType
        {
            Zone,
            GameObject,
            ControlText
        }

        public struct ResizeRules
        {
            public bool AllowLeftRight;
            public bool AllowTopBottom;
            public bool AllowMove;

            //public bool AllowLeft;
            //public bool AllowRight;
            //public bool AllowTop;
            //public bool AllowBottom;

            public ResizeRules(bool allowLeftRight, bool allowTopBottom, bool allowMove)
            {
                AllowLeftRight = allowLeftRight;
                AllowTopBottom = allowTopBottom;
                AllowMove = allowMove;
            }

            public static ResizeRules Get(ObjectType objectType)
            {
                switch (objectType)
                {
                    case ObjectType.Zone:
                        return new ResizeRules(true, true, true);
                    case ObjectType.ControlText:
                        return new ResizeRules(true, true, true);
                    case ObjectType.GameObject:
                        return new ResizeRules(false, false, true);
                }

                return new ResizeRules(false, false, false);
            }
        }
    }
}
