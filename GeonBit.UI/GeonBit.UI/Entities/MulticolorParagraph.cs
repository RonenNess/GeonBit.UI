#region File Description
//-----------------------------------------------------------------------------
// A paragraph extension that support multiple fill colors (change colors via
// special color commands).
//
// Author: Justin Gattuso, Ronen Ness.
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
    /// <summary>A simple class structure to hold color and default fill color instructions</summary>
    public class ColorInstruction
    {
        private bool _useFillColor = false;
        private Color _color = Color.White;

        /// <summary>Constructor to use when creating a color instruction.</summary>
        /// <param name="sColor">The string representation of the color to use for rendering.</param>
        public ColorInstruction(string sColor)
        {
            sColor = sColor.ToUpper();

            if (sColor == "DEFAULT")
            {
                _useFillColor = true;
            }
            else
            {
                _color = StringToColor(sColor);
            }
        }

        /// <summary>
        /// Converts a supported color string to its respective color.
        /// Supported colors are as follows:
        /// red, blue, green, yellow, brown, black, white, cyan, pink, gray, magenta, orange, purple, silver, gold, teal or default (White)
        /// </summary>
        /// <param name="sColor">The color represented as a string value; any invalid value will default to White.</param>
        /// <returns>The actual color object or White as the fallback default.</returns>
        public Color StringToColor(string sColor)
        {
            sColor = sColor.ToUpper();

            switch (sColor)
            {
                case "RED":
                    return Color.Red;
                case "BLUE":
                    return Color.Blue;
                case "GREEN":
                    return Color.Green;
                case "YELLOW":
                    return Color.Yellow;
                case "BROWN":
                    return Color.Brown;
                case "BLACK":
                    return Color.Black;
                case "WHITE":
                    return Color.White;
                case "CYAN":
                    return Color.Cyan;
                case "PINK":
                    return Color.Pink;
                case "GRAY":
                    return Color.Gray;
                case "MAGENTA":
                    return Color.Magenta;
                case "ORANGE":
                    return Color.Orange;
                case "PURPLE":
                    return Color.Purple;
                case "SILVER":
                    return Color.Silver;
                case "GOLD":
                    return Color.Gold;
                case "TEAL":
                    return Color.Teal;
                default:
                    return Color.White;
            }
        }

        /// <summary>Flag represents whether or not to use the default FillColor for this instruction.</summary>
        public bool UseFillColor
        {
            get { return _useFillColor; }
        }

        /// <summary>If UseFillColor is false the color here is used for the instruction.</summary>
        public Color Color
        {
            get { return _color; }
        }
    }

    /// <summary>
    /// Paragraph is a renderable text. It can be multiline, wrap words, have outline, etc.
    /// </summary>
    public class MulticolorParagraph : Paragraph
    {
        /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Get / Set the paragraph text.</summary>
        public override string Text
        {
            get { return _text; }
            set { if (_text != value) { _text = value; ParseColorInstructions(); MarkAsDirty(); } }
        }

        // color-changing instructions in current paragraph
        Dictionary<int, ColorInstruction> _colorInstructions = new Dictionary<int, ColorInstruction>();

        /// <summary>
        /// Create the paragraph.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        public MulticolorParagraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null) :
            base(text, anchor, size, offset)
        {
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
        public MulticolorParagraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null, Vector2? offset = null) :
            base(text, anchor, color, scale, size, offset)
        {
        }

        /// <summary>
        /// Parse special color-changing instructions inside the text.
        /// </summary>
        private void ParseColorInstructions()
        {
            _colorInstructions.Clear();
            if (_text.Contains("{{"))
            {
                int iLastLength = 0;
                System.Text.RegularExpressions.Regex oRegex = new System.Text.RegularExpressions.Regex("{{[^{}]*}}");

                System.Text.RegularExpressions.MatchCollection oMatches = oRegex.Matches(_text);
                foreach (System.Text.RegularExpressions.Match oMatch in oMatches)
                {
                    string sColor = oMatch.Value.Substring(2, oMatch.Value.Length - 4);

                    _colorInstructions.Add(oMatch.Index - iLastLength, new ColorInstruction(sColor));
                    iLastLength += oMatch.Value.Length;
                }

                // Strip out the color instructions from the text to allow the rest of processing to process the actual text content
                _text = oRegex.Replace(_text, string.Empty);
            }
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

            // if there are color changing instructions in paragraph, draw with color changes
            if (_colorInstructions.Count > 0)
            {
                int iTextIndex = 0;
                Color oColor = FillColor;
                Vector2 oCharacterSize = GetCharacterActualSize();
                Vector2 oCurrentPosition = new Vector2(_position.X - oCharacterSize.X, _position.Y);
                foreach (char cCharacter in _processedText)
                {
                    if (_colorInstructions.ContainsKey(iTextIndex))
                    {
                        if (_colorInstructions[iTextIndex].UseFillColor)
                        {
                            oColor = FillColor;
                        }
                        else
                        {
                            oColor = _colorInstructions[iTextIndex].Color;
                        }
                    }

                    if (cCharacter == '\n')
                    {
                        oCurrentPosition.X = _position.X - oCharacterSize.X;
                        oCurrentPosition.Y += _currFont.LineSpacing;
                    }
                    else
                    {
                        iTextIndex++;
                        oCurrentPosition.X += oCharacterSize.X;
                    }

                    spriteBatch.DrawString(_currFont, cCharacter.ToString(), oCurrentPosition, oColor, 0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                }
            }
            // if there are no color-changing instructions, just draw the text as-is
            else
            {
                // draw text itself
                spriteBatch.DrawString(_currFont, _processedText, _position, FillColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
            }

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}
