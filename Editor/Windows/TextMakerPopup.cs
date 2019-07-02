using Microsoft.Xna.Framework;
using SadConsole.Controls;
using SadConsole;
using SadConsoleEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace SadConsoleEditor.Windows
{
    public class TextMakerPopup : SadConsole.Window
    {
        DrawingSurface previewPane;
        TextBox textInput;
        Button okButton;
        Button cancelButton;
        ListBox fontsListbox;
        CheckBox useSpacingCheckbox;

        List<TheDraw.Font> fonts;
        TheDraw.Font selectedFont;

        Point selectedFontTitlePosition;
        Point availableCharsPosition;

        public bool UseTransparentBackground;
        public CellSurface SurfaceResult;

        private bool useSpacing { get { return useSpacingCheckbox.IsSelected; } }

        public TextMakerPopup() : base(SadConsoleEditor.Config.Program.TextMakerSettings.WindowWidth, SadConsoleEditor.Config.Program.TextMakerSettings.WindowHeight)
        {
            Center();

            fontsListbox = new ListBox(15, Height - 8, new FontListBoxItemTheme());
            fontsListbox.Position = new Point(2, 4);
            fontsListbox.SelectedItemChanged += FontsListbox_SelectedItemChanged;

            selectedFontTitlePosition = new Point(fontsListbox.Bounds.Right + 16, fontsListbox.Bounds.Top - 2);

            availableCharsPosition = new Point(fontsListbox.Bounds.Right + 23, fontsListbox.Bounds.Top - 1);


            textInput = new TextBox(Width - fontsListbox.Bounds.Right - 15);
            textInput.Position = new Point(fontsListbox.Bounds.Right + 7, fontsListbox.Bounds.Top + 1);
            textInput.TextChanged += TextInput_TextChanged;


            previewPane = new DrawingSurface(Width - fontsListbox.Bounds.Right - 3, Height - fontsListbox.Bounds.Top - 8);
            previewPane.OnDraw = (d) =>
            {
                if (textInput.Text == "")
                {
                    d.Surface.Fill(Color.Black, Color.White, 0, SpriteEffects.FlipHorizontally);
                }
                else
                {
                    d.Surface.Fill(Color.White, Color.Black, 0, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
                    string clearString = new string(' ', Width - 1 - selectedFontTitlePosition.X);
                    Print(selectedFontTitlePosition.X, selectedFontTitlePosition.Y, clearString);
                    Print(selectedFontTitlePosition.X, selectedFontTitlePosition.Y, selectedFont.Title);

                    int x = 0;
                    int y = 0;
                    int highestHeight = 0;
                    foreach (var item in textInput.Text)
                    {
                        var result = selectedFont.GetSurface(item);

                        if (result != null)
                        {
                            if (x + result.Width > previewPane.Width)
                            {
                                y += highestHeight;
                                highestHeight = 0;
                                x = 0;
                            }

                            if (y + result.Height > previewPane.Height)
                            {
                                continue;
                            }

                            result.Copy(d.Surface, x, y);
                            x += result.Width + (useSpacing ? selectedFont.LetterSpacing : 0);

                            if (result.Height > highestHeight)
                                highestHeight = result.Height;
                        }
                    }
                }
            };
            previewPane.Position = new Point(fontsListbox.Bounds.Right + 1, fontsListbox.Bounds.Top + 4);

            okButton = new Button(8);
            okButton.Position = new Point(Width - okButton.Width - 2, Height - 3);
            okButton.Click += (o, e) => { DialogResult = true; BuildFinalResult(); Hide(); };
            okButton.Text = "Ok";

            cancelButton = new Button(8);
            cancelButton.Position = new Point(2, Height - 3);
            cancelButton.Text = "Cancel";
            cancelButton.Click += (o, e) => { DialogResult = false; Hide(); };


            useSpacingCheckbox = new CheckBox(16, 1);
            useSpacingCheckbox.Text = "Use Spacing";
            useSpacingCheckbox.Position = new Point(cancelButton.Bounds.Right + 2, cancelButton.Position.Y);
            useSpacingCheckbox.IsSelectedChanged += (o, e) => DrawText();


            Add(useSpacingCheckbox);
            Add(textInput);
            Add(previewPane);
            Add(fontsListbox);
            Add(okButton);
            Add(cancelButton);

            fonts = new List<TheDraw.Font>();

            foreach (var file in System.IO.Directory.GetFiles("TheDraw"))
                fonts.AddRange(TheDraw.Font.ReadFonts(file));

            foreach (var font in fonts)
                fontsListbox.Items.Add(font);

            fontsListbox.SelectedItem = fontsListbox.Items[0];
            textInput.Text = "Example!";

            Title = "Text Maker";
        }

        private void BuildFinalResult()
        {
            int x = 0;
            int tempHeight = 0;
            int height = 0;
            int width = 0;

            List<TheDraw.Character> characters = new List<TheDraw.Character>(textInput.Text.Length);

            foreach (var item in textInput.Text)
            {
                if (selectedFont.IsCharacterSupported(item))
                {
                    var character = selectedFont.GetCharacter(item);
                    characters.Add(character);
                    width += character.Width + (useSpacing ? selectedFont.LetterSpacing : 0);

                    tempHeight = character.Rows.Length;

                    if (tempHeight > height)
                        height = tempHeight;
                }
            }

            SurfaceResult = new CellSurface(width - 1, height);

            foreach (var item in characters)
            {
                selectedFont.GetSurface(item.GlyphIndex).Copy(SurfaceResult, x, 0);
                x += item.Width + (useSpacing ? selectedFont.LetterSpacing : 0);
            }
        }

        private void TextInput_TextChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void FontsListbox_SelectedItemChanged(object sender, ListBox.SelectedItemEventArgs e)
        {
            selectedFont = (TheDraw.Font)e.Item;

            Invalidate();
        }

        private void DrawText()
        {
            
        }

        private void PrintValidCharacters()
        {
            
        }

        public override void Invalidate()
        {
            base.Invalidate();

            Print(fontsListbox.Bounds.Left, fontsListbox.Bounds.Top - 2, "Fonts", Theme.Colors.TitleText);
            Print(fontsListbox.Bounds.Left, fontsListbox.Bounds.Top - 1, new string((char)196, fontsListbox.Width));
            Print(fontsListbox.Bounds.Right + 1, fontsListbox.Bounds.Top - 2, "Selected font: ", Theme.Colors.TitleText);
            Print(fontsListbox.Bounds.Right + 1, fontsListbox.Bounds.Top - 1, "Available characters: ", Theme.Colors.TitleText);
            Print(fontsListbox.Bounds.Right + 1, fontsListbox.Bounds.Top + 1, "Text: ", Theme.Colors.TitleText);
            Print(fontsListbox.Bounds.Right + 1, fontsListbox.Bounds.Top + 3, "Preview", Theme.Colors.TitleText);

            if (selectedFont != null)
            {
                if (selectedFont != null)
                {
                    for (int i = 0; i < 47; i++)
                    {
                        SetGlyph(availableCharsPosition.X + i, availableCharsPosition.Y, 33 + i);
                        SetForeground(availableCharsPosition.X + i, availableCharsPosition.Y, selectedFont.CharactersSupported[i] ? Theme.Colors.Green : Theme.Colors.Text);
                        SetGlyph(availableCharsPosition.X + i, availableCharsPosition.Y + 1, 33 + 47 + i);
                        SetForeground(availableCharsPosition.X + i, availableCharsPosition.Y + 1, selectedFont.CharactersSupported[i + 47] ? Theme.Colors.Green : Theme.Colors.Text);
                    }
                }
            }

            previewPane.IsDirty = true;
        }
    }

    class FontListBoxItemTheme : SadConsole.Themes.ListBoxItemTheme
    {
        public override void Draw(CellSurface surface, Rectangle area, object item, ControlStates itemState)
        {
            string value = ((TheDraw.Font)item).Title;
            if (value.Length < area.Width)
                value += new string(' ', area.Width - value.Length);
            else if (value.Length > area.Width)
                value = value.Substring(0, area.Width);
            
            surface.Print(area.Left, area.Top, value, GetStateAppearance(itemState));
        }
    }

}
