#region File Description
//-----------------------------------------------------------------------------
// A paragraph extension that support multiple fill colors (change colors via
// special color commands).
//
// Author: Justin Gattuso, Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Hold style changes instructions for rich paragraphs.
    /// </summary>
    [System.Serializable]
    public struct RichParagraphStyleInstruction
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
        /// <param name="fillColor">Set fill color.</param>
        /// <param name="fontStyle">Set font style.</param>
        /// <param name="outlineWidth">Set outline width.</param>
        /// <param name="outlineColor">Set outline color.</param>
        /// <param name="resetStyles">If true will reset all style properties to defaults.</param>
        public RichParagraphStyleInstruction(Color? fillColor = null, FontStyle? fontStyle = null, int? outlineWidth = null, Color? outlineColor = null, bool resetStyles = false)
        {
            ResetStyles = resetStyles;
            FillColor = fillColor;
            FontStyle = fontStyle;
            OutlineWidth = outlineWidth;
            OutlineColor = outlineColor;
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
        /// Will change outline width.
        /// </summary>
        public int? OutlineWidth { get; private set; }

        /// <summary>
        /// Will change text outline color.
        /// </summary>
        public Color? OutlineColor { get; private set; }

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

            AddInstruction("L_RED", new RichParagraphStyleInstruction(fillColor: new Color(1f, 0.35f, 0.25f)));
            AddInstruction("L_BLUE", new RichParagraphStyleInstruction(fillColor: new Color(0.25f,0.35f, 1f)));
            AddInstruction("L_GREEN", new RichParagraphStyleInstruction(fillColor: Color.LawnGreen));
            AddInstruction("L_YELLOW", new RichParagraphStyleInstruction(fillColor: Color.LightYellow));
            AddInstruction("L_BROWN", new RichParagraphStyleInstruction(fillColor: Color.RosyBrown));
            AddInstruction("L_CYAN", new RichParagraphStyleInstruction(fillColor: Color.LightCyan));
            AddInstruction("L_PINK", new RichParagraphStyleInstruction(fillColor: Color.LightPink));
            AddInstruction("L_GRAY", new RichParagraphStyleInstruction(fillColor: Color.LightGray));
            AddInstruction("L_GOLD", new RichParagraphStyleInstruction(fillColor: Color.LightGoldenrodYellow));

            AddInstruction("D_RED", new RichParagraphStyleInstruction(fillColor: Color.DarkRed));
            AddInstruction("D_BLUE", new RichParagraphStyleInstruction(fillColor: Color.DarkBlue));
            AddInstruction("D_GREEN", new RichParagraphStyleInstruction(fillColor: Color.ForestGreen));
            AddInstruction("D_YELLOW", new RichParagraphStyleInstruction(fillColor: Color.GreenYellow));
            AddInstruction("D_BROWN", new RichParagraphStyleInstruction(fillColor: Color.SaddleBrown));
            AddInstruction("D_CYAN", new RichParagraphStyleInstruction(fillColor: Color.DarkCyan));
            AddInstruction("D_PINK", new RichParagraphStyleInstruction(fillColor: Color.DeepPink));
            AddInstruction("D_GRAY", new RichParagraphStyleInstruction(fillColor: Color.DarkGray));
            AddInstruction("D_GOLD", new RichParagraphStyleInstruction(fillColor: Color.DarkGoldenrod));

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
        /// A function to add per-character manipulation, used for advanced animations.
        /// </summary>
        /// <param name="paragraph">The rich paragraph entity.</param>
        /// <param name="currChar">Value of current character to manipulate.</param>
        /// <param name="index">Character index.</param>
        /// <param name="fillColor">Output fill color.</param>
        /// <param name="outlineColor">Output outline color.</param>
        /// <param name="outlineWidth">Output line width.</param>
        /// <param name="offset">Output position offset from original position.</param>
        /// <param name="scale">Output character scale.</param>
        public delegate void PerCharacterManipulationFunc(RichParagraph paragraph, char currChar, int index, ref Color fillColor, ref Color outlineColor, ref int outlineWidth, ref Vector2 offset, ref float scale);

        /// <summary>
        /// Optional manipulators we can add to change per-character color and position.
        /// </summary>
        public PerCharacterManipulationFunc PerCharacterManipulators;

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
            get 
            { 
                if (_needUpdateStyleInstructions || IsDirty)
                {
                    ParseStyleInstructions();
                    UpdateDestinationRects();
                }
                return _text; 
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    MarkAsDirty();
                    if (EnableStyleInstructions)
                    {
                        _needUpdateStyleInstructions = true;
                    }
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
        /// Parse special style-changing instructions inside the text.
        /// </summary>
        private void ParseStyleInstructions()
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
                    _styleInstructions[oMatch.Index - iLastLength] = RichParagraphStyleInstruction._instructions[key];
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
                ParseStyleInstructions();
                UpdateDestinationRects();
            }

            // if there are color changing instructions in paragraph, draw with color changes
            if (_styleInstructions.Count > 0 || PerCharacterManipulators != null)
            {
                // iterate characters in text and check when there's an instruction to apply
                int iTextIndex = 0;
                Color currColor = Color.White;
                Color currOutlineColor = Color.Black;
                SpriteFont currFont = null;
                Vector2 characterSize = GetCharacterActualSize();
                Vector2 currPosition = new Vector2(_position.X - characterSize.X, _position.Y);
                int currOutlineWidth = 0;

                // function to reset styles back to defaults
                System.Action ResetToDefaults = () =>
                {
                    currColor = UserInterface.Active.DrawUtils.FixColorOpacity(FillColor);
                    currFont = _currFont;
                    currOutlineWidth = OutlineWidth;
                    currOutlineColor = UserInterface.Active.DrawUtils.FixColorOpacity(OutlineColor);
                    characterSize = GetCharacterActualSize();
                };
                ResetToDefaults();

                foreach (char currCharacter in _processedText)
                {
                    // if we found style instruction:
                    RichParagraphStyleInstruction styleInstruction;
                    if (_styleInstructions.TryGetValue(iTextIndex, out styleInstruction))
                    {
                        // reset properties
                        if (styleInstruction.ResetStyles)
                        {
                            ResetToDefaults();
                        }

                        // set fill color
                        if (styleInstruction.FillColor.HasValue)
                        {
                            currColor = UserInterface.Active.DrawUtils.FixColorOpacity(styleInstruction.FillColor.Value);
                        }

                        // set font style
                        if (styleInstruction.FontStyle.HasValue)
                        {
                            currFont = Resources.Instance.Fonts[(int)styleInstruction.FontStyle.Value];
                        }

                        // set outline width
                        if (styleInstruction.OutlineWidth.HasValue)
                        {
                            currOutlineWidth = styleInstruction.OutlineWidth.Value;
                        }

                        // set outline color
                        if (styleInstruction.OutlineColor.HasValue)
                        {
                            currOutlineColor = UserInterface.Active.DrawUtils.FixColorOpacity(styleInstruction.OutlineColor.Value);
                        }
                    }

                    // adjust character position
                    if (currCharacter == '\n')
                    {
                        currPosition.X = _position.X - characterSize.X;
                        currPosition.Y += currFont.LineSpacing * _actualScale;
                    }
                    else
                    {
                        iTextIndex++;
                        currPosition.X += characterSize.X;
                    }

                    // get current char as string
                    var currText = Resources.Instance.GetStringForChar(currCharacter);

                    // do per-character manipulations
                    Vector2 offset = Vector2.Zero;
                    if (PerCharacterManipulators != null)
                    {
                        PerCharacterManipulators.Invoke(this, currCharacter, iTextIndex, ref currColor, ref currOutlineColor, ref currOutlineWidth, ref offset, ref _actualScale);
                    }

                    // draw outline
                    DrawTextOutline(spriteBatch, currText, currOutlineWidth, currFont, _actualScale, currPosition + offset, currOutlineColor, _fontOrigin);

                    // fix color opacity and draw
                    spriteBatch.DrawString(currFont, currText, currPosition + offset, currColor, 0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                }
            }
            // if there are no style-changing instructions, just draw the paragraph as-is
            else
            {
                base.DrawEntity(spriteBatch, phase);
            }
        }
    }
}
