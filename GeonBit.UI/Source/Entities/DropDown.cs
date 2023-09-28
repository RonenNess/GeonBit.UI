#region File Description
//-----------------------------------------------------------------------------
// DropDown is a sub-class of SelectList - it works the same, but in DropDown we
// only see the currently selected value in a special box, and only when its
// clicked the list becomes visible and you can select from it.
//
// DropDown gives you list functionality, for much less UI space!
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GeonBit.UI.Entities
{

    /// <summary>
    /// DropDown is just like a list, but it only shows the currently selected value unless clicked on (the list is
    /// only revealed while interacted with).
    /// </summary>
    [System.Serializable]
    public class DropDown : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static DropDown()
        {
            Entity.MakeSerializable(typeof(DropDown));
        }

        /// <summary>Default text to show when no value is selected from the list.</summary>
        public string DefaultText
        {
            get { return _placeholderText; }
            set { _placeholderText = value; }
        }

        // text used as placeholder when nothing is selected.
        private string _placeholderText = "Click to Select";

        /// <summary>Default style for the dropdown itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for dropdown labels. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default styling for the dropdown panel when its closed (where the currently-selected label is shown).</summary>
        public static StyleSheet DefaultSelectedPanelStyle = new StyleSheet();

        /// <summary>Default styling for the dropdown currently-selected label. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultSelectedParagraphStyle = new StyleSheet();

        // dictionary of special events for specific items selection
        private Dictionary<string, System.Action> _perItemCallbacks = new Dictionary<string, System.Action>();

        // last known selected index
        private int _lastSelected = -1;

        // internal panel and paragraph to show selected value.
        Panel _selectedTextPanel;
        Paragraph _selectedTextParagraph;
        Image _arrowDownImage;

        // an internal select list used when dropdown is opened.
        SelectList _selectList;

        /// <summary>
        /// Get the selected text panel (what's shown when DropDown is closed).
        /// </summary>
        public Panel SelectedTextPanel
        {
            get { return _selectedTextPanel; }
        }

        /// <summary>
        /// If true and user clicks on the item currently selected item, it will still invoke value change event as if 
        /// a new value was selected.
        /// </summary>
        public bool AllowReselectValue
        {
            get { return _selectList.AllowReselectValue; }
            set { _selectList.AllowReselectValue = value; }
        }

        /// <summary>
        /// Get the drop-down list component.
        /// </summary>
        public SelectList SelectList
        {
            get { return _selectList; }
        }

        /// <summary>
        /// Get the selected text panel paragraph (the text that's shown when DropDown is closed).
        /// </summary>
        public Paragraph SelectedTextPanelParagraph
        {
            get { return _selectedTextParagraph; }
        }

        /// <summary>
        /// Get the image entity of the arrow on the side of the Selected Text Panel.
        /// </summary>
        public Image ArrowDownImage
        {
            get
            {
                return _arrowDownImage;
            }
        }

        /// <summary>
        /// Default height, in pixels, of the dropdown selected text panel (the panel when its closed).
        /// This value will be used only if the stylesheet for DefaultClosePanelStyle don't set a size.Y property.
        /// </summary>
        public static int DefaultSelectedTextPanelHeight = 67;

        // closed panel height in pixels
        int _selectedPanelHeight;

        /// <summary>
        /// If true, will auto-set the internal list height based on number of options.
        /// </summary>
        public bool AutoSetListHeight = false;

        /// <summary>
        /// If set to true, whenever user select an item it will trigger event but jump back to placeholder value.
        /// </summary>
        public bool DontKeepSelection = false;

        /// <summary>
        /// Size of the arrow to show on the side of the Selected Text Panel.
        /// </summary>
        public static int ArrowSize = 30;

        /// <summary>Special callback to execute when list size changes.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public EventCallback OnListChange = null;

        /// <summary>
        /// Create the DropDown list.
        /// </summary>
        /// <param name="size">List size (refers to the whole size of the list + the header when dropdown list is opened).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">Panel skin to use for this DropDown list and header.</param>
        /// <param name="listSkin">An optional skin to use for the dropdown list only (if you want a different skin for the list).</param>
        /// <param name="showArrow">If true, will show an up/down arrow next to the dropdown text.</param>
        public DropDown(Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground, PanelSkin? listSkin = null, bool showArrow = true) :
            base(size, anchor, offset)
        {
            // default padding of self is 0
            Padding = Vector2.Zero;

            // to get collision right when list is opened
            UseActualSizeForCollision = true;

            // set dropdown closed panel height
            _selectedPanelHeight = (int)DefaultSelectedPanelStyle.GetStyleProperty("DefaultSize", EntityState.Default).asVector.Y;
            if (_selectedPanelHeight <= 1) { _selectedPanelHeight = DefaultSelectedTextPanelHeight; }

            if (!UserInterface.Active._isDeserializing)
            {
                // create the panel and paragraph used to show currently selected value (what's shown when drop-down is closed)
                _selectedTextPanel = new Panel(new Vector2(0, _selectedPanelHeight), skin, Anchor.TopLeft);
                _selectedTextPanel.UpdateStyle(DefaultSelectedPanelStyle);
                _selectedTextParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.CenterLeft);
                _selectedTextParagraph.UseActualSizeForCollision = false;
                _selectedTextParagraph.UpdateStyle(SelectList.DefaultParagraphStyle);
                _selectedTextParagraph.UpdateStyle(DefaultParagraphStyle);
                _selectedTextParagraph.UpdateStyle(DefaultSelectedParagraphStyle);
                _selectedTextParagraph.Identifier = "_selectedTextParagraph";
                _selectedTextPanel.AddChild(_selectedTextParagraph, true);
                _selectedTextPanel._hiddenInternalEntity = true;
                _selectedTextPanel.Identifier = "_selectedTextPanel";

                // create the arrow down icon
                _arrowDownImage = new Image(Resources.Instance.ArrowDown, new Vector2(ArrowSize, ArrowSize), ImageDrawMode.Stretch, Anchor.CenterRight, new Vector2(-10, 0));
                _selectedTextPanel.AddChild(_arrowDownImage, true);
                _arrowDownImage._hiddenInternalEntity = true;
                _arrowDownImage.Identifier = "_arrowDownImage";
                _arrowDownImage.Visible = showArrow;

                // create the list component
                _selectList = new SelectList(new Vector2(0f, size.Y), Anchor.TopCenter, Vector2.Zero, listSkin ?? skin);

                // update list offset and space before
                _selectList.Offset = new Vector2(0, _selectedPanelHeight);
                _selectList.SpaceBefore = Vector2.Zero;
                _selectList._hiddenInternalEntity = true;
                _selectList.Identifier = "_selectList";

                // add the header and select list as children
                AddChild(_selectedTextPanel);
                AddChild(_selectList);

                InitEvents();
            }
            // if during serialization create just a temp placeholder
            else
            {
                _selectList = new SelectList(new Vector2(0f, size.Y), Anchor.TopCenter, Vector2.Zero, listSkin ?? skin);
            }
        }

        /// <summary>
        /// Create default dropdown.
        /// </summary>
        public DropDown() : this(new Vector2(0, 200))
        {
        }

        /// <summary>
        /// Init event-related stuff after all sub-entities are created.
        /// </summary>
        private void InitEvents()
        {
            // add callback on list value change
            _selectList.OnValueChange = (Entity entity) =>
            {
                // hide list
                ListVisible = false;

                // set selected text
                _selectedTextParagraph.Text = (SelectedValue ?? DefaultText);
            };

            // on click, always hide the selectlist
            _selectList.OnClick = (Entity entity) =>
            {
                ListVisible = false;
            };

            // hide the list by default
            _selectList.Visible = false;

            // setup the callback to show / hide the list when clicking the dropbox
            _selectedTextPanel.OnClick = (Entity self) =>
            {
                // change visibility
                ListVisible = !ListVisible;
            };

            // set starting text
            _selectedTextParagraph.Text = (SelectedValue ?? DefaultText);

            // update styles
            _selectList.UpdateStyle(DefaultStyle);

            // make the list events trigger the dropdown events
            _selectList.PropagateEventsTo(this);

            // make the selected value panel trigger the dropdown events
            _selectedTextPanel.PropagateEventsTo(this);
        }

        /// <summary>
        /// Change the value of this entity, where there's value to change.
        /// </summary>
        /// <param name="newValue">New value to set.</param>
        /// <param name="emitEvent">If true and value changed, will emit 'ValueChanged' event.</param>
        override public void ChangeValue(object newValue, bool emitEvent)
        {
            SelectList.ChangeValue(newValue, emitEvent);
        }

        /// <summary>
        /// Get the value of this entity, where there's value.
        /// </summary>
        /// <returns>Value as object.</returns>
        override public object GetValue()
        {
            return SelectList.GetValue();
        }

        /// <summary>
        /// Special init after deserializing entity from file.
        /// </summary>
        internal protected override void InitAfterDeserialize()
        {
            base.InitAfterDeserialize();

            _selectedTextPanel = Find<Panel>("_selectedTextPanel");
            _selectedTextPanel._hiddenInternalEntity = true;

            _arrowDownImage = _selectedTextPanel.Find<Image>("_arrowDownImage");
            _arrowDownImage._hiddenInternalEntity = true;

            _selectList = Find<SelectList>("_selectList");
            _selectList._hiddenInternalEntity = true;

            _selectedTextParagraph = _selectedTextPanel.Find("_selectedTextParagraph") as Paragraph;

            InitEvents();
        }

        /// <summary>
        /// Set special callback to trigger if a specific value is selected.
        /// </summary>
        /// <param name="itemValue">Item text to trigger event.</param>
        /// <param name="action">Event to trigger.</param>
        public void OnSelectedSpecificItem(string itemValue, System.Action action)
        {
            _perItemCallbacks[itemValue] = action;
        }

        /// <summary>
        /// Clear all the per-item specific events.
        /// </summary>
        public void ClearSpecificItemEvents()
        {
            _perItemCallbacks.Clear();
        }

        /// <summary>
        /// Is the DropDown list currentle opened (visible).
        /// </summary>
        public bool ListVisible
        {
            // get if the list is visible
            get
            {
                return _selectList.Visible;
            }

            // show / hide the list
            set
            {
                // show / hide list
                _selectList.Visible = value;
                OnDropDownVisibilityChange();
            }
        }

        /// <summary>
        /// Return the actual dest rect for auto-anchoring purposes.
        /// This is useful for things like DropDown, that when opened they take a larger part of the screen, but we don't
        /// want it to push down other entities.
        /// </summary>
        override internal protected Rectangle GetDestRectForAutoAnchors()
        {
            _selectedTextPanel.UpdateDestinationRectsIfDirty();
            return _selectedTextPanel.GetActualDestRect();
        }

        /// <summary>
        /// Test if a given point is inside entity's boundaries.
        /// </summary>
        /// <remarks>This function result is affected by the 'UseActualSizeForCollision' flag.</remarks>
        /// <param name="point">Point to test.</param>
        /// <returns>True if point is in entity's boundaries (destination rectangle)</returns>
        override public bool IsTouching(Vector2 point)
        {
            // adjust scrolling
            point += _lastScrollVal.ToVector2();

            // get destination rect based on whether the dropdown is opened or closed
            Rectangle rect;

            // if list is currently visible, use the full size
            if (ListVisible)
            {
                _selectList.UpdateDestinationRectsIfDirty();
                rect = _selectList.GetActualDestRect();
                rect.Height += _selectedPanelHeight;
                rect.Y -= _selectedPanelHeight;
            }
            // if list is not currently visible, use the header size
            else
            {
                _selectedTextPanel.UpdateDestinationRectsIfDirty();
                rect = _selectedTextPanel.GetActualDestRect();
            }

            // now test detection
            return (point.X >= rect.Left && point.X <= rect.Right &&
                    point.Y >= rect.Top && point.Y <= rect.Bottom);
        }

        /// <summary>
        /// Set entity render and update priority.
        /// DropDown entity override this function to give some bonus priority, since when list is opened it needs to override entities
        /// under it, which usually have bigger index in container.
        /// </summary>
        override protected int Priority
        {
            get { return 100 - _indexInParent + PriorityBonus; }
        }

        /// <summary>
        /// Called whenever the dropdown list is shown / hidden.
        /// Note: called *after* _isListVisible is set.
        /// </summary>
        private void OnDropDownVisibilityChange()
        {
            // if during deserialize, skip
            if (UserInterface.Active._isDeserializing)
                return;

            // update arrow image
            _arrowDownImage.Texture = ListVisible ? Resources.Instance.ArrowUp : Resources.Instance.ArrowDown;

            // focus on selectlist
            _selectList.IsFocused = true;
            UserInterface.Active.ActiveEntity = _selectList;

            // update destination rectangles
            _selectList.UpdateDestinationRects();

            // if turned visible, scroll to selected
            if (_selectList.Visible) _selectList.ScrollToSelected();

            // mark self as dirty
            MarkAsDirty();

            // do auto-height
            if (AutoSetListHeight)
            {
                _selectList.MatchHeightToList();
            }
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            if (SelectedIndex == -1 && _selectedTextParagraph.Text != _placeholderText)
            {
                _selectedTextParagraph.Text = _placeholderText;
            }
        }

        /// <summary>
        /// Called every frame after update.
        /// DropDown entity override this function to close the list if necessary and to remove the selected item panel from self.
        /// </summary>
        override protected void DoAfterUpdate()
        {
            // if list currently visible we want to check if we need to close it
            if (ListVisible)
            {
                // check if mouse down and not inside list
                var mousePosition = GetMousePos();
                if (MouseInput.AnyMouseButtonPressed() && !IsTouching(mousePosition))
                {
                    ListVisible = false;
                }
            }

            // call base do-before-update
            base.DoAfterUpdate();

            // do we have a selected item?
            if (HasSelectedValue)
            {
                // trigger per-item events, but only if value changed
                if (SelectedIndex != _lastSelected)
                {
                    System.Action callback = null;
                    if (_perItemCallbacks.TryGetValue(_selectList.SelectedValue, out callback))
                    {
                        callback.Invoke();
                    }
                }

                // if set to not keep selected value, return to original placeholder
                if (DontKeepSelection && SelectedIndex != -1)
                {
                    Unselect();
                }
            }

            // store last known index
            _lastSelected = SelectedIndex;
        }

        /// <summary>
        /// Return if currently have a selected value.
        /// </summary>
        public bool HasSelectedValue
        {
            get { return SelectedIndex != -1; }
        }

        /// <summary>
        /// Currently selected item value (or null if none is selected).
        /// </summary>
        public string SelectedValue
        {
            get { return _selectList.SelectedValue; }
            set { _selectList.SelectedValue = value; }
        }

        /// <summary>
        /// Currently selected item index (or -1 if none is selected).
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectList.SelectedIndex; }
            set { _selectList.SelectedIndex = value; }
        }

        /// <summary>
        /// Current scrollbar position.
        /// </summary>
        public int ScrollPosition
        {
            get { return _selectList.ScrollPosition; }
            set { _selectList.ScrollPosition = value; }
        }

        /// <summary>
        /// Clear current selection.
        /// </summary>
        public void Unselect()
        {
            _selectList.Unselect();
        }

        /// <summary>
        /// Add value to list.
        /// </summary>
        /// <remarks>Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value (will always pick the first found).</remarks>
        /// <param name="value">Value to add.</param>
        public void AddItem(string value)
        {
            _selectList.AddItem(value);
        }

        /// <summary>
        /// Add value to list at a specific index.
        /// </summary>
        /// <remarks>Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value (will always pick the first found).</remarks>
        /// <param name="value">Value to add.</param>
        /// <param name="index">Index to insert the new item into.</param>
        public void AddItem(string value, int index)
        {
            _selectList.AddItem(value, index);
        }

        /// <summary>
        /// Remove value from the list.
        /// </summary>
        /// <param name="value">Value to remove.</param>
        public void RemoveItem(string value)
        {
            _selectList.RemoveItem(value);
        }

        /// <summary>
        /// Remove item from the list, by index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveItem(int index)
        {
            _selectList.RemoveItem(index);
        }


        /// <summary>
        /// Change an existing value in the dropdown list.
        /// </summary>
        /// <param name="index">Index to change.</param>
        /// <param name="newValue">New value to set.</param>
        public void ChangeItem(int index, string newValue)
        {
            // if there's no change, skip
            var oldValue = SelectList.Items[index];
            if (oldValue == newValue) { return; }

            // change and update selected
            _selectList.ChangeItem(index, newValue);
            if (_selectList.Items[SelectedIndex] != SelectedValue) { SelectedValue = newValue; }

            // update per-item callbacks
            if (_perItemCallbacks.ContainsKey(oldValue))
            {
                if (_perItemCallbacks.ContainsKey(newValue)) { throw new System.Exception($"Changed dropdown item '{oldValue}' to '{newValue}', but they both have unique per-item callback attached."); }
                _perItemCallbacks[newValue] = _perItemCallbacks[oldValue];
                _perItemCallbacks.Remove(oldValue);
            }
        }

        /// <summary>
        /// Change an existing value in the dropdown list.
        /// </summary>
        /// <param name="oldValue">Old value to change.</param>
        /// <param name="newValue">New value to set.</param>
        /// <param name="onlyFirst">If true, will stop after first value found.</param>
        public void ChangeItem(string oldValue, string newValue, bool onlyFirst = false)
        {
            // if there's no change, skip
            if (oldValue == newValue) { return; }

            // change and update selected
            _selectList.ChangeItem(oldValue, newValue, onlyFirst);
            if (_selectList.Items[SelectedIndex] != SelectedValue) { SelectedValue = newValue; }

            // update per-item callbacks
            if (_perItemCallbacks.ContainsKey(oldValue))
            {
                if (_perItemCallbacks.ContainsKey(newValue)) { throw new System.Exception($"Changed dropdown item '{oldValue}' to '{newValue}', but they both have unique per-item callback attached."); }
                _perItemCallbacks[newValue] = _perItemCallbacks[oldValue];
                _perItemCallbacks.Remove(oldValue);
            }
        }

        /// <summary>
        /// Remove all items from the list.
        /// </summary>
        public void ClearItems()
        {
            _selectList.ClearItems();
        }

        /// <summary>
        /// How many items currently in the list.
        /// </summary>
        public int Count
        {
            get { return _selectList.Count; }
        }

        /// <summary>
        /// Is the list currently empty.
        /// </summary>
        public bool Empty
        {
            get { return _selectList.Empty; }
        }

        /// <summary>
        /// Is the list a natrually-interactable entity.
        /// </summary>
        /// <returns>True.</returns>
        override internal protected bool IsNaturallyInteractable()
        {
            return true;
        }

        /// <summary>
        /// Move scrollbar to currently selected item.
        /// </summary>
        public void ScrollToSelected()
        {
            _selectList.ScrollToSelected();
        }

        /// <summary>
        /// Move scrollbar to last item in list.
        /// </summary>
        public void scrollToEnd()
        {
            _selectList.scrollToEnd();
        }
    }
}
