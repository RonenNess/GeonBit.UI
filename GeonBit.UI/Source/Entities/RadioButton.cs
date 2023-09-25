#region File Description
//-----------------------------------------------------------------------------
// RadioButtons are like checkboxes, but only one radio button can be selected
// at a time.
//
// Note that radio buttons affect only their direct siblings, eg if you select
// a radio button it will uncheck all the other radio buttons that are under
// the same parent, but will not affect any radio buttons outside the parent.
//
// Radio buttons are good for simple choices, For example a choice between
// OpenGL or DirectX, selecting gender in a form, etc.
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
    /// A Radio Button entity is like a checkbox (label with a box next to it that can be checked / unchecked) with the exception that whenever a radio button is checked, all its siblings are unchecked automatically.
    /// </summary>
    /// <remarks>
    /// Radio Buttons only affect their direct siblings, so if you need multiple groups of radio buttons on the same panel you can use internal panels to group them together.
    /// </remarks>
    [System.Serializable]
    public class RadioButton : CheckBox
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static RadioButton()
        {
            Entity.MakeSerializable(typeof(RadioButton));
        }

        /// <summary>Default styling for the radio button itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for radio button label. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>If set to true, clicking on an already checked radio button will uncheck if. If false (default), will do nothing.</summary>
        public bool CanUncheck = false;

        /// <summary>
        /// Create the radio button.
        /// </summary>
        /// <param name="text">Radio button label text.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="size">Radio button size.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="isChecked">If true, radio button will be created as checked.</param>
        public RadioButton(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null, bool isChecked = false) :
            base(text, anchor, size, offset, isChecked)
        {
            UpdateStyle(DefaultStyle);
            if (TextParagraph != null)
            {
                TextParagraph.UpdateStyle(DefaultParagraphStyle);
            }
        }

        /// <summary>
        /// Create radiobutton without text.
        /// </summary>
        public RadioButton() : this(string.Empty)
        {
        }

        /// <summary>
        /// Helper function to get radio button texture based on state and current value.
        /// </summary>
        /// <returns>Which texture to use for the checkbox.</returns>
        override protected Texture2D GetTexture()
        {
            EntityState state = _entityState;
            if (state != EntityState.MouseDown && (bool)Checked) { state = EntityState.MouseDown; }
            return Resources.Instance.RadioTextures[state];
        }

        /// <summary>
        /// Handle value change.
        /// RadioButton override this function to handle the task of unchecking siblings when selected.
        /// </summary>
        override protected void DoOnValueChange()
        {
            // if not checked, do nothing
            if (!Checked)
            {
                return;
            }

            // disable all sibling radio buttons
            if (_parent != null)
            {
                foreach (Entity entity in _parent._children)
                {
                    // skip self
                    if (entity == this)
                    {
                        continue;
                    }

                    // if entity is a radio button make sure its disabled
                    if (entity is RadioButton)
                    {
                        RadioButton radio = (RadioButton)entity;
                        if (radio.Checked) { radio.Checked = false; }
                    }
                }
            }

            // call base handler
            base.DoOnValueChange();
        }

        /// <summary>
        /// Handle mouse click event. 
        /// Radio buttons entity override this function to handle value toggle.
        /// </summary>
        override protected void DoOnClick()
        {
            // call base handler
            base.DoOnClick();

            // if cannot be unchecked, make sure its checked
            if (!CanUncheck)
            {
                Checked = true;
            }
        }
    }
}
