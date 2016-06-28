using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
using SadConsole.Input;
using SadConsoleEditor.Panels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsoleEditor.Tools
{
    class EntityCenterTool : ITool
    {
        public const string ID = "CENTERANIM";
        public string Id
        {
            get { return ID; }
        }

        public string Title
        {
            get { return "Set Anim. Center"; }
        }
        public char Hotkey { get { return 'r'; } }

        public CustomPanel[] ControlPanels { get; private set; }

        private EntityBrush _brush;

        public override string ToString()
        {
            return Title;
        }

        public EntityCenterTool()
        {
        }

        public void OnSelected()
        {
            _brush = new EntityBrush(1, 1);
            EditorConsoleManager.Instance.UpdateBrush(_brush);
            _brush.CurrentAnimation.Frames[0][0,0].GlyphIndex = 42;
            _brush.IsVisible = false;
            EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.HideCharacter = true;

            //var editor = EditorConsoleManager.Instance.SelectedEditor as Editors.EntityEditor;

            //if (editor != null)
            //{
            //    editor.Surface.Tint = new Color(0f, 0f, 0f, 0.2f);
            //}
        }

        public void OnDeselected()
        {
            //var editor = EditorConsoleManager.Instance.SelectedEditor as Editors.EntityEditor;

            //if (editor != null)
            //{
            //    editor.Surface.Tint = new Color(0f, 0f, 0f, 0.2f);
            //}
        }

        public void RefreshTool()
        {
            //_brush.CurrentAnimation.Frames[0].Fill(EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingForeground, EditorConsoleManager.Instance.ToolPane.CommonCharacterPickerPanel.SettingBackground, 42, null);
        }

        public bool ProcessKeyboard(KeyboardInfo info, ITextSurface surface)
        {
            return false;
        }

        public void ProcessMouse(MouseInfo info, ITextSurface surface)
        {
        }

        public void MouseEnterSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            _brush.Position = info.ConsoleLocation;
            _brush.IsVisible = true;

            if (info.LeftClicked)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);

                var editor = EditorConsoleManager.Instance.SelectedEditor as Editors.EntityEditor;

                if (editor != null)
                {
                    editor.SetAnimationCenter(new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y));
                }
            }
        }
    }
}
