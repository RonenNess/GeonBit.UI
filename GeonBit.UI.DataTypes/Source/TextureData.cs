namespace GeonBit.UI.DataTypes
{
    /// <summary>
    /// Meta data we attach to different textures.
    /// The values of these structs are defined in xml files that share the same name as the texture with _md sufix.
    /// It tells us things like the width of the frame (if texture is for panel), etc.
    /// </summary>
    public class TextureData
    {
        /// <summary>Frame width, in percents relative to texture size (eg if texture width is 100 and frame width is 0.1, final frame width would be 10 pixels)</summary>
        public float FrameWidth;

        /// <summary>Frame height, in percents relative to texture size (eg if texture height is 100 and frame height is 0.1, final frame height would be 10 pixels)</summary>
        public float FrameHeight;
    }
}
