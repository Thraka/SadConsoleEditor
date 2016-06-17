namespace SadConsoleEditor.Tools
{
    using Microsoft.Xna.Framework;
    using SadConsole;
    using SadConsole.Consoles;
    using SadConsole.Entities;
    using SadConsole.Input;
    using System;
    using SadConsoleEditor.Panels;
    using System.Collections.Generic;
    class SceneEntityMoveTool : ITool
    {
        private EntityBrush _entity;
        private Animation _animSinglePoint;

        private BoxToolPanel _settingsPanel;

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
            _animSinglePoint = new Animation("single", 1, 1);
            _animSinglePoint.Font = Engine.DefaultFont;
            var _frameSinglePoint = _animSinglePoint.CreateFrame();
            _frameSinglePoint[0].CharacterIndex = 42;
        }

        public void OnSelected()
        {
            _entity = new EntityBrush();
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
            var entities = new List<SadConsole.Entities.Entity>((EditorConsoleManager.Instance.SelectedEditor as Editors.SceneEditor).GetEntities());
            entities.Reverse();

            foreach (var ent in entities)
            {
                var rect = new Rectangle(ent.Position.X, ent.Position.Y, ent.TextSurface.Width, ent.TextSurface.Height);
                rect.Offset(ent.PositionOffset);

                if (rect.Contains(info.ConsoleLocation))
                {

                }
            }
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
