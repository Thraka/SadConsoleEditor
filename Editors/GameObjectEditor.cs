using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using SadConsole.Game;
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
    class GameObjectEditor: IEditor
    {
        private const int LayerDrawing = 0;
        private const int LayerBackground = 1;
        private const int LayerAnimCenter = 2;
        private const int LayerAnimBox = 3;

        private LayeredTextSurface textSurface;
        private Console consoleWrapper;
        private CustomPanel[] panels;
        private ToolsPanel toolsPanel;
        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;

        private AnimationsPanel animationPanel;
        private GameObjectNamePanel gameObjectNamePanel;
        private AnimationFramesPanel framesPanel;

        private GameObject gameObject;
        private AnimatedTextSurface selectedAnimation;

        public SceneEditor LinkedEditor;

        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.Console; } }

        public string EditorTypeName { get { return "Animated Game Object"; } }

        public int Height { get { return textSurface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public int Width { get { return textSurface.Width; } }

        public CustomPanel[] Panels { get { return panels; } }

        public Console RenderedConsole { get { return consoleWrapper; } }

        private Tools.ITool SelectedTool
        {
            get { return selectedTool; }
            set
            {
                toolsPanel.ToolsListBox.SelectedItem = value;
            }
        }

        public bool IsLinked { get { return LinkedEditor != null; } }

        private bool showCenterLayer;

        public bool ShowCenterLayer
        {
            get
            {
                return showCenterLayer;
            }
            set
            {
                showCenterLayer = value;
                SyncSpecialLayerToAnimation();
            }
        }



        public GameObjectEditor()
        {
            consoleWrapper = new Console(1, 1);
            consoleWrapper.Renderer = new LayeredTextRenderer();
            consoleWrapper.MouseHandler = ProcessMouse;
            consoleWrapper.KeyboardHandler = ProcessKeyboard;
            consoleWrapper.CanUseKeyboard = false;

            consoleWrapper.MouseMove += (o, e) => { toolsPanel.SelectedTool?.MouseMoveSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseEnter += (o, e) => { toolsPanel.SelectedTool?.MouseEnterSurface(e.OriginalMouseInfo, textSurface); };
            consoleWrapper.MouseExit += (o, e) => { toolsPanel.SelectedTool?.MouseExitSurface(e.OriginalMouseInfo, textSurface); };

            toolsPanel = new ToolsPanel();
            animationPanel = new AnimationsPanel(SelectedAnimationChanged);
            gameObjectNamePanel = new GameObjectNamePanel();
            framesPanel = new AnimationFramesPanel(SelectedFrameChanged);

            // Fill tools
            tools = new Dictionary<string, Tools.ITool>();
            tools.Add(Tools.PaintTool.ID, new Tools.PaintTool());
            tools.Add(Tools.LineTool.ID, new Tools.LineTool());
            tools.Add(Tools.CircleTool.ID, new Tools.CircleTool());
            tools.Add(Tools.RecolorTool.ID, new Tools.RecolorTool());
            tools.Add(Tools.FillTool.ID, new Tools.FillTool());
            tools.Add(Tools.BoxTool.ID, new Tools.BoxTool());
            tools.Add(Tools.SelectionTool.ID, new Tools.SelectionTool());
            tools.Add(Tools.EntityCenterTool.ID, new Tools.EntityCenterTool());
            
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.EntityCenterTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.PaintTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.LineTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.CircleTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.RecolorTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.FillTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.BoxTool.ID]);
            toolsPanel.ToolsListBox.Items.Add(tools[Tools.SelectionTool.ID]);

            toolsPanel.ToolsListBox.SelectedItemChanged += ToolsListBox_SelectedItemChanged;

            panels = new CustomPanel[] { gameObjectNamePanel, animationPanel, framesPanel, toolsPanel };
        }

        private void ToolsListBox_SelectedItemChanged(object sender, SadConsole.Controls.ListBox<SadConsole.Controls.ListBoxItem>.SelectedItemEventArgs e)
        {
            Tools.ITool tool = e.Item as Tools.ITool;

            if (e.Item != null)
            {
                List<CustomPanel> newPanels = new List<CustomPanel>() { gameObjectNamePanel, animationPanel, framesPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                panels = newPanels.ToArray();
                EditorConsoleManager.ToolsPane.RedrawPanels();

                if (tool is Tools.EntityCenterTool)
                {
                    textSurface.GetLayer(LayerAnimCenter).IsVisible = true;
                    textSurface.GetLayer(LayerBackground).IsVisible = true;
                    
                    //textSurface.Tint = new Color(0f, 0f, 0f, 0.2f);
                }
                else
                {
                    textSurface.GetLayer(LayerAnimCenter).IsVisible = false;
                    textSurface.GetLayer(LayerBackground).IsVisible = false;
                    //textSurface.Tint = new Color(0f, 0f, 0f, 1f);
                }
            }
        }
        

        private void SelectedAnimationChanged(AnimatedTextSurface animation)
        {
            selectedAnimation = animation;

            consoleWrapper.TextSurface = textSurface = new LayeredTextSurface(animation.Width, animation.Height, Settings.Config.ScreenFont, 4);

            SyncSpecialLayerToAnimation();

            ((LayeredTextSurface)consoleWrapper.TextSurface).SetActiveLayer(0);

            // inform the outer box we've changed size
            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            framesPanel.SetAnimation(animation);
            SelectedTool = selectedTool;
        }

        private void SelectedFrameChanged(TextSurfaceBasic frame)
        {
            //if (((LayeredTextSurface)_consoleLayers.TextSurface).LayerCount != 0)
            //    ((LayeredTextSurface)_consoleLayers.TextSurface).Remove(0);
            //var layer = ((LayeredTextSurface)_consoleLayers.TextSurface).Add();
            //((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(layer.Index);
            //TextSurface tempSurface = new TextSurface(_consoleLayers.Width, _consoleLayers.Height, ((LayeredTextSurface)_consoleLayers.TextSurface).Font);
            //frame.Copy(tempSurface);
            //var meta = LayerMetadata.Create("Root", false, false, true, layer);
            //((LayeredTextSurface)_consoleLayers.TextSurface).SetActiveLayer(0);

            var meta = ((LayeredTextSurface)consoleWrapper.TextSurface).GetLayer(LayerDrawing);
            meta.Cells = meta.RenderCells = frame.Cells;
            ((LayeredTextSurface)consoleWrapper.TextSurface).SetActiveLayer(LayerDrawing);
        }

        private void SyncSpecialLayerToAnimation()
        {
            int previousSelectedLayer = textSurface.ActiveLayerIndex;

            textSurface.SetActiveLayer(LayerAnimCenter);
            consoleWrapper.Fill(Color.White, Color.Transparent, 0);
            textSurface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).GlyphIndex = 42;
            textSurface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).Background = Color.Black;

            textSurface.SetActiveLayer(LayerBackground);
            consoleWrapper.Fill(Color.White, Color.White * 0.6f, 0);

            // Change this to a prop (SHOWCENTER LAYER) and have tool on selected
            textSurface.GetLayer(LayerAnimCenter).IsVisible = ShowCenterLayer;
            textSurface.GetLayer(LayerAnimBox).IsVisible = false;
            textSurface.GetLayer(LayerBackground).IsVisible = ShowCenterLayer;

            textSurface.SetActiveLayer(previousSelectedLayer);
            // TODO: Draw box on LayerAnimBox for collision rect
        }

        public void SetEntity(GameObject entity)
        {
            gameObject = entity;
            gameObject.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            if (!gameObject.Animations.ContainsValue(gameObject.Animation))
                gameObject.Animation = gameObject.Animations.First().Value;

            animationPanel.SetEntity(gameObject);
            gameObjectNamePanel.SetEntity(gameObject);

            SelectedAnimationChanged(gameObject.Animation);
        }

        public void SetAnimationCenter(Point center)
        {
            selectedAnimation.Center = center;
            SyncSpecialLayerToAnimation();
        }
        
        public void New(Color foreground, Color background, int width, int height)
        {
            Reset();
            
            var gameObject = new GameObject(SadConsoleEditor.Settings.Config.ScreenFont);

            AnimatedTextSurface animation = new AnimatedTextSurface("default", width, height, SadConsoleEditor.Settings.Config.ScreenFont);
            animation.DefaultForeground = foreground;
            animation.DefaultBackground = background;
            animation.CreateFrame();
            animation.AnimationDuration = 1;

            gameObject.Animations[animation.Name] = animation;
            gameObject.Animation = animation;
            gameObject.Name = "game object";

            SetEntity(gameObject);
        }

        public void Resize(int width, int height)
        {
            //this.width = width;
            //this.height = height;

            ////var oldSurface = (LayeredTextSurface)_consoleLayers.TextSurface;
            ////var newSurface = new LayeredTextSurface(width, height, oldSurface.LayerCount);

            ////for (int i = 0; i < oldSurface.LayerCount; i++)
            ////{
            ////    var oldLayer = oldSurface.GetLayer(i);
            ////    var newLayer = newSurface.GetLayer(i);
            ////    oldSurface.SetActiveLayer(i);
            ////    newSurface.SetActiveLayer(i);
            ////    oldSurface.Copy(newSurface);
            ////    newLayer.Metadata = oldLayer.Metadata;
            ////    newLayer.IsVisible = oldLayer.IsVisible;
            ////}

            //List<AnimatedTextSurface> newAnimations = new List<AnimatedTextSurface>(_entity.Animations.Count);

            //foreach (var oldAnimation in _entity.Animations.Values)
            //{
            //    var newAnimation = new AnimatedTextSurface(oldAnimation.Name, width, height, SadConsoleEditor.Settings.Config.ScreenFont);

            //    for (int i = 0; i < oldAnimation.Frames.Count; i++)
            //    {
            //        oldAnimation.Frames[i].Copy(newAnimation.CreateFrame());
            //    }

            //    newAnimation.CurrentFrameIndex = 0;
            //    newAnimations.Add(newAnimation);
            //}

            //foreach (var animation in newAnimations)
            //{
            //    _entity.Animations[animation.Name] = animation;

            //    if (_entity.Animation.Name == animation.Name)
            //        _entity.Animation = animation;

            //    if (_selectedAnimation.Name == animation.Name)
            //        _selectedAnimation = animation;
            //}

            //// inform the outer box we've changed size
            ////EditorConsoleManager.Instance.UpdateBox();

            //animationPanel.SetEntity(_entity);
            //entityNamePanel.SetEntity(_entity);
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.GameObject)
            {
                SetEntity((GameObject)((FileLoaders.GameObject)loader).Load(file));
            }
        }

        public void Save()
        {
            var popup = new Windows.SelectFilePopup();
            popup.Center();
            popup.SkipFileExistCheck = true;
            popup.Closed += (s, e) =>
            {
                if (popup.DialogResult)
                {
                    popup.SelectedLoader.Save(gameObject, popup.SelectedFile);
                }
            };
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.GameObject() };
            popup.SelectButtonText = "Save";
            popup.Show(true);
        }

        public void Reset()
        {

        }

        public void Move(int x, int y)
        {
            consoleWrapper.Position = new Point(x, y);

            if (EditorConsoleManager.ActiveEditor == this)
                EditorConsoleManager.UpdateBorder(consoleWrapper.Position);

            EditorConsoleManager.UpdateBrush();
        }

        public void OnClosed()
        {
        }

        public void OnSelected()
        {
            if (IsLinked)
                EditorConsoleManager.ToolsPane.PanelFiles.CloseButton.IsEnabled = false;
            else
                EditorConsoleManager.ToolsPane.PanelFiles.CloseButton.IsEnabled = true;

            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
                SelectedTool = selectedTool;
        }

        public void OnDeselected()
        {
            EditorConsoleManager.ToolsPane.PanelFiles.CloseButton.IsEnabled = true;
        }

        public void Render()
        {
        }

        public void Update()
        {
        }

        public bool ProcessKeyboard(IConsole console, SadConsole.Input.KeyboardInfo info)
        {
            //EditorConsoleManager.Instance.ToolPane.SelectedTool.ProcessKeyboard(info, _consoleLayers.TextSurface);

            if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemOpenBrackets))
                framesPanel.TryPreviousFrame();

            else if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets))
                framesPanel.TryNextFrame();

            if (toolsPanel.SelectedTool != null)
                toolsPanel.SelectedTool.ProcessKeyboard(info, textSurface);

            return false;
        }

        public bool ProcessMouse(IConsole console, SadConsole.Input.MouseInfo info)
        {
            consoleWrapper.MouseHandler = null;
            consoleWrapper.CanUseMouse = true;
            consoleWrapper.ProcessMouse(info);
            consoleWrapper.MouseHandler = ProcessMouse;

            toolsPanel.SelectedTool?.ProcessMouse(info, textSurface);

            if (consoleWrapper.IsMouseOver)
            {
                EditorConsoleManager.SurfaceMouseLocation = info.ConsoleLocation;
                return true;
            }
            else
                EditorConsoleManager.SurfaceMouseLocation = Point.Zero;

            consoleWrapper.CanUseMouse = false;
            return false;
        }
    }
}
