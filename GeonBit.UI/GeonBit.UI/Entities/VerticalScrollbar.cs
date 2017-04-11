#region File Description
//-----------------------------------------------------------------------------
// Vertical scrollbar is used internally to scroll through lists etc.
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
    /// Used internally as a scrollbar for lists, text boxes, etc..
    /// </summary>
    public class VerticalScrollbar : Slider
    {
        // frame and mark actual height
        float _frameActualHeight = 0f;
        int _markHeight = 20;

        /// <summary>Default scrollbar size for when no size is provided or when -1 is set for either width or height.</summary>
        new public static Vector2 DefaultSize = new Vector2(30f, 0f);

        /// <summary>Default styling for vertical scrollbars. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>
        /// Create the scrollbar.
        /// </summary>
        /// <param name="min">Min scrollbar value.</param>
        /// <param name="max">Max scrollbar value.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public VerticalScrollbar(uint min, uint max, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(0, 0, USE_DEFAULT_SIZE, SliderSkin.Default, anchor, offset)
        {
            // set this scrollbar to respond even when direct parent is locked
            DoEventsIfDirectParentIsLocked = true;

            // update default style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Handle mouse down event.
        /// The Scrollbar entity override this function to handle sliding mark up and down, instead of left-right.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoOnMouseDown(InputHelper input)
        {
            // if mouse x is on the 0 side set to min
            if (input.MousePosition.Y <= _destRect.Y + _frameActualHeight)
            {
                Value = _value - GetStepSize();
            }
            // else if mouse x is on the max side, set to max
            else if (input.MousePosition.Y >= _destRect.Bottom - _frameActualHeight)
            {
                Value = _value + GetStepSize();
            }

            // call base function
            base.DoOnMouseDown(input);
        }

        /// <summary>
        /// Handle while mouse is down event.
        /// The Scrollbar entity override this function to handle sliding mark up and down, instead of left-right.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoWhileMouseDown(InputHelper input)
        {
            // if in the middle calculate value based on mouse position
            if ((input.MousePosition.Y >= _destRect.Y + _frameActualHeight * 0.5) && 
               (input.MousePosition.Y <= _destRect.Bottom - _frameActualHeight * 0.5))
            {
                float relativePos = (input.MousePosition.Y - _destRect.Y - _frameActualHeight * 0.5f - _markHeight * 0.5f);
                float internalHeight = (_destRect.Height - _frameActualHeight) - _markHeight * 0.5f;
                float relativeVal = (relativePos / internalHeight);
                Value = (int)System.Math.Round(Min + relativeVal * (Max - Min));
            }

            // call event handler
            WhileMouseDown?.Invoke(this);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // get textures based on type
            Texture2D texture = Resources.VerticalScrollbarTexture;
            Texture2D markTexture = Resources.VerticalScrollbarMarkTexture;
            float FrameHeight = Resources.VerticalScrollbarData.FrameHeight;

            // draw scrollbar body
            UserInterface.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, new Vector2(0f, FrameHeight), 1, FillColor);

            // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
            Vector2 frameSizeTexture = new Vector2(texture.Width, texture.Height * FrameHeight);
            Vector2 frameSizeRender = frameSizeTexture;
            float ScaleYfac = _destRect.Width / frameSizeRender.X;

            // calc the size of the mark piece
            int markWidth = _destRect.Width;
            _markHeight = (int)(((float)markTexture.Height / (float)markTexture.Width) * (float)markWidth);

            // calc frame width in pixels
            _frameActualHeight = FrameHeight * texture.Height * ScaleYfac;

            // now draw mark
            float markY = _destRect.Y + _frameActualHeight + _markHeight * 0.5f + (_destRect.Height - _frameActualHeight * 2 - _markHeight) * (GetValueAsPercent());
            Rectangle markDest = new Rectangle(_destRect.X, (int)System.Math.Round(markY) - _markHeight / 2, markWidth, _markHeight);
            UserInterface.DrawUtils.DrawImage(spriteBatch, markTexture, markDest, FillColor);
        }

        /// <summary>
        /// Called every frame after update.
        /// Scrollbar override this function to handle wheel scroll while pointing on parent entity - we still want to capture that.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoAfterUpdate(InputHelper input)
        {
            // if the active entity is self or parent, listen to mousewheel
            if (_isInteractable && 
                (UserInterface.ActiveEntity == this || 
                UserInterface.ActiveEntity == _parent || 
                (UserInterface.ActiveEntity != null && UserInterface.ActiveEntity.IsDeepChildOf(_parent))))
            {
                if (input.MouseWheelChange != 0)
                {
                    Value = _value - input.MouseWheelChange * GetStepSize();
                }
            }
        }

        /// <summary>
        /// Handle when mouse wheel scroll and this entity is the active entity.
        /// Note: Scrollbar entity override this function to change scrollbar value based on wheel scroll, which is inverted.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoOnMouseWheelScroll(InputHelper input)
        {
            Value = _value - input.MouseWheelChange * GetStepSize();
        }
    }
}
