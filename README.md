# RichText.Net

Port of [RichText](https://github.com/skyrpex/RichText) for .NET.

Rich text class for [SFML 2](https://github.com/SFML/SFML/). Allows the
user to draw lines of text with different styles and colors.

## Example

```cs
using System;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace RichText
{
    class Program
    {

        static void OnClose(object sender, EventArgs e)
        {
            (sender as RenderWindow).Close();
        }

        static void Main(string[] args)
        {
            RenderWindow win = new RenderWindow(new VideoMode(800, 600), "RichText");
            win.SetFramerateLimit(30);

            Font f = new Font("FreeMono.ttf");

            RichText t = new RichText(f);

            t += Text.Styles.Bold;           t += Color.Cyan;                 t += "This ";
            t += Text.Styles.Italic;         t += Color.White;                t += "is\nan\n";
            t += Text.Styles.Regular;        t += Color.Green;                t += "example";
            t += Color.White;                t += ".\n";
            t += Text.Styles.Underlined;     t += "It looks good!\n";
            t += Text.Styles.StrikeThrough;  t += new Outline(Color.Blue, 3); t += "Really good!";

            t.SetCharacterSize(25);
            t.Origin = new Vector2f(t.GlobalBounds.Width / 2f, t.GlobalBounds.Height / 2f);
            t.Position = new Vector2f(400, 300);
            

            win.Closed += OnClose;


            while (win.IsOpen)
            {
                win.DispatchEvents();

                win.Clear();
                win.Draw(t);
                win.Display();
            }
        }
    }
}

```