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
    /// <summary>
    /// Hold style changes instructions for rich paragraphs.
    /// </summary>
    [System.Serializable]
    public class RichParagraphStyleInstruction
    {
        /// <summary>
        /// Add a style insturctions you could later use in rich paragraphs.
        /// </summary>
        /// <param name="key">Key to invoke this style change.</param>
        /// <param name="instruction">Style change data.</param>
        public static void AddInstruction(string key, RichParagraphStyleInstruction instruction)
        {
            _instructions[key] = instruction;
        }

        /// <summary>
        /// Create a rich paragraph style instruction to change font style, color, or other properties.
        /// </summary>
        /// <param name="fillColor"></param>
        /// <param name="fontStyle"></param>
        /// <param name="resetStyles"></param>
        public RichParagraphStyleInstruction(Color? fillColor = null, FontStyle? fontStyle = null, bool resetStyles = false)
        {
            ResetStyles = resetStyles;
            FillColor = fillColor;
            FontStyle = fontStyle;
        }

        /// <summary>
        /// Will change text fill color.
        /// </summary>
        public Color? FillColor { get; private set; }

        /// <summary>
        /// Will change font style.
        /// </summary>
        public FontStyle? FontStyle { get; private set; }

        /// <summary>
        /// If true, will reset all custom styles before applying this instruction.
        /// </summary>
        public bool ResetStyles { get; private set; }

        /// <summary>
        /// Dictionary with all available rich paragraph style instructions.
        /// </summary>
        internal static Dictionary<string, RichParagraphStyleInstruction> _instructions = new Dictionary<string, RichParagraphStyleInstruction>();

        /// <summary>
        /// Set the instructions denote to use, ie symbols that tells us the in-between text is a style-change instruction.
        /// </summary>
        /// <param name="opening">Opening denote string.</param>
        /// <param name="closing">Closing denote string.</param>
        public static void SetStyleInstructionsDenotes(string opening, string closing)
        {
            _styleInstructionsRegex = new System.Text.RegularExpressions.Regex(opening + "[^{}]*" + closing);
            _styleInstructionsOpening = opening;
            _styleInstructionsClosing = closing;
        }

        // regex to find color instructions
        static internal System.Text.RegularExpressions.Regex _styleInstructionsRegex;

        // the style instruction opening denote string
        static internal string _styleInstructionsOpening;

        // the style instruction closing denote string
        static internal string _styleInstructionsClosing;

        /// <summary>
        /// Init default built-in style instructions.
        /// </summary>
        static RichParagraphStyleInstruction()
        {
            // set style instructions denotes
            SetStyleInstructionsDenotes("{{", "}}");

            // reset all properties
            AddInstruction("DEFAULT", new RichParagraphStyleInstruction(resetStyles: true));

            // add color-changing instructions
            AddInstruction("RED", new RichParagraphStyleInstruction(fillColor: Color.Red));
            AddInstruction("BLUE", new RichParagraphStyleInstruction(fillColor: Color.Blue));
            AddInstruction("GREEN", new RichParagraphStyleInstruction(fillColor: Color.Green));
            AddInstruction("YELLOW", new RichParagraphStyleInstruction(fillColor: Color.Yellow));
            AddInstruction("BROWN", new RichParagraphStyleInstruction(fillColor: Color.Brown));
            AddInstruction("BLACK", new RichParagraphStyleInstruction(fillColor: Color.Black));
            AddInstruction("WHITE", new RichParagraphStyleInstruction(fillColor: Color.White));
            AddInstruction("CYAN", new RichParagraphStyleInstruction(fillColor: Color.Cyan));
            AddInstruction("PINK", new RichParagraphStyleInstruction(fillColor: Color.Pink));
            AddInstruction("GRAY", new RichParagraphStyleInstruction(fillColor: Color.Gray));
            AddInstruction("MAGENTA", new RichParagraphStyleInstruction(fillColor: Color.Magenta));
            AddInstruction("ORANGE", new RichParagraphStyleInstruction(fillColor: Color.Orange));
            AddInstruction("PURPLE", new RichParagraphStyleInstruction(fillColor: Color.Purple));
            AddInstruction("SILVER", new RichParagraphStyleInstruction(fillColor: Color.Silver));
            AddInstruction("GOLD", new RichParagraphStyleInstruction(fillColor: Color.Gold));
            AddInstruction("TEAL", new RichParagraphStyleInstruction(fillColor: Color.Teal));
            AddInstruction("NAVY", new RichParagraphStyleInstruction(fillColor: Color.Navy));

            // add font style change instructions
            AddInstruction("BOLD", new RichParagraphStyleInstruction(fontStyle: Entities.FontStyle.Bold));
            AddInstruction("REGULAR", new RichParagraphStyleInstruction(fontStyle: Entities.FontStyle.Regular));
            AddInstruction("ITALIC", new RichParagraphStyleInstruction(fontStyle: Entities.FontStyle.Italic));
        }
    }

    /// <summary>
    /// Multicolor Paragraph is a paragraph that supports in-text color tags that changes the fill color of the text.
    /// </summary>
    [System.Serializable]
    public class RichParagraph : Paragraph
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static RichParagraph()
        {
            Entity.MakeSerializable(typeof(RichParagraph));
        }

        /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        // are style-changing instructions currently enabled
        bool _enableStyleInstructions = true;

        /// <summary>
        /// If true, will enable style-changing instructions.
        /// </summary>
        public bool EnableStyleInstructions
        {
            get
            {
                return _enableStyleInstructions;
            }
            set
            {
                _enableStyleInstructions = value;
                _needUpdateStyleInstructions = true;
            }
        }

        /// <summary>Get / Set the paragraph text.</summary>
        public override string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    MarkAsDirty();
                    if (EnableStyleInstructions) _needUpdateStyleInstructions = true;
                }
            }
        }

        // do we need to update color-related stuff?
        private bool _needUpdateStyleInstructions = true;

        /// <summary>
        /// List of parsed style-changing instructions in this paragraph, and the position they apply.
        /// </summary>
        Dictionary<int, RichParagraphStyleInstruction> _styleInstructions = new Dictionary<int, RichParagraphStyleInstruction>();

        /// <summary>
        /// Create the paragraph.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="scale">Optional font size.</param>
        public RichParagraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null, float? scale = null) :
            base(text, anchor, size, offset, scale)
        {
        }

        /// <summary>
        /// Create default multicolor paragraph with empty text.
        /// </summary>
        public RichParagraph() : this(string.Empty)
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
        public RichParagraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null, Vector2? offset = null) :
            base(text, anchor, color, scale, size, offset)
        {
        }

        /// <summary>
        /// Special init after deserializing entity from file.
        /// </summary>
        internal protected override void InitAfterDeserialize()
        {
            base.InitAfterDeserialize();
            _needUpdateStyleInstructions = true;
        }

        /// <summary>
        /// Parse special color-changing instructions inside the text.
        /// </summary>
        private void ParseColorInstructions()
        {
            // clear previous color instructions
            _styleInstructions.Clear();

            // if color instructions are disabled, stop here
            if (!EnableStyleInstructions)
            {
                _needUpdateStyleInstructions = false;
                return;
            }

            // get opening and closing denotes
            var openingDenote = RichParagraphStyleInstruction._styleInstructionsOpening;
            var closingDenote = RichParagraphStyleInstruction._styleInstructionsClosing;
            var denotesLength = (openingDenote.Length + closingDenote.Length);

            // find and parse color instructions
            if (_text.Contains(openingDenote))
            {
                int iLastLength = 0;

                System.Text.RegularExpressions.MatchCollection oMatches = RichParagraphStyleInstruction._styleInstructionsRegex.Matches(_text);
                foreach (System.Text.RegularExpressions.Match oMatch in oMatches)
                {
                    var key = oMatch.Value.Substring(openingDenote.Length, oMatch.Value.Length - denotesLength);
                    string sColor = oMatch.Value.Substring(openingDenote.Length, oMatch.Value.Length - denotesLength);
                    _styleInstructions.Add(oMatch.Index - iLastLength, RichParagraphStyleInstruction._instructions[key]);
                    iLastLength += oMatch.Value.Length;
                }

                // remove all the style instructions from text so it won't be shown
                _text = RichParagraphStyleInstruction._styleInstructionsRegex.Replace(_text, string.Empty);
            }

            // no longer need to update colors
            _needUpdateStyleInstructions = false;
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
            // update processed text if needed
            if (_needUpdateStyleInstructions)
            {
                ParseColorInstructions();
                UpdateDestinationRects();
            }

            // draw text outline
            int outlineWidth = OutlineWidth;
            if (outlineWidth > 0)
            {
                DrawTextOutline(spriteBatch, outlineWidth);
            }

            // if there are color changing instructions in paragraph, draw with color changes
            if (_styleInstructions.Count > 0)
            {
                // iterate characters in text and check when there's an instruction to apply
                int iTextIndex = 0;
                Color oColor = UserInterface.Active.DrawUtils.FixColorOpacity(FillColor);
                SpriteFont oFont = _currFont;
                Vector2 oCharacterSize = GetCharacterActualSize();
                Vector2 oCurrentPosition = new Vector2(_position.X - oCharacterSize.X, _position.Y);
                foreach (char cCharacter in _processedText)
                {
                    // if we found style instruction:
                    RichParagraphStyleInstruction styleInstruction;
                    if (_styleInstructions.TryGetValue(iTextIndex, out styleInstruction))
                    {
                        // reset properties
                        if (styleInstruction.ResetStyles)
                        {
                            oColor = UserInterface.Active.DrawUtils.FixColorOpacity(FillColor);
                            oFont = _currFont;
                        }

                        // set fill color
                        if (styleInstruction.FillColor.HasValue)
                        {
                            oColor = UserInterface.Active.DrawUtils.FixColorOpacity(styleInstruction.FillColor.Value);
                        }

                        // set font style
                        if (styleInstruction.FontStyle.HasValue)
                        {
                            oFont = Resources.Fonts[(int)styleInstruction.FontStyle.Value];
                        }
                    }

                    // adjust character position
                    if (cCharacter == '\n')
                    {
                        oCurrentPosition.X = _position.X - oCharacterSize.X;
                        oCurrentPosition.Y += oFont.LineSpacing * _actualScale;
                    }
                    else
                    {
                        iTextIndex++;
                        oCurrentPosition.X += oCharacterSize.X;
                    }

                    // fix color opacity and draw
                    spriteBatch.DrawString(oFont, cCharacter.ToString(), oCurrentPosition, oColor, 0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
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
