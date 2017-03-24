﻿#region File Description
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
    /// How to treat entities that overflow panel boundaries.
    /// </summary>
    public enum PanelOverflowBehavior
    {
        /// <summary>
        /// Entity will be rendered as usual outside the panel boundaries.
        /// </summary>
        Overflow,

        /// <summary>
        /// Entities that exceed panel boundaries will be clipped.
        /// Note: Requires render targets.
        /// </summary>
        Clipped,

        /// <summary>
        /// Entities that exceed panel on Y axis will create a scrollbar. Exceeding on X axis will be hidden.
        /// Note: Requires render targets.
        /// </summary>
        VerticalScroll,
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

        // how the panel draw entities that exceed boundaries.
        private PanelOverflowBehavior _overflowMode = PanelOverflowBehavior.Overflow;

        // panel scrollbar
        VerticalScrollbar _scrollbar = null;

        /// <summary>
        /// Get the scrollbar of this panel.
        /// </summary>
        public VerticalScrollbar Scrollbar
        {
            get { return _scrollbar; }
        }

        /// <summary>
        /// Set / get panel overflow behavior.
        /// Note: some modes require Render Targets, eg setting the 'UseRenderTarget' to true.
        /// </summary>
        public PanelOverflowBehavior PanelOverflowBehavior
        {
            get { return _overflowMode; }
            set { _overflowMode = value; UpdateOverflowMode(); }
        }

        /// <summary>If panel got scrollbars, use this render target to scroll.</summary>
        protected RenderTarget2D _renderTarget = null;

        /// <summary>
        /// Store the original destination rectangle if changing due to render target.
        /// </summary>
        private Rectangle _originalInternalDestRect;

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
        /// Set / get current panel skin.
        /// </summary>
        public PanelSkin Skin
        {
            get { return _skin; }
            set { _skin = value; }
        }

        /// <summary>
        /// Called before drawing child entities of this entity.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
        protected override void BeforeDrawChildren(SpriteBatch spriteBatch)
        {
            // if overflow mode is simply overflow, do nothing.
            if (_overflowMode == PanelOverflowBehavior.Overflow)
            {
                _renderTarget = null;
                return;
            }

            // create the render target for this panel
            // note: by recreating the target we make sure its cleared
            _renderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, 
                _destRectInternal.Width, _destRectInternal.Height, false, 
                spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat,
                spriteBatch.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0,
                RenderTargetUsage.PreserveContents);

            // bind the render target
            UserInterface.DrawUtils.PushRenderTarget(_renderTarget);

            // set internal dest rect
            _originalInternalDestRect = _destRectInternal;
            _destRectInternal.X = 2;
            _destRectInternal.Y = 2;
            _destRectInternal.Width -= 2;
            _destRectInternal.Height -= 2;

            // if in scrolling mode, set scrollbar
            if (_overflowMode == PanelOverflowBehavior.VerticalScroll)
            {
                // move items position based on scrollbar
                _destRectInternal.Y -= _scrollbar.Value;

                // update scrollbar position
                _scrollbar.SetOffset(new Vector2(-_scrollbar.GetActualDestRect().Width, -_destRectInternal.Y));
                _scrollbar.BringToFront();

                // adjust internal rect width
                _destRectInternal.Width -= _scrollbar.GetActualDestRect().Width;
            }
        }

        /// <summary>
        /// Called after drawing child entities of this entity.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
        protected override void AfterDrawChildren(SpriteBatch spriteBatch)
        {
            // if this panel got a render target
            if (_renderTarget != null)
            {
                // unbind the render target
                UserInterface.DrawUtils.PopRenderTarget();

                // restore internal dest rect
                _destRectInternal = _originalInternalDestRect;

                // draw the render target
                UserInterface.DrawUtils.StartDraw(spriteBatch, IsDisabled());
                spriteBatch.Draw(_renderTarget, _destRectInternal, Color.White);
                UserInterface.DrawUtils.EndDraw(spriteBatch);

                // since we changed the internal dest rect before drawing children, we need to recalc children dest rect back to normal
                foreach (Entity child in GetChildren())
                {
                    if (child != _scrollbar)
                    {
                        child.UpdateDestinationRects();
                    }
                }

                // fix scrollbar positioning etc
                _destRectInternal.Y -= _scrollbar.Value;
                _destRectInternal.Width -= _scrollbar.GetActualDestRect().Width;
                _scrollbar.UpdateDestinationRects();

                // set destination rect back to normal
                _destRectInternal = _originalInternalDestRect;
            }
        }

        /// <summary>
        /// Called after a change in overflow mode.
        /// </summary>
        private void UpdateOverflowMode()
        {
            // if its vertical scroll mode:
            if (_overflowMode == PanelOverflowBehavior.VerticalScroll)
            {
                // if need to create scrollbar
                if (_scrollbar == null)
                {
                    // create scrollbar
                    _scrollbar = new VerticalScrollbar(0, 0, Anchor.TopRight);
                    _scrollbar.Padding = Vector2.Zero;
                    AddChild(_scrollbar);
                }
            }
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
                UserInterface.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1f, FillColor, Scale);
            }

            // call base draw function
            base.DrawEntity(spriteBatch);
        }
    }
}
