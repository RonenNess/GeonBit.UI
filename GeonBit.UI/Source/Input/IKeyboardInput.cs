#region File Description
//-----------------------------------------------------------------------------
// Define the keyboard-based input interface. This is the object GeonBit.UI uses
// to detect typing and key pressing.
// To support alternative keyboard-like input, inherit from this interface and
// and provide your alternative instance to the interface manager of GeonBit.UI.
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;


namespace GeonBit.UI
{
    /// <summary>
    /// Some special characters input.
    /// Note: enum values are based on ascii table values for these special characters.
    /// </summary>
    enum SpecialChars
    {
        Null = 0,           // no character input
        Delete = 127,       // delete char
        Backspace = 8,      // backspace char
        Space = 32,         // space character input
        ArrowLeft = 1,      // arrow left - moving caret left
        ArrowRight = 2,     // arrow right - moving caret right
        ArrowUp = 3,        // arrow up - moving caret line up
        ArrowDown = 4,      // arrow down - moving caret line down
    };

    /// <summary>
    /// Define the interface GeonBit.UI uses to get keyboard and typing input from users.
    /// </summary>
    public interface IKeyboardInput
    {
        /// <summary>
        /// Update input (called every frame).
        /// </summary>
        /// <param name="gameTime">Update frame game time.</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Get textual input from keyboard.
        /// If user enter keys it will push them into string, if delete or backspace will remove chars, etc.
        /// This also handles keyboard cooldown, to make it feel like windows-input.
        /// </summary>
        /// <param name="txt">String to push text input into.</param>
        /// <param name="lineWidth">How many characters can fit in a line.</param>
        /// <param name="pos">Position to insert / remove characters. -1 to push at the end of string. After done, will contain actual new caret position.</param>
        /// <returns>String after text input applied on it.</returns>
        string GetTextInput(string txt, int lineWidth, ref int pos);
    }
}
