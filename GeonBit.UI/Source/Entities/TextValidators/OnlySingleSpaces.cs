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
    /// Make sure input don't contain double spaces or tabs.
    /// </summary>
    [System.Serializable]
    public class OnlySingleSpaces : ITextValidator
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static OnlySingleSpaces()
        {
            Entity.MakeSerializable(typeof(OnlySingleSpaces));
        }

        /// <summary>
        /// Return true if text input don't contain double spaces or tabs.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public override bool ValidateText(ref string text, string oldText)
        {
            return !text.Contains("  ") && !text.Contains("\t");
        }
    }
}