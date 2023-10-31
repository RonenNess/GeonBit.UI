using Microsoft.Xna.Framework;
using System.Xml.Serialization;


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
        public float? Scale { get; set; } = null;

        /// <summary>
        /// Fill color.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Color? FillColor { get; set; } = null;

        /// <summary>
        /// Outline color.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Color? OutlineColor { get; set; } = null;

        /// <summary>
        /// Outline width.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public int? OutlineWidth { get; set; } = null;

        /// <summary>
        /// For paragraph only - align to center.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public bool? ForceAlignCenter { get; set; } = null;

        /// <summary>
        /// For paragraph only - font style.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public _FontStyle? FontStyle = null;

        /// <summary>
        /// For lists and containers: selected highlight background color.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Color? SelectedHighlightColor = null;

        /// <summary>
        /// Shadow color (set to 00000000 for no shadow).
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Color? ShadowColor = null;

        /// <summary>
        /// Shadow offset.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Vector2? ShadowOffset = null;

        /// <summary>
        /// Entity padding.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Vector2? Padding = null;

        /// <summary>
        /// Space before the entity.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Vector2? SpaceBefore = null;

        /// <summary>
        /// Space after the entity.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Vector2? SpaceAfter = null;

        /// <summary>
        /// Shadow scale.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public float? ShadowScale = null;

        /// <summary>
        /// Default entity size.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public Vector2? DefaultSize = null;
    }
}
