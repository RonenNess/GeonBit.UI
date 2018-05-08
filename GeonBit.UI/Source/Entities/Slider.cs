#region File Description
//-----------------------------------------------------------------------------
// Sliders are horizontal bars, similar to scrollbar, that lets user pick
// numeric values in pre-defined range.
// For example, a slider is often use to pick settings like volume, gamma, etc..
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Different sliders skins (textures).
    /// </summary>
    public enum SliderSkin
    {
        /// <summary>Default, thin slider skin.</summary>
        Default = 0,

        /// <summary>More fancy, thicker slider skin.</summary>
        Fancy = 1,
    }

    /// <summary>
    /// Slider entity looks like a horizontal scrollbar that the user can drag left and right to select a numeric value from range.
    /// </summary>
    public class Slider : Entity
    {
        // slider style
        SliderSkin _skin;

        /// <summary>Min slider value.</summary>
        protected uint _min;

        /// <summary>Max slider value.</summary>
        protected uint _max;

        /// <summary>How many steps (ticks) are in range.</summary>
        protected uint _stepsCount = 0;

        /// <summary>Current value.</summary>
        protected int _value;

        /// <summary>Actual frame width in pixels (used internally).</summary>
        protected float _frameActualWidth = 0f;

        /// <summary>Default styling for the slider itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Actual mark width in pixels (used internally).</summary>
        protected int _markWidth = 20;

        /// <summary>Default slider size for when no size is provided or when -1 is set for either width or height.</summary>
        new public static Vector2 DefaultSize = new Vector2(0f, 30f);

        /// <summary>
        /// Create the slider.
        /// </summary>
        /// <param name="min">Min value (inclusive).</param>
        /// <param name="max">Max value (inclusive).</param>
        /// <param name="size">Slider size.</param>
        /// <param name="skin">Slider skin (texture).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Slider(uint min, uint max, Vector2 size, SliderSkin skin = SliderSkin.Default, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            // store style
            _skin = skin;

            // store min and max and set default value
            Min = min;
            Max = max;

            // set default steps count
            _stepsCount = Max - Min;

            // set starting value to center
            _value = (int)(Min + (Max - Min) / 2);

            // update default style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Create slider with default size.
        /// </summary>
        /// <param name="min">Min value (inclusive).</param>
        /// <param name="max">Max value (inclusive).</param>
        /// <param name="skin">Slider skin (texture).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Slider(uint min = 0, uint max = 10, SliderSkin skin = SliderSkin.Default, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            this(min, max, USE_DEFAULT_SIZE, skin, anchor, offset)
        {
        }

        /// <summary>
        /// Get the size of a single step.
        /// </summary>
        /// <returns>Size of a single step, eg how much value changes in a step.</returns>
        public int GetStepSize()
        {
            if (StepsCount > 0)
            {
                if (Max - Min == StepsCount)
                {
                    return 1;
                }
                return (int)System.Math.Max(((Max - Min) / StepsCount + 1), 2);
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Normalize value to fit slider range and be multiply of steps size.
        /// </summary>
        /// <param name="value">Value to normalize.</param>
        /// <returns>Normalized value.</returns>
        protected int NormalizeValue(int value)
        {
            // round to steps
            float stepSize = (float)GetStepSize();
            value = (int)(System.Math.Round(((double)value) / stepSize) * stepSize);

            // camp between min and max
            value = (int)System.Math.Min(System.Math.Max(value, Min), Max);

            // return normalized value
            return value;
        }

        /// <summary>
        /// Current slider value.
        /// </summary>
        public int Value
        {
            // get current value
            get { return _value; }

            // set new value
            set
            {
                int prevVal = _value;
                _value = NormalizeValue(value);
                if (prevVal != _value) { DoOnValueChange(); }
            }
        }

        /// <summary>
        /// Slider min value (inclusive).
        /// </summary>
        public uint Min
        {
            get { return _min; }
            set { if (_min != value) { _min = value; Value = Value; } }
        }

        /// <summary>
        /// Slider max value (inclusive).
        /// </summary>
        public uint Max
        {
            // get max value
            get
            {
                return _max;
            }

            // set max value
            set
            {
                // only if changed
                if (_max != value)
                {
                    // set value
                    _max = value;

                    // if value is bigger than max, cap it
                    if (Value > Max) Value = (int)Max;
                }
            }
        }

        /// <summary>
        /// How many steps (ticks) in slider range.
        /// </summary>
        public uint StepsCount
        {
            // get current steps count
            get { return _stepsCount; }

            // set steps count and call Value = Value to normalize current value to new steps count.
            set { _stepsCount = value; Value = Value; }
        }

        /// <summary>
        /// Is the slider a natrually-interactable entity.
        /// </summary>
        /// <returns>True.</returns>
        override public bool IsNaturallyInteractable()
        {
            return true;
        }

        /// <summary>
        /// Called every frame while mouse button is down over this entity.
        /// The slider entity override this function to handle slider value change (eg slider mark dragging).
        /// </summary>
        override protected void DoWhileMouseDown()
        {
            // get mouse position and apply scroll value
            var mousePos = GetMousePos();
            mousePos += _lastScrollVal.ToVector2();

            // if mouse x is on the 0 side set to min
            if (mousePos.X <= _destRect.X + _frameActualWidth)
            {
                Value = (int)Min;
            }
            // else if mouse x is on the max side, set to max
            else if (mousePos.X >= _destRect.Right - _frameActualWidth)
            {
                Value = (int)Max;
            }
            // if in the middle calculate value based on mouse position
            else
            {
                float val = ((mousePos.X - _destRect.X - _frameActualWidth + _markWidth / 2) / (_destRect.Width - _frameActualWidth * 2));
                Value = (int)(Min + val * (Max - Min));
            }

            // call base handler
            base.DoWhileMouseDown();
        }

        /// <summary>
        /// Return current value as a percent between min and max.
        /// </summary>
        /// <returns>Current value as percent between min and max (0f-1f).</returns>
        public float GetValueAsPercent()
        {
            return (float)(_value - Min) / (float)(Max - Min);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // get textures based on skin
            Texture2D texture = Resources.SliderTextures[_skin];
            Texture2D markTexture = Resources.SliderMarkTextures[_skin];

            // get slider metadata
            DataTypes.TextureData data = Resources.SliderData[(int)_skin];
            float frameWidth = data.FrameWidth;

            // draw slider body
            UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, new Vector2(frameWidth, 0f), 1, FillColor);

            // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
            Vector2 frameSizeTexture = new Vector2(texture.Width * frameWidth, texture.Height);
            Vector2 frameSizeRender = frameSizeTexture;
            float ScaleXfac = _destRect.Height / frameSizeRender.Y;

            // calc the size of the mark piece
            int markHeight = _destRect.Height;
            _markWidth = (int)(((float)markTexture.Width / (float)markTexture.Height) * (float)markHeight);

            // calc frame width in pixels
            _frameActualWidth = frameWidth * texture.Width * ScaleXfac;

            // now draw mark
            float markX = _destRect.X + _frameActualWidth + _markWidth * 0.5f + (_destRect.Width - _frameActualWidth * 2 - _markWidth) * GetValueAsPercent();
            Rectangle markDest = new Rectangle((int)System.Math.Round(markX) - _markWidth / 2, _destRect.Y, _markWidth, markHeight);
            UserInterface.Active.DrawUtils.DrawImage(spriteBatch, markTexture, markDest, FillColor);

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }

        /// <summary>
        /// Handle when mouse wheel scroll and this entity is the active entity.
        /// Note: Slider entity override this function to change slider value based on wheel scroll.
        /// </summary>
        override protected void DoOnMouseWheelScroll()
        {
            Value = _value + Input.MouseWheelChange * GetStepSize();
        }
    }
}
