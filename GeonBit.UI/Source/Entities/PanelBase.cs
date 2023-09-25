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
        /// <summary>No skin, the panel itself is invisible.</summary>
        None = -1,

        /// <summary>Default panel texture.</summary>
        Default = 0,

        /// <summary>Alternative more decorated panel texture.</summary>
        Fancy = 1,

        /// <summary>Simple panel skin, with less details. Useful for internal frames, eg when inside another panel.</summary>
        Simple = 2,

        /// <summary>Alternative panel skin.</summary>
        Alternative = 3,

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
        /// Optional alternative texture to use with this panel.
        /// </summary>
        protected Texture2D _customTexture;

        /// <summary>
        /// Custom frame size to use with optional custom texture.
        /// </summary>
        protected Vector2? _customFrame = null;

        /// <summary>
        /// Min size for panels when trying to auto-adjust height for child entities.
        /// </summary>
        public static float MinAutoAdjustHeight = 50f;

        /// <summary>
        /// If true, will set panel height automatically based on children.
        /// Note: this will change the Size.Y property every time children under this panel change.
        /// </summary>
        public bool AdjustHeightAutomatically = false;

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
        /// Draw this panel.
        /// </summary>
        /// <param name="spriteBatch">Spritebatch to use when drawing this panel.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // adjust height automatically
            if (AdjustHeightAutomatically && Visible)
            {
                if (!SetHeightBasedOnChildren())
                {
                    return;
                }
            }

            // call base drawing function
            base.Draw(spriteBatch);
        }

        /// <summary>
        /// Set the panel's height to match its children automatically.
        /// Note: to make this happen on its own every frame, set the 'AdjustHeightAutomatically' property to true.
        /// </summary>
        /// <returns>True if succeed to adjust height, false if couldn't for whatever reason.</returns>
        public virtual bool SetHeightBasedOnChildren()
        {
            // get the absolute top of this panel, but if size is 0 skip
            UpdateDestinationRectsIfDirty();
            var selfDestRect = GetActualDestRect();
            var selfTop = selfDestRect.Y - Padding.Y;

            // calculate the max height this panel should have base on children
            var maxHeight = MinAutoAdjustHeight;
            bool didAdjustHeight = false;
            foreach (var child in _children)
            {
                if (child.Size.Y != 0 &&
                    !child.Draggable &&
                    child.Visible &&
                    (child.Anchor == Anchor.TopCenter || child.Anchor == Anchor.TopLeft || child.Anchor == Anchor.TopRight ||
                    child.Anchor == Anchor.Auto || child.Anchor == Anchor.AutoCenter || child.Anchor == Anchor.AutoInline || child.Anchor == Anchor.AutoInlineNoBreak))
                {
                    // update child destination rects
                    child.UpdateDestinationRectsIfDirty();

                    // if child height is 0 skip it
                    if (child.GetActualDestRect().Height == 0) { continue; }

                    // get child height and check if should change this panel's height
                    var childDestRect = child.GetDestRectForAutoAnchors();
                    var currHeight = (childDestRect.Bottom + child.SpaceAfter.Y - selfTop);
                    didAdjustHeight = true;
                    if (currHeight > maxHeight)
                    {
                        maxHeight = currHeight;
                    }
                }
            }

            // check if need to update size
            if ((Size.Y != maxHeight))
            {
                Size = new Vector2(Size.X, maxHeight / UserInterface.Active.GlobalScale);
                UpdateDestinationRects();
                foreach (var child in _children)
                {
                    child.UpdateDestinationRects();
                }
            }

            // return if could adjust height
            return didAdjustHeight;
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
        /// Override the default theme textures and set a custom skin for this specific button.
        /// </summary>
        /// <remarks>You must provide all state textures when overriding button skin.</remarks>
        /// <param name="customTexture">Texture to use for default state.</param>
        /// <param name="frameWidth">The width of the custom texture's frame, in percents of texture size.</param>
        public void SetCustomSkin(Texture2D customTexture, Vector2? frameWidth = null)
        {
            _customTexture = customTexture;
            _customFrame = frameWidth ?? null;
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
                Texture2D texture = _customTexture ?? Resources.Instance.PanelTextures[_skin];
                TextureData data = Resources.Instance.PanelData[(int)_skin];
                Vector2 frameSize = _customFrame ?? new Vector2(data.FrameWidth, data.FrameHeight);

                // draw panel
                UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1f, FillColor, Scale);
            }

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }
    }
}
