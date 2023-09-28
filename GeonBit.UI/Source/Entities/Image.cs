#region File Description
//-----------------------------------------------------------------------------
// Draw any texture as a UI element. This widget lets you add your own images
// into the UI layout.
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
    [System.Serializable]
    public class Image : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static Image()
        {
            Entity.MakeSerializable(typeof(Image));
        }

        /// <summary>How to draw the texture.</summary>
        public ImageDrawMode DrawMode;

        /// <summary>When in Panel draw mode, this will be the frame width in texture percents.</summary>
        public Vector2 FrameWidth = Vector2.One * 0.15f;

        /// <summary>Texture to draw.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public Texture2D Texture;

        /// <summary>
        /// Get / set texture from name.
        /// </summary>
        public string TextureName
        {
            get { return Texture.Name; }
            set { Texture = Resources.Instance._content.Load<Texture2D>(value); }
        }

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
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Image(Texture2D texture, Vector2? size = null, ImageDrawMode drawMode = ImageDrawMode.Stretch, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            // store image DrawMode and texture
            DrawMode = drawMode;
            Texture = texture;

            // update style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Create image from texture path.
        /// </summary>
        /// <param name="texture">Texture path, under active theme folder.</param>
        /// <param name="size">Image size.</param>
        /// <param name="drawMode">How to draw the image (see ImageDrawMode for more info).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Image(string texture, Vector2? size = null, ImageDrawMode drawMode = ImageDrawMode.Stretch, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            this(Resources.Instance.LoadTexture(texture), size, drawMode, anchor, offset)
        {
        }

        /// <summary>
        /// Create image without texture.
        /// </summary>
        public Image() : this((Texture2D)null)
        {
        }

        /// <summary>
        /// Convert a given position to texture coords of this image.
        /// </summary>
        /// <param name="pos">Position to convert.</param>
        /// <returns>Texture coords from position.</returns>
        public Point GetTextureCoordsAt(Vector2 pos)
        {
            // draw mode must be stretch for it to work
            if (DrawMode != ImageDrawMode.Stretch)
            {
                throw new Exceptions.InvalidStateException("Cannot get texture coords on image that is not in stretched mode!");
            }

            // make sure in boundaries
            if (!IsTouching(pos))
            {
                throw new Exceptions.InvalidValueException("Position to get coords for must be inside entity boundaries!");
            }

            // get actual dest rect
            CalcDestRect();
            var rect = GetActualDestRect();

            // calc uv
            Vector2 relativePos = new Vector2(rect.Right - pos.X, rect.Bottom - pos.Y);
            Vector2 uv = new Vector2(1f - relativePos.X / rect.Width, 1f - relativePos.Y / rect.Height);

            // convert to final texture coords
            Point textCoords = new Point((int)(uv.X * Texture.Width), (int)(uv.Y * Texture.Height));
            return textCoords;
        }

        /// <summary>
        /// Get texture color at a given coordinates.
        /// </summary>
        /// <param name="textureCoords">Texture coords to get color for.</param>
        /// <returns>Color of texture at the given texture coords.</returns>
        public Color GetColorAt(Point textureCoords)
        {
            Color[] data = new Color[Texture.Width * Texture.Height];
            var index = textureCoords.X + (textureCoords.Y * Texture.Width);
            Texture.GetData<Color>(data);
            return data[index];
        }

        /// <summary>
        /// Set texture color at a given coordinates.
        /// Note: this will affect all entities using this texture.
        /// </summary>
        /// <param name="textureCoords">Texture coords to set color for.</param>
        /// <param name="color">New color to set.</param>
        public void SetTextureColorAt(Point textureCoords, Color color)
        {
            Color[] data = new Color[] { color };
            Texture.SetData(0, new Rectangle(textureCoords.X, textureCoords.Y, 1, 1), data, 0, 1);
        }

        /// <summary>
        /// Calculate width automatically based on height, to maintain texture's original ratio.
        /// For example if you have a texture of 400x200 pixels (eg 2:1 ratio) and its height in pixels is currently
        /// 100 units, calling this function will update this image width to be 100 x 2 = 200.
        /// </summary>
        public void CalcAutoWidth()
        {
            UpdateDestinationRectsIfDirty();
            var width = ((float)_destRect.Height / (float)Texture.Height) * Texture.Width;
            Size = new Vector2(width, _size.Y);
        }

        /// <summary>
        /// Calculate height automatically based on width, to maintain texture's original ratio.
        /// For example if you have a texture of 400x200 pixels (eg 2:1 ratio) and its width in pixels is currently
        /// 100 units, calling this function will update this image height to be 100 / 2 = 50.
        /// </summary>
        public void CalcAutoHeight()
        {
            UpdateDestinationRectsIfDirty();
            var height = ((float)_destRect.Width / (float)Texture.Width) * Texture.Height;
            Size = new Vector2(_size.X, height);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // draw image based on DrawMode
            switch (DrawMode)
            {
                // panel mode
                case ImageDrawMode.Panel:
                    UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, Texture, _destRect, FrameWidth, Scale, FillColor);
                    break;

                // stretch mode
                case ImageDrawMode.Stretch:
                    UserInterface.Active.DrawUtils.DrawImage(spriteBatch, Texture, _destRect, FillColor, Scale, SourceRectangle);
                    break;
            }

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }
    }
}
