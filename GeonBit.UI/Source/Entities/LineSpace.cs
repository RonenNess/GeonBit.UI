#region File Description
//-----------------------------------------------------------------------------
// This entity just creates an empty space between entities which are auto
// aligned. In many ways, its very similar to the HTML <br> tag.
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
    /// A line space is just a spacer for Auto-Anchored entities, eg a method to create artificial distance between rows.
    /// </summary>
    [System.Serializable]
    public class LineSpace : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static LineSpace()
        {
            Entity.MakeSerializable(typeof(LineSpace));
        }

        /// <summary>
        /// Single line space height.
        /// </summary>
        public static float SpaceSize = 8f;

        /// <summary>
        /// Create a new Line Space entity.
        /// </summary>
        /// <param name="spacesCount">How many spaces to create.</param>
        public LineSpace(int spacesCount) :
            base(Vector2.One, Anchor.Auto, Vector2.Zero)
        {
            // by default locked so it won't do events
            Locked = true;
            Enabled = false;
            ClickThrough = true;

            // to prevent overflow bug
            Size = new Vector2(0, 0.01f);

            // default padding and spacing zero
            SpaceBefore = Padding = Vector2.Zero;
            SpaceAfter = new Vector2(0, SpaceSize * GlobalScale * spacesCount);
        }

        /// <summary>
        /// Create default line space.
        /// </summary>
        public LineSpace() : this(1)
        {
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
        }
    }
}
