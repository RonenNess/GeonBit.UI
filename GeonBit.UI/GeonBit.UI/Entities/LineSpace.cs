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
    public class LineSpace : Entity
    {
        /// <summary>
        /// Create a new Line Space entity.
        /// </summary>
        /// <param name="spacesCount">How many spaces to create.</param>
        public LineSpace(int spacesCount = 1) :
            base(Vector2.One * 8 * UserInterface.SCALE * System.Math.Min(spacesCount, 1), 
                Anchor.Auto, Vector2.Zero)
        {
            // by default locked so it won't do events
            Locked = Disabled = true;
            _size.X = 0.1f;
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
        }
    }
}
