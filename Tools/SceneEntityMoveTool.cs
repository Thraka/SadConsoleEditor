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

    class SceneEntityMoveTool : ITool
    {
        private EntityBrush _entity;
        private GameObject _animSinglePoint;

        private BoxToolPanel _settingsPanel;

        private Point clickOffset;
        private bool isDragging;

        public const string ID = "SCENE-ENT-MOVE";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Move"; }
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
            _entity = new EntityBrush(1, 1);
            _entity.IsVisible = false;
            
            EditorConsoleManager.Instance.UpdateBrush(_entity);
        }

        public void OnDeselected()
        {
        }

        public void RefreshTool()
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

            var editor = (Editors.SceneEditor)EditorConsoleManager.Instance.SelectedEditor;

            if (!isDragging)
            {
                bool overEntity = false;

                for (int i = 0; i < editor.Entities.Count; i++)
                {
                    var area = editor.Entities[i].Animation.RenderArea;
                    area.Offset(editor.Entities[i].Position);
                    editor.Entities[i].Position = new Point(10, 10);

                    Point mousePosition = info.WorldLocation - editor.Entities[i].Position + editor.Entities[i].RenderOffset;


                    // is mouse over?
                    if (area.Contains(mousePosition))
                    {
                        overEntity = true;
                        _entity = new EntityBrush(editor.Entities[i].Animation.Width + 2, editor.Entities[i].Animation.Height + 2);
                        _entity.RenderOffset = editor.Entities[i].RenderOffset;
                        _entity.Position = editor.Entities[i].Position - new Point(1);
                        _entity.IsVisible = true;

                        var box = SadConsole.Shapes.Box.GetDefaultBox();
                        box.Width = _entity.Animation.Width;
                        box.Height = _entity.Animation.Height;
                        box.Fill = false;
                        box.TopLeftCharacter = box.TopSideCharacter = box.TopRightCharacter = box.LeftSideCharacter = box.RightSideCharacter = box.BottomLeftCharacter = box.BottomSideCharacter = box.BottomRightCharacter = 177;

                        Settings.QuickEditor.TextSurface = _entity.Animation.CurrentFrame;

                        box.Draw(Settings.QuickEditor);

                        EditorConsoleManager.Instance.UpdateBrush(_entity);
                        overEntity = true;
                        break;
                    }
                }

                if (!overEntity)
                    _entity.IsVisible = false;
            }
            else
            {

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
