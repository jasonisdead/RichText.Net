using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;


namespace RichText
{
    public class TextStroke
    {
        public Color fill = Color.White;
        public Color outline = Color.Transparent;
        public float thickness = 0;

    }

    public class Outline
    {
        public Color outline;
        public float thickness = 0;

        public Outline(Color ol, float thick)
        {
            outline = ol;
            thickness = thick;
        }
    }
    public class RichText : Transformable, Drawable
    {
        public class Line : Transformable, Drawable
        {
            // PUBLIC
            
            public uint CharacterSize 
            {    
                get => texts.FirstOrDefault().CharacterSize; 
                set
                {
                    foreach (var text in texts)
                    {
                        text.CharacterSize = value;
                    }
                    UpdateGeometry();
                }
            }
            public List<Text> Texts {get => texts;}
            public uint Length
            {
                get
                {
                    uint count = 0;
                    foreach (var text in texts)
                    {
                        count += (uint)text.DisplayedString.Length;
                    }
                    return count;
                }
            }
            public Font Font
            {
                get => texts.FirstOrDefault().Font;
                set
                {
                    foreach (var text in texts)
                    {
                        text.Font = value;
                    }
                    UpdateGeometry();
                }
            }
            public FloatRect LocalBounds {get => bounds;}
            public FloatRect GlobalBounds {get => Transform.TransformRect(bounds);}

            public void SetCharacterColor(int pos, Color color)
            {
                IsolateCharacter(pos);
                int stringToFormat = ConvertLinePosToLocal(pos);
                texts[stringToFormat].FillColor = color;
                UpdateGeometry();
            }

            public void SetCharacterStyle(int pos, Text.Styles style)
            {
                IsolateCharacter(pos);
                int stringToFormat = ConvertLinePosToLocal(pos);
                texts[stringToFormat].Style = style;
                UpdateGeometry();
            }

            public void SetCharacter(int pos, string character)
            {
                Text text = texts[ConvertLinePosToLocal(pos)];
                string str = text.DisplayedString;
                str = str.Remove(pos, 1).Insert(pos, character);
                text.DisplayedString = str;
                UpdateGeometry();
            }

            public Color GetCharacterColor(int pos)
            {
                return texts[ConvertLinePosToLocal(pos)].FillColor;
            }

            public Text.Styles GetCharacterStyle(int pos)
            {
                return texts[ConvertLinePosToLocal(pos)].Style;
            }

            public ushort GetCharacter(int pos)
            {
                Text text = texts[ConvertLinePosToLocal(pos)];
                return text.DisplayedString[pos];
            }

            public void AppendText(Text text)
            {
                UpdateTextAndGeometry(text);
                texts.Add(text);
            }

            public void Draw(RenderTarget target, RenderStates states)
            {
                states.Transform *= Transform;

                foreach (Text text in texts)
                {
                    target.Draw(text, states);
                }
            }
            
            // PRIVATE
            private int ConvertLinePosToLocal(int pos)
            {
                int arrayIndex = 0;
                for (; pos >= texts[arrayIndex].DisplayedString.Length; arrayIndex++)
                {
                    pos -= texts[arrayIndex].DisplayedString.Length;
                }
                return arrayIndex;
            }

            private void IsolateCharacter(int pos)
            {
                int localPos = pos;
                int index = ConvertLinePosToLocal(localPos);
                Text temp = texts[index];

                string str = temp.DisplayedString;
                if (str.Length == 1) return;

                texts.RemoveAt(index);
                if (localPos != str.Length - 1)
                {
                    temp.DisplayedString = str.Substring(localPos+1);
                    texts.Insert(index, temp);
                }

                temp.DisplayedString = str.Substring(localPos, 1);
                texts.Insert(index, temp);

                if (localPos != 0)
                {
                    temp.DisplayedString = str.Substring(0, localPos);
                    texts.Insert(index, temp);
                }
            }

            private void UpdateGeometry()
            {
                bounds = new FloatRect();
                foreach (var text in Texts)
                {
                    UpdateTextAndGeometry(text);
                }
            }
            private void UpdateTextAndGeometry(Text text)
            {
                // set text offset
                text.Position = new Vector2f(bounds.Width, 0);

                // Update bounds
                float lineSpacing = (float)Math.Floor(text.Font.GetLineSpacing(text.CharacterSize));
                bounds.Height = Math.Max(bounds.Height, lineSpacing);
                bounds.Width = text.GetGlobalBounds().Width;
            }
            private List<Text> texts = new List<Text>();
            private FloatRect bounds;
        }

        // PUBLIC
        public uint CharacterSize {get => characterSize;}
        public FloatRect LocalBounds {get => bounds;}
        public FloatRect GlobalBounds
        {
            get
            {
                return Transform.TransformRect(LocalBounds);
            }
        }
        public List<Line> Lines {get;}
        public Font Font {get => font;}

        public RichText() {}

        public RichText(Font f)
        {
            font = f;
            characterSize = 30;
            currentStroke = new TextStroke();
            currentStyle = Text.Styles.Regular;
        }

        public static RichText operator +(RichText rt, TextStroke stroke)
        {
            rt.currentStroke = stroke;
            return rt;
        }

        public static RichText operator +(RichText rt, Outline outline)
        {
            rt.currentStroke.outline = outline.outline;
            rt.currentStroke.thickness = outline.thickness;
            return rt;
        }
        public static RichText operator +(RichText rt, Color color)
        {
            rt.currentStroke.fill = color;
            return rt;
        }
        public static RichText operator +(RichText rt, Text.Styles style)
        {
            rt.currentStyle = style;
            return rt;
        }
        public static RichText operator +(RichText rt, string str)
        { 

            // Maybe skip
            if (str == "") return rt;

            // Split into substrings
            List<string> subStrings = new List<string>(str.Split('\n'));
            
            //Append first substring using the last line
            
            foreach (string sub in subStrings)
            {
                Line line;
                if (sub == subStrings.FirstOrDefault())
                {
                    // if there isn't lines, make it
                    if (rt.lines.Count == 0) rt.lines.Add(new Line());

                    // Remove last line's height
                    line = rt.lines.Last();
                    rt.bounds.Height -= line.GlobalBounds.Height;

                    // Append text
                    line.AppendText(rt.CreateText(subStrings.FirstOrDefault()));

                    // update Bounds
                    rt.bounds.Height += line.GlobalBounds.Height;
                    rt.bounds.Width = Math.Max(rt.bounds.Width,
                     line.GlobalBounds.Width);
                }
                else
                {
                    //Append the rest of the substrings as new lines
                    line = new Line();
                    line.Position = new Vector2f(0, rt.bounds.Height);
                    line.AppendText(rt.CreateText(sub));
                    rt.lines.Add(line);
                    
                    // Update bounds
                    rt.bounds.Height += line.GlobalBounds.Height;
                    rt.bounds.Width = Math.Max(rt.bounds.Width, line.GlobalBounds.Width);
                }
                
                
            }

            return rt;
        }

        public void SetCharacterColor(uint line, uint pos, Color color)
        {
            lines[(int)line].SetCharacterColor((int)pos, color);
            UpdateGeometry();
        }

        public void SetCharacterStyle(uint line, uint pos, Text.Styles style)
        {
            lines[(int)line].SetCharacterStyle((int)pos, style);
            UpdateGeometry();
        }
        public void SetCharacter(uint line, uint pos, string character)
        {
            lines[(int)line].SetCharacter((int)pos, character);
            UpdateGeometry();
        }

        public void SetCharacterSize(uint size)
        {
            //Maybe skip
            if (CharacterSize == size) return;

            //Update character size
            characterSize = size;

            // Set texts character size
            foreach (Line line in lines)
            {
                line.CharacterSize = size;
            }

            UpdateGeometry();
        }

        public void SetFont(Font f)
        {
            // Maybe skip
            if (font == f) return;

            //Update font
            font = f;

            // Set texts font
            foreach (Line line in lines)
            {
                line.Font = f;
            }

            UpdateGeometry();
        }
        public void Clear()
        {
            // Clear texts
            lines.Clear();
            // Reset Bounds
            bounds = new FloatRect();
        }

        public Color GetCharacterColor(ushort line, ushort pos)
        {
            return lines[(int)line].GetCharacterColor(pos);
        }
        public Text.Styles GetCharacterStyle(ushort line, ushort pos)
        {
            return lines[(int)line].GetCharacterStyle(pos);
        }

        public ushort GetCharacter(ushort line, ushort pos)
        {
            return lines[(int)line].GetCharacter(pos);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;

            foreach (Line line in lines)
            {
                target.Draw(line, states);
            }
        }

        // PRIVATE
        private Text CreateText(string str)
        {
            Text text = new Text();
            text.DisplayedString = str;
            text.FillColor = currentStroke.fill;
            text.OutlineColor = currentStroke.outline;
            text.OutlineThickness = currentStroke.thickness;
            text.Style = currentStyle;
            text.CharacterSize = characterSize;
            if (font is Font) text.Font = font;
            return text;

        }

        private void UpdateGeometry()
        {
            bounds = new FloatRect();
            foreach (Line line in lines)
            {
                line.Position = new Vector2f(0, bounds.Height);

                bounds.Height += line.GlobalBounds.Height;
                bounds.Width = Math.Max(bounds.Width, line.GlobalBounds.Width);
            }
        }

        private List<Line> lines = new List<Line>();
        private Font font;
        private FloatRect bounds;
        private TextStroke currentStroke;
        private Text.Styles currentStyle;
        private uint characterSize;

    }
}