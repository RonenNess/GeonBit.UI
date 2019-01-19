using Microsoft.Xna.Framework;


namespace GeonBit.UI.Animators
{
    /// <summary>
    /// An animator that makes an entity types text into Paragraph over time.
    /// Note: this animator override the Text property.
    /// </summary>
    public class TypeWriterAnimator : IAnimator
    {
        /// <summary>
        /// Text to type.
        /// Changing it will reset animation.
        /// </summary>
        public string TextToType
        {
            get { return _text; }
            set { _text = value; Reset(); }
        }

        // text to type value
        private string _text = null;

        // current text position (where we got to in typing).
        private int _currPosition;

        // time until typing next character
        private float _timeForNextChar = 1f;

        /// <summary>
        /// Typing animation speed. 
        /// If value = 1f it means it will take a second to type each character.
        /// </summary>
        public float SpeedFactor = 10f;
        
        /// <summary>
        /// Return if an entity type is compatible with this animator.
        /// </summary>
        /// <param name="entity">Entity to test.</param>
        /// <returns>True if compatible, false otherwise.</returns>
        public override bool CheckEntityCompatibility(Entities.Entity entity)
        {
            return entity is Entities.Paragraph;
        }

        /// <summary>
        /// Do animation.
        /// </summary>
        public override void Update()
        {
            // nothing to type? don't do anything
            if (_text == null)
            {
                return;
            }

            // finished animation? skip
            if (IsDone)
            {
                return;
            }

            // update animation and calc new position
            var dt = (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds;
            _timeForNextChar -= dt * SpeedFactor;
            if (_timeForNextChar <= 0f)
            {
                // reset time for next character and update current position
                _timeForNextChar = 1f;
                _currPosition++;

                // special case - if its rich paragraph and we started style instruction, complete the command
                if (TargetEntity is Entities.RichParagraph)
                {
                    var openingDenote = Entities.RichParagraphStyleInstruction._styleInstructionsOpening;
                    var closingDenote = Entities.RichParagraphStyleInstruction._styleInstructionsClosing;
                    if (_currPosition + openingDenote.Length < _text.Length && 
                        _text.Substring(_currPosition, openingDenote.Length) == openingDenote)
                    {
                        var closingPosition = _text.IndexOf(closingDenote, _currPosition);
                        if (closingPosition != -1)
                        {
                            _currPosition = closingPosition + closingDenote.Length;
                        }
                    }
                }
            }

            // update target entity text
            ((Entities.Paragraph)(TargetEntity)).Text = _text.Substring(0, _currPosition);
        }

        /// <summary>
        /// Reset animation.
        /// </summary>
        public override void Reset()
        {
            _currPosition = 0;
            _timeForNextChar = 1f;
        }

        /// <summary>
        /// Did this animator finish?
        /// </summary>
        public override bool IsDone
        {
            get
            {
                return _currPosition >= _text.Length;
            }
        }
    }
}
