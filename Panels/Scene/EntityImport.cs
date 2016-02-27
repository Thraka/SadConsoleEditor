using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Controls;
using SadConsole.Input;

namespace SadConsoleEditor.Panels.Scene
{
    class EntityImport : CustomPanel
    {
        private Button _importButton;
        private Button _deleteButton;

        public EntityImport()
        {
            Title = "Scene Editor";

            // Load background console

            _importButton = new Button(Consoles.ToolPane.PanelWidth, 1)
            {
                Text = "Add Entity",
                CanUseKeyboard = false,
            };

            _importButton.ButtonClicked += ImportButton_Click;

            Controls = new ControlBase[] { _importButton };
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            Windows.SelectFilePopup fileDialogPopup = new Windows.SelectFilePopup();

            fileDialogPopup.FileFilter = "*.entity";
            fileDialogPopup.SelectButtonText = "Open";
            fileDialogPopup.Show(true);
            fileDialogPopup.Center();
            fileDialogPopup.CurrentFolder = Environment.CurrentDirectory;
            fileDialogPopup.Closed += (s, e2) =>
            {
                if (fileDialogPopup.DialogResult)
                {
                    try
                    {
                        var entity = SadConsole.Entities.Entity.Load(fileDialogPopup.SelectedFile);
                        entity.Position = new Microsoft.Xna.Framework.Point(0, 0);
                        entity.PositionOffset = (EditorConsoleManager.Instance.SelectedEditor as Editors.SceneEditor).GetPosition();
                        (EditorConsoleManager.Instance.SelectedEditor as Editors.SceneEditor)?.AddEntity(entity);
                    }
                    catch
                    {
                        SadConsole.Consoles.Window.Message(new SadConsole.ColoredString("Loading this entity failed."), "Close");
                    }
                }
            };
        }

        public override void Loaded()
        {
        }

        public override void ProcessMouse(MouseInfo info)
        {
        }

        public override int Redraw(ControlBase control)
        {
            return 0;
        }
    }
}
