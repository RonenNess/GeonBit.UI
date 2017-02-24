#region File Description
//-----------------------------------------------------------------------------
// Draw any texture as a UI element. This widget lets you add your own images
// into the UI layout.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI.DataTypes;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Image drawing modes, eg how to draw the image and fill the destination rectangle with its texture.
    /// </summary>
    public enum ImageDrawMode
    {
        /// <summary>With this mode texture will just stretch over the entire size of the destination rectangle.</summary>
        Stretch = 0,

        /// <summary>With this mode texture will be tiled and drawed with a frame, just like panels.</summary>
        Panel = 1,
    }

    /// <summary>
    /// A renderable image (draw custom texture on UI entities).
    /// </summary>
    public class Image : Entity
    {
        /// <summary>How to draw the texture.</summary>
        public ImageDrawMode DrawMode;

        /// <summary>When in Panel draw mode, this will be the frame width in texture percents.</summary>
        public Vector2 FrameWidth = Vector2.One * 0.15f;

        /// <summary>Texture to draw.</summary>
        public Texture2D Texture;

        /// <summary>Default styling for images. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>If provided, will be used as a source rectangle when drawing images in Stretch mode.</summary>
        public Rectangle? SourceRectangle = null;

        /// <summary>
        /// Create the new image entity.
        /// </summary>
        /// <param name="texture">Image texture.</param>
        /// <param name="size">Image size.</param>
        /// <param name="drawMode">How to draw the image (see ImageDrawMode for more info).</param>
        /// <param name="anchor">Poisition anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Image(Texture2D texture, Vector2 size, ImageDrawMode drawMode = ImageDrawMode.Stretch, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            // store image DrawMode and texture
            DrawMode = drawMode;
            Texture = texture;

            // update style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // draw image based on DrawMode
            switch (DrawMode)
            {
                // panel mode
                case ImageDrawMode.Panel:
                    UserInterface.DrawUtils.DrawSurface(spriteBatch, Texture, _destRect, FrameWidth, Scale, FillColor);
                    break;

                // stretch mode
                case ImageDrawMode.Stretch:
                    UserInterface.DrawUtils.DrawImage(spriteBatch, Texture, _destRect, FillColor, Scale, SourceRectangle);
                    break;
            }

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}
