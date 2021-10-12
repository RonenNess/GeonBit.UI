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
        /// <summary>
        /// Regular font.
        /// </summary>
        Regular,

        /// <summary>
        /// Bold font.
        /// </summary>
        Bold,

        /// <summary>
        /// Italic font.
        /// </summary>
        Italic
    };

    /// <summary>
    /// All the stylesheet possible settings for an entity state.
    /// </summary>
    public class DefaultStyles
    {
        /// <summary>
        /// Entity scale.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public float? Scale = null;

        /// <summary>
        /// Fill color.
        /// </summary>
        [XmlElement("Color", IsNullable = true)]
        public Color? FillColor = null;

        /// <summary>
        /// Outline color.
        /// </summary>
        [XmlElement("Color", IsNullable = true)]
        public Color? OutlineColor = null;

        /// <summary>
        /// Outline width.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public int? OutlineWidth = null;

        /// <summary>
        /// For paragraph only - align to center.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public bool? ForceAlignCenter = null;

        /// <summary>
        /// For paragraph only - font style.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public _FontStyle? FontStyle = null;

        /// <summary>
        /// For lists and containers: selected highlight background color.
        /// </summary>
        [XmlElement("Color", IsNullable = true)]
        public Color? SelectedHighlightColor = null;

        /// <summary>
        /// Shadow color (set to 00000000 for no shadow).
        /// </summary>
        [XmlElement("Color", IsNullable = true)]
        public Color? ShadowColor = null;

        /// <summary>
        /// Shadow offset.
        /// </summary>
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? ShadowOffset = null;

        /// <summary>
        /// Entity padding.
        /// </summary>
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? Padding = null;

        /// <summary>
        /// Space before the entity.
        /// </summary>
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? SpaceBefore = null;

        /// <summary>
        /// Space after the entity.
        /// </summary>
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? SpaceAfter = null;

        /// <summary>
        /// Shadow scale.
        /// </summary>
        public float? ShadowScale = null;

        /// <summary>
        /// Default entity size.
        /// </summary>
        [XmlElement("Vector", IsNullable = true)]
        public Vector2? DefaultSize = null;
    }
}
