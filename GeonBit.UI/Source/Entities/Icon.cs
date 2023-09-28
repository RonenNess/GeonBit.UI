#region File Description
//-----------------------------------------------------------------------------
// Icons are predefined small images with few pre-defined icons. It has things
// like potions, shield, sword, etc.
//
// In addition, an icon comes with a built-in background that looks like an 
// inventory slot, that you can enable with the DrawBackground property.
//
// Note that you can easily make your own icons by overriding the 'Texture'
// property after creation. An Icon is just a subclass of the Image entity and
// extend its API.
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
    /// Pre-defined icons you can use.
    /// </summary>
    public enum IconType
    {
        /// <summary>'Sword' Icon.</summary>
        Sword = 0,
        /// <summary>'Shield' Icon.</summary>
        Shield,
        /// <summary>'Armor' Icon.</summary>
        Armor,
        /// <summary>'Ring' Icon.</summary>
        Ring,
        /// <summary>'RingRuby' Icon.</summary>
        RingRuby,
        /// <summary>'RingGold' Icon.</summary>
        RingGold,
        /// <summary>'RingGoldRuby' Icon.</summary>
        RingGoldRuby,
        /// <summary>'Heart' Icon.</summary>
        Heart,
        /// <summary>'Apple' Icon.</summary>
        Apple,
        /// <summary>'MagicWand' Icon.</summary>
        MagicWand,
        /// <summary>'Book' Icon.</summary>
        Book,
        /// <summary>'Key' Icon.</summary>
        Key,
        /// <summary>'Scroll' Icon.</summary>
        Scroll,
        /// <summary>'Skull' Icon.</summary>
        Skull,
        /// <summary>'Bone' Icon.</summary>
        Bone,
        /// <summary>'RubyPink' Icon.</summary>
        RubyPink,
        /// <summary>'RubyBlue' Icon.</summary>
        RubyBlue,
        /// <summary>'RubyGreen' Icon.</summary>
        RubyGreen,
        /// <summary>'RubyRed' Icon.</summary>
        RubyRed,
        /// <summary>'RubyPurple' Icon.</summary>
        RubyPurple,
        /// <summary>'Diamond' Icon.</summary>
        Diamond,
        /// <summary>'Helmet' Icon.</summary>
        Helmet,
        /// <summary>'Shovel' Icon.</summary>
        Shovel,
        /// <summary>'Explanation' Icon.</summary>
        Explanation,
        /// <summary>'Sack' Icon.</summary>
        Sack,
        /// <summary>'GoldCoins' Icon.</summary>
        GoldCoins,
        /// <summary>'MagicBook' Icon.</summary>
        MagicBook,
        /// <summary>'Map' Icon.</summary>
        Map,
        /// <summary>'Feather' Icon.</summary>
        Feather,
        /// <summary>'ShieldAndSword' Icon.</summary>
        ShieldAndSword,
        /// <summary>'Cubes' Icon.</summary>
        Cubes,
        /// <summary>'FloppyDisk' Icon.</summary>
        FloppyDisk,
        /// <summary>'BloodySword' Icon.</summary>
        BloodySword,
        /// <summary>'Axe' Icon.</summary>
        Axe,
        /// <summary>'PotionRed' Icon.</summary>
        PotionRed,
        /// <summary>'PotionYellow' Icon.</summary>
        PotionYellow,
        /// <summary>'PotionPurple' Icon.</summary>
        PotionPurple,
        /// <summary>'PotionBlue' Icon.</summary>
        PotionBlue,
        /// <summary>'PotionCyan' Icon.</summary>
        PotionCyan,
        /// <summary>'PotionGreen' Icon.</summary>
        PotionGreen,
        /// <summary>'Pistol' Icon.</summary>
        Pistol,
        /// <summary>'SilverShard' Icon.</summary>
        SilverShard,
        /// <summary>'GoldShard' Icon.</summary>
        GoldShard,
        /// <summary>'OrbRed' Icon.</summary>
        OrbRed,
        /// <summary>'OrbBlue' Icon.</summary>
        OrbBlue,
        /// <summary>'OrbGreen' Icon.</summary>
        OrbGreen,
        /// <summary>'ZoomIn' Icon.</summary>
        ZoomIn,
        /// <summary>'ZoomOut' Icon.</summary>
        ZoomOut,
        /// <summary>Special icon that is just an empty texture.</summary>
        None,
    }

    /// <summary>
    /// A simple UI icon.
    /// Comes we a selection of pre-defined icons to use + optional inventory-like background.
    /// </summary>
    [System.Serializable]
    public class Icon : Image
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static Icon()
        {
            Entity.MakeSerializable(typeof(Icon));
        }

        /// <summary>If true, will draw inventory-like background to this icon.</summary>
        public bool DrawBackground = false;

        /// <summary>Default styling for icons. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>
        /// Icon background size in pixels.
        /// </summary>
        public static int BackgroundSize = 10;

        /// <summary>
        /// Set / get icon.
        /// </summary>
        public IconType IconType
        {
            get { return _icon; }
            set { Texture = Resources.Instance.IconTextures[value]; _icon = value; }
        }
        IconType _icon;

        /// <summary>
        /// Create a new icon.
        /// Note: if you want to use your own texture for the icon, simply set 'icon' to be IconType.None and replace 'Texture' with
        /// your own texture after it is created.
        /// </summary>
        /// <param name="icon">A pre-defined icon to draw.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="scale">Icon default scale.</param>
        /// <param name="background">Whether or not to show icon inventory-like background.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Icon(IconType icon, Anchor anchor = Anchor.Auto, float scale = 1.0f, bool background = false, Vector2? offset = null) :
            base((Texture2D)null, USE_DEFAULT_SIZE, ImageDrawMode.Stretch, anchor, offset)
        {
            // set scale and basic properties
            Scale = scale;
            DrawBackground = background;
            IconType = icon;

            // set default background color
            SetStyleProperty("BackgroundColor", new StyleProperty(Color.White));

            // if have background, add default space-after
            if (background)
            {
                SpaceAfter = Vector2.One * BackgroundSize;
            }

            // update default style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Create default icon.
        /// </summary>
        public Icon() : this (IconType.Apple)
        {
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // draw background
            if (DrawBackground)
            {
                // get background color based on phase
                Color backColor = Color.White;
                switch (phase)
                {
                    case DrawPhase.Base:
                        backColor = GetActiveStyle("BackgroundColor").asColor;
                        break;

                    case DrawPhase.Outline:
                        backColor = OutlineColor;
                        break;

                    case DrawPhase.Shadow:
                        backColor = ShadowColor;
                        break;
                }

                // get background dest rect
                Rectangle dest = _destRect;
                dest.X -= BackgroundSize / 2; dest.Y -= BackgroundSize / 2; dest.Width += BackgroundSize; dest.Height += BackgroundSize;

                // draw background
                UserInterface.Active.DrawUtils.DrawImage(spriteBatch, Resources.Instance.IconBackgroundTexture, dest, backColor);
            }

            // now draw the image itself
            base.DrawEntity(spriteBatch, phase);
        }
    }
}
