#region File Description
//-----------------------------------------------------------------------------
// TextInput are entities that allow the user to type in free text using the keyboard.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GeonBit.UI.Entities.TextValidators;

namespace GeonBit.UI.Entities
{

    /// <summary>
    /// A textbox that allow users to put in free text.
    /// </summary>
    [System.Serializable]
    public class TextInput : PanelBase
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static TextInput()
        {
            Entity.MakeSerializable(typeof(TextInput));
        }

        // current text value
        string _value = string.Empty;

        // current caret position (-1 is last character).
        int _caret = -1;

        /// <summary>The Paragraph object showing current text value.</summary>
        public Paragraph TextParagraph;

        /// <summary>A placeholder paragraph to show when text input is empty.</summary>
        public Paragraph PlaceholderParagraph;

        /// <summary>If false, it will only allow one line input.</summary>
        protected bool _multiLine = false;

        /// <summary>
        /// Set / get multiline mode.
        /// </summary>
        public bool Multiline
        {
            get { return _multiLine; }
            set { if (_multiLine != value) { _multiLine = value; UpdateMultilineState(); } }
        }

        // scrollbar to use if text height exceed the input box size
        VerticalScrollbar _scrollbar;

        /// <summary>
        /// If provided, will automatically put this value whenever the user leave the input box and its empty.
        /// </summary>
        public string ValueWhenEmpty = null;

        // current caret animation step
        float _caretAnim = 0f;

        /// <summary>Text to show when there's no input. Note that this text will be drawn with PlaceholderParagraph, and not TextParagraph.</summary>
        string _placeholderText = string.Empty;

        /// <summary>Set to any number to limit input by characters count.
        /// Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.</summary>
        public int CharactersLimit = 0;

        /// <summary>If true, will limit max input length to fit textbox size.
        /// Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.</summary>
        public bool LimitBySize = false;

        /// <summary>
        /// If provided, hide input and replace it with the given character.
        /// This is useful for stuff like password input field.
        /// </summary>
        public char? HideInputWithChar;

        /// <summary>Default styling for the text input itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default style for paragraph that show current value.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default style for the placeholder paragraph.</summary>
        public static StyleSheet DefaultPlaceholderStyle = new StyleSheet();

        /// <summary>How fast to blink caret when text input is selected.</summary>
        public static float CaretBlinkingSpeed = 2f;

        /// <summary>The actual displayed text, after wordwrap and other processing. 
        /// note: only the text currently visible by scrollbar.</summary>
        string _actualDisplayText = string.Empty;

        /// <summary>List of validators to apply on text input.</summary>
        public List<ITextValidator> Validators = new List<ITextValidator>();

        /// <summary>
        /// Create the text input.
        /// </summary>
        /// <param name="multiline">If true, text input will accept multiple lines.</param>
        /// <param name="size">Input box size.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">TextInput skin, eg which texture to use.</param>
        public TextInput(bool multiline, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground) :
            base(size, skin, anchor, offset)
        {
            // set multiline mode
            _multiLine = multiline;

            // update default style
            UpdateStyle(DefaultStyle);
            
            // default size of multiline text input is 4 times bigger
            if (multiline)
            {
                SetStyleProperty(StylePropertyIds.DefaultSize, new DataTypes.StyleProperty(EntityDefaultSize * new Vector2(1, 4)));
            }

            // set limit by size - default true in single-line, default false in multi-line
            LimitBySize = !_multiLine;

            if (!UserInterface.Active._isDeserializing)
            {

                // create paragraph to show current value
                TextParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
                TextParagraph.UpdateStyle(DefaultParagraphStyle);
                TextParagraph._hiddenInternalEntity = true;
                TextParagraph.Identifier = "_TextParagraph";
                AddChild(TextParagraph, true);

                // create the placeholder paragraph
                PlaceholderParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
                PlaceholderParagraph.UpdateStyle(DefaultPlaceholderStyle);
                PlaceholderParagraph._hiddenInternalEntity = true;
                PlaceholderParagraph.Identifier = "_PlaceholderParagraph";
                AddChild(PlaceholderParagraph, true);

                // update multiline related stuff
                UpdateMultilineState();

                // if the default paragraph type is multicolor, disable it for input
                RichParagraph colorTextParagraph = TextParagraph as RichParagraph;
                if (colorTextParagraph != null)
                {
                    colorTextParagraph.EnableStyleInstructions = false;
                }
            }
        }

        /// <summary>
        /// Update after multiline state was changed.
        /// </summary>
        private void UpdateMultilineState()
        {
            // we are now multiline
            if (_multiLine)
            {
                _scrollbar = new VerticalScrollbar(0, 0, Anchor.CenterRight, offset: new Vector2(-8, 0));
                _scrollbar.Value = 0;
                _scrollbar.Visible = false;
                _scrollbar._hiddenInternalEntity = true;
                _scrollbar.Identifier = "__inputScrollbar";
                AddChild(_scrollbar, false);
            }
            // we are not multiline
            else
            {
                if (_scrollbar != null)
                {
                    _scrollbar.RemoveFromParent();
                    _scrollbar = null;
                }
            }

            // set default wrap words state
            TextParagraph.WrapWords = _multiLine;
            PlaceholderParagraph.WrapWords = _multiLine;
            TextParagraph.Anchor = PlaceholderParagraph.Anchor = _multiLine ? Anchor.TopLeft : Anchor.CenterLeft;
        }

        /// <summary>
        /// Special init after deserializing entity from file.
        /// </summary>
        internal protected override void InitAfterDeserialize()
        {
            base.InitAfterDeserialize();

            // set main text paragraph
            TextParagraph = Find("_TextParagraph") as Paragraph;
            TextParagraph._hiddenInternalEntity = true;

            // set scrollbar
            _scrollbar = Find<VerticalScrollbar>("__inputScrollbar");
            if (_scrollbar != null)
                _scrollbar._hiddenInternalEntity = true;

            // set placeholder paragraph
            PlaceholderParagraph = Find("_PlaceholderParagraph") as Paragraph;
            PlaceholderParagraph._hiddenInternalEntity = true;

            // recalc dest rects
            UpdateMultilineState();
        }

        /// <summary>
        /// Create the text input with default size.
        /// </summary>
        /// <param name="multiline">If true, text input will accept multiple lines.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public TextInput(bool multiline, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
           this(multiline, USE_DEFAULT_SIZE, anchor, offset)
        {
        }

        /// <summary>
        /// Create default single-line text input.
        /// </summary>
        public TextInput() : this(false)
        {
        }

        /// <summary>
        /// Is the text input a natrually-interactable entity.
        /// </summary>
        /// <returns>True.</returns>
        override internal protected bool IsNaturallyInteractable()
        {
            return true;
        }

        /// <summary>
        /// Text to show when there's no input using the placeholder style.
        /// </summary>
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set { _placeholderText = _multiLine ? value : value.Replace("\n", string.Empty); }
        }

        /// <summary>
        /// Current input text value.
        /// </summary>
        public string Value
        {
            get 
            { 
                return _value; 
            }

            set
            {
                value = value ?? string.Empty;
                var newValue = _multiLine ? value : value.Replace("\n", string.Empty);
                if (_value != newValue)
                {
                    _value = newValue;
                    FixCaretPosition();
                    DoOnValueChange();
                }
            }
        }

        /// <summary>
        /// Change the value of this entity, where there's value to change.
        /// </summary>
        /// <param name="newValue">New value to set.</param>
        /// <param name="emitEvent">If true and value changed, will emit 'ValueChanged' event.</param>
        override public void ChangeValue(object newValue, bool emitEvent)
        {
            var stringValue = (string)newValue;
            if (_value != stringValue)
            {
                _value = stringValue;
                if (emitEvent) { DoOnValueChange(); }
            }
        }

        /// <summary>
        /// Get the value of this entity, where there's value.
        /// </summary>
        /// <returns>Value as object.</returns>
        override public object GetValue()
        {
            return _value;
        }

        /// <summary>
        /// Move scrollbar to show caret position.
        /// </summary>
        public void ScrollToCaret()
        {
            // skip if no scrollbar
            if (_scrollbar == null)
            {
                return;
            }

            // make sure caret position is legal
            if (_caret >= _value.Length)
            {
                _caret = -1;
            }

            // if caret is at end of text jump to it
            if (_caret == -1)
            {
                _scrollbar.Value = (int)_scrollbar.Max;
            }
            // if not try to find the right pos
            else
            {
                TextParagraph.Text = _value;
                TextParagraph.CalcTextActualRectWithWrap();
                string processedValueText = TextParagraph.GetProcessedText();
                int currLine = processedValueText.Substring(0, _caret).Split('\n').Length;
                _scrollbar.Value = currLine - 1;
            }
        }

        /// <summary>
        /// Current cursor, eg where we are about to put next character.
        /// Set to -1 to jump to end.
        /// </summary>
        public int Caret
        {
            get { return _caret; }
            set { _caret = value; }
        }

        /// <summary>
        /// Current scrollbar position.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int ScrollPosition
        {
            get { return _scrollbar != null ? _scrollbar.Value : 0; }
            set { if (_scrollbar != null) _scrollbar.Value = value; }
        }

        /// <summary>
        /// Move caret to the end of text.
        /// </summary>
        /// <param name="scrollToCaret">If true, will also scroll to show caret position.</param>
        public void ResetCaret(bool scrollToCaret)
        {
            Caret = -1;
            if (scrollToCaret)
            {
                ScrollToCaret();
            }
        }

        /// <summary>
        /// Prepare the input paragraph for display.
        /// </summary>
        /// <param name="usePlaceholder">If true, will use the placeholder text. Else, will use the real input text.</param>
        /// <param name="showCaret">If true, will also add the caret text when needed. If false, will not show caret.</param>
        /// <returns>Processed text that will actually be displayed on screen.</returns>
        protected string PrepareInputTextForDisplay(bool usePlaceholder, bool showCaret)
        {
            // set caret char
            string caretShow = showCaret ? ((int)_caretAnim % 2 == 0) ? "|" : " " : string.Empty;

            // set main text when hidden with password char
            if (HideInputWithChar != null)
            {
                var hiddenVal = new string(HideInputWithChar.Value, _value.Length);
                TextParagraph.Text = hiddenVal.Insert(_caret >= 0 ? _caret : hiddenVal.Length, caretShow);
            }
            // set main text for regular text input
            else
            {
                TextParagraph.Text = _value.Insert(_caret >= 0 ? _caret : _value.Length, caretShow);
            }

            // update placeholder text
            PlaceholderParagraph.Text = _placeholderText;

            // get current paragraph and prepare to draw
            Paragraph currParagraph = usePlaceholder ? PlaceholderParagraph : TextParagraph;
            TextParagraph.UpdateDestinationRectsIfDirty();

            // get text to display
            return currParagraph.GetProcessedText();
        }

        /// <summary>
        /// Handle mouse click event.
        /// TextInput override this function to handle picking caret position.
        /// </summary>
        override protected void DoOnClick()
        {
            // first call base DoOnClick
            base.DoOnClick();

            // check if hit paragraph
            if (_value.Length > 0)
            {
                // get relative position
                Vector2 actualParagraphPos = new Vector2(_destRectInternal.Location.X, _destRectInternal.Location.Y);
                Vector2 relativeOffset = GetMousePos(-actualParagraphPos);

                // calc caret position
                Vector2 charSize = TextParagraph.GetCharacterActualSize();
                int x = (int)(relativeOffset.X / charSize.X);
                _caret = x;

                // if multiline, take line into the formula
                if (_multiLine)
                {
                    // get the whole processed text
                    TextParagraph.Text = _value;
                    TextParagraph.CalcTextActualRectWithWrap();
                    string processedValueText = TextParagraph.GetProcessedText();

                    // calc y position and add scrollbar value to it
                    int y = (int)(relativeOffset.Y / charSize.Y) + _scrollbar.Value;

                    // break actual text into lines
                    List<string> lines = new List<string>(processedValueText.Split('\n'));
                    for (int i = 0; i < y && i < lines.Count; ++i)
                    {
                        _caret += lines[i].Length + 1;
                    }
                }

                // if override text length reset caret
                if (_caret >= _value.Length)
                {
                    _caret = -1;
                }
            }
            // if don't click on the paragraph itself, reset caret position.
            else
            {
                _caret = -1;
            }
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // call base draw function to draw the panel part
            base.DrawEntity(spriteBatch, phase);

            // get which paragraph we currently show - real or placeholder
            bool showPlaceholder = !(IsFocused || _value.Length > 0);
            Paragraph currParagraph = showPlaceholder ? PlaceholderParagraph : TextParagraph;

            // get actual processed string
            _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder, IsFocused);

            // for multiline only - handle scrollbar visibility and max
            if (_multiLine && (_actualDisplayText != null) && (_destRectInternal.Height > 0))
            {
                // get how many lines can fit in the textbox and how many lines display text actually have
                int linesFit = _destRectInternal.Height / (int)(System.Math.Max(currParagraph.GetCharacterActualSize().Y, 1));
                int linesInText = _actualDisplayText.Split('\n').Length;

                // if there are more lines than can fit, show scrollbar and manage scrolling:
                if (linesInText > linesFit)
                {
                    // fix paragraph width to leave room for the scrollbar
                    float prevWidth = currParagraph.Size.X;
                    currParagraph.Size = new Vector2(_destRectInternal.Width / GlobalScale - 20, 0);
                    if (currParagraph.Size.X != prevWidth)
                    {
                        // update size and re-calculate lines in text
                        _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder, IsFocused);
                        linesInText = _actualDisplayText.Split('\n').Length;
                    }

                    // set scrollbar max and steps
                    _scrollbar.Max = System.Math.Max(linesInText - linesFit, 2);
                    _scrollbar.StepsCount = (uint)(_scrollbar.Max - _scrollbar.Min);
                    _scrollbar.Visible = true;

                    // update text to fit scrollbar. first, rebuild the text with just the visible segment
                    List<string> lines = new List<string>(_actualDisplayText.Split('\n'));
                    int from = System.Math.Min(_scrollbar.Value, lines.Count - 1);
                    int size = System.Math.Min(linesFit, lines.Count - from);
                    lines = lines.GetRange(from, size);
                    _actualDisplayText = string.Join("\n", lines);
                    currParagraph.Text = _actualDisplayText;
                }
                // if no need for scrollbar make it invisible
                else
                {
                    currParagraph.Size = Vector2.Zero;
                    _scrollbar.Visible = false;
                }
            }

            // set placeholder and main text visibility based on current value
            TextParagraph.Visible = !showPlaceholder;
            PlaceholderParagraph.Visible = showPlaceholder;
        }

        /// <summary>
        /// Validate current text input after change (usually addition of text).
        /// </summary>
        /// <param name="newVal">New text value, to check validity.</param>
        /// <param name="oldVal">Previous text value, before the change.</param>
        /// <returns>True if new input is valid, false otherwise.</returns>
        private bool ValidateInput(ref string newVal, string oldVal)
        {
            // if new characters were added, and we now exceed characters limit, revet to previous value.
            if (CharactersLimit != 0 &&
                newVal.Length > CharactersLimit)
            {
                newVal = oldVal;
                return false;
            }

            // if not multiline and got line break, revet to previous value
            if (!_multiLine && newVal.Contains("\n"))
            {
                newVal = oldVal;
                return false;
            }

            // if set to limit by size make sure we don't exceed it
            if (LimitBySize)
            {
                // prepare display
                PrepareInputTextForDisplay(false, false);

                // get main paragraph actual size
                Rectangle textSize = TextParagraph.GetActualDestRect();

                // if multiline, compare heights
                if (_multiLine && textSize.Height >= _destRectInternal.Height)
                {
                    newVal = oldVal;
                    return false;
                }
                // if single line, compare widths
                else if (textSize.Width >= _destRectInternal.Width)
                {
                    newVal = oldVal;
                    return false;
                }
            }

            // if got here we iterate over additional validators
            foreach (var validator in Validators)
            {
                if (!validator.ValidateText(ref newVal, oldVal))
                {
                    newVal = oldVal;
                    return false;
                }
            }

            // looks good!
            return true;
        }

        /// <summary>
        /// Make sure caret position is currently valid and in range.
        /// </summary>
        private void FixCaretPosition()
        {
            if (_caret < -1) { _caret = 0; }
            if (_caret >= _value.Length || _value.Length == 0) { _caret = -1; }
        }

        /// <summary>
        /// Called every frame before update.
        /// TextInput implement this function to get keyboard input and also to animate caret timer.
        /// </summary>
        override protected void DoBeforeUpdate()
        {
            // animate caret
            _caretAnim += (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds * CaretBlinkingSpeed;

            // if focused, and got character input in this frame..
            if (IsFocused)
            {
                // validate caret position
                FixCaretPosition();

                // get caret position
                int pos = _caret;

                // store old string and update based on user input
                string oldVal = _value;
                _value = KeyboardInput.GetTextInput(_value, TextParagraph.MaxCharactersInLine, ref pos);

                // update caret position
                _caret = pos;

                // if value changed:
                if (_value != oldVal)
                {
                    // if new characters were added and input is now illegal, revert to previous value
                    if (!ValidateInput(ref _value, oldVal))
                    {
                        _value = oldVal;
                    }

                    // call change event
                    if (_value != oldVal)
                    {
                        DoOnValueChange();
                    }

                    // after change, scroll to caret
                    ScrollToCaret();

                    // fix caret position
                    FixCaretPosition();
                }
            }

            // call base do-before-update
            base.DoBeforeUpdate();
        }

        /// <summary>
        /// Called every time this entity is focused / unfocused.
        /// </summary>
        override protected void DoOnFocusChange()
        {
            // call base on focus change
            base.DoOnFocusChange();
            
            // check if need to set default value
            if (ValueWhenEmpty != null && Value.Length == 0)
            {
                Value = ValueWhenEmpty;
            }
        }
    }
}
