#region File Description
//-----------------------------------------------------------------------------
// Define the mouse-based input interface. This is the object GeonBit.UI uses
// to detect clicks, drags, mouse pointing on entities, etc.
// To support things like touch input, inherit from this interface and
// emulate mouse inputs from touch (and provide your instance to the interface
// manager of GeonBit.UI).
//
// Author: Ronen Ness.
// Since: 2018.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;


namespace GeonBit.UI
{
    /// <summary>
    /// Mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        ///<summary>Left mouse button.</summary>
        Left,

        ///<summary>Right mouse button.</summary>
        Right,

        ///<summary>Middle mouse button (scrollwheel when clicked).</summary>
        Middle
    };

    /// <summary>
    /// Define the interface GeonBit.UI uses to get mouse or mouse-like input from users.
    /// </summary>
    public interface IMouseInput
    {
        /// <summary>
        /// Update input (called every frame).
        /// </summary>
        /// <param name="gameTime">Update frame game time.</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Get current mouse Position.
        /// </summary>
        Vector2 MousePosition { get; }

        /// <summary>
        /// Get mouse position change since last frame.
        /// </summary>
        Vector2 MousePositionDiff { get; }

        /// <summary>
        /// Move the cursor to be at the center of the screen.
        /// </summary>
        /// <param name="pos">New mouse position.</param>
        void UpdateMousePosition(Vector2 pos);

        /// <summary>
        /// Calculate and return current cursor position transformed by a matrix.
        /// </summary>
        /// <param name="transform">Matrix to transform cursor position by.</param>
        /// <returns>Cursor position with optional transform applied.</returns>
        Vector2 TransformMousePosition(Matrix? transform);

        /// <summary>
        /// Check if a given mouse button is down.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is down.</return>
        bool MouseButtonDown(MouseButton button = MouseButton.Left);

        /// <summary>
        /// Return if any of mouse buttons is down.
        /// </summary>
        /// <returns>True if any mouse button is currently down.</returns>
        bool AnyMouseButtonDown();

        /// <summary>
        /// Check if a given mouse button was pressed in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was pressed in this frame.</return>
        bool MouseButtonPressed(MouseButton button = MouseButton.Left);

        /// <summary>
        /// Return if any mouse button was pressed in current frame.
        /// </summary>
        /// <returns>True if any mouse button was pressed in current frame.</returns>
        bool AnyMouseButtonPressed();

        /// <summary>
        /// Check if a given mouse button was just clicked (eg released after being pressed down)
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is clicked.</return>
        bool MouseButtonClick(MouseButton button = MouseButton.Left);

        /// <summary>
        /// Return if any of mouse buttons was clicked this frame.
        /// </summary>
        /// <returns>True if any mouse button was clicked.</returns>
        bool AnyMouseButtonClicked();

        /// <summary>
        /// Check if a given mouse button was released in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was released in this frame.</return>
        bool MouseButtonReleased(MouseButton button = MouseButton.Left);

        /// <summary>
        /// Return if any mouse button was released this frame.
        /// </summary>
        /// <returns>True if any mouse button was released.</returns>
        bool AnyMouseButtonReleased();

        /// <summary>
        /// Current mouse wheel value.
        /// </summary>
        int MouseWheel { get; }

        /// <summary>
        /// Mouse wheel change sign (eg 0, 1 or -1) since last frame.
        /// </summary>
        int MouseWheelChange { get; }
    }
}
