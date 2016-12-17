#region File Description
//-----------------------------------------------------------------------------
// This root panel is a special type of Panel that don't expect to have a
// parent and is always fullscreen and without padding.
// It is used internally as a root element for the UI tree.
// Normally you wouldn't create any RootPanels, but it won't do any harm either.
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
    /// A special panel used as the root panel that covers the entire screen.
    /// This panel is used internally to serve as the constant root entity in the entities tree.
    /// </summary>
    public class RootPanel : Panel
    {
        /// <summary>
        /// Create the root panel.
        /// </summary>
        public RootPanel() :
            base(Vector2.Zero, PanelSkin.None, Anchor.Center, Vector2.Zero)
        {
            Padding = Vector2.Zero;
            ShadowColor = Color.Transparent;
        }

        /// <summary>
        /// Override the function to calculate the destination rectangle, so the root panel will always cover the entire screen.
        /// </summary>
        /// <returns>Rectangle in the size of the whole screen.</returns>
        override public Rectangle CalcDestRect()
        {
            int width = UserInterface.ScreenWidth;
            int height = UserInterface.ScreenHeight;
            return new Rectangle(0, 0, width, height);
        }
    }
}
