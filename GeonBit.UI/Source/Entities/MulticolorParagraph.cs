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
    /// <summary>Hold color instructions for MultiColor paragraphs.</summary>
    public class ColorInstruction
    {
        // should we use the paragraph original color?
        private bool _useFillColor = false;

        // color for this instruction
        private Color _color = Color.White;

        // dictionary with available colors and the code to use them
        internal static Dictionary<string, Color> _colors = new Dictionary<string, Color>()
        {
            { "RED", Color.Red },
            { "BLUE", Color.Blue },
            { "GREEN", Color.Green },
            { "YELLOW", Color.Yellow },
            { "BROWN", Color.Brown },
            { "BLACK", Color.Black },
            { "WHITE", Color.White },
            { "CYAN", Color.Cyan },
            { "PINK", Color.Pink },
            { "GRAY", Color.Gray },
            { "MAGENTA", Color.Magenta },
            { "ORANGE", Color.Orange },
            { "PURPLE", Color.Purple },
            { "SILVER", Color.Silver },
            { "GOLD", Color.Gold },
            { "TEAL", Color.Teal },
            { "NAVY", Color.Navy },
        };

        /// <summary>
        /// Add a custom color we can use in the multicolor paragraph.
        /// You can also override one of the built-in colors.
        /// </summary>
        /// <param name="key">Color key to use this color.</param>
        /// <param name="color">Color to set.</param>
        public static void AddCustomColor(string key, Color color)
        {
            _colors[key] = color;
        }

        /// <summary>Constructor to use when creating a color instruction.</summary>
        /// <param name="sColor">The string representation of the color to use for rendering.</param>
        public ColorInstruction(string sColor)
        {
            // use default paragraph fill color
            if (sColor == "DEFAULT")
            {
                _useFillColor = true;
            }
            // change to custom color
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
            Color outColor;
            if (!_colors.TryGetValue(sColor, out outColor))
            {
                if (UserInterface.Active.SilentSoftErrors) return Color.White;
                throw new Exceptions.InvalidValueException("Unknown color code '" + sColor + "'.");
            }
            return outColor;
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
    /// Multicolor Paragraph is a paragraph that supports in-text color tags that changes the fill color of the text.
    /// </summary>
    public class MulticolorParagraph : Paragraph
    {
        /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        // are color instructions currently enabled
        bool _enableColorInstructions = true;

        /// <summary>
        /// If true, will enable color instructions (special codes that change fill color inside the text).
        /// </summary>
        public bool EnableColorInstructions
        {
            // get if color instructions are enabled
            get
            {
                return _enableColorInstructions;
            }

            // set if color instructions are enabled and parse instructions if needed
            set
            {
                _enableColorInstructions = value;
                ParseColorInstructions();
            }
        }

        /// <summary>Get / Set the paragraph text.</summary>
        public override string Text
        {
            get { return _text; }
            set { if (_text != value) { _text = value; ParseColorInstructions(); MarkAsDirty(); } }
        }

        // color-changing instructions in current paragraph. key is char index, value is color change command.
        Dictionary<int, ColorInstruction> _colorInstructions = new Dictionary<int, ColorInstruction>();

        /// <summary>
        /// Create the paragraph.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="scale">Optional font size.</param>
        public MulticolorParagraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null, float? scale = null) :
            base(text, anchor, size, offset, scale)
        {
        }


        /// <summary>
        /// Create the paragraph with optional fill color and font size.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="color">Text fill color.</param>
        /// <param name="scale">Optional font size.</param>
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
            // clear previous color instructions
            _colorInstructions.Clear();

            // if color instructions are disabled, stop here
            if (!EnableColorInstructions) { return; }

            // find and parse color instructions
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
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // get outline width
            int outlineWidth = OutlineWidth;

            // if we got outline draw it
            if (outlineWidth > 0)
            {
                // get outline color
                Color outlineColor = UserInterface.Active.DrawUtils.FixColorOpacity(OutlineColor);

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
                    ColorInstruction colorInstruction;
                    if (_colorInstructions.TryGetValue(iTextIndex, out colorInstruction))
                    {
                        if (colorInstruction.UseFillColor)
                        {
                            oColor = FillColor;
                        }
                        else
                        {
                            oColor = colorInstruction.Color;
                        }
                    }

                    if (cCharacter == '\n')
                    {
                        oCurrentPosition.X = _position.X - oCharacterSize.X;
                        oCurrentPosition.Y += _currFont.LineSpacing * _actualScale;
                    }
                    else
                    {
                        iTextIndex++;
                        oCurrentPosition.X += oCharacterSize.X;
                    }

                    // fix color opacity and draw
                    Color fillCol = UserInterface.Active.DrawUtils.FixColorOpacity(oColor);
                    spriteBatch.DrawString(_currFont, cCharacter.ToString(), oCurrentPosition, fillCol, 0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                }
            }
            // if there are no color-changing instructions, just draw the paragraph as-is
            else
            {
                base.DrawEntity(spriteBatch, phase);
            }
        }
    }
}
