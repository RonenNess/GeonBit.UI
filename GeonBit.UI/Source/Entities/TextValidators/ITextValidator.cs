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
    /// Note: this cannot be an interface due to serialization.
    /// </summary>
    public partial class ITextValidator
    {
        /// <summary>
        /// Get the new text input value and return true if valid.
        /// This function can either return false to scrap input changes, or change the text and return true.
        /// </summary>
        /// <param name="text">New text input value.</param>
        /// <param name="oldText">Previous text input value.</param>
        /// <returns>If TextInput value is legal.</returns>
        public virtual bool ValidateText(ref string text, string oldText) { return true; }
    }
}
