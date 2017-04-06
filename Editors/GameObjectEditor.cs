using Microsoft.Xna.Framework;
using SadConsole.Surfaces;
using SadConsole.GameHelpers;
using SadConsole.Input;
using SadConsoleEditor.Consoles;
using SadConsoleEditor.Panels;
using SadConsoleEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = SadConsole.Console;
using SadConsole;
using SadConsole.Renderers;

namespace SadConsoleEditor.Editors
{
    class GameObjectEditor: IEditor
    {
        private const int LayerDrawing = 0;
        private const int LayerBackground = 1;
        private const int LayerAnimCenter = 2;
        private const int LayerAnimBox = 3;

        private LayeredSurface BasicSurface;
        private Console consoleWrapper;
        private CustomPanel[] panels;
        private ToolsPanel toolsPanel;
        private Dictionary<string, Tools.ITool> tools;
        private Tools.ITool selectedTool;

        private AnimationsPanel animationPanel;
        private GameObjectNamePanel gameObjectNamePanel;
        private AnimationFramesPanel framesPanel;

        private GameObject gameObject;
        private AnimatedSurface selectedAnimation;

        //public SceneEditor LinkedEditor;

        public ISurface Surface => BasicSurface;

        public ISurfaceRenderer Renderer => new SurfaceRenderer();

        public GameObject GameObject { get { return gameObject; } }

        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.GameObject; } }

        public string Title { get; set; }

        public string EditorTypeName { get { return "Animated Game Object"; } }

        public int Height { get { return BasicSurface.Height; } }

        public Point Position { get { return consoleWrapper.Position; } }

        public int Width { get { return BasicSurface.Width; } }

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

        public bool IsLinked { get { return false; } } // LinkedEditor != null; } }

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
            consoleWrapper.Renderer = new SadConsole.Renderers.LayeredSurfaceRenderer();
            consoleWrapper.MouseHandler = ProcessMouse;
            consoleWrapper.UseKeyboard = false;

            consoleWrapper.MouseMove += (o, e) => { toolsPanel.SelectedTool?.MouseMoveSurface(e.MouseState, BasicSurface); };
            consoleWrapper.MouseEnter += (o, e) => { toolsPanel.SelectedTool?.MouseEnterSurface(e.MouseState, BasicSurface); };
            consoleWrapper.MouseExit += (o, e) => { toolsPanel.SelectedTool?.MouseExitSurface(e.MouseState, BasicSurface); };

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
                selectedTool = tool;

                List<CustomPanel> newPanels = new List<CustomPanel>() { gameObjectNamePanel, animationPanel, framesPanel, toolsPanel };

                if (tool.ControlPanels != null && tool.ControlPanels.Length != 0)
                    newPanels.AddRange(tool.ControlPanels);

                panels = newPanels.ToArray();
                MainScreen.Instance.ToolsPane.RedrawPanels();

                //if (tool is Tools.EntityCenterTool)
                //{
                //    BasicSurface.GetLayer(LayerAnimCenter).IsVisible = true;
                //    BasicSurface.GetLayer(LayerBackground).IsVisible = true;
                    
                //    //BasicSurface.Tint = new Color(0f, 0f, 0f, 0.2f);
                //}
                //else
                //{
                //    BasicSurface.GetLayer(LayerAnimCenter).IsVisible = false;
                //    BasicSurface.GetLayer(LayerBackground).IsVisible = false;
                //    //BasicSurface.Tint = new Color(0f, 0f, 0f, 1f);
                //}
            }
        }
        

        private void SelectedAnimationChanged(AnimatedSurface animation)
        {
            selectedAnimation = animation;

            consoleWrapper.TextSurface = BasicSurface = new LayeredSurface(animation.Width, animation.Height, Settings.Config.ScreenFont, 4);

            SyncSpecialLayerToAnimation();

            ((LayeredSurface)consoleWrapper.TextSurface).SetActiveLayer(0);

            // inform the outer box we've changed size
            //if (MainScreen.Instance.ActiveEditor == this)
            //    MainScreen.Instance.UpdateBorder(consoleWrapper.Position);

            framesPanel.SetAnimation(animation);
            SelectedTool = selectedTool;
        }

        private void SelectedFrameChanged(BasicSurface frame)
        {
            //if (((LayeredSurface)_consoleLayers.BasicSurface).LayerCount != 0)
            //    ((LayeredSurface)_consoleLayers.BasicSurface).Remove(0);
            //var layer = ((LayeredSurface)_consoleLayers.BasicSurface).Add();
            //((LayeredSurface)_consoleLayers.BasicSurface).SetActiveLayer(layer.Index);
            //BasicSurface tempSurface = new BasicSurface(_consoleLayers.Width, _consoleLayers.Height, ((LayeredSurface)_consoleLayers.BasicSurface).Font);
            //frame.Copy(tempSurface);
            //var meta = LayerMetadata.Create("Root", false, false, true, layer);
            //((LayeredSurface)_consoleLayers.BasicSurface).SetActiveLayer(0);

            var meta = ((LayeredSurface)consoleWrapper.TextSurface).GetLayer(LayerDrawing);
            meta.Cells = meta.RenderCells = frame.Cells;
            ((LayeredSurface)consoleWrapper.TextSurface).SetActiveLayer(LayerDrawing);
        }

        private void SyncSpecialLayerToAnimation()
        {
            int previousSelectedLayer = BasicSurface.ActiveLayerIndex;

            BasicSurface.SetActiveLayer(LayerAnimCenter);
            consoleWrapper.Fill(Color.White, Color.Transparent, 0);
            BasicSurface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).Glyph = 42;
            BasicSurface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).Background = Color.Black;

            BasicSurface.SetActiveLayer(LayerBackground);
            consoleWrapper.Fill(Color.White, Color.White * 0.6f, 0);

            // Change this to a prop (SHOWCENTER LAYER) and have tool on selected
            BasicSurface.GetLayer(LayerAnimCenter).IsVisible = ShowCenterLayer;
            BasicSurface.GetLayer(LayerAnimBox).IsVisible = false;
            BasicSurface.GetLayer(LayerBackground).IsVisible = ShowCenterLayer;

            BasicSurface.SetActiveLayer(previousSelectedLayer);
            // TODO: Draw box on LayerAnimBox for collision rect
        }

        public void SetEntity(GameObject entity)
        {
            gameObject = entity;
            gameObject.Animation.Font = SadConsoleEditor.Settings.Config.ScreenFont;

            if (!gameObject.Animations.ContainsValue(gameObject.Animation))
                gameObject.Animation = gameObject.Animations.First().Value;

            animationPanel.SetEntity(gameObject);
            gameObjectNamePanel.SetEntity(gameObject);
            Title = entity.Name;

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
            
            var gameObject = new GameObject(1, 1, SadConsoleEditor.Settings.Config.ScreenFont);

            AnimatedSurface animation = new AnimatedSurface("default", width, height, SadConsoleEditor.Settings.Config.ScreenFont);
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
            //var oldSurface = (LayeredSurface)_consoleLayers.BasicSurface;
            //var newSurface = new LayeredSurface(width, height, oldSurface.LayerCount);

            //for (int i = 0; i < oldSurface.LayerCount; i++)
            //{
            //    var oldLayer = oldSurface.GetLayer(i);
            //    var newLayer = newSurface.GetLayer(i);
            //    oldSurface.SetActiveLayer(i);
            //    newSurface.SetActiveLayer(i);
            //    oldSurface.Copy(newSurface);
            //    newLayer.Metadata = oldLayer.Metadata;
            //    newLayer.IsVisible = oldLayer.IsVisible;
            //}

            List<AnimatedSurface> newAnimations = new List<AnimatedSurface>(gameObject.Animations.Count);

            foreach (var oldAnimation in gameObject.Animations.Values)
            {
                var newAnimation = new AnimatedSurface(oldAnimation.Name, width, height, SadConsoleEditor.Settings.Config.ScreenFont);

                for (int i = 0; i < oldAnimation.Frames.Count; i++)
                {
                    oldAnimation.Frames[i].Copy(newAnimation.CreateFrame());
                }

                newAnimation.CurrentFrameIndex = 0;
                newAnimations.Add(newAnimation);
                newAnimation.AnimationDuration = oldAnimation.AnimationDuration;
                newAnimation.Center = oldAnimation.Center;
            }

            foreach (var animation in newAnimations)
            {
                gameObject.Animations[animation.Name] = animation;

                if (gameObject.Animation.Name == animation.Name)
                    gameObject.Animation = animation;

                if (selectedAnimation.Name == animation.Name)
                    selectedAnimation = animation;
            }

            // inform the outer box we've changed size
            //MainScreen.Instance.Instance.UpdateBox();

            SetEntity(gameObject);

            if (MainScreen.Instance.ActiveEditor == this)
            {
                MainScreen.Instance.CenterEditor();
            }
        }

        public void Load(string file, FileLoaders.IFileLoader loader)
        {
            if (loader is FileLoaders.GameObject)
            {
                SetEntity((GameObject)((FileLoaders.GameObject)loader).Load(file));
                Title = System.IO.Path.GetFileName(file);

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

            //if (MainScreen.Instance.ActiveEditor == this)
            //    MainScreen.Instance.UpdateBorder(consoleWrapper.Position);
        }

        public void OnClosed()
        {
        }

        public void OnSelected()
        {
            if (IsLinked)
                MainScreen.Instance.ToolsPane.PanelFiles.CloseButton.IsEnabled = false;
            else
                MainScreen.Instance.ToolsPane.PanelFiles.CloseButton.IsEnabled = true;

            if (selectedTool == null)
                SelectedTool = tools.First().Value;
            else
            {
                var oldTool = selectedTool;
                SelectedTool = null;
                SelectedTool = selectedTool;
            }
        }

        public void OnDeselected()
        {
            MainScreen.Instance.ToolsPane.PanelFiles.CloseButton.IsEnabled = true;
        }

        public void Render()
        {
        }

        public void Update()
        {
            selectedTool.Update();
        }


        public bool ProcessKeyboard(Keyboard info)
        {
            bool toolHandled = toolsPanel.SelectedTool.ProcessKeyboard(info, BasicSurface);

            if (!toolHandled)
            {
                if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemOpenBrackets))
                {
                    framesPanel.TryPreviousFrame();
                    return true;
                }

                else if (info.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets))
                {
                    framesPanel.TryNextFrame();
                    return true;
                }
                else
                {
                    var keys = info.KeysReleased.Select(k => k.Character).ToList();

                    foreach (var item in tools.Values)
                    {
                        if (keys.Contains(item.Hotkey))
                        {
                            SelectedTool = item;
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        public bool ProcessMouse(IConsole console, SadConsole.Input.MouseConsoleState info)
        {
            consoleWrapper.MouseHandler = null;
            consoleWrapper.UseMouse = true;
            consoleWrapper.ProcessMouse(info);
            consoleWrapper.MouseHandler = ProcessMouse;

            toolsPanel.SelectedTool?.ProcessMouse(info, BasicSurface);

            if (consoleWrapper.IsMouseOver)
            {
                MainScreen.Instance.SurfaceMouseLocation = info.ConsolePosition;
                return true;
            }
            else
                MainScreen.Instance.SurfaceMouseLocation = Point.Zero;

            consoleWrapper.UseMouse = false;
            return false;
        }
    }
}
