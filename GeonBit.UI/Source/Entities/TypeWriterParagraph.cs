using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GeonBit.UI.Source.Entities
{
    /// <summary>
    /// TypeWriter Paragraph is a paragraph that supports the type writer effect and in-text color tags that changes the fill color of the text.
    /// </summary>
    [System.Serializable]
    public class TypeWriterParagraph : MulticolorParagraph
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static TypeWriterParagraph()
        {
            Entity.MakeSerializable(typeof(TypeWriterParagraph));
        }
        
        /// <summary>
        /// Enables or disables the type writer effect.
        /// </summary>
        public bool EnableTypeWriterEffect
        {
            get { return _enableTypeWriterEffect; }
            set { _enableTypeWriterEffect = value; }
        }
        private bool _enableTypeWriterEffect = true;

        /// <summary>
        /// The structure of a type writer CharProperty
        /// </summary>
        struct CharProperty
        {
            /// <summary>
            /// Actual char.
            /// </summary>
            public char Char;
            /// <summary>
            /// Color of the char.
            /// </summary>
            public Color Color;
            /// <summary>
            /// Size of the char.
            /// </summary>
            public Vector2 Size;
            /// <summary>
            /// Position of the char.
            /// </summary>
            public Vector2 Position;
        }

        // this list contains all the char properties generated out of the _processedText string.
        private List<CharProperty> _charPropertyList = new List<CharProperty>();

        // this timespan defines the time in milliseconds which should pass by before a new CharProperty gets added to the _CharPropertyList.
        private TimeSpan _timeBetweenCharacters = TimeSpan.FromMilliseconds(1);
        // this timespan gets raised by 1ms each frame and will be checked if it's greater-equal to the _timeBetweenCharacters timespan.
        private TimeSpan _currentTime = TimeSpan.Zero;

        // this index gets raised every frame and checks against the length of the _processedText string.
        private int _iTextIndex;

        // caches the last ColorInstructions color to fill the gap between different ColorInstruction colors.
        private Color? _lastColor;

        // the current maximum char width (x) reflects its position by adding its width to the character size.
        private float _wholeCharSizeX;

        // the current maximum char height (y) reflects its position by adding its height to the character size.
        private float _wholeCharSizeY;

        /// <summary>
        /// Create the paragraph.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="scale">Optional font size.</param>
        /// <param name="timeBetweenChars">After this time a new char of the text value will be drawn. Default: 1ms</param>
        public TypeWriterParagraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null, float? scale = null, TimeSpan? timeBetweenChars = null) :
            base(text, anchor, size, offset, scale)
        {
            _timeBetweenCharacters = timeBetweenChars ?? _timeBetweenCharacters;
        }

        /// <summary>
        /// Create the paragraph with optional fill color and font size.
        /// </summary>
        /// <param name="text">Paragraph text (accept new line characters).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="color">Text fill color.</param>
        /// <param name="scale">Optional font size.</param>
        /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="timeBetweenChars">After this time a new char of the text value will be drawn. Default: 1ms</param>
        public TypeWriterParagraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null, Vector2? offset = null, TimeSpan? timeBetweenChars = null) :
            base(text, anchor, color, scale, size, offset)
        {
            _timeBetweenCharacters = timeBetweenChars ?? _timeBetweenCharacters;
        }

        /// <summary>
        /// Create default TypeWriterParagraph with empty text.
        /// </summary>
        public TypeWriterParagraph() : this(string.Empty)
        {
        }

        /// <summary>
        /// Draw entity outline. Note: in paragraph its a special case and we implement it inside the DrawEntity function.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntityOutline(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            if (_enableTypeWriterEffect)
            {
                // update processed text if needed
                if (NeedUpdateColors)
                {
                    ParseColorInstructions();
                    UpdateDestinationRects();
                }

                CreateCharArray();
                DrawCharProperty(spriteBatch);
            }
            else base.DrawEntity(spriteBatch, phase);
        }

        /// <summary>
        /// Draw a char together with its properties to the spritebatch.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        private void DrawCharProperty(SpriteBatch spriteBatch)
        {
            foreach (CharProperty charProperty in _charPropertyList)
            {
                DrawOutline(spriteBatch, charProperty.Char.ToString(), charProperty.Position);

                // fix color opacity and draw the corresponding main string
                spriteBatch.DrawString(
                    _currFont,
                    charProperty.Char.ToString(),
                    charProperty.Position,
                    UserInterface.Active.DrawUtils.FixColorOpacity(charProperty.Color),
                    0,
                    _fontOrigin,
                    _actualScale,
                    SpriteEffects.None,
                    0.5f);
            }
        }

        // iterates through the _processedText string and creates CharProperty objects based on its chars
        private void CreateCharArray()
        {
            if (_enableTypeWriterEffect)
            {
                if (_currentTime >= _timeBetweenCharacters)
                {
                    if (_iTextIndex < _processedText.Length)
                    {
                        AddChar(_iTextIndex);

                        _currentTime = TimeSpan.Zero;
                        _iTextIndex++;
                    }
                }
                else _currentTime += TimeSpan.FromMilliseconds(1);
            }
            else
            {
                for (int i = 0; i < _processedText.Length; i++) AddChar(i);
            }
        }

        // adds a single char together with its properties to the _CharPropertyList.
        private void AddChar(int index)
        {
            if (_charPropertyList.Count < _processedText.Length)
            {
                CharProperty charProperty = new CharProperty();
                charProperty.Color = FillColor;
                charProperty.Char = _processedText[index];
                charProperty.Size = GetCharacterActualSize();
                charProperty.Position = _position;

                if (ColorInstructions.Count > 0)
                {
                    ColorInstruction colorInstruction;
                    if (ColorInstructions != null && ColorInstructions.TryGetValue(index, out colorInstruction))
                    {
                        if (colorInstruction.UseFillColor) charProperty.Color = FillColor;
                        else charProperty.Color = colorInstruction.Color;

                        _lastColor = charProperty.Color;
                    }
                    else charProperty.Color = _lastColor ?? FillColor;
                }

                if (charProperty.Char != '\n')
                {
                    _wholeCharSizeX += charProperty.Size.X;
                }
                else
                {
                    _wholeCharSizeX = 0;
                    _wholeCharSizeY += charProperty.Size.Y;
                }
                charProperty.Position.X += _wholeCharSizeX;
                charProperty.Position.Y += _wholeCharSizeY;

                _charPropertyList.Add(charProperty);
            }
        }
    }
}
