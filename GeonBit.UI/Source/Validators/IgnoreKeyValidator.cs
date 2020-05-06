using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework.Input;

namespace GeonBit.UI.Validators
{
    /// <summary>
    /// Ignores a given key
    /// </summary>
    public class IgnoreKeyValidator : ITextValidator
    {
        private readonly Keys _key;

        /// <summary>
        /// Ignores a given key
        /// </summary>
        /// <param name="key">The key to ignore</param>
        public IgnoreKeyValidator(Keys key)
        {
            _key = key;
        }

        /// <summary>
        /// Checks if the given key has been pressed, and if so, resets the text to its original text.
        /// </summary>
        public override bool ValidateText(ref string text, string oldText)
        {
            if (Keyboard.GetState().IsKeyDown(_key))
                text = oldText;
            
            return base.ValidateText(ref text, oldText);
        }
    }
}