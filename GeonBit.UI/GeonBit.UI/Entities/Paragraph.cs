﻿#region File Description
//-----------------------------------------------------------------------------
// Paragraph is a simple text to display.
// It support multilines, outline color, different colors for when mouse hover
// or click, auto word wrap, and align to center.
//
// Note that by default paragraph align based on its anchor, eg. anchoring right
// will align right, left will align left, and anything with center will align
// to center. This behavior can be overrided with the AlignCenter property.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI.DataTypes;
using System.Text;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Font styles.
    /// </summary>
    public enum FontStyle
    {
        /// <summary>Regular font.</summary>
        Regular,
        /// <summary>Bold font.</summary>
        Bold,
        /// <summary>Italic font.</summary>
        Italic
    };

    /// <summary>
    /// Paragraph is a renderable text. It can be multiline, wrap words, have outline, etc.
    /// </summary>
    public class Paragraph : Entity
    {
        /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        // the paragraph displayed text.
        private string _text = "";

        /// <summary>Get / Set the paragraph text.</summary>
        public string Text
        {
            get { return _text; }
            set { if (_text != value) { _text = value; MarkAsDirty(); } }
        }

        // is wrap-words enabled?
        private bool _wrapWords = true;

        /// <summary>
        /// Get / Set word wrap mode.
        /// If true, and text exceeded destination width, the paragraph will wrap words by adding line breaks where needed.
        /// </summary>
        public bool WrapWords
        {
            get { return _wrapWords; }
            set { _wrapWords = value; MarkAsDirty(); }
        }

        // text actual destination rect
        Rectangle _actualDestRect = new Rectangle();

        /// <summary>If the outline width is less than this value, the outline will be optimized but will appear slightly less sharp on corners.</summary>
        protected static int MaxOutlineWidthToOptimize = 1;

        // internal calculated values like position, origin, wrapped text, etc..
        string _processedText;
        SpriteFont _currFont;
        float _actualScale;
        Vector2 _position;
        Vector2 _fontOrigin;
        
        // should we break words too long if in wrap mode?
        private bool _breakWordsIfMust = true;

        /// <summary>
        /// If WrapWords is true and there's a word that's too long (eg longer than max width), will break the word in the middle.
        /// If false, word wrap will only break lines in between words (eg spaces) and never break words.
        /// </summary>
        public bool BreakWordsIfMust
        {
            get { return _breakWordsIfMust; }
            set { _breakWordsIfMust = value; MarkAsDirty(); }
        }

        // should we add a hyphen whenever we break words?
        private bool _addHyphenWhenBreakWord = true;

        /// <summary>
        /// If true and a long word is broken due to word wrap, will add hyphen at the breaking point.
        /// </summary>
        public bool AddHyphenWhenBreakWord
        {
            get { return _addHyphenWhenBreakWord; }
            set { _addHyphenWhenBreakWord = value; MarkAsDirty(); }
        }

        /// <summary>Base font size. Change this property to affect the size of all paragraphs and other text entities.</summary>
        public static float BaseSize = 1f;

        /// <summary>
        /// Create the paragraph.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Paragraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            Text = text;
            UpdateStyle(DefaultStyle);
        }


        /// <summary>
        /// Create the paragraph with optional fill color and font size.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="color">Text fill color.</param>
        /// <param name="scale">Font size.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Paragraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null, Vector2? offset = null) :
            this(text, anchor, size, offset)
        {
            SetStyleProperty("FillColor", new StyleProperty(color));
            if (scale != null) { SetStyleProperty("Scale", new StyleProperty((float)scale)); }
        }

        /// <summary>
        /// Get the actual destination rect that this paragraph takes (based on text content, font size, and word wrap).
        /// </summary>
        /// <returns>Actual paragraph destination rect.</returns>
        override public Rectangle GetActualDestRect()
        {
            return _actualDestRect;
        }

        /// <summary>
        /// Get the size, in pixels, of a single character in paragraph.
        /// </summary>
        /// <returns>Actual size, in pixels, of a single character.</returns>
        public Vector2 GetCharacterActualSize()
        {
            SpriteFont font = GetCurrFont();
            float scale = Scale * BaseSize * UserInterface.GlobalScale;
            return font.MeasureString(" ") * scale;
        }

        /// <summary>
        /// Wrap text to fit destination rect.
        /// Most if this code is coming from: http://stackoverflow.com/questions/15986473/how-do-i-implement-word-wrap
        /// </summary>
        /// <param name="font">Font of the text to wrap.</param>
        /// <param name="text">Text content.</param>
        /// <param name="maxLineWidth">Max line width to wrap.</param>
        /// <param name="fontSize">Font scale (scale you are about to use when drawing the text).</param>
        /// <returns>Text that is wrapped to fit the given length (by adding line breaks at the right places).</returns>
        public string WrapText(SpriteFont font, string text, float maxLineWidth, float fontSize)
        {
            // invalid width (can happen during init steps - skip
            if (maxLineWidth <= 0) { return text; }

            // create string to return as result
            StringBuilder ret = new StringBuilder("");

            // if text got line breaks, break into lines and process them seperately
            if (text.Contains("\n"))
            {
                // break into lines
                string[] lines = text.Split('\n');

                // iterate lines and wrap them
                foreach (string line in lines)
                {
                    ret.AppendLine(WrapText(font, line, maxLineWidth, fontSize));
                }

                // remove the last extra linebreak that was added in this process and return.
                ret = ret.Remove(ret.Length - 1, 1);
                return ret.ToString();
            }

            // if got here it means we are processing a single line. break it into words.
            // note: we use a list so we can push words in the middle while iterating (to handle words too long).
            List<string> words = new List<string>(text.Split(' '));

            // iterate words
            int currWidth = 0;
            for (int i = 0; i < words.Count; ++i)
            {
                // get current word and its width
                string word = words[i];
                int wordWidth = (int)(font.MeasureString(word + ' ').X * fontSize);

                // special case: word itself is longer than line width
                if (BreakWordsIfMust && wordWidth >= maxLineWidth && word.Length >= 4)
                {
                    // find breaking position
                    int breakPos = 0;
                    int currWordWidth = (int)(font.MeasureString(" ").X * fontSize);
                    foreach (char c in word)
                    {
                        currWordWidth += (int)(font.MeasureString("" + c).X * fontSize);
                        if (currWordWidth >= maxLineWidth)
                        {
                            break;
                        }
                        breakPos++;
                    }
                    breakPos -= 3;
                    if (breakPos >= word.Length - 1) { breakPos -= 2; }
                    if (breakPos <= 0) { breakPos = 1; }

                    // break the word into two and add to the list of words after this position.
                    // we will process them in next loop iterations.
                    string firstHalf = word.Substring(0, breakPos);
                    string secondHalf = word.Substring(breakPos, word.Length - breakPos);
                    if (AddHyphenWhenBreakWord) { firstHalf += '-'; }
                    words.Insert(i + 1, firstHalf);
                    words.Insert(i + 2, secondHalf);

                    // continue to skip current word (it will be added later, with its broken parts)
                    continue;
                }

                // add to total width
                currWidth += wordWidth;

                // did overflow max width? add line break and reset current width.
                if (currWidth >= maxLineWidth)
                {
                    ret.Append('\n');
                    ret.Append(word);
                    ret.Append(' ');
                    currWidth = wordWidth;
                }
                // if didn't overflow just add the word as-is
                else
                {
                    ret.Append(word);
                    ret.Append(' ');
                }
            }

            // remove the extra space that was appended to the end during the process and return wrapped text.
            ret = ret.Remove(ret.Length - 1, 1);

            // special case - if last word was just the size of the line, it will add a useless trailing \n and create double line breaks.
            // remove that extra line break.
            if (ret.Length > 0 && ret[ret.Length-1] == '\n')
            {
                ret = ret.Remove(ret.Length - 1, 1);
            }

            // return the final wrapped text
            return ret.ToString();
        }

        /// <summary>
        /// Return the processed text that is actually displayed on screen, after word-wrap etc.
        /// </summary>
        /// <returns>Actual displayed text with word-wrap and other runtime processing.</returns>
        public string GetProcessedText()
        {
            return _processedText;
        }

        /// <summary>
        /// Current font style - this is just a sugarcoat to access the default font style property.
        /// </summary>
        public FontStyle TextStyle
        {
            set { SetStyleProperty("FontStyle", new StyleProperty((int)value)); }
            get { return (FontStyle)GetActiveStyle("FontStyle").asInt; }
        }

        /// <summary>
        /// Should we align text to center - this is just a sugarcoat to access the default force-align-to-center style property.
        /// </summary>
        public bool AlignToCenter
        {
            set { SetStyleProperty("ForceAlignCenter", new StyleProperty(value)); }
            get { return GetActiveStyle("ForceAlignCenter").asBool; }
        }

        /// <summary>
        /// Get the currently active font for this paragraph.
        /// </summary>
        /// <returns>Current font.</returns>
        protected SpriteFont GetCurrFont()
        {
            return Resources.Fonts[(int)TextStyle];
        }

        /// <summary>
        /// Update dest rect and internal dest rect.
        /// This is called internally whenever a change is made to the entity or its parent.
        /// </summary>
        override public void UpdateDestinationRects()
        {
            // call base function
            base.UpdateDestinationRects();

            // do extra preperation for text entities
            CalcTextActualRectWithWrap();
        }

        /// <summary>
        /// Calculate the paragraph actual destination rect with word-wrap and other factors taken into consideration.
        /// </summary>
        public void CalcTextActualRectWithWrap()
        {
            // get font
            SpriteFont font = GetCurrFont();
            if (font != _currFont)
            {
                MarkAsDirty();
                _currFont = font;
            }

            // calc actual scale
            float actualScale = Scale * BaseSize * UserInterface.GlobalScale;
            if (actualScale != _actualScale)
            {
                _actualScale = actualScale;
                MarkAsDirty();
            }

            // get text and add things like line-breaks to wrap words etc.
            string newProcessedText = Text;
            if (WrapWords)
            {
                newProcessedText = WrapText(font, newProcessedText, _destRect.Width, _actualScale);
                newProcessedText = newProcessedText.TrimEnd(' ');
            }

            // if processed text changed
            if (newProcessedText != _processedText)
            {
                _processedText = newProcessedText;
                MarkAsDirty();
            }

            // due to the mechanism of calculating destination rect etc based on parent and anchor,
            // to set text alignment all we need to do is keep the size the actual text size.
            // so we just update _size every frame and the text alignemtn (left, right, center..) fix itself by the destination rect.
            _fontOrigin = Vector2.Zero;
            _position = new Vector2(_destRect.X, _destRect.Y);
            Vector2 size = font.MeasureString(_processedText);

            // set position and origin based on anchor.
            // note: no top-left here because thats the default set above.
            bool alreadyCentered = false;
            switch (_anchor)
            {
                case Anchor.Center:
                    _fontOrigin = size / 2;
                    _position += new Vector2(_destRect.Width / 2, _destRect.Height / 2);
                    alreadyCentered = true;
                    break;
                case Anchor.AutoCenter:
                case Anchor.TopCenter:
                    _fontOrigin = new Vector2(size.X / 2, 0);
                    _position += new Vector2(_destRect.Width / 2, 0f);
                    alreadyCentered = true;
                    break;
                case Anchor.TopRight:
                    _fontOrigin = new Vector2(size.X, 0);
                    _position += new Vector2(_destRect.Width, 0f);
                    break;
                case Anchor.BottomCenter:
                    _fontOrigin = new Vector2(size.X / 2, size.Y);
                    _position += new Vector2(_destRect.Width / 2, _destRect.Height);
                    alreadyCentered = true;
                    break;
                case Anchor.BottomRight:
                    _fontOrigin = new Vector2(size.X, size.Y);
                    _position += new Vector2(_destRect.Width, _destRect.Height);
                    break;
                case Anchor.BottomLeft:
                    _fontOrigin = new Vector2(0f, size.Y);
                    _position += new Vector2(0f, _destRect.Height);
                    break;
                case Anchor.CenterLeft:
                    _fontOrigin = new Vector2(0f, size.Y / 2);
                    _position += new Vector2(0f, _destRect.Height / 2);
                    break;
                case Anchor.CenterRight:
                    _fontOrigin = new Vector2(size.X, size.Y / 2);
                    _position += new Vector2(_destRect.Width, _destRect.Height / 2);
                    break;
            }

            // force center align
            if (AlignToCenter && !alreadyCentered)
            {
                _fontOrigin.X = size.X / 2;
                _position.X = _destRect.X + _destRect.Width / 2;
            }

            // set actual height
            _actualDestRect.X = (int)_position.X;
            _actualDestRect.Y = (int)_position.Y;
            _actualDestRect.Width = (int)((_fontOrigin.X + size.X) * _actualScale);
            _actualDestRect.Height = (int)((_fontOrigin.Y + size.Y) * _actualScale);
        }

        /// <summary>
        /// Draw entity outline. Note: in paragraph its a special case and we implement it inside the DrawEntity function.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntityOutline(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // get outline width
            int outlineWidth = OutlineWidth;

            // if we got outline draw it
            if (outlineWidth > 0)
            {
                // get outline color
                Color outlineColor = OutlineColor;

                // for not-too-thick outline we render just two corners
                if (outlineWidth <= MaxOutlineWidthToOptimize)
                {
                    spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.One * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                    spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.One * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                }
                // for really thick outline we need to cover the other corners as well
                else
                {
                    spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.UnitX * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                    spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.UnitX * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                    spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.UnitY * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                    spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.UnitY * outlineWidth, outlineColor,
                        0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                }
            }

            // draw text itself
            spriteBatch.DrawString(_currFont, _processedText, _position, FillColor,
                0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}
