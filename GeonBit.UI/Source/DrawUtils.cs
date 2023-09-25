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
using System.Collections.Generic;

namespace GeonBit.UI
{
    /// <summary>
    /// Helper class with drawing-related functionality.
    /// </summary>
    public class DrawUtils
    {
        // Stack of rendering targets
        private Stack<RenderTarget2D> _renderTargets = new Stack<RenderTarget2D>();

        // last used render target
        RenderTarget2D _lastRenderTarget = null;

        /// <summary>
        /// Add a render target to the render targets stack.
        /// </summary>
        /// <param name="target"></param>
        public void PushRenderTarget(RenderTarget2D target)
        {
            // sanity check - make sure we are in use-render-target mode
            if (!UserInterface.Active.UseRenderTarget)
            {
                throw new Exceptions.InvalidStateException("UserInterface.Active.UseRenderTarget must be 'true' to use render-target features!");
            }

            // add render target
            _renderTargets.Push(target);
        }

        /// <summary>
        /// Pop a render target from the render targets stack.
        /// </summary>
        public void PopRenderTarget()
        {
            _renderTargets.Pop();
        }

        /// <summary>
        /// Scale a rectangle by given factor
        /// </summary>
        /// <param name="rect">Rectangle to scale.</param>
        /// <param name="scale">By how much to scale the rectangle.</param>
        /// <returns>Scaled rectangle.</returns>
        public virtual Rectangle ScaleRect(Rectangle rect, float scale)
        {
            // if scale is 1 just return rect as-is
            if (scale == 1f)
            {
                return rect;
            }

            // clone the rectangle to scale it
            Rectangle ret = rect;

            // update width
            Point prevSize = ret.Size;
            ret.Width = (int)(ret.Width * scale);
            ret.Height = (int)(ret.Height * scale);

            // update position
            Point move = (ret.Size - prevSize);
            move.X /= 2; move.Y /= 2;
            ret.Location -= move;

            // return the scaled rect
            return ret;
        }

        /// <summary>
        /// Get a 2d vector and convert to a Point object, while applying Floor() to make sure its round down.
        /// </summary>
        /// <param name="vector">Vector to convert to point.</param>
        /// <returns>new rounded point instance.</returns>
        protected Point VectorToRoundPoint(Vector2 vector)
        {
            return new Point((int)System.Math.Floor(vector.X), (int)System.Math.Floor(vector.Y));
        }

        /// <summary>
        /// Set default color to white and fix RGB based on Alpha channel.
        /// </summary>
        /// <param name="color">Color to process.</param>
        /// <returns>Color if provided or default color, with alpha applied.</returns>
        public virtual Color FixColorOpacity(Color? color)
        {
            return FixColorOpacity(color ?? Color.White);
        }

        /// <summary>
        /// Fix RGB based on Alpha channel.
        /// </summary>
        /// <param name="color">Color to process.</param>
        /// <returns>Color if provided or default color, with alpha applied.</returns>
        public virtual Color FixColorOpacity(Color color)
        {
            return color * ((float)color.A / 255.0f);
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
        /// <param name="sourceRect">Optional source rectangle to use.</param>
        public virtual void DrawImage(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Color? color = null, float scale = 1f, Rectangle? sourceRect = null)
        {
            // default color
            color = FixColorOpacity(color);

            // get source rectangle
            Rectangle src = sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height);

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
        /// <param name="frameScale">Optional scale factor for the frame parts.</param>
        public virtual void DrawSurface(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Vector2 textureFrameWidth, float scale = 1f, Color? color = null, float frameScale = 1f)
        {
            // default color
            color = FixColorOpacity(color);

            // if frame width Y is 0, use DrawSurfaceHorizontal()
            if (textureFrameWidth.Y == 0)
            {
                DrawSurfaceHorizontal(spriteBatch, texture, destination, textureFrameWidth.X, color, frameScale);
                return;
            }

            // if frame width X is 0, use DrawSurfaceVertical()
            if (textureFrameWidth.X == 0)
            {
                DrawSurfaceVertical(spriteBatch, texture, destination, textureFrameWidth.Y, color, frameScale);
                return;
            }

            // apply scale on dest rect
            destination = ScaleRect(destination, scale);

            // source rect and dest rect (reused throughout the function to reduce new allocations)
            Rectangle srcRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            // factor used to scale between source in texture file and dest on the screen
            float ScaleFactor = UserInterface.Active.GlobalScale * frameScale;

            // calc the surface frame size in texture file (Src) and for drawing destination (Dest)
            Vector2 frameSizeSrcVec = new Vector2(texture.Width, texture.Height) * textureFrameWidth;
            Point frameSizeSrc = VectorToRoundPoint(frameSizeSrcVec);
            Point frameSizeDest = VectorToRoundPoint(frameSizeSrcVec * ScaleFactor);

            // calc the surface center part in texture file (Src) and for drawing destination (Dest)
            Vector2 frameCenterSrcVec = new Vector2(texture.Width, texture.Height) - frameSizeSrcVec * 2;
            Point centerSizeSrc = VectorToRoundPoint(frameCenterSrcVec);
            Point centerSizeDest = VectorToRoundPoint(frameCenterSrcVec * ScaleFactor);

            // start by rendering corners
            // top left corner
            {
                srcRect.X = 0; srcRect.Y = 0; srcRect.Width = frameSizeSrc.X; srcRect.Height = frameSizeSrc.Y;
                destRect.X = destination.X; destRect.Y = destination.Y; destRect.Width = frameSizeDest.X; destRect.Height = frameSizeDest.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // top right corner
            {
                srcRect.X = texture.Width - frameSizeSrc.X; srcRect.Y = 0;
                destRect.X = destination.Right - frameSizeDest.X; destRect.Y = destination.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // bottom left corner
            {
                srcRect.X = 0; srcRect.Y = texture.Height - frameSizeSrc.Y;
                destRect.X = destination.X; destRect.Y = destination.Bottom - frameSizeDest.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // bottom right corner
            {
                srcRect.X = texture.Width - frameSizeSrc.X;
                destRect.X = destination.Right - frameSizeDest.X;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }

            // draw top and bottom strips
            bool needTopBottomStrips = destination.Width > frameSizeDest.X * 2;
            if (needTopBottomStrips)
            {
                // current x position
                int currX = frameSizeDest.X;

                // set source rectangle (except for y, which changes internally)
                srcRect.X = frameSizeSrc.X;
                srcRect.Width = centerSizeSrc.X;
                srcRect.Height = frameSizeSrc.Y;

                // set dest rectangle width and height (x and y change internally)
                destRect.Width = centerSizeDest.X;
                destRect.Height = frameSizeDest.Y;

                // draw top and bottom strips until get to edge
                do
                {
                    // set destination x
                    destRect.X = destination.X + currX;

                    // special case - if its last call and this segment overflows right frame, cut it
                    int toCut = destRect.Right - (destination.Right - frameSizeDest.X);
                    if (toCut > 0)
                    {
                        destRect.Width -= toCut;
                        srcRect.Width -= (int)((float)toCut / ScaleFactor);
                    }

                    // draw upper part
                    srcRect.Y = 0;
                    destRect.Y = destination.Y;
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // draw lower part
                    srcRect.Y = texture.Height - frameSizeSrc.Y;
                    destRect.Y = destination.Bottom - frameSizeDest.Y;
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // advance current x position
                    currX += centerSizeDest.X;

                    // stop loop when reach the right side
                } while (currX < destination.Width - frameSizeDest.X);
            }

            // draw left and right strips
            bool needSideStrips = destination.Height > frameSizeDest.Y * 2;
            if (needSideStrips)
            {
                // current y position
                int currY = frameSizeDest.Y;

                // set source rectangle (except for x, which changes internally)
                srcRect.Y = frameSizeSrc.Y;
                srcRect.Width = frameSizeSrc.X;
                srcRect.Height = centerSizeSrc.Y;

                // set dest rectangle width and height (x and y change internally)
                destRect.Width = frameSizeDest.X;
                destRect.Height = centerSizeDest.Y;

                // draw top and bottom strips until get to edge
                do
                {
                    // set destination y
                    destRect.Y = destination.Y + currY;

                    // special case - if its last call and this segment overflows bottom, cut it
                    int toCut = destRect.Bottom - (destination.Bottom - frameSizeDest.Y);
                    if (toCut > 0)
                    {
                        destRect.Height -= toCut;
                        srcRect.Height -= (int)((float)toCut / ScaleFactor);
                    }

                    // draw left part
                    srcRect.X = 0;
                    destRect.X = destination.X;
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // draw right part
                    srcRect.X = texture.Width - frameSizeSrc.X;
                    destRect.X = destination.Right - frameSizeDest.X;
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // advance current y position
                    currY += centerSizeDest.Y;

                // stop loop when reach bottom
                } while (currY < destination.Height - frameSizeDest.Y);
            }

            // now at last draw the center parts
            if (needTopBottomStrips && needSideStrips)
            {
                // current x position
                int currX = 0;

                // set source rectangle
                srcRect.X = frameSizeSrc.X;
                srcRect.Y = frameSizeSrc.Y;
                srcRect.Width = centerSizeSrc.X;

                // set dest rectangle width (x and y change internally)
                destRect.Width = centerSizeDest.X;

                // iterate over center segments width
                do
                {
                    // set destination x
                    destRect.X = destination.X + frameSizeDest.X + currX;

                    // current y position of segment
                    int currY = 0;

                    // set source and dest rect height
                    srcRect.Height = centerSizeSrc.Y;
                    destRect.Height = centerSizeDest.Y;

                    // special case - if its last call and this segment overflows right side, cut it
                    int toCutX = destRect.Right - (destination.Right - frameSizeDest.X);
                    if (toCutX > 0)
                    {
                        destRect.Width -= toCutX;
                        srcRect.Width -= (int)((float)toCutX / ScaleFactor);
                    }

                    // iterate over center segments height
                    do
                    {
                        // set destination y
                        destRect.Y = destination.Y + frameSizeDest.Y + currY;

                        // special case - if its last call and this segment overflows bottom, cut it
                        int toCutY = destRect.Bottom - (destination.Bottom - frameSizeDest.Y);
                        if (toCutY > 0)
                        {
                            destRect.Height -= toCutY;
                            srcRect.Height -= (int)((float)toCutY / ScaleFactor);
                        }

                        // draw center segment
                        spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                        // advance current y position
                        currY += centerSizeDest.Y;

                    // stop loop when reach the bottom
                    } while (currY < destination.Height - frameSizeDest.Y * 2);

                    // advance current x position
                    currX += centerSizeDest.X;

                // stop loop when reach the right side
                } while (currX < destination.Width - frameSizeDest.X * 2);
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
        /// <param name="frameScale">Optional scale for the frame part.</param>
        public virtual void DrawSurfaceHorizontal(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, float frameWidth, Color? color = null, float frameScale = 1f)
        {
            // default color
            color = FixColorOpacity(color);

            // calc frame size in texture file (Src) and in destination render rect (Dest)
            float ScaleXfac = (float)destination.Height / (float)texture.Height;
            Vector2 frameSizeTextureVector = new Vector2(texture.Width * frameWidth, texture.Height);
            Point frameSizeSrc = VectorToRoundPoint(frameSizeTextureVector);
            Point frameSizeDest = VectorToRoundPoint(new Vector2(frameSizeTextureVector.X * ScaleXfac * frameScale, destination.Height));

            // calc the surface center in texture file (Src) and for drawing destination (Dest)
            Vector2 frameCenterSrcVec = new Vector2(texture.Width - frameSizeSrc.X * 2, texture.Height);
            Point centerSizeSrc = VectorToRoundPoint(frameCenterSrcVec);
            Point centerSizeDest = VectorToRoundPoint(new Vector2(destination.Width - frameSizeDest.X * 2, destination.Height));

            // source rect and dest rect (reused throughout the function to reduce new allocations)
            Rectangle srcRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            // draw left side
            {
                srcRect.X = 0; srcRect.Y = 0; srcRect.Width = frameSizeSrc.X; srcRect.Height = frameSizeSrc.Y;
                destRect.X = destination.X; destRect.Y = destination.Y; destRect.Width = frameSizeDest.X; destRect.Height = frameSizeDest.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // draw right side
            {
                srcRect.X = texture.Width - frameSizeSrc.X;
                destRect.X = destination.Right - frameSizeDest.X;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // draw center parts
            if (destination.Width > frameSizeDest.X * 2)
            {
                // current x position
                int currX = frameSizeDest.X;

                // set source rectangle
                srcRect.Y = 0;
                srcRect.X = frameSizeSrc.X;
                srcRect.Width = centerSizeSrc.X;
                srcRect.Height = centerSizeSrc.Y;

                // set dest rectangle (except for x which changes internally)
                destRect.Y = destination.Y;
                destRect.Width = centerSizeDest.X;
                destRect.Height = centerSizeDest.Y;

                // draw top and bottom strips until get to edge
                do
                {
                    // set destination x
                    destRect.X = destination.X + currX;

                    // special case - if its last call and this segment overflows right frame, cut it
                    int toCut = destRect.Right - (destination.Right - frameSizeDest.X);
                    if (toCut > 0)
                    {
                        destRect.Width -= toCut;
                        srcRect.Width -= (int)((float)toCut / ((float)srcRect.Width / (float)destRect.Width));
                    }

                    // draw center segment
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // advance current x position
                    currX += centerSizeDest.X;

                    // stop loop when reach the right side
                } while (currX < destination.Width - frameSizeDest.X);
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
        /// <param name="frameScale">Optional scale for the frame part.</param>
        public virtual void DrawSurfaceVertical(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, float frameWidth, Color? color = null, float frameScale = 1f)
        {
            // default color
            color = FixColorOpacity(color);

            // calc frame size in texture file (Src) and in destination render rect (Dest)
            float ScaleYfac = (float)destination.Width / (float)texture.Width;
            Vector2 frameSizeTextureVector = new Vector2(texture.Width, texture.Height * frameWidth);
            Point frameSizeSrc = VectorToRoundPoint(frameSizeTextureVector);
            Point frameSizeDest = VectorToRoundPoint(new Vector2(destination.Width, frameSizeTextureVector.Y * ScaleYfac * frameScale));

            // calc the surface center in texture file (Src) and for drawing destination (Dest)
            Vector2 frameCenterSrcVec = new Vector2(texture.Width, texture.Height - frameSizeSrc.Y * 2);
            Point centerSizeSrc = VectorToRoundPoint(frameCenterSrcVec);
            Point centerSizeDest = VectorToRoundPoint(new Vector2(destination.Width, destination.Height - frameSizeDest.Y * 2));

            // source rect and dest rect (reused throughout the function to reduce new allocations)
            Rectangle srcRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            // draw upper side
            {
                srcRect.X = 0; srcRect.Y = 0; srcRect.Width = frameSizeSrc.X; srcRect.Height = frameSizeSrc.Y;
                destRect.X = destination.X; destRect.Y = destination.Y; destRect.Width = frameSizeDest.X; destRect.Height = frameSizeDest.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // draw bottom
            {
                srcRect.Y = texture.Height - frameSizeSrc.Y;
                destRect.Y = destination.Bottom - frameSizeDest.Y;
                spriteBatch.Draw(texture, destRect, srcRect, (Color)color);
            }
            // draw center parts
            if (destination.Height > frameSizeDest.Y * 2)
            {
                // current y position
                int currY = frameSizeDest.Y;

                // set source rectangle
                srcRect.X = 0;
                srcRect.Y = frameSizeSrc.Y;
                srcRect.Width = centerSizeSrc.X;
                srcRect.Height = centerSizeSrc.Y;

                // set dest rectangle (except for x which changes internally)
                destRect.Y = destination.Y;
                destRect.Width = centerSizeDest.X;
                destRect.Height = centerSizeDest.Y;

                // draw top and bottom strips until get to edge
                do
                {
                    // set destination y
                    destRect.Y = destination.Y + currY;

                    // special case - if its last call and this segment overflows bottom, cut it
                    int toCut = destRect.Bottom - (destination.Bottom - frameSizeDest.Y);
                    if (toCut > 0)
                    {
                        destRect.Height -= toCut;
                        srcRect.Height -= (int)((float)toCut / ((float)srcRect.Height / (float)destRect.Height));
                    }

                    // draw center segment
                    spriteBatch.Draw(texture, destRect, srcRect, (Color)color);

                    // advance current y position
                    currY += centerSizeDest.Y;

                    // stop loop when reach bottom
                } while (currY < destination.Height - frameSizeDest.Y);
            }
        }

        /// <summary>
        /// Start drawing on a given SpriteBatch.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        /// <param name="isDisabled">If true, will use the greyscale 'disabled' effect.</param>
        public virtual void StartDraw(SpriteBatch spriteBatch, bool isDisabled)
        {
            // start drawing
            spriteBatch.Begin(SpriteSortMode.Deferred, UserInterface.Active.BlendState, UserInterface.Active.SamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                isDisabled ? Resources.Instance.DisabledEffect : null);

            // update drawing target
            UpdateRenderTarget(spriteBatch);
        }

        /// <summary>
        /// Start drawing on a given SpriteBatch, but only draw colored Silhouette of the texture.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public virtual void StartDrawSilhouette(SpriteBatch spriteBatch)
        {
            // start drawing silhouette
            spriteBatch.Begin(SpriteSortMode.Deferred, UserInterface.Active.BlendState, UserInterface.Active.SamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, Resources.Instance.SilhouetteEffect);

            // update drawing target
            UpdateRenderTarget(spriteBatch);
        }

        /// <summary>
        /// Update the current rendering target.
        /// </summary>
        /// <param name="spriteBatch">Current spritebatch we are using.</param>
        protected virtual void UpdateRenderTarget(SpriteBatch spriteBatch)
        {
            // get current render target
            RenderTarget2D newRenderTarget = null;
            if (_renderTargets.Count > 0)
            {
                newRenderTarget = _renderTargets.Peek();
            }
            else
            {
                newRenderTarget = UserInterface.Active.RenderTarget;
            }

            // only if changed, set render target (costly function)
            if (_lastRenderTarget != newRenderTarget)
            {
                _lastRenderTarget = newRenderTarget;
                spriteBatch.GraphicsDevice.SetRenderTarget(_lastRenderTarget);
            }
        }

        /// <summary>
        /// Finish drawing on a given SpriteBatch
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public virtual void EndDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }
    }
}
