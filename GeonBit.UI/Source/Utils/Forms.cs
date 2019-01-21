using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GeonBit.UI.Utils
{
    /// <summary>
    /// Helper class to generate forms.
    /// </summary>
    public static class Forms
    {
        /// <summary>
        /// Form field types.
        /// </summary>
        public enum FieldType
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
        }

        /// <summary>
        /// Form field data.
        /// </summary>
        public struct FieldData
        {
            /// <summary>
            /// Field type.
            /// </summary>
            public FieldType Type;

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
            /// Custom initialize function that will be called when the field's entity is created.
            /// </summary>
            public System.Action<Entity> OnFieldInit;
        }

        /// <summary>
        /// Form instance.
        /// </summary>
        public class Form
        {
            // fields data and entities used to represent them
            Dictionary<string, FieldData> _fieldsData = new Dictionary<string, FieldData>();
            Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();

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
            public Form(IEnumerable<FieldData> fields, Panel parent)
            {
                // create root panel
                FormPanel = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.Auto);
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
                    
                    // add label and entity
                    if (needLabel)
                    {
                        FormPanel.AddChild(new Label(field.FieldLabel));
                    }
                    FormPanel.AddChild(fieldEntity);

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
            private Entity CreateEntityForField(FieldData fieldData, ref bool needLabel)
            {
                // by default need label
                needLabel = true;

                // create entity based on type
                switch (fieldData.Type)
                {
                    case FieldType.Checkbox:
                        needLabel = false;
                        return new CheckBox(fieldData.FieldLabel, isChecked: (bool)(fieldData.DefaultValue));

                    case FieldType.DropDown:
                        var dropdown = new DropDown(new Vector2(0, -1));
                        foreach (var choice in fieldData.Choices)
                        {
                            dropdown.AddItem(choice);
                        }
                        if (fieldData.DefaultValue is string)
                        {
                            dropdown.SelectedValue = fieldData.DefaultValue as string;
                        }
                        else if (fieldData.DefaultValue is int)
                        {
                            dropdown.SelectedIndex = (int)fieldData.DefaultValue;
                        }
                        return dropdown;

                    case FieldType.MultilineTextInput:
                        var multiText = new TextInput(true);
                        multiText.Value = fieldData.DefaultValue as string;
                        return multiText;

                    case FieldType.TextInput:
                        var text = new TextInput(false);
                        text.Value = fieldData.DefaultValue as string;
                        return text;

                    case FieldType.Slider:
                        var slider = new Slider((uint)fieldData.Min, (uint)fieldData.Max);
                        if (fieldData.DefaultValue is int)
                        {
                            slider.Value = (int)fieldData.DefaultValue;
                        }
                        return slider;

                    case FieldType.SelectList:
                        var selectlist = new SelectList(new Vector2(0, -1));
                        foreach (var choice in fieldData.Choices)
                        {
                            selectlist.AddItem(choice);
                        }
                        if (fieldData.DefaultValue is string)
                        {
                            selectlist.SelectedValue = fieldData.DefaultValue as string;
                        }
                        else if (fieldData.DefaultValue is int)
                        {
                            selectlist.SelectedIndex = (int)fieldData.DefaultValue;
                        }
                        return selectlist;

                    case FieldType.RadioButtons:
                        var radiosPanel = new Panel(new Vector2(0, -1), PanelSkin.None, Anchor.Auto);
                        radiosPanel.Padding = Vector2.Zero;
                        foreach (var choice in fieldData.Choices)
                        {
                            var radio = new RadioButton(choice, isChecked: (fieldData.DefaultValue as string) == choice);
                            radiosPanel.AddChild(radio);
                        }
                        return radiosPanel;

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
            /// Get field's value.
            /// </summary>
            /// <param name="fieldId">Field to get.</param>
            /// <returns>Field's value.</returns>
            public object GetValue(string fieldId)
            {
                // get field data and entity
                var fieldData = _fieldsData[fieldId];
                var entity = _entities[fieldId];

                // return value based on type
                switch (fieldData.Type)
                {
                    case FieldType.Checkbox:
                        return ((CheckBox)(entity)).Checked;

                    case FieldType.DropDown:
                        return ((DropDown)(entity)).SelectedValue;

                    case FieldType.MultilineTextInput:
                        return ((TextInput)(entity)).Value;

                    case FieldType.TextInput:
                        return ((TextInput)(entity)).Value;

                    case FieldType.Slider:
                        return ((Slider)(entity)).Value;

                    case FieldType.SelectList:
                        return ((SelectList)(entity)).SelectedValue;

                    case FieldType.RadioButtons:
                        foreach (var child in ((Panel)(entity)).Children)
                        {
                            if (child is RadioButton && ((RadioButton)child).Checked)
                            {
                                return (child as RadioButton).TextParagraph.Text;
                            }
                        }
                        return null;

                    default:
                        throw new Exceptions.InvalidStateException("Unknown field type!");
                }
            }
        }
    }
}
