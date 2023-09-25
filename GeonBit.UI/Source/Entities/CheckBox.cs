#region File Description
//-----------------------------------------------------------------------------
// CheckBoxes are inline paragraphs with a little square next to them that can either
// be checked or unchecked.
// 
// They are useful for binary settings, like fullscreen mode, mute / unmute,
// play hardcore mode, etc.
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
    /// A checkbox entity, eg a label with a square you can mark as checked or uncheck.
    /// Holds a boolean value.
    /// </summary>
    [System.Serializable]
    public class CheckBox : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static CheckBox()
        {
            Entity.MakeSerializable(typeof(CheckBox));
        }

        /// <summary>CheckBox label. Use this if you want to change the checkbox text or font style.</summary>
        public Paragraph TextParagraph;

        /// <summary>Current checkbox value.</summary>
        protected bool _value = false;

        // checkbox widget size (the graphic box part)
        static Vector2 CHECKBOX_SIZE = new Vector2(35, 35);

        /// <summary>Default styling for the checkbox itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for checkbox label. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>
        /// Create a new checkbox entity.
        /// </summary>
        /// <param name="text">CheckBox label text.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">CheckBox size.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="isChecked">If true, this checkbox will be created as 'checked'.</param>
        public CheckBox(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null, bool isChecked = false) :
            base(size, anchor, offset)
        {
            // update default style
            UpdateStyle(DefaultStyle);

            // create and set checkbox paragraph
            if (!UserInterface.Active._isDeserializing)
            {
                TextParagraph = UserInterface.DefaultParagraph(text, Anchor.CenterLeft);
                TextParagraph.UpdateStyle(DefaultParagraphStyle);
                TextParagraph.Offset = new Vector2(25, 0);
                TextParagraph._hiddenInternalEntity = true;
                TextParagraph.Identifier = "_checkbox_text";
                AddChild(TextParagraph, true);
            }

            // checkboxes are promiscuous by default.
            PromiscuousClicksMode = true;

            // set value
            Checked = isChecked;
        }

        /// <summary>
        /// Create checkbox without text.
        /// </summary>
        public CheckBox() : this(string.Empty)
        {
        }

        /// <summary>
        /// Special init after deserializing entity from file.
        /// </summary>
        internal protected override void InitAfterDeserialize()
        {
            base.InitAfterDeserialize();
            TextParagraph = Find("_checkbox_text") as Paragraph;
            TextParagraph._hiddenInternalEntity = true;
        }

        /// <summary>
        /// Is the checkbox a natrually-interactable entity.
        /// </summary>
        /// <returns>True.</returns>
        override internal protected bool IsNaturallyInteractable()
        {
            return true;
        }

        /// <summary>
        /// Change the value of this entity, where there's value to change.
        /// </summary>
        /// <param name="newValue">New value to set.</param>
        /// <param name="emitEvent">If true and value changed, will emit 'ValueChanged' event.</param>
        override public void ChangeValue(object newValue, bool emitEvent)
        {
            var boolValue = (bool)newValue;
            if (_value != boolValue)
            {
                _value = boolValue;
                if (emitEvent) { DoOnValueChange(); }
            }
        }

        /// <summary>
        /// Get the value of this entity, where there's value.
        /// </summary>
        /// <returns>Value as object.</returns>
        override public object GetValue()
        {
            return _value;
        }

        /// <summary>
        /// CheckBox current value, eg if its checked or unchecked.
        /// </summary>
        public bool Checked
        {
            get 
            { 
                return _value == true; 
            }
            set 
            {
                if (_value != value)
                {
                    _value = value;
                    DoOnValueChange();
                }
            }
        }

        /// <summary>
        /// Helper function to get checkbox texture based on state and current value.
        /// </summary>
        /// <returns>Which texture to use for the checkbox.</returns>
        virtual protected Texture2D GetTexture()
        {
            EntityState state = _entityState;
            if (state != EntityState.MouseDown && Checked) { state = EntityState.MouseDown; }
            return Resources.Instance.CheckBoxTextures[state];
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {

            // get texture based on checkbox / mouse state
            Texture2D texture = GetTexture();

            // calculate actual size
            Vector2 actualSize = CHECKBOX_SIZE * GlobalScale;

            // dest rect
            Rectangle dest = new Rectangle(_destRect.X,
                                (int)(_destRect.Y + _destRect.Height / 2 - actualSize.Y / 2),
                                (int)(actualSize.X),
                                (int)(actualSize.Y));
            dest = UserInterface.Active.DrawUtils.ScaleRect(dest, Scale);

            // source rect
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);

            // draw checkbox
            spriteBatch.Draw(texture, dest, src, FillColor);

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }

        /// <summary>
        /// Handle mouse click event. 
        /// CheckBox entity override this function to handle value toggle.
        /// </summary>
        override protected void DoOnClick()
        {
            // toggle value
            Checked = !_value;

            // call base handler
            base.DoOnClick();
        }
    }
}
