#region File Description
//-----------------------------------------------------------------------------
// Helper utility that implements default mouse and keyboard input for GeonBit.UI.
// You can create your own mouse/keyboard inputs to replace this.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GeonBit.UI
{
    /// <summary>
    /// Implement Mouse Input and Keyboard Input for GeonBit.UI + provide some helpful utils you can use externally.
    /// This is the object we provide to GeonBit.UI by default, if no other input providers were set by user.
    /// </summary>
    public class DefaultInputProvider : IMouseInput, IKeyboardInput
    {
        // store current & previous keyboard states so we can detect key release
        KeyboardState _newKeyboardState;
        KeyboardState _oldKeyboardState;

        // store current & previous mouse states so we can detect key release and diff
        MouseState _newMouseState;
        MouseState _oldMouseState;

        // store old and new mouse position so we can get diff
        Vector2 _newMousePos;
        Vector2 _oldMousePos;

        // store current frame gametime
        GameTime _currTime;

        // store all Key values in lookup array
        Keys[] _allKeyValues;

        /// <summary>An artificial "lag" after a key is pressed when typing text input, to prevent mistake duplications.</summary>
        public float KeysTypeCooldown = 0.6f;

        // last character that was pressed down
        char _currCharacterInput = (char)SpecialChars.Null;

        // last key that provide character input and was pressed
        Keys _currCharacterInputKey = Keys.Escape;

        // keyboard input cooldown for textual input
        float _keyboardInputCooldown = 0f;

        // true when a new keyboard key is pressed
        bool _newKeyIsPressed = false;

        // current capslock state
        bool _capslock = false;

        /// <summary>
        /// Current mouse wheel value.
        /// </summary>
        public int MouseWheel { get; private set; }

        /// <summary>
        /// Mouse wheel change sign (eg 0, 1 or -1) since last frame.
        /// </summary>
        public int MouseWheelChange { get; private set; }

        /// <summary>
        /// Create the input helper.
        /// </summary>
        public DefaultInputProvider()
        {
            _allKeyValues = (Keys[])System.Enum.GetValues(typeof(Keys));

            // init keyboard states
            _newKeyboardState = _oldKeyboardState;

            // init mouse states
            _newMouseState = _oldMouseState;
            _newMousePos = new Vector2(_newMouseState.X, _newMouseState.Y);

            // call first update to get starting positions
            Update(new GameTime());
        }

        /// <summary>
        /// Current frame game time.
        /// </summary>
        public GameTime CurrGameTime
        {
            get { return _currTime; }
        }

        /// <summary>
        /// Update current states.
        /// If used outside GeonBit.UI, this function should be called first thing inside your game 'Update()' function,
        /// and before you make any use of this class.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // store game time
            _currTime = gameTime;

            // store previous states
            _oldMouseState = _newMouseState;
            _oldKeyboardState = _newKeyboardState;

            // get new states
            _newMouseState = Mouse.GetState();
            _newKeyboardState = Keyboard.GetState();

            // get mouse position
            _oldMousePos = _newMousePos;
            _newMousePos = new Vector2(_newMouseState.X, _newMouseState.Y);

            // get mouse wheel state
            int prevMouseWheel = MouseWheel;
            MouseWheel = _newMouseState.ScrollWheelValue;
            MouseWheelChange = System.Math.Sign(MouseWheel - prevMouseWheel);

            // update capslock state
            if (_newKeyboardState.IsKeyDown(Keys.CapsLock) && !_oldKeyboardState.IsKeyDown(Keys.CapsLock))
            {
                _capslock = !_capslock;
            }

            // decrease keyboard input cooldown time
            if (_keyboardInputCooldown > 0f)
            {
                _newKeyIsPressed = false;
                _keyboardInputCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // if current text input key is no longer down, reset text input character
            if (_currCharacterInput != (char)SpecialChars.Null &&
                !_newKeyboardState.IsKeyDown(_currCharacterInputKey))
            {
                _currCharacterInput = (char)SpecialChars.Null;
            }

            // send key-down events
            foreach (var key in _allKeyValues)
            {
                if (_newKeyboardState.IsKeyDown(key) && !_oldKeyboardState.IsKeyDown(key))
                {
                    OnKeyPressed(key);
                }
            }
        }

        /// <summary>
        /// Move the cursor to be at the center of the screen.
        /// </summary>
        /// <param name="pos">New mouse position.</param>
        public void UpdateMousePosition(Vector2 pos)
        {
            // move mouse position back to center
            Mouse.SetPosition((int)pos.X, (int)pos.Y);
            _newMousePos = _oldMousePos = pos;
        }

        /// <summary>
        /// Calculate and return current cursor position transformed by a matrix.
        /// </summary>
        /// <param name="transform">Matrix to transform cursor position by.</param>
        /// <returns>Cursor position with optional transform applied.</returns>
        public Vector2 TransformMousePosition(Matrix? transform)
        {
            var newMousePos = _newMousePos;
            if (transform != null)
            {
                return Vector2.Transform(newMousePos, transform.Value) - new Vector2(transform.Value.Translation.X, transform.Value.Translation.Y);
            }
            return newMousePos;
        }

        /// <summary>
        /// Called every time a keyboard key is pressed (called once on the frame key was pressed).
        /// </summary>
        /// <param name="key">Key code that is being pressed on this frame.</param>
        protected void OnKeyPressed(Keys key)
        {
            NewKeyTextInput(key);
        }

        /// <summary>
        /// This update the character the user currently type down, for text input.
        /// This function called whenever a new key is pressed down, and becomes the current input
        /// until it is released.
        /// </summary>
        /// <param name="key">Key code that is being pressed down on this frame.</param>
        private void NewKeyTextInput(Keys key)
        {
            // reset cooldown time and set new key pressed = true
            _keyboardInputCooldown = KeysTypeCooldown;
            _newKeyIsPressed = true;

            // get if shift is currently down
            bool isShiftDown = _newKeyboardState.IsKeyDown(Keys.LeftShift) || _newKeyboardState.IsKeyDown(Keys.RightShift);

            // set curr input key, but also keep the previous key in case we need to revert
            Keys prevKey = _currCharacterInputKey;
            _currCharacterInputKey = key;

            // handle special keys and characters
            switch (key)
            {
                case Keys.Space:
                    _currCharacterInput = (char)SpecialChars.Space;
                    return;

                case Keys.Left:
                    _currCharacterInput = (char)SpecialChars.ArrowLeft;
                    return;

                case Keys.Right:
                    _currCharacterInput = (char)SpecialChars.ArrowRight;
                    return;

                case Keys.Up:
                    _currCharacterInput = (char)SpecialChars.ArrowUp;
                    return;

                case Keys.Down:
                    _currCharacterInput = (char)SpecialChars.ArrowDown;
                    return;

                case Keys.Delete:
                    _currCharacterInput = (char)SpecialChars.Delete;
                    return;

                case Keys.Back:
                    _currCharacterInput = (char)SpecialChars.Backspace;
                    return;

                case Keys.CapsLock:
                case Keys.RightShift:
                case Keys.LeftShift:
                    _newKeyIsPressed = false;
                    return;

                // line break
                case Keys.Enter:
                    _currCharacterInput = '\n';
                    return;

                // number 0
                case Keys.D0:
                case Keys.NumPad0:
                    _currCharacterInput = (isShiftDown && key == Keys.D0) ? ')' : '0';
                    return;

                // number 9
                case Keys.D9:
                case Keys.NumPad9:
                    _currCharacterInput = (isShiftDown && key == Keys.D9) ? '(' : '9';
                    return;

                // number 8
                case Keys.D8:
                case Keys.NumPad8:
                    _currCharacterInput = (isShiftDown && key == Keys.D8) ? '*' : '8';
                    return;

                // number 7
                case Keys.D7:
                case Keys.NumPad7:
                    _currCharacterInput = (isShiftDown && key == Keys.D7) ? '&' : '7';
                    return;

                // number 6
                case Keys.D6:
                case Keys.NumPad6:
                    _currCharacterInput = (isShiftDown && key == Keys.D6) ? '^' : '6';
                    return;

                // number 5
                case Keys.D5:
                case Keys.NumPad5:
                    _currCharacterInput = (isShiftDown && key == Keys.D5) ? '%' : '5';
                    return;

                // number 4
                case Keys.D4:
                case Keys.NumPad4:
                    _currCharacterInput = (isShiftDown && key == Keys.D4) ? '$' : '4';
                    return;

                // number 3
                case Keys.D3:
                case Keys.NumPad3:
                    _currCharacterInput = (isShiftDown && key == Keys.D3) ? '#' : '3';
                    return;

                // number 2
                case Keys.D2:
                case Keys.NumPad2:
                    _currCharacterInput = (isShiftDown && key == Keys.D2) ? '@' : '2';
                    return;

                // number 1
                case Keys.D1:
                case Keys.NumPad1:
                    _currCharacterInput = (isShiftDown && key == Keys.D1) ? '!' : '1';
                    return;

                // question mark
                case Keys.OemQuestion:
                    _currCharacterInput = isShiftDown ? '?' : '/';
                    return;

                // quotes
                case Keys.OemQuotes:
                    _currCharacterInput = isShiftDown ? '\"' : '\'';
                    return;

                // semicolon
                case Keys.OemSemicolon:
                    _currCharacterInput = isShiftDown ? ':' : ';';
                    return;

                // tilde
                case Keys.OemTilde:
                    _currCharacterInput = isShiftDown ? '~' : '`';
                    return;

                // open brackets
                case Keys.OemOpenBrackets:
                    _currCharacterInput = isShiftDown ? '{' : '[';
                    return;

                // close brackets
                case Keys.OemCloseBrackets:
                    _currCharacterInput = isShiftDown ? '}' : ']';
                    return;

                // add
                case Keys.OemPlus:
                case Keys.Add:
                    _currCharacterInput = (isShiftDown || key == Keys.Add) ? '+' : '=';
                    return;

                // substract
                case Keys.OemMinus:
                case Keys.Subtract:
                    _currCharacterInput = isShiftDown ? '_' : '-';
                    return;

                // decimal dot
                case Keys.OemPeriod:
                case Keys.Decimal:
                    _currCharacterInput = isShiftDown ? '>' : '.';
                    return;

                // divide
                case Keys.Divide:
                    _currCharacterInput = isShiftDown ? '?' : '/';
                    return;

                // multiply
                case Keys.Multiply:
                    _currCharacterInput = '*';
                    return;

                // backslash
                case Keys.OemBackslash:
                    _currCharacterInput = isShiftDown ? '|' : '\\';
                    return;

                // comma
                case Keys.OemComma:
                    _currCharacterInput = isShiftDown ? '<' : ',';
                    return;

                // tab
                case Keys.Tab:
                    _currCharacterInput = ' ';
                    return;

                // not a special char - revert last character input key and continue processing.
                default:
                    _currCharacterInputKey = prevKey;
                    break;
            };

            // get current key thats getting pressed as a string
            string lastCharPressedStr = key.ToString();

            // if character is not a valid char but a special key we don't want to handle, skip
            // (note: keys that are characters should have length of 1)
            if (lastCharPressedStr.Length > 1)
            {
                return;
            }

            // set current key as the current text input key
            _currCharacterInputKey = key;

            // get current capslock state and invert if shift is down
            bool capsLock = _capslock;
            if (isShiftDown)
            {
                capsLock = !capsLock;
            }

            // fix case and set as current char pressed
            _currCharacterInput = (capsLock ? lastCharPressedStr.ToUpper() : lastCharPressedStr.ToLower())[0];

        }

        /// <summary>
        /// Get textual input from keyboard.
        /// If user enter keys it will push them into string, if delete or backspace will remove chars, etc.
        /// This also handles keyboard cooldown, to make it feel like windows-input.
        /// </summary>
        /// <param name="txt">String to push text input into.</param>
        /// <param name="lineWidth">How many characters can fit in a line.</param>
        /// <param name="pos">Position to insert / remove characters. -1 to push at the end of string. After done, will contain actual new caret position.</param>
        /// <returns>String after text input applied on it.</returns>
        public string GetTextInput(string txt, int lineWidth, ref int pos)
        {
            // if need to skip due to cooldown time
            if (!_newKeyIsPressed && _keyboardInputCooldown > 0f)
            {
                return txt;
            }

            // if no valid characters are currently input
            if (_currCharacterInput == (char)SpecialChars.Null)
            {
                return txt;
            }

            // get default position
            if (pos == -1)
            {
                pos = txt.Length;
            }

            // handle special chars
            switch (_currCharacterInput)
            {
                case (char)SpecialChars.ArrowLeft:
                    if (--pos < 0) { pos = 0; }
                    return txt;

                case (char)SpecialChars.ArrowRight:
                    if (++pos > txt.Length) { pos = txt.Length; }
                    return txt;

                case (char)SpecialChars.ArrowUp:
                    pos -= lineWidth;
                    if (pos < 0) { pos = 0; }
                    return txt;

                case (char)SpecialChars.ArrowDown:
                    pos += lineWidth;
                    if (pos > txt.Length) { pos = txt.Length; }
                    return txt;

                case (char)SpecialChars.Backspace:
                    pos--;
                    return (pos < txt.Length && pos >= 0 && txt.Length > 0) ? txt.Remove(pos, 1) : txt;

                case (char)SpecialChars.Delete:
                    return (pos < txt.Length && txt.Length > 0) ? txt.Remove(pos, 1) : txt;
            }

            // add current character
            return txt.Insert(pos++, _currCharacterInput.ToString());
        }

        /// <summary>
        /// Get current mouse Position.
        /// </summary>
        public Vector2 MousePosition
        {
            get { return _newMousePos; }
        }

        /// <summary>
        /// Get mouse position change since last frame.
        /// </summary>
        /// <return>Mouse position change as a 2d vector.</return>
        public Vector2 MousePositionDiff
        {
            get { return _newMousePos - _oldMousePos; }
        }

        /// <summary>
        /// Check if a given mouse button is down.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is down.</return>
        public bool MouseButtonDown(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any of mouse buttons is down.
        /// </summary>
        /// <returns>True if any mouse button is currently down.</returns>
        public bool AnyMouseButtonDown()
        {
            return MouseButtonDown(MouseButton.Left) ||
                MouseButtonDown(MouseButton.Right) ||
                MouseButtonDown(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was released in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was released in this frame.</return>
        public bool MouseButtonReleased(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any mouse button was released this frame.
        /// </summary>
        /// <returns>True if any mouse button was released.</returns>
        public bool AnyMouseButtonReleased()
        {
            return MouseButtonReleased(MouseButton.Left) ||
                MouseButtonReleased(MouseButton.Right) ||
                MouseButtonReleased(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was pressed in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was pressed in this frame.</return>
        public bool MouseButtonPressed(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Pressed && GetMousePreviousButtonState(button) == ButtonState.Released;
        }

        /// <summary>
        /// Return if any mouse button was pressed in current frame.
        /// </summary>
        /// <returns>True if any mouse button was pressed in current frame..</returns>
        public bool AnyMouseButtonPressed()
        {
            return MouseButtonPressed(MouseButton.Left) ||
                MouseButtonPressed(MouseButton.Right) ||
                MouseButtonPressed(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was just clicked (eg released after being pressed down)
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is clicked.</return>
        public bool MouseButtonClick(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any of mouse buttons was clicked this frame.
        /// </summary>
        /// <returns>True if any mouse button was clicked.</returns>
        public bool AnyMouseButtonClicked()
        {
            return
                MouseButtonClick(MouseButton.Left) ||
                MouseButtonClick(MouseButton.Right) ||
                MouseButtonClick(MouseButton.Middle);
        }

        /// <summary>
        /// Return the state of a mouse button (up / down).
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMouseButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _newMouseState.LeftButton;
                case MouseButton.Right:
                    return _newMouseState.RightButton;
                case MouseButton.Middle:
                    return _newMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Return the state of a mouse button (up / down), in previous frame.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMousePreviousButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _oldMouseState.LeftButton;
                case MouseButton.Right:
                    return _oldMouseState.RightButton;
                case MouseButton.Middle:
                    return _oldMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Check if a given keyboard key is down.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button is down.</return>
        public bool IsKeyDown(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a given keyboard key was previously pressed down and now released in this frame.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button was just released.</return>
        public bool IsKeyReleased(Keys key)
        {
            return _oldKeyboardState.IsKeyDown(key) &&
                   _newKeyboardState.IsKeyUp(key);
        }
    }
}
