#region File Description
//-----------------------------------------------------------------------------
// Contains graphic-related and drawing utilities.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeonBit.UI
{
    /// <summary>
    /// Helper class with drawing-related functionality.
    /// </summary>
    public class DrawUtils
    {
        /// <summary>
        /// Scale a rectangle by given factor
        /// </summary>
        /// <param name="rect">Rectangle to scale.</param>
        /// <param name="scale">By how much to scale the rectangle.</param>
        /// <returns>Scaled rectangle.</returns>
        public static Rectangle ScaleRect(Rectangle rect, float scale)
        {
            Rectangle ret = rect;
            if (scale != 1f)
            {
                Point prevSize = ret.Size;
                ret.Width = (int)(ret.Width * scale);
                ret.Height = (int)(ret.Height * scale);
                Point move = (ret.Size - prevSize);
                move.X /= 2; move.Y /= 2;
                ret.Location -= move;
            }
            return ret;
        }

        /// <summary>
        /// Draw a simple image with texture and destination rectangle.
        /// This function will stretch the texture to fit the destination rect.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="destination">Destination rectangle.</param>
        /// <param name="color">Optional color tint.</param>
        /// <param name="scale">Optional scale factor.</param>
        public static void DrawImage(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Color? color = null, float scale = 1f)
        {
            // default color
            color = color ?? Color.White;

            // get source rectangle
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);

            // scale
            destination = ScaleRect(destination, scale);

            // draw image
            spriteBatch.Draw(texture, destination, src, (Color)color);
        }

        /// <summary>
        /// Draw a tiled texture with frame on destination rectangle.
        /// This function will draw repeating frame parts + tile center parts to cover destination rect.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="destination">Destination rectangle.</param>
        /// <param name="textureFrameWidth">Frame width in percents relative to texture file size. For example, 0.1, 0.1 means the frame takes 10% of the texture file.</param>
        /// <param name="scale">Optional scale factor.</param>
        /// <param name="color">Optional color tint.</param>
        public static void DrawSurface(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Vector2 textureFrameWidth, float scale = 1f, Color? color = null)
        {
            // default color
            color = color ?? Color.White;
            
            // if frame width Y is 0, use DrawSurfaceHorizontal()
            if (textureFrameWidth.Y == 0)
            {
                DrawSurfaceHorizontal(spriteBatch, texture, destination, textureFrameWidth.X, color);
                return;
            }

            // if frame width X is 0, use DrawSurfaceVertical()
            if (textureFrameWidth.X == 0)
            {
                DrawSurfaceVertical(spriteBatch, texture, destination, textureFrameWidth.Y, color);
                return;
            }

            // apply scale
            destination = ScaleRect(destination, scale);

            // calc some helpers
            float SizeFactor = UserInterface.SCALE * 3.0f;
            Vector2 frameSizeTexture = new Vector2(texture.Width, texture.Height) * textureFrameWidth;
            Vector2 frameSizeRender = frameSizeTexture * SizeFactor;
            frameSizeRender.X = (int)System.Math.Ceiling((double)frameSizeRender.X);
            frameSizeRender.Y = (int)System.Math.Ceiling((double)frameSizeRender.Y);

            // start by rendering corners
            // top left corner
            {
                Rectangle src = new Rectangle(0, 0, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.X, destination.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }
            // bottom left corner
            {
                Rectangle src = new Rectangle(0, texture.Height - (int)frameSizeTexture.Y, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.X, destination.Bottom - (int)frameSizeRender.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }
            // top right corner
            {
                Rectangle src = new Rectangle(texture.Width - (int)frameSizeTexture.X, 0, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.Right - (int)frameSizeRender.X, destination.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }
            // bottom right corner
            {
                Rectangle src = new Rectangle(texture.Width - (int)frameSizeTexture.X, texture.Height - (int)frameSizeTexture.Y, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.Right - (int)frameSizeRender.X, destination.Bottom - (int)frameSizeRender.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }

            // sides sizes
            int sideTotalHeight = (int)(destination.Height - frameSizeRender.Y * 2);
            int sideTextureHeight = System.Math.Max(texture.Height - (int)frameSizeTexture.Y * 2, 1);
            int sideUnitHeight = (int)((float)sideTextureHeight * SizeFactor);

            int sideTotalWidth = (int)(destination.Width - frameSizeRender.X * 2);
            int sideTextureWidth = System.Math.Max(texture.Width - (int)frameSizeTexture.X * 2, 1);
            int sideUnitWidth = (int)((float)sideTextureWidth * SizeFactor);


            // render sides
            if (sideTextureHeight > 0 && sideUnitHeight > 0)
            {
                // calc source rect for left and right sides
                Rectangle leftSrcOrigin = new Rectangle(0, (int)frameSizeTexture.Y, (int)frameSizeTexture.X, sideTextureHeight);
                Rectangle rightSrcOrigin = new Rectangle(texture.Width - (int)frameSizeTexture.X, (int)frameSizeTexture.Y, (int)frameSizeTexture.X, sideTextureHeight);

                // iterate over frame height
                for (int i = 0; i < System.Math.Ceiling((double)sideTotalHeight / sideUnitHeight); ++i)
                {
                    // get source rectangles
                    Rectangle rightSrc = rightSrcOrigin;
                    Rectangle leftSrc = leftSrcOrigin;

                    // render frame right side
                    Rectangle dest = new Rectangle(destination.Right - (int)frameSizeRender.X, destination.Y + (int)frameSizeRender.Y + i * sideUnitHeight, (int)frameSizeRender.X, sideUnitHeight);
                    int toCut = dest.Bottom - (int)(destination.Bottom - frameSizeRender.Y);
                    if (toCut > 0)
                    {
                        int decSrc = (int)(toCut * ((float)rightSrc.Height / (float)dest.Height));
                        rightSrc.Height -= decSrc;
                        leftSrc.Height -= decSrc;
                        dest.Height -= toCut;
                    }
                    spriteBatch.Draw(texture, dest, rightSrc, (Color)color);

                    // render frame left side
                    dest.X = destination.X;
                    spriteBatch.Draw(texture, dest, leftSrc, (Color)color);
                }
            }
            // top / bottom sides
            if (sideTextureWidth > 0 && sideUnitWidth > 0)
            {
                // calc source rect for left and right sides
                Rectangle topSrcOrigin = new Rectangle((int)frameSizeTexture.X, 0, sideTextureWidth, (int)frameSizeTexture.Y);
                Rectangle bottomSrcOrigin = new Rectangle((int)frameSizeTexture.X, texture.Height - (int)frameSizeTexture.Y, sideTextureWidth, (int)frameSizeTexture.Y);

                // iterate over frame height
                for (int i = 0; i < System.Math.Ceiling((double)sideTotalWidth / sideUnitWidth); ++i)
                {
                    // get source rectangles
                    Rectangle bottomSrc = bottomSrcOrigin;
                    Rectangle topSrc = topSrcOrigin;

                    // render frame bottom side
                    Rectangle dest = new Rectangle(destination.X + (int)frameSizeRender.X + i * sideUnitWidth, destination.Bottom - (int)frameSizeRender.Y, sideUnitWidth, (int)frameSizeRender.Y);
                    int toCut = dest.Right - (int)(destination.Right - frameSizeRender.X);
                    if (toCut > 0)
                    {
                        int decSrc = (int)(toCut * ((float)bottomSrc.Width / (float)dest.Width));
                        bottomSrc.Width -= decSrc;
                        topSrc.Width -= decSrc;
                        dest.Width -= toCut;
                    }
                    spriteBatch.Draw(texture, dest, bottomSrc, (Color)color);

                    // render frame left side
                    dest.Y = destination.Y;
                    spriteBatch.Draw(texture, dest, topSrc, (Color)color);
                }
            }

            // calc center parts
            Vector2 centerSizeTexture = new Vector2(texture.Width - frameSizeTexture.X * 2,
                                                    texture.Height - frameSizeTexture.Y * 2);
            if (centerSizeTexture.X <= 0) centerSizeTexture.X = 2;
            if (centerSizeTexture.Y <= 0) centerSizeTexture.Y = 2;
            Vector2 centerSizeRender = centerSizeTexture * SizeFactor;
            Vector2 centerSizeTotal = new Vector2(destination.Width - frameSizeRender.X * 2,
                                                  destination.Height - frameSizeRender.Y * 2);

            // render center
            {
                int limitI = (int)System.Math.Max(1, System.Math.Ceiling((double)centerSizeTotal.X / centerSizeRender.X));
                int limitJ = (int)System.Math.Max(1, System.Math.Ceiling((double)centerSizeTotal.Y / centerSizeRender.Y));
                for (int i = 0; i < limitI; ++i)
                {
                    for (int j = 0; j < limitJ; ++j)
                    {
                        // calc dest rect
                        Rectangle dest = new Rectangle((int)System.Math.Floor(destination.X + (int)frameSizeRender.X + i * centerSizeRender.X),
                                                       (int)System.Math.Floor(destination.Y + (int)frameSizeRender.Y + j * centerSizeRender.Y),
                                                       (int)centerSizeRender.X + 2, (int)centerSizeRender.Y + 2);

                        // calc source rect
                        Rectangle src = new Rectangle((int)frameSizeTexture.X, (int)frameSizeTexture.Y,
                                                      (int)centerSizeTexture.X, (int)centerSizeTexture.Y);

                        // make sure size doesn't overflow
                        int toCut = dest.Right - (int)(destination.Right - frameSizeRender.X);
                        if (toCut > 0)
                        {
                            src.Width -= (int)(toCut * ((float)src.Width / (float)dest.Width));
                            dest.Width -= toCut;
                        }
                        toCut = dest.Bottom - (int)(destination.Bottom - frameSizeRender.Y);
                        if (toCut > 0)
                        {
                            src.Height -= (int)(toCut * ((float)src.Height / (float)dest.Height));
                            dest.Height -= toCut;
                        }


                        // draw center part
                        spriteBatch.Draw(texture, dest, src, (Color)color);
                    }
                }
            }
        }

        /// <summary>
        /// Just like DrawSurface, but will stretch texture on Y axis.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="destination">Destination rectangle.</param>
        /// <param name="frameWidth">Frame width in percents relative to texture file size. For example, 0.1 means the frame takes 10% of the texture file.</param>
        /// <param name="color">Optional tint color to draw texture with.</param>
        public static void DrawSurfaceHorizontal(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, float frameWidth, Color? color = null)
        {
            // default color
            color = color ?? Color.White;

            // calc some helpers
            Vector2 frameSizeTexture = new Vector2(texture.Width * frameWidth, texture.Height);
            Vector2 frameSizeRender = frameSizeTexture;
            float ScaleXfac = destination.Height / frameSizeRender.Y;
            frameSizeRender.X = (int)System.Math.Ceiling((double)frameSizeRender.X * ScaleXfac);
            frameSizeRender.Y = destination.Height;

            // start by rendering sides
            // left side
            {
                Rectangle src = new Rectangle(0, 0, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.X, destination.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }
            // right side
            {
                Rectangle src = new Rectangle(texture.Width - (int)frameSizeTexture.X, 0, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.Right - (int)frameSizeRender.X, destination.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }

            // calc center parts
            Vector2 centerSizeTexture = new Vector2(texture.Width - frameSizeTexture.X * 2, texture.Height);
            if (centerSizeTexture.X <= 0) centerSizeTexture.X = 2;
            if (centerSizeTexture.Y <= 0) centerSizeTexture.Y = 2;
            Vector2 centerSizeRender = centerSizeTexture * ScaleXfac;
            centerSizeRender.Y = destination.Height - 2;
            Vector2 centerSizeTotal = new Vector2(destination.Width - frameSizeRender.X * 2,
                                                  destination.Height - frameSizeRender.Y * 2);

            // render center
            {
                int limitI = (int)System.Math.Max(1, System.Math.Ceiling((double)centerSizeTotal.X / centerSizeRender.X));
                for (int i = 0; i < limitI; ++i)
                {
                    // calc dest rect
                    Rectangle dest = new Rectangle((int)System.Math.Floor(destination.X + (int)frameSizeRender.X + i * centerSizeRender.X),
                                                    (int)destination.Y,
                                                    (int)centerSizeRender.X + 2, 
                                                    (int)centerSizeRender.Y + 2);

                    // calc source rect
                    Rectangle src = new Rectangle((int)frameSizeTexture.X, 0, (int)centerSizeTexture.X, texture.Height);

                    // make sure size doesn't overflow
                    int toCut = dest.Right - (int)(destination.Right - frameSizeRender.X);
                    if (toCut > 0)
                    {
                        src.Width -= (int)(toCut * ((float)src.Width / (float)dest.Width));
                        dest.Width -= toCut;
                    }

                    // draw center part
                    spriteBatch.Draw(texture, dest, src, (Color)color);

                }
            }
        }

        /// <summary>
        /// Just like DrawSurface, but will stretch texture on X axis.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="destination">Destination rectangle.</param>
        /// <param name="frameWidth">Frame width in percents relative to texture file size. For example, 0.1 means the frame takes 10% of the texture file.</param>
        /// <param name="color">Optional tint color to draw texture with.</param>
        public static void DrawSurfaceVertical(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, float frameWidth, Color? color = null)
        {
            // default color
            color = color ?? Color.White;

            // calc some helpers
            Vector2 frameSizeTexture = new Vector2(texture.Width, texture.Height * frameWidth);
            Vector2 frameSizeRender = frameSizeTexture;
            float ScaleYfac = destination.Width / frameSizeRender.X;
            frameSizeRender.Y = (int)System.Math.Ceiling((double)frameSizeRender.Y * ScaleYfac);
            frameSizeRender.X = destination.Width;

            // start by rendering sides
            // top side
            {
                Rectangle src = new Rectangle(0, 0, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.X, destination.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }
            // bottom side
            {
                Rectangle src = new Rectangle(0, texture.Height - (int)frameSizeTexture.Y, (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                Rectangle dest = new Rectangle(destination.X, destination.Bottom - (int)frameSizeRender.Y, (int)frameSizeRender.X, (int)frameSizeRender.Y);
                spriteBatch.Draw(texture, dest, src, (Color)color);
            }

            // calc center parts
            Vector2 centerSizeTexture = new Vector2(texture.Height, texture.Height - frameSizeTexture.Y * 2);
            if (centerSizeTexture.X <= 0) centerSizeTexture.X = 2;
            if (centerSizeTexture.Y <= 0) centerSizeTexture.Y = 2;
            Vector2 centerSizeRender = centerSizeTexture * ScaleYfac;
            centerSizeRender.X = destination.Width - 2;
            Vector2 centerSizeTotal = new Vector2(destination.Width - frameSizeRender.X * 2,
                                                  destination.Height - frameSizeRender.Y * 2);

            // render center
            {
                int limitI = (int)System.Math.Max(1, System.Math.Ceiling((double)centerSizeTotal.Y / centerSizeRender.Y));
                for (int i = 0; i < limitI; ++i)
                {
                    // calc dest rect
                    Rectangle dest = new Rectangle((int)destination.X, 
                                                    (int)System.Math.Floor(destination.Y + (int)frameSizeRender.Y + i * centerSizeRender.Y),
                                                    (int)centerSizeRender.X + 2,
                                                    (int)centerSizeRender.Y + 2);

                    // calc source rect
                    Rectangle src = new Rectangle(0, (int)frameSizeTexture.Y, texture.Width, (int)centerSizeTexture.Y);

                    // make sure size doesn't overflow
                    int toCut = dest.Bottom - (int)(destination.Bottom - frameSizeRender.Y);
                    if (toCut > 0)
                    {
                        src.Height -= (int)(toCut * ((float)src.Height / (float)dest.Height));
                        dest.Height -= toCut;
                    }

                    // draw center part
                    spriteBatch.Draw(texture, dest, src, (Color)color);

                }
            }
        }

        /// <summary>
        /// Start drawing on a given SpriteBatch.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="isDisabled">If true, will use the greyscale 'disabled' effect.</param>
        public static void StartDraw(SpriteBatch spriteBatch, bool isDisabled)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                isDisabled ? Resources.DisabledEffect : null);
        }


        /// <summary>
        /// Start drawing on a given SpriteBatch, but only draw colored Silhouette of the texture.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public static void StartDrawSilhouette(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, Resources.SilhouetteEffect);
        }

        /// <summary>
        /// Finish drawing on a given SpriteBatch
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public static void EndDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }
    }
}
