#region File Description
//-----------------------------------------------------------------------------
// A panel is a surface you can add elements on. Its a graphical way to group
// together entities with a common logic.
// 
// Panels can have different styles (see PanelSkin for more info), or be
// invisible and just serve as an anchor and control group for its child
// entities.
//
// Usually you'd want to build your UI layout with panels. They are also very
// useful for things like message boxes etc.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI.DataTypes;

namespace GeonBit.UI.Entities
{
    /// <summary>
    /// Different panel textures you can use.
    /// </summary>
    public enum PanelSkin
    {
        /// <summary>No skin, eg panel itself is invisible.</summary>
        None = -1,

        /// <summary>Default panel texture.</summary>
        Default = 0,

        /// <summary>Alternative panel texture.</summary>
        Fancy = 1,

        /// <summary>Simple, grey panel. Useful for internal frames, eg when inside another panel.</summary>
        Simple = 2,

        /// <summary>Shiny golden panel.</summary>
        Golden = 3,

        /// <summary>Special panel skin used for lists and input background.</summary>
        ListBackground = 4,
    }

    /// <summary>
    /// A graphical panel or form you can create and add entities to.
    /// Used to group together entities with common logic.
    /// </summary>
    public class Panel : Entity
    {
        // panel style
        PanelSkin _skin;

        /// <summary>Default styling for panels. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>
        /// Create the panel.
        /// </summary>
        /// <param name="size">Panel size.</param>
        /// <param name="skin">Panel skin (texture to use). Use PanelSkin.None for invisible panels.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Panel(Vector2 size, PanelSkin skin = PanelSkin.Default, Anchor anchor = Anchor.Center, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            _skin = skin;
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // draw panel itself, but only if got style
            if (_skin != PanelSkin.None)
            {
                // get texture based on skin
                Texture2D texture = Resources.PanelTextures[(int)_skin];
                TextureData data = Resources.PanelData[(int)_skin];
                Vector2 frameSize = new Vector2(data.FrameWidth, data.FrameHeight);

                // draw panel
                DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1f, FillColor);
            }

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}
