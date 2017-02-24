using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;

namespace GeonBit.UI.DataTypes
{
    /// <summary>
    /// Font styles (should match the font styles defined in GeonBit.UI engine).
    /// </summary>
    public enum _FontStyle
    {
        Regular,
        Bold,
        Italic
    };

    /// <summary>
    /// All the stylesheet possible settings for an entity state.
    /// </summary>
    public class DefaultStyles
    {
        // entity scale
        [XmlElement(IsNullable = true)]
        public float? Scale = null;

        // fill color
        [XmlElement("Color", IsNullable = true)]
        public Color? FillColor = null;

        // outline color
        [XmlElement("Color", IsNullable = true)]
        public Color? OutlineColor = null;

        // outline width
        [XmlElement(IsNullable = true)]
        public int? OutlineWidth = null;

        // for paragraph only - align to center
        [XmlElement(IsNullable = true)]
        public bool? ForceAlignCenter = null;

        // for paragraph only - font style
        [XmlElement(IsNullable = true)]
        public _FontStyle? FontStyle = null;

        // for lists etc - selected highlight background color
        [XmlElement("Color", IsNullable = true)]
        public Color? SelectedHighlightColor = null;

        // shadow color (set to 00000000 for no shadow)
        [XmlElement("Color", IsNullable = true)]
        public Color? ShadowColor = null;

        // shadow offset
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? ShadowOffset = null;

        // padding
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? Padding = null;

        // space before
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? SpaceBefore = null;

        // space after
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? SpaceAfter = null;

        // shadow scale
        public float? ShadowScale = null;
    }
}
