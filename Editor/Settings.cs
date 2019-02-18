using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SadConsole;
using System.Runtime.Serialization;

namespace SadConsoleEditor
{
    public class WindowSettings
    {
        public int WindowWidth;
        public int WindowHeight;
    }

    public class ProgramSettings
    {
        private Dictionary<string, EditorSettings> EditorSettings = new Dictionary<string, EditorSettings>();

        public int WindowWidth;
        public int WindowHeight;
        public string ProgramFontFile;
        public string ScreenFontFile;
        public WindowSettings ColorPickerSettings;

        public WindowSettings TextMakerSettings;

        public int ToolPaneWidth;

        public Font ScreenFont;


        public int WindowWidthAsScreenFont
        {
            get
            {
                return new Point(Global.WindowWidth, 0).PixelLocationToConsole(ScreenFont.Size.X, ScreenFont.Size.Y).X;
            }
        }

        public int WindowHeightAsScreenFont
        {
            get
            {
                return new Point(0, Global.WindowHeight).PixelLocationToConsole(ScreenFont.Size.X, ScreenFont.Size.Y).Y;
            }
        }

        public EditorSettings GetSettings(string editorId)
        {
            if (EditorSettings.ContainsKey(editorId))
                return EditorSettings[editorId];

            throw new Exception($"Editor settings not found for editor: {editorId}");
        }

        public void AddSettings(string editorId, EditorSettings settings) =>
            EditorSettings[editorId] = settings;
    }

    public class EditorSettings
    {
        public string EditorId;
        public int DefaultWidth;
        public int DefaultHeight;
        public int BoundsWidth;
        public int BoundsHeight;
        public Color DefaultForeground;
        public Color DefaultBackground;
        public string[] Tools;
        public string[] FileLoaders;
    }

    public static class Config
    {
        public static ProgramSettings Program;

        public const string FileObjectTypes = "editor.objecttypes.json";
    }
}
