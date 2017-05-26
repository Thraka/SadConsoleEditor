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

        private SadConsole.Renderers.LayeredSurfaceRenderer renderer;
        private SadConsole.Surfaces.LayeredSurface surface;
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

        public ISurface Surface => surface;

        public ISurfaceRenderer Renderer => renderer;

        public GameObject GameObject { get { return gameObject; } }

        public string DocumentTitle { get; set; }

        public Editors EditorType { get { return Editors.GameObject; } }

        public IEditor LinkedEditor { get; set; }

        public string Title { get; set; }

        public string EditorTypeName { get { return "Animated Game Object"; } }

        public int Height { get { return surface.Height; } }

        public int Width { get { return surface.Width; } }

        public CustomPanel[] Panels { get { return panels; } }

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
            renderer = new LayeredSurfaceRenderer();
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

                if (tool is Tools.EntityCenterTool)
                {
                    surface.GetLayer(LayerAnimCenter).IsVisible = true;
                    surface.GetLayer(LayerBackground).IsVisible = true;
                }
                else
                {
                    surface.GetLayer(LayerAnimCenter).IsVisible = false;
                    surface.GetLayer(LayerBackground).IsVisible = false;
                }
            }
        }
        

        private void SelectedAnimationChanged(AnimatedSurface animation)
        {
            selectedAnimation = animation;

            surface = new LayeredSurface(animation.Width, animation.Height, Settings.Config.ScreenFont, 4);

            SyncSpecialLayerToAnimation();

            surface.SetActiveLayer(0);

            // inform the outer box we've changed size
            if (MainScreen.Instance.ActiveEditor == this)
                MainScreen.Instance.RefreshBorder();

            framesPanel.SetAnimation(animation);
            SelectedTool = selectedTool;

        }

        private void SelectedFrameChanged(BasicSurface frame)
        {
            var meta = surface.GetLayer(LayerDrawing);
            meta.Cells = meta.RenderCells = frame.Cells;
            surface.SetActiveLayer(LayerDrawing);
            surface.IsDirty = true;
        }

        private void SyncSpecialLayerToAnimation()
        {
            int previousSelectedLayer = surface.ActiveLayerIndex;

            surface.SetActiveLayer(LayerAnimCenter);
            Settings.QuickEditor.TextSurface = surface;
            Settings.QuickEditor.Fill(Color.White, Color.Transparent, 0);
            surface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).Glyph = 42;
            surface.GetCell(selectedAnimation.Center.X, selectedAnimation.Center.Y).Background = Color.Black;

            surface.SetActiveLayer(LayerBackground);
            Settings.QuickEditor.Fill(Color.White, Color.White * 0.6f, 0);

            // Change this to a prop (SHOWCENTER LAYER) and have tool on selected
            surface.GetLayer(LayerAnimCenter).IsVisible = ShowCenterLayer;
            surface.GetLayer(LayerAnimBox).IsVisible = false;
            surface.GetLayer(LayerBackground).IsVisible = ShowCenterLayer;

            surface.SetActiveLayer(previousSelectedLayer);
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

            int renderWidth = Math.Min(MainScreen.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainScreen.Instance.InnerEmptyBounds.Height, height);

            var gameObject = new GameObject(renderWidth, renderHeight, SadConsoleEditor.Settings.Config.ScreenFont);

            AnimatedSurface animation = new AnimatedSurface("default", renderWidth, renderHeight, SadConsoleEditor.Settings.Config.ScreenFont);
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
            int renderWidth = Math.Min(MainScreen.Instance.InnerEmptyBounds.Width, width);
            int renderHeight = Math.Min(MainScreen.Instance.InnerEmptyBounds.Height, height);

            List<AnimatedSurface> newAnimations = new List<AnimatedSurface>(gameObject.Animations.Count);

            foreach (var oldAnimation in gameObject.Animations.Values)
            {
                var newAnimation = new AnimatedSurface(oldAnimation.Name, renderWidth, renderHeight, SadConsoleEditor.Settings.Config.ScreenFont);

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
                selectedTool.OnSelected();
            }
        }

        public void OnDeselected()
        {
            MainScreen.Instance.ToolsPane.PanelFiles.CloseButton.IsEnabled = true;
        }

        public void Draw()
        {
        }

        public void Update()
        {
            selectedTool.Update();
        }


        public bool ProcessKeyboard(Keyboard info)
        {
             bool toolHandled = toolsPanel.SelectedTool.ProcessKeyboard(info, surface);

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

        public bool ProcessMouse(SadConsole.Input.MouseConsoleState info, bool isInBounds)
        {
            toolsPanel.SelectedTool?.ProcessMouse(info, surface, isInBounds);
            return false;
        }
    }
}
