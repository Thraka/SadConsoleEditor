using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Entities;
using SadConsole.Input;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsoleEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleEditor.Editors
{
    class EntityEditor: IEditor
    {
        private int _width;
        private int _height;
        private LayeredConsole _consoleLayers;
        private AnimationsPanel _animationPanel;
        private AnimationFramesPanel _framesPanel;

        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseExit;
        public event EventHandler<MouseEventArgs> MouseMove;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public EditorSettings Settings { get { return SadConsoleEditor.Settings.Config.EntityEditor; } }

        private Entity _entity;
        private Animation _selectedAnimation;
        private Frame _selectedFrame;

        public LayeredConsole Surface { get { return _consoleLayers; } }

        public const string ID = "ANIM";

        public string Id { get { return ID; } }

        public string Title { get { return "Entity"; } }

        public string FileExtensions { get { return "*.ent;*.entity"; } }
        public CustomPanel[] ControlPanels { get; private set; }

        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public EntityEditor()
        {
            _animationPanel = new AnimationsPanel(SelectedAnimationChanged);
            _framesPanel = new AnimationFramesPanel(SelectedFrameChanged);

            _entity = new Entity(SadConsoleEditor.Settings.Config.ScreenFont);

            Reset();

            _animationPanel.SetEntity(_entity);
        }

        private void SelectedAnimationChanged(Animation animation)
        {
            _width = animation.Width;
            _height = animation.Height;

            _selectedAnimation = animation;

            if (_consoleLayers.Layers != 0)
                _consoleLayers.RemoveLayer(0);

            _consoleLayers.Resize(animation.Width, animation.Height);

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();

            _framesPanel.SetAnimation(animation);
        }

        private void SelectedFrameChanged(Frame frame)
        {
            if (_consoleLayers.Layers != 0)
                _consoleLayers.RemoveLayer(0);

            _consoleLayers.AddLayer(frame);
            _consoleLayers.GetLayerMetadata(0).Name = "Root";
            _consoleLayers.GetLayerMetadata(0).IsRemoveable = false;
            _consoleLayers.GetLayerMetadata(0).IsMoveable = false;
            _consoleLayers.SetActiveLayer(0);
        }

        public void Reset()
        {
            ControlPanels = new CustomPanel[] { EditorConsoleManager.Instance.ToolPane.FilesPanel, _animationPanel, _framesPanel, EditorConsoleManager.Instance.ToolPane.ToolsPanel };

            if (_consoleLayers != null)
            {
                _consoleLayers.MouseMove -= _mouseMoveHandler;
                _consoleLayers.MouseEnter -= _mouseEnterHandler;
                _consoleLayers.MouseExit -= _mouseExitHandler;
            }

            _consoleLayers = new LayeredConsole(1, 25, 10);
            _consoleLayers.Font = SadConsoleEditor.Settings.Config.ScreenFont;
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            _consoleLayers.GetLayerMetadata(0).Name = "Root";
            _consoleLayers.GetLayerMetadata(0).IsRemoveable = false;
            _consoleLayers.GetLayerMetadata(0).IsMoveable = false;

            _width = 25;
            _height = 10;

            _mouseMoveHandler = (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseEnterHandler = (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };
            _mouseExitHandler = (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers.ActiveLayer, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.ActiveLayer); };

            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;

            _entity = new Entity(SadConsoleEditor.Settings.Config.ScreenFont);

            _animationPanel.SetEntity(_entity);
        }

        public override string ToString()
        {
            return Title;
        }

        public void ProcessKeyboard(KeyboardInfo info)
        {
            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemOpenBrackets))
                _framesPanel.TryPreviousFrame();

            else if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets))
                _framesPanel.TryNextFrame();

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.ActiveLayer);
        }

        public void ProcessMouse(MouseInfo info)
        {
            _consoleLayers.ProcessMouse(info);

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers.ActiveLayer);

            if (_consoleLayers.IsMouseOver)
            {
                EditorConsoleManager.Instance.SurfaceMouseLocation = info.ConsoleLocation;
            }
            else
                EditorConsoleManager.Instance.SurfaceMouseLocation = Point.Zero;

        }

        public void Render()
        {
            Surface.Render();
        }
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            _consoleLayers.Resize(width, height);

            _selectedAnimation.Resize(width, height);

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();
        }

        public void Position(int x, int y)
        {
            _consoleLayers.Move(new Point(x, y));
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Move(newPosition);
        }

        public Point GetPosition()
        {
            return _consoleLayers.Position;
        }

        public void Save(string file)
        {
            _entity.Save(file);
        }

        public void Load(string file)
        {
            if (System.IO.File.Exists(file))
            {
                _entity = Entity.Load(file);
                _entity.Font = SadConsoleEditor.Settings.Config.ScreenFont;

                _animationPanel.SetEntity(_entity);
            }
        }

        public void RemoveLayer(int index)
        {
            Surface.RemoveLayer(index);
        }

        public void MoveLayerUp(int index)
        {
            Surface.MoveLayer(index, index + 1);
        }

        public void MoveLayerDown(int index)
        {
            Surface.MoveLayer(index, index - 1);
        }

        public void AddNewLayer(string name)
        {
            Surface.AddLayer(name);
        }

        public bool LoadLayer(string file)
        {
            if (System.IO.File.Exists(file))
            {
                var surface = SadConsole.CellSurface.Load(file);

                if (surface.Width != EditorConsoleManager.Instance.SelectedEditor.Surface.Width || surface.Height != EditorConsoleManager.Instance.SelectedEditor.Height)
                {
                    var newLayer = EditorConsoleManager.Instance.SelectedEditor.Surface.AddLayer("Loaded");
                    surface.Copy(newLayer.CellData);
                }
                else
                    EditorConsoleManager.Instance.SelectedEditor.Surface.AddLayer(surface);

                return true;
            }
            else
                return false;
        }

        public void SaveLayer(int index, string file)
        {
            EditorConsoleManager.Instance.SelectedEditor.Surface[index].CellData.Save(file);
        }

        public void SetActiveLayer(int index)
        {
            Surface.SetActiveLayer(index);
        }

        public class FrameWrapper
        {
            public Frame Frame;
            public int CurrentIndex;
        }
    }
}
