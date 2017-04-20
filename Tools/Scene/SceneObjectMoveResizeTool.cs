namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using System.Collections.Generic;
    using SadConsole.Game;
    using System.Linq;

    class SceneObjectMoveResizeTool : ITool
    {
        private GameObject _boundingBox;

        private BoxToolPanel _settingsPanel;

        private Point clickOffset;
        private bool isDragging;
        private ResizableObject movingEntity;

        private Point resizeStartPosition;
        private Point resizeBounds;
        private bool lastLeftMouseDown = false;


        private bool moveRight;
        private bool moveTopRight;
        private bool moveBottomRight;

        private bool moveLeft;
        private bool moveTopLeft;
        private bool moveBottomLeft;

        private bool moveTop;
        private bool moveBottom;

        private bool isMoving;
        private bool isSelected;
        private bool isResizing;






        public const string ID = "SCENE-ENT-MOVE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Move/Resize Object"; }
        }

        public char Hotkey { get { return 'm'; } }


        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public SceneObjectMoveResizeTool()
        {

        }

        public void OnSelected()
        {
            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).ShowDarkLayer = true;
            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).HighlightType = Editors.SceneEditor.HighlightTypes.GameObject;
        }

        public void OnDeselected()
        {
            ((Editors.SceneEditor)EditorConsoleManager.ActiveEditor).ShowDarkLayer = false;
        }

        public void RefreshTool()
        {
        }

        public void Update()
        {
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
            var editor = EditorConsoleManager.ActiveEditor as Editors.SceneEditor;

            if(editor != null && !isResizing && !isMoving)
            {
                var zones = editor.Zones.ToList(); zones.Reverse();
                var allObjects = editor.Objects.Union(zones).ToList();

                for (int i = 0; i < allObjects.Count; i++)
                {
                    var area = allObjects[i].GameObject.Animation.RenderArea;
                    area.Offset(allObjects[i].GameObject.Position - allObjects[i].GameObject.Animation.Center);
                    area.Inflate(1, 1);

                    var mousePosition = info.WorldLocation - allObjects[i].RenderOffset;

                    if (!area.Contains(mousePosition))
                        continue;

                    if (movingEntity != null)
                        movingEntity.IsSelected = false;

                    movingEntity = allObjects[i];
                    movingEntity.IsSelected = true;

                    if (!info.LeftButtonDown)
                        return;

                    // Select the zone in the list box
                    if (movingEntity.Type == ResizableObject.ObjectType.Zone && editor.ZonesPanel.SelectedGameObject != movingEntity)
                        editor.ZonesPanel.SelectedGameObject = movingEntity;
                    else if (movingEntity.Type == ResizableObject.ObjectType.GameObject && editor.GameObjectPanel.SelectedGameObject != movingEntity)
                        editor.GameObjectPanel.SelectedGameObject = movingEntity;

                    var gameObject = movingEntity.GameObject;
                    var overlay = movingEntity.Overlay;
                    var rules = movingEntity.Rules;
                    
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
                        if (movingEntity.Type == ResizableObject.ObjectType.GameObject)
                            editor.GameObjectPanel.SelectedGameObject = movingEntity;

                        clickOffset = info.ConsoleLocation - gameObject.Position;
                        isMoving = true;
                        return;
                    }

                    else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1 && info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                    {
                        isResizing = true;
                        moveBottomRight = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X, gameObject.Position.Y);
                        return;
                    }
                    else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y && info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                    {
                        isResizing = true;
                        moveTopRight = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X, gameObject.Position.Y + gameObject.Height - 1);
                        return;
                    }
                    else if (rules.AllowLeftRight &&
                        info.ConsoleLocation.X == overlay.Position.X + overlay.Width - 1)
                    {
                        isResizing = true;
                        moveRight = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X, 0);
                        return;
                    }
                    else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1 && info.ConsoleLocation.X == overlay.Position.X)
                    {
                        isResizing = true;
                        moveBottomLeft = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, gameObject.Position.Y);
                        return;
                    }
                    else if (rules.AllowLeftRight && rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y && info.ConsoleLocation.X == overlay.Position.X)
                    {
                        isResizing = true;
                        moveTopLeft = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, gameObject.Position.Y + gameObject.Height - 1);
                        return;
                    }
                    else if (rules.AllowLeftRight &&
                        info.ConsoleLocation.X == overlay.Position.X)
                    {
                        isResizing = true;
                        moveLeft = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(gameObject.Position.X + gameObject.Width - 1, 0);
                        return;
                    }
                    else if (rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y)
                    {
                        isResizing = true;
                        moveTop = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(0, gameObject.Position.Y + gameObject.Height - 1);
                        return;
                    }
                    else if (rules.AllowTopBottom &&
                        info.ConsoleLocation.Y == overlay.Position.Y + overlay.Height - 1)
                    {
                        isResizing = true;
                        moveBottom = true;
                        resizeStartPosition = info.ConsoleLocation;
                        resizeBounds = new Point(0, gameObject.Position.Y);
                        return;
                    }
                }

                if (!info.LeftButtonDown && movingEntity != null)
                {
                    movingEntity.IsSelected = false;
                    movingEntity = null;
                }
            }

            if (isResizing)
            {
                if (!info.LeftButtonDown)
                {
                    isResizing = false;
                    return;
                }

                if (moveRight && info.ConsoleLocation.X > resizeBounds.X)
                {
                    movingEntity.ResizeObject(info.ConsoleLocation.X - movingEntity.GameObject.Position.X, movingEntity.GameObject.Height);
                }
                else if (moveBottomRight && info.ConsoleLocation.X > resizeBounds.X && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    movingEntity.ResizeObject(info.ConsoleLocation.X - movingEntity.GameObject.Position.X, info.ConsoleLocation.Y - movingEntity.GameObject.Position.Y);
                }
                else if (moveTopRight && info.ConsoleLocation.X > resizeBounds.X && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    movingEntity.ResizeObject(info.ConsoleLocation.X - movingEntity.GameObject.Position.X, movingEntity.GameObject.Position.Y + movingEntity.GameObject.Height - (info.ConsoleLocation.Y + 1), null, info.ConsoleLocation.Y + 1);
                }
                else if (moveLeft && info.ConsoleLocation.X < resizeBounds.X)
                {
                    movingEntity.ResizeObject(movingEntity.GameObject.Position.X + movingEntity.GameObject.Width - (info.ConsoleLocation.X + 1), movingEntity.GameObject.Height, info.ConsoleLocation.X + 1, null);
                }
                else if (moveBottomLeft && info.ConsoleLocation.X < resizeBounds.X && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    movingEntity.ResizeObject(movingEntity.GameObject.Position.X + movingEntity.GameObject.Width - (info.ConsoleLocation.X + 1), info.ConsoleLocation.Y - movingEntity.GameObject.Position.Y, info.ConsoleLocation.X + 1, null);
                }
                else if (moveTopLeft && info.ConsoleLocation.X < resizeBounds.X && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    movingEntity.ResizeObject(movingEntity.GameObject.Position.X + movingEntity.GameObject.Width - (info.ConsoleLocation.X + 1), movingEntity.GameObject.Position.Y + movingEntity.GameObject.Height - (info.ConsoleLocation.Y + 1), info.ConsoleLocation.X + 1, info.ConsoleLocation.Y + 1);
                }
                else if (moveTop && info.ConsoleLocation.Y < resizeBounds.Y)
                {
                    movingEntity.ResizeObject(movingEntity.GameObject.Width, movingEntity.GameObject.Position.Y + movingEntity.GameObject.Height - (info.ConsoleLocation.Y + 1), null, info.ConsoleLocation.Y + 1);
                }
                else if (moveBottom && info.ConsoleLocation.Y > resizeBounds.Y)
                {
                    movingEntity.ResizeObject(movingEntity.GameObject.Width, info.ConsoleLocation.Y - movingEntity.GameObject.Position.Y);
                }
                return;
            }
            else if (isMoving)
            {
                if (!info.LeftButtonDown)
                {
                    isMoving = false;
                    return;
                }

                movingEntity.GameObject.Position = info.ConsoleLocation - clickOffset;
                movingEntity.ProcessOverlay();
            }

            lastLeftMouseDown = info.LeftButtonDown;

            return;
        }

        public void MouseEnterSurface(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {

        }
    }
}
