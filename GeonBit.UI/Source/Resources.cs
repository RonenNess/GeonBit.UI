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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

namespace GeonBit.UI
{
    /// <summary>
    /// A class to get texture with index and constant path part.
    /// Used internally.
    /// </summary>
    public class TexturesGetter<TEnum> where TEnum : Enum, IConvertible
    {
        // textures we already loaded
        Texture2D[] _loadedTextures;

        /// <summary>
        /// Get texture for enum state.
        /// This is for textures that don't have different states, like mouse hover, down, or default.
        /// </summary>
        /// <param name="i">Texture enum identifier.</param>
        /// <returns>Loaded texture.</returns>
        public Texture2D this[TEnum i]
        {
            // get texture for a given type
            get
            {
                int indx = GetIndex(i);
                if (_loadedTextures[indx] == null)
                {
                    var path = $"{Resources.Instance._root}{_basepath}{EnumToString(i)}{_suffix}";
                    try
                    {
                        _loadedTextures[indx] = Resources.Instance._content.Load<Texture2D>(path);
                    }
                    catch (ContentLoadException)
                    {
                        // for backward compatibility when alternative was called 'golden'
                        if (i.ToString() == PanelSkin.Alternative.ToString())
                        {
                            path = $"{Resources.Instance._root}{_basepath}golden{_suffix}";
                            _loadedTextures[indx] = Resources.Instance._content.Load<Texture2D>(path);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return _loadedTextures[indx];
            }

            // force-override texture for a given type
            set
            {
                int indx = GetIndex(i);
                _loadedTextures[indx] = value;
            }
        }

        /// <summary>
        /// Get texture for enum state and entity state.
        /// This is for textures that don't have different states, like mouse hover, down, or default.
        /// </summary>
        /// <param name="i">Texture enum identifier.</param>
        /// <param name="s">Entity state to get texture for.</param>
        /// <returns>Loaded texture.</returns>
        public Texture2D this[TEnum i, EntityState s]
        {
            // get texture for a given type and state
            get
            {
                int indx = GetIndex(i, s);
                if (_loadedTextures[indx] == null)
                {
                    var path = Resources.Instance._root + _basepath + EnumToString(i) + _suffix + StateEnumToString(s);
                    _loadedTextures[indx] = Resources.Instance._content.Load<Texture2D>(path);
                }
                return _loadedTextures[indx];
            }

            // force-override texture for a given type and state
            set
            {
                int indx = GetIndex(i, s);
                _loadedTextures[indx] = value;
            }
        }

        /// <summary>
        /// Get index from enum type with optional state.
        /// </summary>
        private int GetIndex(TEnum i, EntityState? s = null)
        {
            if (s != null)
                return Convert.ToInt32(i) + (_typesCount * (int)s);
            return Convert.ToInt32(i);
        }

        /// <summary>
        /// Convert enum to its string for filename.
        /// </summary>
        private string EnumToString(TEnum e)
        {
            // entity state enum
            if (typeof(TEnum) == typeof(EntityState))
            {
                return StateEnumToString((EntityState)(object)e);
            }

            // icon type enum
            if (typeof(TEnum) == typeof(IconType))
            {
                return e.ToString();
            }

            // all other type of enums
            return e.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Convert entity state enum to string.
        /// </summary>
        private string StateEnumToString(EntityState e)
        {
            switch (e)
            {
                case EntityState.MouseDown:
                    return "_down";
                case EntityState.MouseHover:
                    return "_hover";
                case EntityState.Default:
                    return string.Empty;
            }
            return null;
        }

        // base path of textures to load (index will be appended to them).
        string _basepath;

        // suffix to add to the end of texture path
        string _suffix;

        // textures types count
        int _typesCount;

        /// <summary>
        /// Create the texture getter with base path.
        /// </summary>
        /// <param name="path">Resource path, under geonbit.ui content.</param>
        /// <param name="suffix">Suffix to add to the texture path after the enum part.</param>
        /// <param name="usesStates">If true, it means these textures may also use entity states, eg mouse hover / down / default.</param>
        public TexturesGetter(string path, string suffix = null, bool usesStates = true)
        {
            _basepath = path;
            _suffix = suffix ?? string.Empty;
            _typesCount = Enum.GetValues(typeof(TEnum)).Length;
            _loadedTextures = new Texture2D[usesStates ? _typesCount * 3 : _typesCount];
        }
    }

    /// <summary>
    /// A class to init and store all UI resources (textures, effects, fonts, etc.)
    /// </summary>
    public class Resources
    {
        /// <summary>
        /// Resources singleton instance.
        /// </summary>
        public static Resources Instance { get; private set; }

        /// <summary>
        /// Reset resources manager.
        /// </summary>
        public static void Reset()
        {
            Instance = new Resources();
        }

        /// <summary>Lookup for char > string conversion</summary>
        private Dictionary<char, string> charStringDict = new Dictionary<char, string>();

        /// <summary>Just a plain white texture, used internally.</summary>
        public Texture2D WhiteTexture;

        /// <summary>Cursor textures.</summary>
        public TexturesGetter<CursorType> Cursors = new TexturesGetter<CursorType>("textures/cursor_");

        /// <summary>Metadata about cursor textures.</summary>
        public CursorTextureData[] CursorsData;

        /// <summary>All panel skin textures.</summary>
        public TexturesGetter<PanelSkin> PanelTextures = new TexturesGetter<PanelSkin>("textures/panel_");

        /// <summary>Metadata about panel textures.</summary>
        public TextureData[] PanelData;

        /// <summary>Button textures (accessed as [skin, state]).</summary>
        public TexturesGetter<ButtonSkin> ButtonTextures = new TexturesGetter<ButtonSkin>("textures/button_");

        /// <summary>Metadata about button textures.</summary>
        public TextureData[] ButtonData;

        /// <summary>CheckBox textures.</summary>
        public TexturesGetter<EntityState> CheckBoxTextures = new TexturesGetter<EntityState>("textures/checkbox");

        /// <summary>Radio button textures.</summary>
        public TexturesGetter<EntityState> RadioTextures = new TexturesGetter<EntityState>("textures/radio");

        /// <summary>ProgressBar texture.</summary>
        public Texture2D ProgressBarTexture;

        /// <summary>Metadata about progressbar texture.</summary>
        public TextureData ProgressBarData;

        /// <summary>ProgressBar fill texture.</summary>
        public Texture2D ProgressBarFillTexture;

        /// <summary>HorizontalLine texture.</summary>
        public Texture2D HorizontalLineTexture;

        /// <summary>Sliders base textures.</summary>
        public TexturesGetter<SliderSkin> SliderTextures = new TexturesGetter<SliderSkin>("textures/slider_");

        /// <summary>Sliders mark textures (the sliding piece that shows current value).</summary>
        public TexturesGetter<SliderSkin> SliderMarkTextures = new TexturesGetter<SliderSkin>("textures/slider_", "_mark");

        /// <summary>Metadata about slider textures.</summary>
        public TextureData[] SliderData;

        /// <summary>All icon textures.</summary>
        public TexturesGetter<IconType> IconTextures = new TexturesGetter<IconType>("textures/icons/");

        /// <summary>Icons inventory background texture.</summary>
        public Texture2D IconBackgroundTexture;

        /// <summary>Vertical scrollbar base texture.</summary>
        public Texture2D VerticalScrollbarTexture;

        /// <summary>Vertical scrollbar mark texture.</summary>
        public Texture2D VerticalScrollbarMarkTexture;

        /// <summary>Metadata about scrollbar texture.</summary>
        public TextureData VerticalScrollbarData;

        /// <summary>Arrow-down texture (used in dropdown).</summary>
        public Texture2D ArrowDown;

        /// <summary>Arrow-up texture (used in dropdown).</summary>
        public Texture2D ArrowUp;

        /// <summary>Default font types.</summary>
        public SpriteFont[] Fonts;

        /// <summary>Effect for disabled entities (greyscale).</summary>
        public Effect DisabledEffect;

        /// <summary>An effect to draw just a silhouette of the texture.</summary>
        public Effect SilhouetteEffect;

        /// <summary>Store the content manager instance</summary>
        internal ContentManager _content;

        /// <summary>Root for geonbit.ui content</summary>
        internal string _root;

        /// <summary>
        /// Load all GeonBit.UI resources.
        /// </summary>
        /// <param name="content">Content manager to use.</param>
        /// <param name="theme">Which theme to load resources from.</param>
        public void LoadContent(ContentManager content, string theme = "default")
        {
            InitialiseCharStringDict();

            // set resources root path and store content manager
            _root = "GeonBit.UI/themes/" + theme + "/";
            _content = content;

            // set Texture2D fields
            HorizontalLineTexture = _content.Load<Texture2D>(_root + "textures/horizontal_line");
            WhiteTexture = _content.Load<Texture2D>(_root + "textures/white_texture");
            IconBackgroundTexture = _content.Load<Texture2D>(_root + "textures/icons/background");
            VerticalScrollbarTexture = _content.Load<Texture2D>(_root + "textures/scrollbar");
            VerticalScrollbarMarkTexture = _content.Load<Texture2D>(_root + "textures/scrollbar_mark");
            ArrowDown = _content.Load<Texture2D>(_root + "textures/arrow_down");
            ArrowUp = _content.Load<Texture2D>(_root + "textures/arrow_up");
            ProgressBarTexture = _content.Load<Texture2D>(_root + "textures/progressbar");
            ProgressBarFillTexture = _content.Load<Texture2D>(_root + "textures/progressbar_fill");

            // load cursors metadata
            CursorsData = new CursorTextureData[Enum.GetValues(typeof(CursorType)).Length];
            foreach (CursorType cursor in Enum.GetValues(typeof(CursorType)))
            {
                string cursorName = cursor.ToString().ToLowerInvariant();
                CursorsData[(int)cursor] = LoadXmlCursorData(_root + "textures/cursor_" + cursorName + "_md");
            }

            // load panels
            PanelData = new TextureData[Enum.GetValues(typeof(PanelSkin)).Length];
            foreach (PanelSkin skin in Enum.GetValues(typeof(PanelSkin)))
            {
                // skip none panel skin
                if (skin == PanelSkin.None)
                {
                    continue;
                }

                // load panels metadata
                string skinName = skin.ToString().ToLowerInvariant();
                try
                {
                    PanelData[(int)skin] = LoadXmlTextureData(_root + "textures/panel_" + skinName + "_md");
                }
                catch (ContentLoadException ex)
                {
                    // for backwards compatability from when it was called 'Golden'.
                    if (skin == PanelSkin.Alternative)
                    {
                        PanelData[(int)skin] = LoadXmlTextureData(_root + "textures/panel_golden_md");
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            // load scrollbar metadata
            VerticalScrollbarData = LoadXmlTextureData(_root + "textures/scrollbar_md");

            // load slider metadata
            SliderData = new TextureData[Enum.GetValues(typeof(SliderSkin)).Length];
            foreach (SliderSkin skin in Enum.GetValues(typeof(SliderSkin)))
            {
                string skinName = skin.ToString().ToLowerInvariant();
                SliderData[(int)skin] = LoadXmlTextureData(_root + "textures/slider_" + skinName + "_md");
            }

            // load fonts
            Fonts = new SpriteFont[Enum.GetValues(typeof(FontStyle)).Length];
            foreach (FontStyle style in Enum.GetValues(typeof(FontStyle)))
            {
                Fonts[(int)style] = content.Load<SpriteFont>(_root + "fonts/" + style.ToString());
            }

            // load buttons metadata
            ButtonData = new TextureData[Enum.GetValues(typeof(ButtonSkin)).Length];
            foreach (ButtonSkin skin in Enum.GetValues(typeof(ButtonSkin)))
            {
                string skinName = skin.ToString().ToLowerInvariant();
                ButtonData[(int)skin] = LoadXmlTextureData(_root + "textures/button_" + skinName + "_md");
            }

            // load progress bar metadata
            ProgressBarData = LoadXmlTextureData(_root + "textures/progressbar_md");

            // load effects
            DisabledEffect = content.Load<Effect>(_root + "effects/disabled");
            SilhouetteEffect = content.Load<Effect>(_root + "effects/silhouette");

            // load default styleSheets
            LoadDefaultStyles( Entity.DefaultStyle, "Entity", _root);
            LoadDefaultStyles( Paragraph.DefaultStyle, "Paragraph", _root);
            LoadDefaultStyles( Button.DefaultStyle, "Button", _root);
            LoadDefaultStyles( Button.DefaultParagraphStyle, "ButtonParagraph", _root);
            LoadDefaultStyles( CheckBox.DefaultStyle, "CheckBox", _root);
            LoadDefaultStyles( CheckBox.DefaultParagraphStyle, "CheckBoxParagraph", _root);
            LoadDefaultStyles( ColoredRectangle.DefaultStyle, "ColoredRectangle", _root);
            LoadDefaultStyles( DropDown.DefaultStyle, "DropDown", _root);
            try
            {
                LoadDefaultStyles(DropDown.DefaultSelectedPanelStyle, "DropDownSelectedPanel", _root);
            }
            catch (ContentLoadException)
            {
                LoadDefaultStyles(DropDown.DefaultSelectedPanelStyle, "Panel", _root);
                DropDown.DefaultSelectedPanelStyle.SetStyleProperty("DefaultSize", new StyleProperty(Vector2.Zero));
            }
            LoadDefaultStyles( DropDown.DefaultParagraphStyle, "DropDownParagraph", _root);
            LoadDefaultStyles( DropDown.DefaultSelectedParagraphStyle, "DropDownSelectedParagraph", _root);
            LoadDefaultStyles( Header.DefaultStyle, "Header", _root);
            LoadDefaultStyles( HorizontalLine.DefaultStyle, "HorizontalLine", _root);
            LoadDefaultStyles( Icon.DefaultStyle, "Icon", _root);
            LoadDefaultStyles( Image.DefaultStyle, "Image", _root);
            LoadDefaultStyles( Label.DefaultStyle, "Label", _root);
            LoadDefaultStyles( Panel.DefaultStyle, "Panel", _root);
            LoadDefaultStyles( ProgressBar.DefaultStyle, "ProgressBar", _root);
            LoadDefaultStyles( ProgressBar.DefaultFillStyle, "ProgressBarFill", _root);
            LoadDefaultStyles( RadioButton.DefaultStyle, "RadioButton", _root);
            LoadDefaultStyles( RadioButton.DefaultParagraphStyle, "RadioButtonParagraph", _root);
            LoadDefaultStyles( SelectList.DefaultStyle, "SelectList", _root);
            LoadDefaultStyles( SelectList.DefaultParagraphStyle, "SelectListParagraph", _root);
            LoadDefaultStyles( Slider.DefaultStyle, "Slider", _root);
            LoadDefaultStyles( TextInput.DefaultStyle, "TextInput", _root);
            LoadDefaultStyles( TextInput.DefaultParagraphStyle, "TextInputParagraph", _root);
            LoadDefaultStyles( TextInput.DefaultPlaceholderStyle, "TextInputPlaceholder", _root);
            LoadDefaultStyles( VerticalScrollbar.DefaultStyle, "VerticalScrollbar", _root);
            LoadDefaultStyles( PanelTabs.DefaultButtonStyle, "PanelTabsButton", _root);
            LoadDefaultStyles( PanelTabs.DefaultButtonParagraphStyle, "PanelTabsButtonParagraph", _root);
        }


        /// <summary>
        /// Creates Dictionary containing char > string lookup
        /// </summary>
        private void InitialiseCharStringDict()
        {
            charStringDict.Clear();

            var asciiValues = Enumerable.Range('\x1', 127).ToArray();

            for (var i = 0; i < asciiValues.Length; i++)
            {
                var c = (char)asciiValues[i];
                charStringDict.Add(c, c.ToString());
            }
        }

        /// <summary>
        /// Returns string from char > string lookup
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string GetStringForChar(char c)
        {
            if (!charStringDict.ContainsKey(c)) { return c.ToString(); }
            return charStringDict[c];
        }
        /// <summary>
        /// Load xml file either directly from xml file, or from the content manager.
        /// </summary>
        /// <param name="name">XML file name.</param>
        /// <returns>T instance loaded from xml file or content manager.</returns>
        private T LoadXml<T>(string name) where T : new()
        {
            // try to load xml directly from full path
            string fullPath = Path.Combine(_content.RootDirectory, name + ".xml");
            if (File.Exists(fullPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var reader = File.OpenText(fullPath))
                {
                    XmlDeserializationEvents eventsHandler = new XmlDeserializationEvents()
                    {
                        OnUnknownAttribute = (object sender, XmlAttributeEventArgs e) => { throw new Exception("Error parsing file '" + fullPath + "': invalid attribute '" + e.Attr.Name + "' at line " + e.LineNumber); },
                        OnUnknownElement = (object sender, XmlElementEventArgs e) => { throw new Exception("Error parsing file '" + fullPath + "': invalid element '" + e.Element.Name + "' at line " + e.LineNumber); },
                        OnUnknownNode = (object sender, XmlNodeEventArgs e) => { throw new Exception("Error parsing file '" + fullPath + "': invalid element '" + e.Name + "' at line " + e.LineNumber); },
                        OnUnreferencedObject = (object sender, UnreferencedObjectEventArgs e) => { throw new Exception("Error parsing file '" + fullPath + "': unreferenced object '" + e.UnreferencedObject.ToString() + "'"); },
                    };
                    return (T)serializer.Deserialize(System.Xml.XmlReader.Create(reader), eventsHandler);
                }
            }

            // if xml file not found, try to load xnb instead
            var ret = _content.Load<T>(name);
            return ret;
        }

        /// <summary>
        /// Load xml texture data either directly from xml file, or from the content manager.
        /// </summary>
        /// <param name="name">XML name.</param>
        /// <returns>Texture data loaded from xml or xnb.</returns>
        private TextureData LoadXmlTextureData(string name)
        {
            return LoadXml<TextureData>(name);
        }

        /// <summary>
        /// Load xml cursor data either directly from xml file, or from the content manager.
        /// </summary>
        /// <param name="name">XML name.</param>
        /// <returns>Cursor texture data loaded from xml or xnb.</returns>
        private CursorTextureData LoadXmlCursorData(string name)
        {
            return LoadXml<CursorTextureData>(name);
        }

        /// <summary>
        /// Load xml styles either directly from xml file, or from the content manager.
        /// </summary>
        /// <param name="name">XML name.</param>
        /// <returns>Default styles loaded from xml or xnb.</returns>
        private DefaultStyles LoadXmlStyles(string name)
        {
            return LoadXml<DefaultStyles>(name);
        }

        /// <summary>
        /// Load default stylesheets for a given entity name and put values inside the sheet.
        /// </summary>
        /// <param name="sheet">StyleSheet to load.</param>
        /// <param name="entityName">Entity unique identifier for file names.</param>
        /// <param name="themeRoot">Path of the current theme root directory.</param>
        private void LoadDefaultStyles(StyleSheet sheet, string entityName, string themeRoot)
        {
            // clear previous styles
            sheet.Clear();

            // get stylesheet root path (eg everything before the state part)
            string stylesheetBase = themeRoot + "styles/" + entityName;

            // load default styles
            FillDefaultStyles(sheet, EntityState.Default, LoadXmlStyles($"{stylesheetBase}-Default"));

            // load mouse-hover styles
            FillDefaultStyles(sheet, EntityState.MouseHover, LoadXmlStyles($"{stylesheetBase}-MouseHover"));

            // load mouse-down styles
            FillDefaultStyles(sheet, EntityState.MouseDown, LoadXmlStyles($"{stylesheetBase}-MouseDown"));
        }

        /// <summary>
        /// Load texture from path.
        /// </summary>
        /// <param name="path">Texture path, under theme folder.</param>
        /// <returns>Texture instance.</returns>
        public Texture2D LoadTexture(string path)
        {
            return _content.Load<Texture2D>(Path.Combine(_root, path));
        }

        /// <summary>
        /// Fill a set of default styles into a given stylesheet.
        /// </summary>
        /// <param name="sheet">StyleSheet to fill.</param>
        /// <param name="state">State to fill values for.</param>
        /// <param name="styles">Default styles, as loaded from xml file.</param>
        private void FillDefaultStyles(StyleSheet sheet, EntityState state, DefaultStyles styles)
        {
            if (styles.FillColor != null) { sheet[$"{state}.FillColor"] = new StyleProperty((Color)styles.FillColor); }
            if (styles.FontStyle != null) { sheet[$"{state}.FontStyle"] = new StyleProperty((int)styles.FontStyle); }
            if (styles.ForceAlignCenter != null) { sheet[$"{state}.ForceAlignCenter"] = new StyleProperty((bool)styles.ForceAlignCenter); }
            if (styles.OutlineColor != null) { sheet[$"{state}.OutlineColor"] = new StyleProperty((Color)styles.OutlineColor); }
            if (styles.OutlineWidth != null) { sheet[$"{state}.OutlineWidth"] = new StyleProperty((int)styles.OutlineWidth); }
            if (styles.Scale != null) { sheet[$"{state}.Scale"] = new StyleProperty((float)styles.Scale); }
            if (styles.SelectedHighlightColor != null) { sheet[$"{state}.SelectedHighlightColor"] = new StyleProperty((Color)styles.SelectedHighlightColor); }
            if (styles.ShadowColor != null) { sheet[$"{state}.ShadowColor"] = new StyleProperty((Color)styles.ShadowColor); }
            if (styles.ShadowOffset != null) { sheet[$"{state}.ShadowOffset"] = new StyleProperty((Vector2)styles.ShadowOffset); }
            if (styles.Padding != null) { sheet[$"{state}.Padding"] = new StyleProperty((Vector2)styles.Padding); }
            if (styles.SpaceBefore != null) { sheet[$"{state}.SpaceBefore"] = new StyleProperty((Vector2)styles.SpaceBefore); }
            if (styles.SpaceAfter != null) { sheet[$"{state}.SpaceAfter"] = new StyleProperty((Vector2)styles.SpaceAfter); }
            if (styles.ShadowScale != null) { sheet[$"{state}.ShadowScale"] = new StyleProperty((float)styles.ShadowScale); }
            if (styles.DefaultSize != null) { sheet[$"{state}.DefaultSize"] = new StyleProperty((Vector2)styles.DefaultSize); }
        }
    }
}
