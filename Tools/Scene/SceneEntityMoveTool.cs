﻿namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using System.Collections.Generic;
    using SadConsole.Game;

    class SceneEntityMoveTool : ITool
    {
        private GameObject _boundingBox;

        private BoxToolPanel _settingsPanel;

        private Point clickOffset;
        private bool isDragging;
        private GameObject movingEntity;

        public const string ID = "SCENE-ENT-MOVE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Move Object"; }
        }

        public char Hotkey { get { return 'm'; } }


        public CustomPanel[] ControlPanels { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public SceneEntityMoveTool()
        {

        }

        public void OnSelected()
        {
        }

        public void OnDeselected()
        {
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
            //var entities = new List<SadConsole.Entities.Entity>((EditorConsoleManager.Instance.SelectedEditor as Editors.SceneEditor).GetEntities());
            //entities.Reverse();

            //foreach (var ent in entities)
            //{
            //    var rect = new Rectangle(ent.Position.X, ent.Position.Y, ent.TextSurface.Width, ent.TextSurface.Height);
            //    rect.Offset(ent.PositionOffset);

            //    if (rect.Contains(info.ConsoleLocation))
            //    {

            //    }
            //}

            var editor = (Editors.SceneEditor)EditorConsoleManager.ActiveEditor;

            if (!isDragging)
            {
                bool overEntity = false;

                Point mousePosition = new Point(0);

                for (int i = 0; i < editor.GameObjects.Count; i++)
                {
                    var area = editor.GameObjects[i].Animation.RenderArea;
                    area.Offset(editor.GameObjects[i].Position - editor.GameObjects[i].Animation.Center);

                    mousePosition = info.WorldLocation - editor.GameObjects[i].RenderOffset;

                    // is mouse over?
                    if (area.Contains(mousePosition))
                    {
                        overEntity = true;

                        if (movingEntity != editor.GameObjects[i])
                        {
                            movingEntity = editor.GameObjects[i];
                            _boundingBox = new GameObject(Settings.Config.ScreenFont);
                            _boundingBox.RenderOffset = movingEntity.RenderOffset;
                            _boundingBox.Position = movingEntity.Position - movingEntity.Animation.Center - new Point(1);

                            _boundingBox.Animation = new AnimatedTextSurface("default", movingEntity.Animation.Width + 2, movingEntity.Animation.Height + 2, Settings.Config.ScreenFont);
                            var frame = _boundingBox.Animation.CreateFrame();

                            var box = SadConsole.Shapes.Box.GetDefaultBox();
                            box.Width = _boundingBox.Animation.Width;
                            box.Height = _boundingBox.Animation.Height;
                            box.Fill = false;
                            box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;

                            Settings.QuickEditor.TextSurface = frame;

                            box.Draw(Settings.QuickEditor);

                            Settings.QuickEditor.SetGlyph(movingEntity.Animation.Center.X + 1, movingEntity.Animation.Center.Y + 1, '*');
                            Settings.QuickEditor.SetBackground(movingEntity.Animation.Center.X + 1, movingEntity.Animation.Center.Y + 1, Color.Black);
                            _boundingBox.Animation.Tint = Color.Black * 0.3f;
                        }

                        EditorConsoleManager.Brush = _boundingBox;
                        EditorConsoleManager.UpdateBrush();

                        overEntity = true;
                        _boundingBox.IsVisible = true;

                        break;
                    }
                }

                if (!overEntity)
                {
                    if (_boundingBox != null)
                        _boundingBox.IsVisible = false;

                    movingEntity = null;
                }
                else
                {
                    if (info.LeftButtonDown)
                        isDragging = true;

                    clickOffset = mousePosition - movingEntity.Position;
                }
            }
            else
            {
                if (!info.LeftButtonDown)
                    isDragging = false;
                else
                {
                    movingEntity.Position = info.WorldLocation - movingEntity.RenderOffset - clickOffset;
                    _boundingBox.Position = movingEntity.Position - movingEntity.Animation.Center - new Point(1);
                }
            }
            //if (editor.SelectedEntity != null)
            //{
            //    editor.SelectedEntity.Position = info.ConsoleLocation;

            //    if (info.LeftClicked)
            //        EditorConsoleManager.Instance.ToolPane.SelectedTool = (ITool) EditorConsoleManager.Instance.ToolPane.ToolsPanel.ToolsListBox.Items[0];
            //}
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
