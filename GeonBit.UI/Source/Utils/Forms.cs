using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GeonBit.UI.Utils.Forms
{
    /// <summary>
    /// Form field types.
    /// </summary>
    public enum FormFieldType
    {
        /// <summary>
        /// Checkbox field.
        /// </summary>
        Checkbox,

        /// <summary>
        /// Radio buttons (require choices).
        /// </summary>
        RadioButtons,

        /// <summary>
        /// Inline text input.
        /// </summary>
        TextInput,

        /// <summary>
        /// Multiline text input.
        /// </summary>
        MultilineTextInput,

        /// <summary>
        /// Select list.
        /// </summary>
        SelectList,

        /// <summary>
        /// Dropdown choices.
        /// </summary>
        DropDown,

        /// <summary>
        /// Slider field.
        /// </summary>
        Slider,

        /// <summary>
        /// Start a new form section, with a header and horizontal line.
        /// </summary>
        Section,
    }

    /// <summary>
    /// Form field data.
    /// </summary>
    public class FormFieldData
    {
        /// <summary>
        /// Field type.
        /// </summary>
        public FormFieldType Type;

        /// <summary>
        /// Field unique identifier.
        /// </summary>
        public string FieldId;

        /// <summary>
        /// Label to show above field.
        /// </summary>
        public string FieldLabel;

        /// <summary>
        /// Default value to start with.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Choices of this field, used for fields like dropdown, radio buttons, etc.
        /// </summary>
        public string[] Choices;

        /// <summary>
        /// Min value (when input is integer field like slider).
        /// </summary>
        public int Min;

        /// <summary>
        /// Max value (when input is integer field like slider).
        /// </summary>
        public int Max;

        /// <summary>
        /// Tooltip text to assign to the field entity.
        /// </summary>
        public string ToolTipText;

        /// <summary>
        /// Custom initialize function that will be called when the field's entity is created.
        /// </summary>
        public System.Action<Entity> OnFieldCreated;

        /// <summary>
        /// Create form field data.
        /// </summary>
        /// <param name="type">Field type.</param>
        /// <param name="id">Field unique id.</param>
        /// <param name="label">Optional label.</param>
        public FormFieldData(FormFieldType type, string id, string label = null)
        {
            Type = type;
            FieldId = id;
            FieldLabel = label;
        }
    }

    /// <summary>
    /// Form instance.
    /// </summary>
    public class Form
    {
        // fields data and entities used to represent them
        Dictionary<string, FormFieldData> _fieldsData = new Dictionary<string, FormFieldData>();
        Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();

        /// <summary>
        /// Extra space to set between fields.
        /// </summary>
        public static int ExtraSpaceBetweenFields = 10;

        /// <summary>
        /// Form's root panel.
        /// </summary>
        public Panel FormPanel;

        /// <summary>
        /// Create the form from a list of fields data.
        /// Note: the returned form will be inside an invisible panel without padding and 100% size. This means
        /// that you need to provide a panel of your own to put the form inside.
        /// </summary>
        /// <param name="fields">Fields to generate form from.</param>
        /// <param name="parent">Optional panel to contain the result form.</param>
        public Form(IEnumerable<FormFieldData> fields, Panel parent)
        {
            // create root panel
            FormPanel = new Panel(new Vector2(0, -1), PanelSkin.None, Anchor.Auto);
            FormPanel.Padding = Vector2.Zero;

            // create fields
            foreach (var field in fields)
            {
                // store field data
                _fieldsData[field.FieldId] = field;

                // field entity to create and do we need a label for it
                Entity fieldEntity;
                bool needLabel = true;

                // create entity based on type
                fieldEntity = CreateEntityForField(field, ref needLabel);
                fieldEntity.Identifier = "form-entity-" + field.FieldId;
                fieldEntity.SpaceAfter += new Vector2(0, ExtraSpaceBetweenFields);

                // add label
                if (needLabel)
                {
                    var label = FormPanel.AddChild(new Label(field.FieldLabel));
                    label.Identifier = "form-entity-label-" + field.FieldId;
                }

                // add entity to form
                FormPanel.AddChild(fieldEntity);

                // set tooltiptext and call custom init function
                if (!string.IsNullOrEmpty(field.ToolTipText)) { fieldEntity.ToolTipText = field.ToolTipText; }
                field.OnFieldCreated?.Invoke(fieldEntity);

                // store entity in entities dictionary
                _entities[field.FieldId] = fieldEntity;
            }

            // add to parent
            if (parent != null)
            {
                parent.AddChild(FormPanel);
            }
        }

        /// <summary>
        /// Create and return an entity for a field type.
        /// </summary>
        /// <param name="fieldData">Field data to generate entity for.</param>
        /// <param name="needLabel">Will set if need to generate a label for this entity or not.</param>
        /// <returns></returns>
        protected virtual Entity CreateEntityForField(FormFieldData fieldData, ref bool needLabel)
        {
            // by default need label
            needLabel = !string.IsNullOrEmpty(fieldData.FieldLabel);

            // create entity based on type
            switch (fieldData.Type)
            {
                // create checkbox
                case FormFieldType.Checkbox:
                    needLabel = false;
                    return new CheckBox(fieldData.FieldLabel, isChecked: fieldData.DefaultValue != null && (bool)(fieldData.DefaultValue));

                // create dropdown
                case FormFieldType.DropDown:

                    // create entity and set choices
                    var dropdown = new DropDown(new Vector2(0, -1));
                    foreach (var choice in fieldData.Choices)
                    {
                        dropdown.AddItem(choice);
                    }

                    // if got few items adjust height automatically
                    if (dropdown.SelectList.Items.Length < 10) { dropdown.SelectList.AdjustHeightAutomatically = true; }

                    // set default value and return
                    if (fieldData.DefaultValue is string)
                    {
                        dropdown.SelectedValue = fieldData.DefaultValue as string;
                    }
                    else if (fieldData.DefaultValue is int)
                    {
                        dropdown.SelectedIndex = (int)fieldData.DefaultValue;
                    }
                    return dropdown;

                // create multi-line text input
                case FormFieldType.MultilineTextInput:
                    var multiText = new TextInput(true);
                    multiText.Value = fieldData.DefaultValue as string;
                    return multiText;

                // create single-line text input
                case FormFieldType.TextInput:
                    var text = new TextInput(false);
                    if (fieldData.DefaultValue != null) { text.Value = fieldData.DefaultValue as string; }
                    return text;

                // create slider input
                case FormFieldType.Slider:
                    var slider = new Slider(fieldData.Min, fieldData.Max);
                    if (fieldData.DefaultValue is int)
                    {
                        slider.Value = (int)fieldData.DefaultValue;
                    }
                    return slider;

                // create select list input
                case FormFieldType.SelectList:
                    
                    // create the entity itself
                    var selectlist = new SelectList(new Vector2(0, -1));
                    foreach (var choice in fieldData.Choices)
                    {
                        selectlist.AddItem(choice);
                    }

                    // if got few items set height automatically
                    if (selectlist.Items.Length < 10) { selectlist.AdjustHeightAutomatically = true; }

                    // set default value and return
                    if (fieldData.DefaultValue is string)
                    {
                        selectlist.SelectedValue = fieldData.DefaultValue as string;
                    }
                    else if (fieldData.DefaultValue is int)
                    {
                        selectlist.SelectedIndex = (int)fieldData.DefaultValue;
                    }
                    return selectlist;

                // create radio buttons
                case FormFieldType.RadioButtons:
                    var radiosPanel = new Panel(new Vector2(0, -1), PanelSkin.None, Anchor.Auto);
                    radiosPanel.Padding = Vector2.Zero;
                    foreach (var choice in fieldData.Choices)
                    {
                        var radio = new RadioButton(choice, isChecked: (fieldData.DefaultValue as string) == choice);
                        radiosPanel.AddChild(radio);
                    }
                    return radiosPanel;

                // create a new secion
                case FormFieldType.Section:
                    var containerPanel = new Panel(new Vector2(0, -1), PanelSkin.None, Anchor.Auto);
                    containerPanel.Padding = Vector2.Zero;
                    if (needLabel) { containerPanel.AddChild(new Paragraph(fieldData.FieldLabel)); }
                    containerPanel.AddChild(new HorizontalLine());
                    needLabel = false;
                    return containerPanel;

                // unknown type!
                default:
                    throw new Exceptions.InvalidStateException("Unknown field type!");
            }
        }

        /// <summary>
        /// Remove this form from its parent.
        /// </summary>
        public void Remove()
        {
            FormPanel.RemoveFromParent();
        }

        /// <summary>
        /// Get entity for field.
        /// </summary>
        /// <param name="fieldId">Field id to get entity for.</param>
        /// <returns>Field's root entity.</returns>
        public Entity GetEntity(string fieldId)
        {
            return _entities[fieldId];
        }

        /// <summary>
        /// Get field's value.
        /// </summary>
        /// <param name="fieldId">Field to get.</param>
        /// <param name="defaultVal">Default value to return if value to get is null, unselected, or empty string.</param>
        /// <returns>Field's value.</returns>
        public virtual object GetValue(string fieldId, object defaultVal = null)
        {
            // get field data and entity
            var fieldData = _fieldsData[fieldId];
            var entity = _entities[fieldId];

            // anonymous function to return default value if value from form is null or empty
            System.Func<object, object> ApplyDefaultVal = (object val) =>
            {
                if (defaultVal != null && (val == null || string.IsNullOrEmpty(val.ToString())))
                {
                    return defaultVal;
                }
                return val;
            };

            // return value based on type
            switch (fieldData.Type)
            {
                case FormFieldType.Checkbox:
                    return ApplyDefaultVal(((CheckBox)(entity)).Checked);

                case FormFieldType.DropDown:
                    return ApplyDefaultVal(((DropDown)(entity)).SelectedValue);

                case FormFieldType.MultilineTextInput:
                    return ApplyDefaultVal(((TextInput)(entity)).Value);

                case FormFieldType.TextInput:
                    return ApplyDefaultVal(((TextInput)(entity)).Value);

                case FormFieldType.Slider:
                    return ApplyDefaultVal(((Slider)(entity)).Value);

                case FormFieldType.SelectList:
                    return ApplyDefaultVal(((SelectList)(entity)).SelectedValue);

                case FormFieldType.RadioButtons:
                    foreach (var child in ((Panel)(entity)).Children)
                    {
                        if (child is RadioButton && ((RadioButton)child).Checked)
                        {
                            return ApplyDefaultVal((child as RadioButton).TextParagraph.Text);
                        }
                    }
                    return ApplyDefaultVal(null);

                case FormFieldType.Section:
                    return ApplyDefaultVal(null);

                default:
                    throw new Exceptions.InvalidStateException("Unknown field type!");
            }
        }

        /// <summary>
        /// Get field value as a trimmed string.
        /// </summary>
        /// <param name="fieldId">Field to get.</param>
        /// <param name="defaultVal">Default value to return if value to get is null, unselected, or empty string.</param>
        /// <returns>Field's value.</returns>
        public virtual string GetValueString(string fieldId, string defaultVal = null)
        {
            var ret = GetValue(fieldId, defaultVal);
            return ret.ToString().Trim();
        }

        /// <summary>
        /// Get field value as boolean.
        /// </summary>
        /// <param name="fieldId">Field to get.</param>
        /// <param name="defaultVal">Default value to return if value to get is null, unselected, or empty string.</param>
        /// <returns>Field's value.</returns>
        public virtual bool GetValueBool(string fieldId, bool? defaultVal = null)
        {
            var ret = GetValue(fieldId, defaultVal);
            return System.Boolean.Parse(ret.ToString());
        }

        /// <summary>
        /// Get field value as integer.
        /// </summary>
        /// <param name="fieldId">Field to get.</param>
        /// <param name="defaultVal">Default value to return if value to get is null, unselected, or empty string.</param>
        /// <returns>Field's value.</returns>
        public virtual int GetValueInt(string fieldId, int? defaultVal = null)
        {
            var ret = GetValue(fieldId, defaultVal);
            return System.Int32.Parse(ret.ToString());
        }

        /// <summary>
        /// Get field value as float.
        /// </summary>
        /// <param name="fieldId">Field to get.</param>
        /// <param name="defaultVal">Default value to return if value to get is null, unselected, or empty string.</param>
        /// <returns>Field's value.</returns>
        public virtual float GetValueFloat(string fieldId, float? defaultVal = null)
        {
            var ret = GetValue(fieldId, defaultVal);
            return System.Single.Parse(ret.ToString());
        }
    }
}
