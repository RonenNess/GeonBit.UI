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
    /// GeonBit.UI.Entities.TextValidators contains different text validators and processors you can attach to TextInput entities.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// A class that validates text input to make sure its valid.
    /// These classes can be added to any TextInput to limit the type of input the user can enter.
    /// </summary>
    public interface ITextValidator
    {
        /// <summary>
        /// Get the new text input value and return true if valid.
        /// This function can either return false to scrap input changes, or change the text and return true.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        bool ValidateText(ref string text, string oldText);
    }

    /// <summary>
    /// Make sure input is numeric and optionally validate min / max values.
    /// </summary>
    public class TextValidatorNumbersOnly : ITextValidator
    {
        bool _allowDecimalPoint;
        double? _min;
        double? _max;

        /// <summary>
        /// Create the number validator.
        /// </summary>
        /// <param name="allowDecimal">If true, will allow decimal point in input.</param>
        /// <param name="min">If provided, will force min value.</param>
        /// <param name="max">If provided, will force max value.</param>
        public TextValidatorNumbersOnly(bool allowDecimal = false, double? min = null, double? max = null)
        {
            _allowDecimalPoint = allowDecimal;
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Return true if text input is a valid number.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public bool ValidateText(ref string text, string oldText)
        {
            // if string empty return true
            if (text.Length == 0)
            {
                return true;
            }

            // will contain value as number
            double num;

            // try to parse as double
            if (_allowDecimalPoint)
            {
                if (!double.TryParse(text, out num))
                {
                    return false;
                }
            }
            // try to parse as int
            else
            {
                int temp;
                if (!int.TryParse(text, out temp))
                {
                    return false;
                }
                num = temp;
            }

            // validate range
            if (_min != null && num < (double)_min) { text = _min.ToString(); }
            if (_max != null && num > (double)_max) { text = _max.ToString(); }

            // valid number input
            return true;
        }
    }

    /// <summary>
    /// Make sure input contains only english characters.
    /// </summary>
    public class TextValidatorEnglishCharsOnly : ITextValidator
    {
        // the regex to use
        System.Text.RegularExpressions.Regex _regex;

        // regex for english only with spaces
        static readonly System.Text.RegularExpressions.Regex _slugNoSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z|]+$");

        // regex for english only without spaces
        static readonly System.Text.RegularExpressions.Regex _slugWithSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z|\ ]+$");

        /// <summary>
        /// Create the validator.
        /// </summary>
        /// <param name="allowSpaces">If true, will allow spaces.</param>
        public TextValidatorEnglishCharsOnly(bool allowSpaces = false)
        {
            _regex = allowSpaces ? _slugWithSpaces : _slugNoSpaces;
        }

        /// <summary>
        /// Return true if text input is only english characters.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public bool ValidateText(ref string text, string oldText)
        {
            return (text.Length == 0 || _regex.IsMatch(text));
        }
    }

    /// <summary>
    /// Make sure input contains only letters, numbers, underscores or hyphens (and optionally spaces).
    /// </summary>
    public class SlugValidator : ITextValidator
    {
        // the regex to use
        System.Text.RegularExpressions.Regex _regex;

        // regex for slug with spaces
        static readonly System.Text.RegularExpressions.Regex _slugNoSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\-_0-9]+$");

        // regex for slug without spaces
        static readonly System.Text.RegularExpressions.Regex _slugWithSpaces = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\-_\ 0-9]+$");

        /// <summary>
        /// Create the slug validator.
        /// </summary>
        /// <param name="allowSpaces">If true, will allow spaces.</param>
        public SlugValidator(bool allowSpaces = false)
        {
            _regex = allowSpaces ? _slugWithSpaces : _slugNoSpaces;
        }

        /// <summary>
        /// Return true if text input is slug.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public bool ValidateText(ref string text, string oldText)
        {
            return (text.Length == 0 || _regex.IsMatch(text));
        }
    }


    /// <summary>
    /// Make sure input don't contain double spaces or tabs.
    /// </summary>
    public class OnlySingleSpaces : ITextValidator
    {
        /// <summary>
        /// Return true if text input don't contain double spaces or tabs.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public bool ValidateText(ref string text, string oldText)
        {
            return !text.Contains("  ") && !text.Contains("\t");
        }
    }

    /// <summary>
    /// Make sure input is always title, eg starts with a capital letter followed by lowercase.
    /// </summary>
    public class TextValidatorMakeTitle : ITextValidator
    {
        /// <summary>
        /// Always return true, and make first character uppercase while all following
        /// chars lowercase.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>Always return true.</returns>
        public bool ValidateText(ref string text, string oldText)
        {
            if (text.Length > 0)
            {
                text = text.ToLower();
                text = text[0].ToString().ToUpper() + text.Remove(0, 1);
            }
            return true;
        }
    }
}