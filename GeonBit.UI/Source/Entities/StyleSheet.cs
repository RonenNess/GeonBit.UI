#region File Description
//-----------------------------------------------------------------------------
// StyleSheet is basically a dictionary that contain data about styling and 
// colors for different entity state. It will contain information like
// font style when mouse hover, fill color when clicked, etc..
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using System.Text;
using System.Collections.Generic;
using GeonBit.UI.DataTypes;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Set of style properties for different entity states.
    /// For example, stylesheet can define that when mouse hover over a paragraph, its text turns red.
    /// </summary>
    [System.Serializable]
    public class StyleSheet : Utils.SerializableDictionary<string, StyleProperty>
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static StyleSheet()
        {
            Entity.MakeSerializable(typeof(StyleSheet));
        }

        // caching of states as strings, to eliminate state.ToString() calls
        private static readonly string[] StateAsString =
        {
            "Default",
            "MouseHover",
            "MouseDown",
        };

        // internal mechanism to reduce memory usage.
        private static Dictionary<KeyValuePair<string, EntityState>, string> _identifiersCache = new Dictionary<KeyValuePair<string, EntityState>, string>();

        /// <summary>
        /// Get the full string that represent a style property identifier.
        /// </summary>
        private string GetPropertyFullId(string property, EntityState state)
        {
            // get identifier from cache
            var pair = new KeyValuePair<string, EntityState>(property, state);
            if (_identifiersCache.ContainsKey(pair))
                return _identifiersCache[pair];

            // build and return new identifier
            StringBuilder fullPropertyIdentifier = new StringBuilder(StateAsString[(int)state]);
            fullPropertyIdentifier.Append('.');
            fullPropertyIdentifier.Append(property);
            return _identifiersCache[pair] = fullPropertyIdentifier.ToString();
        }

        /// <summary>
        /// Return stylesheet property for a given state.
        /// </summary>
        /// <param name="property">Property identifier.</param>
        /// <param name="state">State to get property for (if undefined will fallback to default state).</param>
        /// <param name="fallbackToDefault">If true and property not found for given state, will fallback to default state.</param>
        /// <returns>Style property value for given state or default, or null if undefined.</returns>
        public StyleProperty GetStyleProperty(string property, EntityState state = EntityState.Default, bool fallbackToDefault = true)
        {
            // try to get for current state
            StyleProperty ret;
            bool gotVal = TryGetValue(GetPropertyFullId(property, state), out ret);

            // if not found, try default
            if (!gotVal && (state != EntityState.Default) && fallbackToDefault)
            {
                return GetStyleProperty(property, EntityState.Default);
            }

            // return style value
            return ret;
        }

        /// <summary>
        /// Set a stylesheet property.
        /// </summary>
        /// <param name="property">Property identifier.</param>
        /// <param name="value">Property value.</param>
        /// <param name="state">State to set property for.</param>
        public void SetStyleProperty(string property, StyleProperty value, EntityState state = EntityState.Default)
        {
            this[GetPropertyFullId(property, state)] = value;
        }

        /// <summary>
        /// Update the entire stylesheet from a different stylesheet.
        /// </summary>
        /// <param name="other">Other StyleSheet to update from.</param>
        public void UpdateFrom(StyleSheet other)
        {
            foreach (var de in other)
            {
                this[de.Key] = de.Value;
            }
        }
    };
}
