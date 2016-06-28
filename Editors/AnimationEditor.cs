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
using Console = SadConsole.Consoles.Console;

namespace SadConsoleEditor.Editors
{
    class EntityEditor: IEditor
    {
        private int _width;
        private int _height;
        private Console _consoleLayers;
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
        private TextSurface _selectedFrame;
        private AnimationsPanel.CustomTool _customTool;

        private LayeredTextRenderer _specialToolRenderer;
        private LayeredTextSurface _specialToolLayer;

        public ITextSurface Surface { get { return _consoleLayers.TextSurface; } }

        public const string ID = "ANIM";

        public string ShortName { get { return "Ent"; } }

        public string Id { get { return ID; } }

        public string Title { get { return "Entity"; } }

        public string FileExtensionsLoad { get { return "*.entity"; } }
        public string FileExtensionsSave { get { return "*.entity"; } }
        public CustomPanel[] ControlPanels { get; private set; }

        public Animation SelectedAnimation { get { return _selectedAnimation; } }

        public string[] Tools
        {
            get
            {
                return new string[] { PaintTool.ID, RecolorTool.ID, FillTool.ID, TextTool.ID, SelectionTool.ID, LineTool.ID, BoxTool.ID, CircleTool.ID, EntityCenterTool.ID };
            }
        }

        private EventHandler<MouseEventArgs> _mouseMoveHandler;
        private EventHandler<MouseEventArgs> _mouseEnterHandler;
        private EventHandler<MouseEventArgs> _mouseExitHandler;


        public EntityEditor()
        {
            _animationPanel = new AnimationsPanel(SelectedAnimationChanged);
            _framesPanel = new AnimationFramesPanel(SelectedFrameChanged);
            _specialToolRenderer = new LayeredTextRenderer();
            _entity = new Entity(8, 5, SadConsoleEditor.Settings.Config.ScreenFont);

            Reset();

            _animationPanel.SetEntity(_entity);
        }

        private void SelectedAnimationChanged(Animation animation)
        {
            _width = animation.Width;
            _height = animation.Height;

            _selectedAnimation = animation;

            //if (((LayeredTextSurface)_consoleLayers.TextSurface).LayerCount > 1)
            //    ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(0);

            _consoleLayers.TextSurface = new LayeredTextSurface(animation.Width, animation.Height, 2);

            SyncSpecialLayerToAnimation();

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();

            _framesPanel.SetAnimation(animation);
        }

        private void SyncSpecialLayerToAnimation()
        {
            _specialToolLayer = new LayeredTextSurface(_selectedAnimation.Width, _selectedAnimation.Height, 3);
            var con = new SadConsole.Consoles.Console(_specialToolLayer);
            _specialToolLayer.SetActiveLayer(1);
            _specialToolLayer.GetCell(_selectedAnimation.Center.X, _selectedAnimation.Center.Y).GlyphIndex = 42;
            _specialToolLayer.GetCell(_selectedAnimation.Center.X, _selectedAnimation.Center.Y).Background = Color.Black;
            _specialToolLayer.SetActiveLayer(0);
            _specialToolLayer.Tint = new Color(0f, 0f, 0f, 0.2f);
            // TODO: Draw box on layer 1 for collision rect;
        }

        public void SetAnimationCenter(Point center)
        {
            _specialToolLayer.SetActiveLayer(1);
            SadConsoleEditor.Settings.QuickEditor.TextSurface = _specialToolLayer;
            SadConsoleEditor.Settings.QuickEditor.Clear();
            SadConsoleEditor.Settings.QuickEditor[center.X, center.Y].GlyphIndex = 42;
            SadConsoleEditor.Settings.QuickEditor[center.X, center.Y].Background = Color.Black;
            _selectedAnimation.Center = center;
        }

        private void SelectedFrameChanged(TextSurface frame)
        {
            if (((LayeredTextSurface)_consoleLayers.TextSurface).LayerCount != 0)
                ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(0);
            var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
            ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(layer.Index);
            TextSurface tempSurface = new TextSurface(_consoleLayers.Width, _consoleLayers.Height, ((LayeredTextSurface)_consoleLayers.TextSurface).Font);
            TextSurface.Copy(frame, tempSurface);
            var meta = LayerMetadata.Create("Root", false, false, true, layer);
            ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(0);
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

            _consoleLayers = new Console(25, 10);
            _consoleLayers.TextSurface = new LayeredTextSurface(25, 10, 1, SadConsoleEditor.Settings.Config.ScreenFont);
            
            _consoleLayers.CanUseMouse = true;
            _consoleLayers.CanUseKeyboard = true;
            LayerMetadata.Create("Root", false, false, true, ((LayeredTextSurface)_consoleLayers.TextSurface).ActiveLayer);
            
            _width = 25;
            _height = 10;

            _mouseMoveHandler = (o, e) => { if (this.MouseMove != null) this.MouseMove(_consoleLayers, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseMoveSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseEnterHandler = (o, e) => { if (this.MouseEnter != null) this.MouseEnter(_consoleLayers, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseEnterSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };
            _mouseExitHandler = (o, e) => { if (this.MouseExit != null) this.MouseExit(_consoleLayers, e); EditorConsoleManager.Instance.ToolPane.SelectedTool.MouseExitSurface(e.OriginalMouseInfo, _consoleLayers.TextSurface); };

            _consoleLayers.MouseMove += _mouseMoveHandler;
            _consoleLayers.MouseEnter += _mouseEnterHandler;
            _consoleLayers.MouseExit += _mouseExitHandler;

            _entity = new Entity(25, 10, SadConsoleEditor.Settings.Config.ScreenFont);
            _entity.CurrentAnimation.AnimationDuration = 1;

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

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);
        }

        public void ProcessMouse(MouseInfo info)
        {
            _consoleLayers.ProcessMouse(info);

            EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessMouse(info, _consoleLayers.TextSurface);

            if (_consoleLayers.IsMouseOver)
                EditorConsoleManager.Instance.SurfaceMouseLocation = info.ConsoleLocation;
            else
                EditorConsoleManager.Instance.SurfaceMouseLocation = Point.Zero;

        }

        public void Render()
        {
            _consoleLayers.Render();

            if (EditorConsoleManager.Instance.ToolPane.SelectedTool is Tools.EntityCenterTool || EditorConsoleManager.Instance.ToolPane.SelectedTool is Tools.EntityCollisionBoxTool)
            {
                _specialToolRenderer.Render(_specialToolLayer, _consoleLayers.Position);
            }
        }

        public void Update()
        {
            _consoleLayers.Update();
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            var oldSurface = (LayeredTextSurface)_consoleLayers.TextSurface;
            var oldAnimation = _selectedAnimation;
            var newSurface = new LayeredTextSurface(width, height, oldSurface.LayerCount);
            var newAnimation = new Animation(oldAnimation.Name, width, height);

            newAnimation.Frames.Clear();

            for (int i = 0; i < oldSurface.LayerCount; i++)
            {
                var oldLayer = oldSurface.GetLayer(i);
                var newLayer = newSurface.GetLayer(i);
                oldSurface.SetActiveLayer(i);
                newSurface.SetActiveLayer(i);
                TextSurface.Copy(oldSurface, newSurface);
                newLayer.Metadata = oldLayer.Metadata;
                newLayer.IsVisible = oldLayer.IsVisible;
            }

            for (int i = 0; i < oldAnimation.Frames.Count; i++)
            {
                TextSurface.Copy(oldAnimation.Frames[i], newAnimation.CreateFrame());
            }

            newAnimation.CurrentFrameIndex = 0;

            _selectedAnimation = newAnimation;
            _consoleLayers.TextSurface = newSurface;
            _consoleLayers.TextSurface.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            // inform the outer box we've changed size
            EditorConsoleManager.Instance.UpdateBox();

            SyncSpecialLayerToAnimation();
        }

        public void Position(int x, int y)
        {
            _consoleLayers.Position = new Point(x, y);
        }

        public void Position(Point newPosition)
        {
            _consoleLayers.Position = newPosition;
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
            ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(index);
        }

        public void MoveLayerUp(int index)
        {
            var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
            ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index + 1);
        }

        public void MoveLayerDown(int index)
        {
            var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).GetLayer(index);
            ((LayeredTextSurface)_consoleLayers.TextSurface).Move(layer, index - 1);
        }

        public void AddNewLayer(string name)
        {
            LayerMetadata.Create(name, true, true, true, ((LayeredTextSurface)_consoleLayers.TextSurface).Add());
        }

        public bool LoadLayer(string file)
        {
            if (System.IO.File.Exists(file))
            {
                //typeof(LayerMetadata)
                var surface = SadConsole.Consoles.TextSurface.Load(file);

                if (surface.Width != EditorConsoleManager.Instance.SelectedEditor.Surface.Width || surface.Height != EditorConsoleManager.Instance.SelectedEditor.Height)
                {
                    var newLayer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
                    LayerMetadata.Create("Loaded", true, true, true, newLayer);
                    var tempSurface = new TextSurface(_consoleLayers.Width,
                                                      _consoleLayers.Height,
                                                      _consoleLayers.TextSurface.Font, newLayer.Cells);
                    TextSurface.Copy(surface, tempSurface);
                    newLayer.Cells = tempSurface.Cells;
                }
                else
                {
                    var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
                    LayerMetadata.Create("Loaded", true, true, true, layer);
                    layer.Cells = surface.Cells;

                }

                return true;
            }
            else
                return false;
        }

        public void SaveLayer(int index, string file)
        {
            // TODO: Fix the save layer. This saves the whole surface, not a specific layer.
            ((LayeredTextSurface)_consoleLayers.TextSurface).Save(file, typeof(LayerMetadata));
        }

        public void SetActiveLayer(int index)
        {
            ((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(index);
        }

        //public class FrameWrapper
        //{
        //    public Frame Frame;
        //    public int CurrentIndex;
        //}
    }
}
