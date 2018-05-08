using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace GeonBit.UI.DataTypes
{

    /// <summary>
    /// Represent a single style property to apply on entity and state.
    /// For example, coloring for paragraph when mouse is over.
    /// This class acts like a Union, eg we don't use all the fields.
    /// This is a waste of some memory, but we need it to be able to serialize / desrialize to XMLs.
    /// </summary>
    public struct StyleProperty
    {
        /// <summary>Color value.</summary>
        private Color? _color;

        /// <summary>Vector value.</summary>
        private Vector2? _vector;

        /// <summary>Float value.</summary>
        private float _float;

        /// <summary>bool value.</summary>
        private bool _bool;

        /// <summary>helper function to get / set color value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        public Color asColor { get { return _color != null ? (Color)_color : Color.White; } set { _color = value; } }

        /// <summary>helper function to get / set vector value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        public Vector2 asVector { get { return _vector != null ? (Vector2)_vector : Vector2.One; } set { _vector = value; } }

        /// <summary>helper function to get / set float value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        public float asFloat { get { return _float; } set { _float = value; } }

        /// <summary>helper function to get / set int value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        public int asInt { get { return (int)_float; } set { _float = value; } }

        /// <summary>helper function to get / set bool value.</summary>
        [ContentSerializerAttribute(Optional = true)]
        public bool asBool { get { return _bool; } set { _bool = value; } }

        /// <summary>
        /// Init with float value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(float value)
        {
            _float = value;
            _color = null;
            _bool = false;
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
            _bool = false;
            _float = 0;
        }

        /// <summary>
        /// Init with color value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(Color value)
        {
            _color = value;
            _vector = null;
            _bool = false;
            _float = 0;
        }

        /// <summary>
        /// Init with bool value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public StyleProperty(bool value)
        {
            _bool = value;
            _vector = null;
            _color = null;
            _float = 0;
        }
    }
}
