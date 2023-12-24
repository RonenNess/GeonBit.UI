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
    /// Make sure input is numeric and optionally validate min / max values.
    /// </summary>
    [System.Serializable]
    public class NumbersOnly : ITextValidator
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static NumbersOnly()
        {
            Entity.MakeSerializable(typeof(NumbersOnly));
        }

        /// <summary>
        /// Do we allow decimal point?
        /// </summary>
        public bool AllowDecimalPoint;

        /// <summary>
        /// Optional min value.
        /// </summary>
        public double? Min;

        /// <summary>
        /// Optional max value.
        /// </summary>
        public double? Max;

        /// <summary>
        /// Create the number validator.
        /// </summary>
        /// <param name="allowDecimal">If true, will allow decimal point in input.</param>
        /// <param name="min">If provided, will force min value.</param>
        /// <param name="max">If provided, will force max value.</param>
        public NumbersOnly(bool allowDecimal, double? min = null, double? max = null)
        {
            AllowDecimalPoint = allowDecimal;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create number validator with default params.
        /// </summary>
        public NumbersOnly() : this(false)
        {
        }

        /// <summary>
        /// Return true if text input is a valid number.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public override bool ValidateText(ref string text, string oldText)
        {
            // if string empty return true
            if (text.Length == 0)
            {
                return true;
            }

            // make sure no spaces
            if (text.Contains(' '))
            {
                return false;
            }

            // will contain value as number
            double num;

            // try to parse as double
            if (AllowDecimalPoint)
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
            if (Min != null && num < (double)Min) { text = Min.ToString(); }
            if (Max != null && num > (double)Max) { text = Max.ToString(); }

            // valid number input
            return true;
        }
    }
}