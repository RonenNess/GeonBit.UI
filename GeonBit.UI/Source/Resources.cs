#region File Description
//-----------------------------------------------------------------------------
// This file pre-load and hold all the resources (textures, fonts, etc..) that
// GeonBit.UI needs. If you edit and add new files to content, you probably
// need to update this file as well.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using GeonBit.UI.Entities;
using GeonBit.UI.DataTypes;


namespace GeonBit.UI
{

    /// <summary>
    /// A static class to init and store all UI resources (textures, effects, fonts, etc.)
    /// </summary>
    public static class Resources
    {
        /// <summary>Just a plain white texture, used internally.</summary>
        public static Texture2D WhiteTexture;

        /// <summary>Cursor textures.</summary>
        public static Texture2D[] Cursors;

        /// <summary>Metadata about cursor textures.</summary>
        public static CursorTextureData[] CursorsData;

        /// <summary>All panel skin textures.</summary>
        public static Texture2D[] PanelTextures;

        /// <summary>Metadata about panel textures.</summary>
        public static TextureData[] PanelData;

        /// <summary>Button textures (texture array is for [skin,state]).</summary>
        public static Texture2D[,] ButtonTextures;

        /// <summary>Metadata about button textures.</summary>
        public static TextureData[] ButtonData;

        /// <summary>CheckBox textures.</summary>
        public static Texture2D[] CheckBoxTextures;

        /// <summary>Radio button textures.</summary>
        public static Texture2D[] RadioTextures;

        /// <summary>ProgressBar texture.</summary>
        public static Texture2D ProgressBarTexture;

        /// <summary>Metadata about progressbar texture.</summary>
        public static TextureData ProgressBarData;

        /// <summary>ProgressBar fill texture.</summary>
        public static Texture2D ProgressBarFillTexture;

        /// <summary>HorizontalLine texture.</summary>
        public static Texture2D HorizontalLineTexture;

        /// <summary>Sliders base textures.</summary>
        public static Texture2D[] SliderTextures;

        /// <summary>Sliders mark textures (the sliding piece that shows current value).</summary>
        public static Texture2D[] SliderMarkTextures;

        /// <summary>Metadata about slider textures.</summary>
        public static TextureData[] SliderData;

        /// <summary>All icon textures.</summary>
        public static Texture2D[] IconTextures;

        /// <summary>Icons inventory background texture.</summary>
        public static Texture2D IconBackgroundTexture;

        /// <summary>Vertical scrollbar base texture.</summary>
        public static Texture2D VerticalScrollbarTexture;

        /// <summary>Vertical scrollbar mark texture.</summary>
        public static Texture2D VerticalScrollbarMarkTexture;

        /// <summary>Metadata about scrollbar texture.</summary>
        public static TextureData VerticalScrollbarData;

        /// <summary>Arrow-down texture (used in dropdown).</summary>
        public static Texture2D ArrowDown;

        /// <summary>Arrow-up texture (used in dropdown).</summary>
        public static Texture2D ArrowUp;

        /// <summary>Default font types.</summary>
        public static SpriteFont[] Fonts;

        /// <summary>Effect for disabled entities (greyscale).</summary>
        public static Effect DisabledEffect;

        /// <summary>An effect to draw just a silhouette of the texture.</summary>
        public static Effect SilhouetteEffect;

        /// <summary>
        /// Load all GeonBit.UI resources.
        /// </summary>
        /// <param name="content">Content manager to use.</param>
        /// <param name="theme">Which theme to load resources from.</param>
        static public void LoadContent(ContentManager content, string theme = "default")
        {
            // set resources root path
            string root = "GeonBit.UI/themes/" + theme + "/";

            // load cursor textures
            // note: in order not to break old themes etc if the new cursor style is not found, we load the default cursor
            // in the old themes style.
            Cursors = new Texture2D[Enum.GetValues(typeof(CursorType)).Length];
            CursorsData = new CursorTextureData[Enum.GetValues(typeof(CursorType)).Length];
            foreach (CursorType cursor in Enum.GetValues(typeof(CursorType)))
            {
                int cursorI = (int)cursor;
                try
                {
                    string cursorName = cursor.ToString().ToLower();
                    Cursors[cursorI] = content.Load<Texture2D>(root + "textures/cursor_" + cursorName);
                    CursorsData[cursorI] = content.Load<CursorTextureData>(root + "textures/cursor_" + cursorName + "_md");
                }
                catch (ContentLoadException)
                {
                    Cursors[cursorI] = content.Load<Texture2D>(root + "textures/cursor");
                    CursorsData[cursorI] = new CursorTextureData();
                }
            }

            // load white texture for rectangle
            WhiteTexture = content.Load<Texture2D>(root + "textures/white_texture");

            // panel textures
            PanelTextures = new Texture2D[Enum.GetValues(typeof(PanelSkin)).Length];
            PanelData = new TextureData[Enum.GetValues(typeof(PanelSkin)).Length];
            foreach (PanelSkin skin in Enum.GetValues(typeof(PanelSkin)))
            {
                // skip none panel skin
                if (skin == PanelSkin.None)
                {
                    continue;
                }

                // load panel texture and metadata
                string skinName = skin.ToString().ToLower();
                PanelTextures[(int)skin] = content.Load<Texture2D>(root + "textures/panel_" + skinName);
                PanelData[(int)skin] = content.Load<TextureData>(root + "textures/panel_" + skinName + "_md");
            }

            // load arrow down texture
            ArrowDown = content.Load<Texture2D>(root + "textures/arrow_down");

            // load arrow up texture, but if doesn't exist use the same as arrow down
            try
            {
                ArrowUp = content.Load<Texture2D>(root + "textures/arrow_up");
            }
            catch (ContentLoadException)
            {
                ArrowUp = ArrowDown;
            }

            // scrollbar texture
            VerticalScrollbarTexture = content.Load<Texture2D>(root + "textures/scrollbar");
            VerticalScrollbarMarkTexture = content.Load<Texture2D>(root + "textures/scrollbar_mark");

            // scrollbar metadata
            VerticalScrollbarData = content.Load<TextureData>(root + "textures/scrollbar_md");

            // slider textures
            SliderTextures = new Texture2D[Enum.GetValues(typeof(SliderSkin)).Length];
            SliderMarkTextures = new Texture2D[Enum.GetValues(typeof(SliderSkin)).Length];
            SliderData = new TextureData[Enum.GetValues(typeof(SliderSkin)).Length];
            foreach (SliderSkin skin in Enum.GetValues(typeof(SliderSkin)))
            {
                // load slider textures
                string skinName = skin.ToString().ToLower();
                SliderTextures[(int)skin] = content.Load<Texture2D>(root + "textures/slider_" + skinName);
                SliderMarkTextures[(int)skin] = content.Load<Texture2D>(root + "textures/slider_" + skinName + "_mark");

                // load slider textures metadata
                SliderData[(int)skin] = content.Load<TextureData>(root + "textures/slider_" + skinName + "_md");
            }

            // horizontal line texture
            HorizontalLineTexture = content.Load<Texture2D>(root + "textures/horizontal_line");

            // font for paragraphs and text
            Fonts = new SpriteFont[Enum.GetValues(typeof(FontStyle)).Length];
            foreach (FontStyle style in Enum.GetValues(typeof(FontStyle)))
            {
                Fonts[(int)style] = content.Load<SpriteFont>(root + "fonts/" + style.ToString());
                Fonts[(int)style].LineSpacing += 2;
            }

            // get mouse states count
            int mouseStatesOnEntityCount = Enum.GetValues(typeof(EntityState)).Length;

            // init button textures
            ButtonTextures = new Texture2D[Enum.GetValues(typeof(ButtonSkin)).Length, mouseStatesOnEntityCount];
            ButtonData = new TextureData[Enum.GetValues(typeof(ButtonSkin)).Length];
            foreach (ButtonSkin skin in Enum.GetValues(typeof(ButtonSkin)))
            {
                // load textures
                string skinName = skin.ToString().ToLower();
                ButtonTextures[(int)skin, (int)EntityState.Default] = content.Load<Texture2D>(root + "textures/button_" + skinName);
                ButtonTextures[(int)skin, (int)EntityState.MouseDown] = content.Load<Texture2D>(root + "textures/button_" + skinName + "_down");
                ButtonTextures[(int)skin, (int)EntityState.MouseHover] = content.Load<Texture2D>(root + "textures/button_" + skinName + "_hover");

                // load metadata
                ButtonData[(int)skin] = content.Load<TextureData>(root + "textures/button_" + skinName + "_md");
            }
            
            // checkbox textures
            CheckBoxTextures = new Texture2D[mouseStatesOnEntityCount];
            CheckBoxTextures[(int)EntityState.Default] = content.Load<Texture2D>(root + "textures/checkbox");
            CheckBoxTextures[(int)EntityState.MouseDown] = content.Load<Texture2D>(root + "textures/checkbox_down");
            CheckBoxTextures[(int)EntityState.MouseHover] = content.Load<Texture2D>(root + "textures/checkbox_hover");

            // radio button textures
            RadioTextures = new Texture2D[mouseStatesOnEntityCount];
            RadioTextures[(int)EntityState.Default] = content.Load<Texture2D>(root + "textures/radio");
            RadioTextures[(int)EntityState.MouseDown] = content.Load<Texture2D>(root + "textures/radio_down");
            RadioTextures[(int)EntityState.MouseHover] = content.Load<Texture2D>(root + "textures/radio_hover");

            // progress bar texture
            ProgressBarTexture = content.Load<Texture2D>(root + "textures/progressbar");
            ProgressBarFillTexture = content.Load<Texture2D>(root + "textures/progressbar_fill");
            ProgressBarData = content.Load<TextureData>(root + "textures/progressbar_md");

            // load icons
            IconTextures = new Texture2D[Enum.GetValues(typeof(IconType)).Length];
            foreach (IconType icon in Enum.GetValues(typeof(IconType)))
            {
                IconTextures[(int)(icon)] = content.Load<Texture2D>(root + "textures/icons/" + icon.ToString());
            }
            IconBackgroundTexture = content.Load<Texture2D>(root + "textures/icons/background");

            // load effects
            DisabledEffect = content.Load<Effect>(root + "effects/disabled");
            SilhouetteEffect = content.Load<Effect>(root + "effects/silhouette");

            // load default styleSheets
            LoadDefaultStyles(ref Entity.DefaultStyle, "Entity", root, content);
            LoadDefaultStyles(ref Paragraph.DefaultStyle, "Paragraph", root, content);
            LoadDefaultStyles(ref Button.DefaultStyle, "Button", root, content);
            LoadDefaultStyles(ref Button.DefaultParagraphStyle, "ButtonParagraph", root, content);
            LoadDefaultStyles(ref CheckBox.DefaultStyle, "CheckBox", root, content);
            LoadDefaultStyles(ref CheckBox.DefaultParagraphStyle, "CheckBoxParagraph", root, content);
            LoadDefaultStyles(ref ColoredRectangle.DefaultStyle, "ColoredRectangle", root, content);
            LoadDefaultStyles(ref DropDown.DefaultStyle, "DropDown", root, content);
            LoadDefaultStyles(ref DropDown.DefaultParagraphStyle, "DropDownParagraph", root, content);
            LoadDefaultStyles(ref DropDown.DefaultSelectedParagraphStyle, "DropDownSelectedParagraph", root, content);
            LoadDefaultStyles(ref Header.DefaultStyle, "Header", root, content);
            LoadDefaultStyles(ref HorizontalLine.DefaultStyle, "HorizontalLine", root, content);
            LoadDefaultStyles(ref Icon.DefaultStyle, "Icon", root, content);
            LoadDefaultStyles(ref Image.DefaultStyle, "Image", root, content);
            LoadDefaultStyles(ref Label.DefaultStyle, "Label", root, content);
            LoadDefaultStyles(ref Panel.DefaultStyle, "Panel", root, content);
            LoadDefaultStyles(ref ProgressBar.DefaultStyle, "ProgressBar", root, content);
            LoadDefaultStyles(ref ProgressBar.DefaultFillStyle, "ProgressBarFill", root, content);
            LoadDefaultStyles(ref RadioButton.DefaultStyle, "RadioButton", root, content);
            LoadDefaultStyles(ref RadioButton.DefaultParagraphStyle, "RadioButtonParagraph", root, content);
            LoadDefaultStyles(ref SelectList.DefaultStyle, "SelectList", root, content);
            LoadDefaultStyles(ref SelectList.DefaultParagraphStyle, "SelectListParagraph", root, content);
            LoadDefaultStyles(ref Slider.DefaultStyle, "Slider", root, content);
            LoadDefaultStyles(ref TextInput.DefaultStyle, "TextInput", root, content);
            LoadDefaultStyles(ref TextInput.DefaultParagraphStyle, "TextInputParagraph", root, content);
            LoadDefaultStyles(ref TextInput.DefaultPlaceholderStyle, "TextInputPlaceholder", root, content);
            LoadDefaultStyles(ref VerticalScrollbar.DefaultStyle, "VerticalScrollbar", root, content);
            LoadDefaultStyles(ref PanelTabs.DefaultButtonStyle, "PanelTabsButton", root, content);
            LoadDefaultStyles(ref PanelTabs.DefaultButtonParagraphStyle, "PanelTabsButtonParagraph", root, content);
        }

        /// <summary>
        /// Load default stylesheets for a given entity name and put values inside the sheet.
        /// </summary>
        /// <param name="sheet">StyleSheet to load.</param>
        /// <param name="entityName">Entity unique identifier for file names.</param>
        /// <param name="themeRoot">Path of the current theme root directory.</param>
        /// <param name="content">Content manager to allow us to load xmls.</param>
        private static void LoadDefaultStyles(ref StyleSheet sheet, string entityName, string themeRoot, ContentManager content)
        {
            // get stylesheet root path (eg everything before the state part)
            string stylesheetBase = themeRoot + "styles/" + entityName;

            // load default styles
            FillDefaultStyles(ref sheet, EntityState.Default, content.Load<DefaultStyles>(stylesheetBase + "-Default"));
            
            // load mouse-hover styles
            FillDefaultStyles(ref sheet, EntityState.MouseHover, content.Load<DefaultStyles>(stylesheetBase + "-MouseHover"));

            // load mouse-down styles
            FillDefaultStyles(ref sheet, EntityState.MouseDown, content.Load<DefaultStyles>(stylesheetBase + "-MouseDown"));
        }

        /// <summary>
        /// Fill a set of default styles into a given stylesheet.
        /// </summary>
        /// <param name="sheet">StyleSheet to fill.</param>
        /// <param name="state">State to fill values for.</param>
        /// <param name="styles">Default styles, as loaded from xml file.</param>
        private static void FillDefaultStyles(ref StyleSheet sheet, EntityState state, DefaultStyles styles)
        {
            if (styles.FillColor != null) { sheet[state.ToString() + "." + "FillColor"] = new StyleProperty((Color)styles.FillColor); }
            if (styles.FontStyle != null) { sheet[state.ToString() + "." + "FontStyle"] = new StyleProperty((int)styles.FontStyle); }
            if (styles.ForceAlignCenter != null) { sheet[state.ToString() + "." + "ForceAlignCenter"] = new StyleProperty((bool)styles.ForceAlignCenter); }
            if (styles.OutlineColor != null) { sheet[state.ToString() + "." + "OutlineColor"] = new StyleProperty((Color)styles.OutlineColor); }
            if (styles.OutlineWidth != null) { sheet[state.ToString() + "." + "OutlineWidth"] = new StyleProperty((int)styles.OutlineWidth); }
            if (styles.Scale != null) { sheet[state.ToString() + "." + "Scale"] = new StyleProperty((float)styles.Scale); }
            if (styles.SelectedHighlightColor != null) { sheet[state.ToString() + "." + "SelectedHighlightColor"] = new StyleProperty((Color)styles.SelectedHighlightColor); }
            if (styles.ShadowColor != null) { sheet[state.ToString() + "." + "ShadowColor"] = new StyleProperty((Color)styles.ShadowColor); }
            if (styles.ShadowOffset != null) { sheet[state.ToString() + "." + "ShadowOffset"] = new StyleProperty((Vector2)styles.ShadowOffset); }
            if (styles.Padding != null) { sheet[state.ToString() + "." + "Padding"] = new StyleProperty((Vector2)styles.Padding); }
            if (styles.SpaceBefore != null) { sheet[state.ToString() + "." + "SpaceBefore"] = new StyleProperty((Vector2)styles.SpaceBefore); }
            if (styles.SpaceAfter != null) { sheet[state.ToString() + "." + "SpaceAfter"] = new StyleProperty((Vector2)styles.SpaceAfter); }
            if (styles.ShadowScale != null) { sheet[state.ToString() + "." + "ShadowScale"] = new StyleProperty((float)styles.ShadowScale); }
        }
    }
}
