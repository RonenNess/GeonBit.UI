#region File Description
//-----------------------------------------------------------------------------
// Validators you can attach to TextInput entities to manipulate and validate
// user input. These are used to create things like text input for numbers only,
// limit characters to english chars, etc.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion

namespace GeonBit.UI.Entities.TextValidators
{
    /// <summary>
    /// Make sure input contains only letters, numbers, underscores or hyphens (and optionally spaces).
    /// </summary>
    [System.Serializable]
    public class SlugValidator : ITextValidator
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static SlugValidator()
        {
            Entity.MakeSerializable(typeof(SlugValidator));
        }

        // the regex to use
        System.Text.RegularExpressions.Regex _regex;

        // regex for slug with spaces
        static readonly System.Text.RegularExpressions.Regex _slugNoSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\-_0-9]+$");

        // regex for slug without spaces
        static readonly System.Text.RegularExpressions.Regex _slugWithSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\-_\ 0-9]+$");

        // do we allow spaces in text?
        private bool _allowSpaces;

        /// <summary>
        /// Set / get if we allow spaces in text.
        /// </summary>
        public bool AllowSpaces
        {
            get { return _allowSpaces; }
            set { _allowSpaces = value; _regex = _allowSpaces ? _slugWithSpaces : _slugNoSpaces; }
        }

        /// <summary>
        /// Create the slug validator.
        /// </summary>
        /// <param name="allowSpaces">If true, will allow spaces.</param>
        public SlugValidator(bool allowSpaces)
        {
            AllowSpaces = allowSpaces;
        }

        /// <summary>
        /// Create the validator with default params.
        /// </summary>
        public SlugValidator() : this(false)
        {
        }

        /// <summary>
        /// Return true if text input is slug.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public override bool ValidateText(ref string text, string oldText)
        {
            return (text.Length == 0 || _regex.IsMatch(text));
        }
    }
}