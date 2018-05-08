#region File Description
//-----------------------------------------------------------------------------
// ProgressBar is a type of slider, but with a prograss bar graphics.
//
// It can be useful to show things like loading progress, HP left, XP needed
// until level up, etc.
//
// Please note:
// 1. By default, prograssbar are not locked and can be changed by the user.
//		To make the prograssbar locked (eg changeable only through code), use
//		the 'Locked' property.
// 2. The default color is greed, but you can easily change it via the
//		FillColor property.
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
    /// A sub-class of the slider entity, with graphics more fitting for a progress bar or things like hp bar etc.
    /// Behaves the same as a slider, if you want it to be for display only (and not changeable by user), simple set Locked = true.
    /// </summary>
    public class ProgressBar : Slider
    {
        /// <summary>Default styling for progress bar. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for the progress bar fill part. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultFillStyle = new StyleSheet();

        /// <summary>Default progressbar size for when no size is provided or when -1 is set for either width or height.</summary>
        new public static Vector2 DefaultSize = new Vector2(0f, 52f);

        /// <summary>The fill part of the progress bar.</summary>
        public Image ProgressFill;

        /// <summary>An optional caption to display over the center of the progress bar.</summary>
        public Label Caption;

        /// <summary>
        /// Create progress bar with size.
        /// </summary>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="size">Entity size.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public ProgressBar(uint min, uint max, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(min, max, size, SliderSkin.Default, anchor, offset)
        {
            // update default styles
            UpdateStyle(DefaultStyle);

            // create the fill part
            Padding = Vector2.Zero;
            ProgressFill = new Image(Resources.ProgressBarFillTexture, Vector2.Zero, ImageDrawMode.Stretch, Anchor.CenterLeft);
            ProgressFill.UpdateStyle(DefaultFillStyle);
            ProgressFill._hiddenInternalEntity = true;
            AddChild(ProgressFill, true);

            // the caption is just a label centered on the progress bar itself
            Caption = new Label(string.Empty, Anchor.Center);
            Caption.ClickThrough = true;
            Caption._hiddenInternalEntity = true;
            AddChild(Caption);
        }

        /// <summary>
        /// Create progress bar.
        /// </summary>
        /// <param name="min">Min value.</param>
        /// <param name="max">Max value.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public ProgressBar(uint min = 0, uint max = 10, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            this(min, max, USE_DEFAULT_SIZE, anchor, offset)
        { }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // get progressbar frame width
            float progressbarFrameWidth = Resources.ProgressBarData.FrameWidth;

            // draw progress bar frame
            Texture2D barTexture = Resources.ProgressBarTexture;
            UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, barTexture, _destRect, new Vector2(progressbarFrameWidth, 0f), 1, FillColor);

            // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
            Vector2 frameSizeTexture = new Vector2(barTexture.Width * progressbarFrameWidth, barTexture.Height);
            Vector2 frameSizeRender = frameSizeTexture;
            float ScaleXfac = _destRect.Height / frameSizeRender.Y;

            // calc frame width in pixels
            _frameActualWidth = progressbarFrameWidth * barTexture.Width * ScaleXfac;

            // update the progress bar color and size
            int markWidth = (int)((_destRect.Width - _frameActualWidth * 2) * GetValueAsPercent());
            ProgressFill.SetOffset(new Vector2(_frameActualWidth / GlobalScale, 0));
            ProgressFill.Size = new Vector2(markWidth, _destRectInternal.Height) / GlobalScale;
            ProgressFill.Visible = markWidth > 0;
        }
    }
}
