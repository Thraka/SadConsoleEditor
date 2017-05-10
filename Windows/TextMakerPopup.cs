using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsole.Surfaces;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole;
using Microsoft.Xna.Framework.Graphics;

namespace SadConsoleEditor.Windows
{
    class TextMakerPopup : SadConsole.Window
    {
        DrawingSurface previewPane;
        ScrollBar scrollBarH;
        InputBox textInput;
        Button previewButton;
        Button okButton;
        Button cancelButton;
        ListBox<FontListBoxItem> fontsListbox;

        List<TheDraw.Font> fonts;
        TheDraw.Font selectedFont;


        public TextMakerPopup() : base(Settings.Config.TextMakerSettings.WindowWidth, Settings.Config.TextMakerSettings.WindowHeight)
        {
            Center();
            Title = "Text Maker";

            previewButton = new Button(11);
            previewButton.Text = "Preview";
            previewButton.Position = new Point(Width - previewButton.Width - 2, 2);

            textInput = new InputBox(previewButton.Bounds.Left - 9);
            textInput.Position = new Point(8, 2);

            fontsListbox = new ListBox<FontListBoxItem>(15, Height - 7);
            fontsListbox.Position = new Point(2, 4);
            fontsListbox.SelectedItemChanged += FontsListbox_SelectedItemChanged;
            fontsListbox.HideBorder = true;

            previewPane = new DrawingSurface(Width - fontsListbox.Bounds.Right - 3, Height - 10);
            previewPane.Fill(Color.Black, Color.White, 0, Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
            previewPane.Position = new Point(fontsListbox.Bounds.Right + 1, 5);

            Print(fontsListbox.Bounds.Left, fontsListbox.Bounds.Top - 2, "Fonts", Settings.Color_TitleText);
            Print(fontsListbox.Bounds.Left, fontsListbox.Bounds.Top - 1, new string((char)196, fontsListbox.Width));


            //Add(previewButton);
            //Add(textInput);
            Add(previewPane);
            Add(fontsListbox);

            fonts = new List<TheDraw.Font>();

            foreach (var file in System.IO.Directory.GetFiles("TheDraw"))
                fonts.AddRange(TheDraw.Font.ReadFonts(file));

            foreach (var font in fonts)
                fontsListbox.Items.Add(font);
        }

        private void FontsListbox_SelectedItemChanged(object sender, ListBox<FontListBoxItem>.SelectedItemEventArgs e)
        {
            selectedFont = (TheDraw.Font)e.Item;

            PrintValidCharacters();
        }


        public override void Redraw()
        {
            base.Redraw();

            Print(2, 2, "Text: ", Settings.Green);

            Print(2, 4, "Preview: ", Settings.Green);

            PrintValidCharacters();
        }

        private void PrintValidCharacters()
        {
            if (selectedFont != null)
            {
                for (int i = 0; i < 47; i++)
                {
                    SetGlyph(fontsListbox.Bounds.Right + 1 + i, fontsListbox.Bounds.Top, 33 + i);
                    SetForeground(fontsListbox.Bounds.Right + 1 + i, fontsListbox.Bounds.Top, selectedFont.CharactersSupported[i] ? Color.Green : Settings.Color_Text);
                    SetGlyph(fontsListbox.Bounds.Right + 1 + i, fontsListbox.Bounds.Top + 1, 33 + 47 + i);
                    SetForeground(fontsListbox.Bounds.Right + 1 + i, fontsListbox.Bounds.Top + 1, selectedFont.CharactersSupported[i + 47] ? Color.Green : Settings.Color_Text);
                }
                string text = "HELLO!";

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 94; i++)
                {
                    builder.Append((char)i);
                }

                text = builder.ToString();

                previewPane.Fill(Color.White, Color.Black, 0, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
                
                int x = 0;
                foreach (var item in text)
                {
                    var result = selectedFont.GetCharacter(item);

                    if (result != null)
                    {
                        if (x + result.Width < previewPane.Width)
                        {
                            result.Copy(previewPane.TextSurface, x, 0);
                            x += result.Width;
                        }
                    }

                }
            }
        }
    }

    class FontListBoxItem : ListBoxItem
    {
        public override void Draw(ISurface surface, Rectangle area)
        {
            string value = ((TheDraw.Font)Item).Title;
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            var editor = new SurfaceEditor(surface);
            editor.Print(area.Left, area.Top, value, _currentAppearance);
            _isDirty = false;
        }
    }

}
