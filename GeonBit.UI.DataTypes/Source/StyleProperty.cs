using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace GeonBit.UI.DataTypes
{

    /// <summary>
    /// Represent a single style property to apply on entity and state.
    /// For example, coloring for paragraph when mouse is over.
    /// This class acts like a Union, eg we don't use all the fields.
    /// This is a waste of some memory, but we need it to be able to serialize / desrialize to XMLs.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Color))]
    [System.Xml.Serialization.XmlInclude(typeof(Vector2))]
    public struct StyleProperty
    {
        /// <summary>Color value.</summary>
        private Color? _color;

        /// <summary>Vector value.</summary>
        private Vector2? _vector;

        /// <summary>Float value.</summary>
        private float? _float;

        /// <summary>helper function to get / set color value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        [System.Xml.Serialization.XmlIgnore]
        public Color asColor { get { return _color != null ? (Color)_color : Color.White; } set { _color = value; } }

        /// <summary>helper function to get / set vector value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        [System.Xml.Serialization.XmlIgnore]
        public Vector2 asVector { get { return _vector != null ? (Vector2)_vector : Vector2.One; } set { _vector = value; } }

        /// <summary>helper function to get / set float value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        [System.Xml.Serialization.XmlIgnore]
        public float asFloat { get { return _float ?? 0f; } set { _float = value; } }

        /// <summary>helper function to get / set int value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        [System.Xml.Serialization.XmlIgnore]
        public int asInt { get { return (int)(_float ?? 0); } set { _float = value; } }

        /// <summary>helper function to get / set bool value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        [System.Xml.Serialization.XmlIgnore]
        public bool asBool { get { return (_float ?? 0) > 0f; } set { _float = value ? 1f : 0f; } }

        /// <summary>
        /// Get/set currently-set value, for serialization.
        /// </summary>
        [ContentSerializerAttribute(Optional = true)]
        public object _Value
        {
            get
            {
                if (_color != null)
                    return _color.Value;

                else if (_vector != null)
                    return _vector.Value;

                else
                    return _float ?? 0;
            }
            set
            {
                if (value is Color)
                    _color = (Color)value;

                else if (value is Vector2)
                    _vector = (Vector2)value;

                else
                    _float = (float)value;
            }
        }

        /// <summary>
        /// Init with float value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(float value)
        {
            _float = value;
            _color = null;
            _vector = null;
        }

        /// <summary>
        /// Init with int value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(int value)
        {
            _float = value;
            _color = null;
            _vector = null;
        }

        /// <summary>
        /// Init with vector value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(Vector2 value)
        {
            _vector = value;
            _color = null;
            _float = null;
        }

        /// <summary>
        /// Init with color value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(Color value)
        {
            _color = value;
            _vector = null;
            _float = null;
        }

        /// <summary>
        /// Init with bool value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(bool value)
        {
            _vector = null;
            _color = null;
            _float = value ? 1f : 0f;
        }
    }
}
