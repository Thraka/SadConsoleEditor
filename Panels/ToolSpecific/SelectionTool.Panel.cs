using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using SadConsole.Controls;
using SadConsole.Input;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsoleEditor.Windows;

namespace SadConsoleEditor.Panels
{
    class SelectionToolAltPanel : CustomPanel
    {
        private CheckBox skipEmptyColor;
        private CheckBox altEmptyColorCheck;
        private Controls.ColorPresenter altEmptyColor;


        public bool SkipEmptyCells { get { return skipEmptyColor.IsSelected; } }
        public bool UseAltEmptyColor { get { return altEmptyColorCheck.IsSelected; } }
        public Color AltEmptyColor { get { return altEmptyColor.SelectedColor; } }

        public SelectionToolAltPanel()
        {
            Title = "Sel.Rect Options";

            skipEmptyColor = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            skipEmptyColor.Text = "Skip Empty";

            altEmptyColorCheck = new CheckBox(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            altEmptyColorCheck.Text = "Use Alt. Empty";

            altEmptyColor = new Controls.ColorPresenter("Alt. Empty Clr", Settings.Green, 18);
            altEmptyColor.SelectedColor = Color.Black;

            Controls = new ControlBase[] { skipEmptyColor, altEmptyColorCheck, altEmptyColor };
        }

        public override void ProcessMouse(MouseInfo info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            if (control == skipEmptyColor)
                return 1;

            return 0;
        }

        public override void Loaded()
        {
        }
    }

    class SelectionToolPanel : CustomPanel
    {
        private CloneState state;
        private Button reset;
        private Button loadBrush;
        private Button saveBrush;
        private Button clone;
        private Button clear;
        private Button move;

        private string lastFolder = null;

        private Func<TextSurface> saveBrushHandler;
        private Action<TextSurface> loadBrushHandler;

        private int _currentStepChar = 175;


        public CloneState State
        {
            get { return state; }
            set
            {
                state = value;

                saveBrush.IsEnabled = value == CloneState.Selected;
                clone.IsEnabled = value == CloneState.Selected;
                clear.IsEnabled = value == CloneState.Selected;
                move.IsEnabled = value == CloneState.Selected;

                if (StateChangedHandler != null)
                    StateChangedHandler(value);
            } 
        }

        public Action<CloneState> StateChangedHandler;

        public enum CloneState
        {
            SelectingPoint1,
            SelectingPoint2,
            Selected,
            Clone,
            Clear,
            Move
        }

        

        public SelectionToolPanel(Action<TextSurface> loadBrushHandler, Func<TextSurface> saveBrushHandler)
        {
            reset = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            reset.Text = "Reset Steps";
            reset.ButtonClicked += (o, e) => State = CloneState.SelectingPoint1;

            loadBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            loadBrush.Text = "Import Brush";
            loadBrush.ButtonClicked += _loadBrush_ButtonClicked;

            saveBrush = new Button(SadConsoleEditor.Consoles.ToolPane.PanelWidth - 2, 1);
            saveBrush.Text = "Export Brush";
            saveBrush.ButtonClicked += _saveBrush_ButtonClicked;

            clone = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            clone.Text = "Clone";
            clone.ButtonClicked += clone_ButtonClicked;

            clear = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            clear.Text = "Clear";
            clear.ButtonClicked += clear_ButtonClicked;

            move = new Button(Consoles.ToolPane.PanelWidth - 2, 1);
            move.Text = "Move";
            move.ButtonClicked += move_ButtonClicked;

            

            Controls = new ControlBase[] { reset, loadBrush, saveBrush, clone, clear, move };

            this.loadBrushHandler = loadBrushHandler;
            this.saveBrushHandler = saveBrushHandler;

            Title = "Clone";
            State = CloneState.SelectingPoint1;
        }

        private void move_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Move;
        }

        private void clear_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Clear;
        }

        private void clone_ButtonClicked(object sender, EventArgs e)
        {
            State = CloneState.Clone;
        }

        private void _saveBrush_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    popup.SelectedLoader.Save(saveBrushHandler(), popup.SelectedFile);
                }
            };
            popup.CurrentFolder = lastFolder ?? Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface(), new FileLoaders.TextFile() };
            popup.SelectButtonText = "Save";
            popup.SkipFileExistCheck = true;
            popup.Show(true);
            popup.Center();
        }

        private void _loadBrush_ButtonClicked(object sender, EventArgs e)
        {
            SelectFilePopup popup = new SelectFilePopup();
            popup.Closed += (o2, e2) =>
            {
                if (popup.DialogResult)
                {
                    if (System.IO.File.Exists(popup.SelectedFile))
                    {
                        lastFolder = System.IO.Path.GetDirectoryName(popup.SelectedFile);

                        //if (System.IO.Path.GetExtension(popup.SelectedFile) == ".ans")
                        //{
                        //    using (var ansi = new SadConsole.Ansi.Document(popup.SelectedFile))
                        //    {
                        //        var console = new SadConsole.Consoles.Console(80, 1);
                        //        console.TextSurface.ResizeOnShift = true;
                        //        SadConsole.Ansi.AnsiWriter writer = new SadConsole.Ansi.AnsiWriter(ansi, console);
                        //        writer.ReadEntireDocument();
                        //        _loadBrushHandler(console.CellData);
                        //    }

                        //}
                        //else
                        loadBrushHandler((TextSurface)popup.SelectedLoader.Load(popup.SelectedFile));
                    }
                }
            };
            popup.CurrentFolder = lastFolder ?? Environment.CurrentDirectory;
            popup.FileLoaderTypes = new FileLoaders.IFileLoader[] { new FileLoaders.TextSurface(), new FileLoaders.TextFile() };
            popup.Show(true);
            popup.Center();
        }

        public override void ProcessMouse(MouseInfo info)
        {

        }

        public override int Redraw(ControlBase control)
        {
            if (control == saveBrush || control == move)
            {
                return 1;
            }

            return 0;
        }

        public override void Loaded()
        {
        }
    }
}
