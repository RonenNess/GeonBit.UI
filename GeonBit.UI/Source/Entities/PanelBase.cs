#region File Description
//-----------------------------------------------------------------------------
// Base class for panel and other panel-based entities.
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
    [System.Serializable]
    public class PanelBase : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static PanelBase()
        {
            Entity.MakeSerializable(typeof(PanelBase));
        }

        // panel style
        PanelSkin _skin;

        /// <summary>
        /// Create the panel.
        /// </summary>
        /// <param name="size">Panel size.</param>
        /// <param name="skin">Panel skin (texture to use). Use PanelSkin.None for invisible panels.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public PanelBase(Vector2 size, PanelSkin skin = PanelSkin.Default, Anchor anchor = Anchor.Center, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            _skin = skin;
            UpdateStyle(Panel.DefaultStyle);
        }

        /// <summary>
        /// Create the panel with default params.
        /// </summary>
        public PanelBase() :
            this(new Vector2(500, 500))
        {
        }

        /// <summary>
        /// Panel destructor.
        /// </summary>
        ~PanelBase()
        {
        }

        /// <summary>
        /// Set / get current panel skin.
        /// </summary>
        public PanelSkin Skin
        {
            get { return _skin; }
            set { _skin = value; }
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // draw panel itself, but only if got style
            if (_skin != PanelSkin.None)
            {
                // get texture based on skin
                Texture2D texture = Resources.PanelTextures[_skin];
                TextureData data = Resources.PanelData[(int)_skin];
                Vector2 frameSize = new Vector2(data.FrameWidth, data.FrameHeight);

                // draw panel
                UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1f, FillColor, Scale);
            }

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }
    }
}
