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
        private ResizeRules rules;
        private Point renderOffset;

        private bool isSelected;
        private bool isResizing;

        public GameObject GameObject { get { return gameObject; } }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; ProcessOverlay(); }
        }

        public bool IsResizing
        {
            get { return isResizing; }
            set { isResizing = value; }
        }

        public ResizableObject(ObjectType objectType, GameObject gameObject)
        {
            this.objectType = objectType;
            this.gameObject = gameObject;
            
            renderOffset = gameObject.RenderOffset;

            rules = ResizeRules.Get(objectType);
            ProcessOverlay();
        }

        public Point RenderOffset
        {
            get { return renderOffset; }
            set { renderOffset = value; ProcessOverlay(); }
        }

        private bool moveRight;
        private bool moveTopRight;
        private bool moveBottomRight;

        private bool moveLeft;
        private bool moveTopLeft;
        private bool moveBottomLeft;

        private bool moveTop;
        private bool moveBottom;

        private bool isMoving;


        private Point resizeStartPosition;
        private Point resizeBounds;
        private Point clickOffset;
        private bool lastLeftMouseDown = false;

        public bool ProcessMouse(MouseInfo info)
        {
            if (!isResizing && !isMoving && info.LeftButtonDown && lastLeftMouseDown == false)
            {
                lastLeftMouseDown = true;

                moveRight = false;
                moveTopRight = false;
                moveBottomRight = false;

                moveLeft = false;
                moveTopLeft = false;
                moveBottomLeft = false;

                moveTop = false;
                moveBottom = false;

                // Check and see if mouse is over me
                if (rules.AllowMove &&
                    info.ConsoleLocation.X >= gameObject.Position.X && info.ConsoleLocation.X <= gameObject.Position.X + gameObject.Width - 1 &&
                    info.ConsoleLocation.Y >= gameObject.Position.Y && info.ConsoleLocation.Y <= gameObject.Position.Y + gameObject.Height - 1)
                {
                    clickOffset = info.ConsoleLocation - gameObject.Position;
                    isMoving = true;
                }

                else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1 && info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                {
                    isResizing = true;
                    moveBottomRight = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X, gameObject.Position.Y);
                    return true;
                }
                else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y && info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                {
                    isResizing = true;
                    moveTopRight = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X, gameObject.Position.Y + gameObject.Height - 1);
                    return true;
                }
                else if (rules.AllowLeftRight &&
                    info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                {
                    isResizing = true;
                    moveRight = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X, 0);
                    return true;
                }
                else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1 && info.ConsoleLocation.X == overlay.Position.X)
                {
                    isResizing = true;
                    moveBottomLeft = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, gameObject.Position.Y);
                    return true;
                }
                else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y && info.ConsoleLocation.X == overlay.Position.X)
                {
                    isResizing = true;
                    moveTopLeft = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, gameObject.Position.Y + gameObject.Height - 1);
                    return true;
                }
                else if (rules.AllowLeftRight &&
                    info.ConsoleLocation.X == overlay.Position.X)
                {
                    isResizing = true;
                    moveLeft = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, 0);
                    return true;
                }
                else if (rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y)
                {
                    isResizing = true;
                    moveTop = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(0, gameObject.Position.Y + gameObject.Height - 1);
                    return true;
                }
                else if (rules.AllowTopBottom &&
                    info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1)
                {
                    isResizing = true;
                    moveBottom = true;
                    resizeStartPosition = info.ConsoleLocation;
                    resizeBounds = new Point(0, gameObject.Position.Y);
                    return true;
                }
            }

            if (isResizing)
            {
                if (!info.LeftButtonDown)
                {
                    isResizing = false;
                    return false;
                }

                if (moveRight && info.ConsoleLocation.X > resizeBounds.X)
                {
                    ResizeObject(info.ConsoleLocation.X - gameObject.Position.X, gameObject.Height);
                }
                else if (moveBottomRight && info.ConsoleLocation.X > resizeBounds.X && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    ResizeObject(info.ConsoleLocation.X - gameObject.Position.X, info.ConsoleLocation.Y - gameObject.Position.Y);
                }
                else if (moveTopRight && info.ConsoleLocation.X > resizeBounds.X && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    ResizeObject(info.ConsoleLocation.X - gameObject.Position.X, gameObject.Position.Y + gameObject.Height - (info.ConsoleLocation.Y + 1), null, info.ConsoleLocation.Y + 1);
                }
                else if (moveLeft && info.ConsoleLocation.X < resizeBounds.X)
                {
                    ResizeObject(gameObject.Position.X + gameObject.Width - (info.ConsoleLocation.X + 1), gameObject.Height, info.ConsoleLocation.X + 1, null);
                }
                else if (moveBottomLeft && info.ConsoleLocation.X < resizeBounds.X && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    ResizeObject(gameObject.Position.X + gameObject.Width - (info.ConsoleLocation.X + 1), info.ConsoleLocation.Y - gameObject.Position.Y, info.ConsoleLocation.X + 1, null);
                }
                else if (moveTopLeft && info.ConsoleLocation.X < resizeBounds.X && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    ResizeObject(gameObject.Position.X + gameObject.Width - (info.ConsoleLocation.X + 1), gameObject.Position.Y + gameObject.Height - (info.ConsoleLocation.Y + 1), info.ConsoleLocation.X + 1, info.ConsoleLocation.Y + 1);
                }
                else if (moveTop && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    ResizeObject(gameObject.Width, gameObject.Position.Y + gameObject.Height - (info.ConsoleLocation.Y + 1), null, info.ConsoleLocation.Y + 1);
                }
                else if (moveBottom && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    ResizeObject(gameObject.Width, info.ConsoleLocation.Y - gameObject.Position.Y);
                }
                return true;
            }
            else if (isMoving)
            {
                if (!info.LeftButtonDown)
                {
                    isMoving = false;
                    return false;
                }

                gameObject.Position = info.ConsoleLocation - clickOffset;
                ProcessOverlay();
            }

            lastLeftMouseDown = info.LeftButtonDown;

            return false;
        }

        private void ResizeObject(int width, int height, int? positionX = null, int? positionY = null)
        {
            if (gameObject.Width != width || gameObject.Height != height)
            {
                var animation = gameObject.Animation;
                var backColor = animation.CurrentFrame[0].Background;

                var newAnimation = new AnimatedTextSurface(gameObject.Animation.Name, width, height);
                Settings.QuickEditor.TextSurface = newAnimation.CreateFrame();
                Settings.QuickEditor.Fill(Color.White, backColor, 0);
                gameObject.Animation = newAnimation;
                gameObject.Update();

                gameObject.Position = new Point(positionX ?? gameObject.Position.X, positionY ?? gameObject.Position.Y);

                ProcessOverlay();
            }
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

            if (rules.AllowLeftRight && rules.AllowTopBottom)
            {
                Settings.QuickEditor.SetGlyph(0, 0, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, 0, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, Settings.QuickEditor.Height - 1, 254);
                Settings.QuickEditor.SetGlyph(0, Settings.QuickEditor.Height - 1, 254);
            }

            if (rules.AllowLeftRight)
            {
                Settings.QuickEditor.SetGlyph(0, centers.Y, 254);
                Settings.QuickEditor.SetGlyph(Settings.QuickEditor.Width - 1, centers.Y, 254);
            }

            if (rules.AllowTopBottom)
            {
                Settings.QuickEditor.SetGlyph(centers.X, 0, 254);
                Settings.QuickEditor.SetGlyph(centers.X, Settings.QuickEditor.Height - 1, 254);
            }


            //Settings.QuickEditor.SetGlyph(gameObject.Animation.Center.X + 1, gameObject.Animation.Center.Y + 1, '*');
            //Settings.QuickEditor.SetBackground(gameObject.Animation.Center.X + 1, gameObject.Animation.Center.Y + 1, Color.Black);
            //overlay.Animation.Tint = Color.Black * 0.3f;

            //EditorConsoleManager.Brush = overlay;
            //EditorConsoleManager.UpdateBrush();
        }




        public enum ObjectType
        {
            Region,
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
                    case ObjectType.Region:
                        return new ResizeRules(true, true, true);
                    case ObjectType.ControlText:
                        return new ResizeRules(true, true, true);
                    case ObjectType.GameObject:
                        return new ResizeRules(true, false, true);
                }

                return new ResizeRules(false, false, false);
            }
        }
    }
}
