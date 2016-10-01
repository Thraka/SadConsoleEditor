using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Consoles;
using SadConsole.Game;
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

        public GameObject Brush;

        public override string ToString()
        {
            return Title;
        }

        public EntityCenterTool()
        {
        }

        public void OnSelected()
        {
            Brush = new SadConsole.Game.GameObject(Settings.Config.ScreenFont);
            Brush.Animation = new AnimatedTextSurface("default", 1, 1);
            Brush.Animation.DefaultBackground = Color.Black;
            Brush.Animation.DefaultForeground = Color.White;
            Brush.Animation.CreateFrame()[0].GlyphIndex = 42;
            Brush.IsVisible = false;
            RefreshTool();
            EditorConsoleManager.Brush = Brush;
            EditorConsoleManager.UpdateBrush();

            var editor = EditorConsoleManager.ActiveEditor as Editors.GameObjectEditor;

            if (editor != null)
                editor.ShowCenterLayer = true;
        }

        public void OnDeselected()
        {
            var editor = EditorConsoleManager.ActiveEditor as Editors.GameObjectEditor;

            if (editor != null)
                editor.ShowCenterLayer = false;
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
            Brush.IsVisible = true;
        }

        public void MouseExitSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.IsVisible = false;
        }

        public void MouseMoveSurface(MouseInfo info, ITextSurface surface)
        {
            Brush.Position = info.ConsoleLocation;
            Brush.IsVisible = true;

            if (info.LeftClicked)
            {
                var cell = surface.GetCell(info.ConsoleLocation.X, info.ConsoleLocation.Y);
                var editor = EditorConsoleManager.ActiveEditor as Editors.GameObjectEditor;

                if (editor != null)
                {
                    editor.SetAnimationCenter(new Point(info.ConsoleLocation.X, info.ConsoleLocation.Y));
                }
            }
        }
    }
}
