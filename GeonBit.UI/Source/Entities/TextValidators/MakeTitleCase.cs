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
    /// Make sure input is always title, eg starts with a capital letter followed by lowercase.
    /// </summary>
    [System.Serializable]
    public class MakeTitleCase : ITextValidator
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static MakeTitleCase()
        {
            Entity.MakeSerializable(typeof(MakeTitleCase));
        }

        /// <summary>
        /// Always return true, and make first character uppercase while all following
        /// chars lowercase.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>Always return true.</returns>
        public override bool ValidateText(ref string text, string oldText)
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