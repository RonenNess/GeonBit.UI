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

namespace GeonBit.UI.Entities
{

    /// <summary>
    /// DropDown is just like a list, but it only shows the currently selected value unless clicked on (the list is
    /// only revealed while interacted with).
    /// </summary>
    public class DropDown : Entity
    {
        /// <summary>Default text to show when no value is selected from the list.</summary>
        public string DefaultText = "Click to Select";

        /// <summary>Default style for the dropdown itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for dropdown labels. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default styling for the dropdown currently-selected label. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultSelectedParagraphStyle = new StyleSheet();

        /// <summary>Default select list size in pixels.</summary>
        new public static Vector2 DefaultSize = new Vector2(0f, 220f);

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
        /// Default height, in pixels, of the selected text panel.
        /// </summary>
        public static int SelectedPanelHeight = 67;

        /// <summary>
        /// Size of the arrow to show on the side of the Selected Text Panel.
        /// </summary>
        public static int ArrowSize = 30;

        /// <summary>Special callback to execute when list size changes.</summary>
        public EventCallback OnListChange = null;

        /// <summary>
        /// Create the DropDown list.
        /// </summary>
        /// <param name="size">List size (refers to the whole size of the list + the header when dropdown list is opened).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">Panel skin to use for this DropDown list and header.</param>
        public DropDown(Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground) :
            base(size, anchor, offset)
        {
            // default padding of self is 0
            Padding = Vector2.Zero;

            // to get collision right when list is opened
            UseActualSizeForCollision = true;

            // create the panel and paragraph used to show currently selected value (what's shown when drop-down is closed)
            _selectedTextPanel = new Panel(new Vector2(0, SelectedPanelHeight), skin, Anchor.TopLeft);
            _selectedTextParagraph = new Paragraph("", Anchor.CenterLeft);
            _selectedTextParagraph.UseActualSizeForCollision = false;
            _selectedTextParagraph.UpdateStyle(SelectList.DefaultParagraphStyle);
            _selectedTextParagraph.UpdateStyle(DefaultParagraphStyle);
            _selectedTextParagraph.UpdateStyle(DefaultSelectedParagraphStyle);
            _selectedTextPanel.AddChild(_selectedTextParagraph, true);

            // create the arrow down icon
            _arrowDownImage = new Image(Resources.ArrowDown, new Vector2(ArrowSize, ArrowSize), ImageDrawMode.Stretch, Anchor.CenterRight, new Vector2(-10, 0));
            _selectedTextPanel.AddChild(_arrowDownImage, true);

            // create the list component
            _selectList = new SelectList(size, Anchor.TopCenter, Vector2.Zero, skin);

            // update list offset and space before
            _selectList.SetOffset(new Vector2(0, SelectedPanelHeight));
            _selectList.SpaceBefore = Vector2.Zero;

            // add the header and select list as children
            AddChild(_selectedTextPanel);
            AddChild(_selectList);

            // add callback on list value change
            _selectList.OnValueChange = (Entity entity) =>
            {
                // hide list
                ListVisible = false;

                // set selected text
                _selectedTextParagraph.Text = (SelectedValue ?? DefaultText);
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
            _selectList.OnListChange += (Entity entity) => { OnListChange?.Invoke(this); };
            _selectList.OnMouseDown += (Entity entity) => { OnMouseDown?.Invoke(this); };
            _selectList.OnMouseReleased += (Entity entity) => { OnMouseReleased?.Invoke(this); };
            _selectList.WhileMouseDown += (Entity entity) => { WhileMouseDown?.Invoke(this); };
            _selectList.WhileMouseHover += (Entity entity) => { WhileMouseHover?.Invoke(this); };
            _selectList.OnClick += (Entity entity) => { OnClick?.Invoke(this); };
            _selectList.OnValueChange += (Entity entity) => { OnValueChange?.Invoke(this); };
            _selectList.OnMouseEnter += (Entity entity) => { OnMouseEnter?.Invoke(this); };
            _selectList.OnMouseLeave += (Entity entity) => { OnMouseLeave?.Invoke(this); };
            _selectList.OnMouseWheelScroll += (Entity entity) => { OnMouseWheelScroll?.Invoke(this); };
            _selectList.OnStartDrag += (Entity entity) => { OnStartDrag?.Invoke(this); };
            _selectList.OnStopDrag += (Entity entity) => { OnStopDrag?.Invoke(this); };
            _selectList.WhileDragging += (Entity entity) => { WhileDragging?.Invoke(this); };
            _selectList.BeforeDraw += (Entity entity) => { BeforeDraw?.Invoke(this); };
            _selectList.AfterDraw += (Entity entity) => { AfterDraw?.Invoke(this); };
            _selectList.BeforeUpdate += (Entity entity) => { BeforeUpdate?.Invoke(this); };
            _selectList.AfterUpdate += (Entity entity) => { AfterUpdate?.Invoke(this); };

            // make the selected value panel trigger the dropdown events
            _selectedTextPanel.OnMouseDown += (Entity entity) => { OnMouseDown?.Invoke(this); };
            _selectedTextPanel.OnMouseReleased += (Entity entity) => { OnMouseReleased?.Invoke(this); };
            _selectedTextPanel.WhileMouseDown += (Entity entity) => { WhileMouseDown?.Invoke(this); };
            _selectedTextPanel.WhileMouseHover += (Entity entity) => { WhileMouseHover?.Invoke(this); };
            _selectedTextPanel.OnClick += (Entity entity) => { OnClick?.Invoke(this); };
            _selectedTextPanel.OnValueChange += (Entity entity) => { OnValueChange?.Invoke(this); };
            _selectedTextPanel.OnMouseEnter += (Entity entity) => { OnMouseEnter?.Invoke(this); };
            _selectedTextPanel.OnMouseLeave += (Entity entity) => { OnMouseLeave?.Invoke(this); };
            _selectedTextPanel.OnMouseWheelScroll += (Entity entity) => { OnMouseWheelScroll?.Invoke(this); };
            _selectedTextPanel.OnStartDrag += (Entity entity) => { OnStartDrag?.Invoke(this); };
            _selectedTextPanel.OnStopDrag += (Entity entity) => { OnStopDrag?.Invoke(this); };
            _selectedTextPanel.WhileDragging += (Entity entity) => { WhileDragging?.Invoke(this); };
            _selectedTextPanel.BeforeDraw += (Entity entity) => { BeforeDraw?.Invoke(this); };
            _selectedTextPanel.AfterDraw += (Entity entity) => { AfterDraw?.Invoke(this); };
            _selectedTextPanel.BeforeUpdate += (Entity entity) => { BeforeUpdate?.Invoke(this); };
            _selectedTextPanel.AfterUpdate += (Entity entity) => { AfterUpdate?.Invoke(this); };

        }

        /// <summary>
        /// Is the DropDown list currentle opened (visible).
        /// </summary>
        public bool ListVisible
        {

            // get if the list is visible
            get {return _selectList.Visible;}

            // show / hide the list
            set
            {
                // show / hide list
                _selectList.Visible = value;
                OnDropDownVisibilityChange();
            }
        }

        /// <summary>
        /// Handle when current value changes. DropDown entity override this to turn the list invisible.
        /// </summary>
        override protected void DoOnValueChange()
        {
            ListVisible = false;
            base.DoOnValueChange();
        }

        /// <summary>
        /// Get actual destination rect for positioning, which is the size of the DropDown entity when list is closed.
        /// </summary>
        /// <returns>Actual destination rect when DropDown list is closed.</returns>
        override public Rectangle GetActualDestRect()
        {
            // if list is currently visible, return the full size
            if (ListVisible)
            {
                _selectList.UpdateDestinationRectsIfDirty();
                Rectangle ret = _selectList.GetActualDestRect();
                ret.Height += SelectedPanelHeight;
                ret.Y -= SelectedPanelHeight;
                return ret;
            }
            // if list is not currently visible, return the header size
            else
            {
                _selectedTextPanel.UpdateDestinationRectsIfDirty();
                return _selectedTextPanel.GetActualDestRect();
            }
        }

        /// <summary>
        /// Set entity render and update priority.
        /// DropDown entity override this function to give some bonus priority, since when list is opened it needs to override entities
        /// under it, which usually have bigger index in container.
        /// </summary>
        override public int Priority
        {
            get { return 100 - _indexInParent; }
        }

        /// <summary>
        /// Called whenever the dropdown list is shown / hidden.
        /// Note: called *after* _isListVisible is set.
        /// </summary>
        private void OnDropDownVisibilityChange()
        {
            // update arrow image
            _arrowDownImage.Texture = ListVisible ? Resources.ArrowUp : Resources.ArrowDown;

            // focus on selectlist
            _selectList.IsFocused = true;
            UserInterface.ActiveEntity = _selectList;

            // update destination rectangles
            _selectList.UpdateDestinationRects();

            // scroll to selected
            _selectList.ScrollToSelected();

            // mark self as dirty
            MarkAsDirty();
        }        

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Called every frame after update.
        /// DropDown entity override this function to close the list if necessary and to remove the selected item panel from self.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoAfterUpdate(InputHelper input)
        {
            // if list currently visible we want to check if we need to close it
            if (ListVisible)
            {
                // check if mouse down and not inside list
                if (input.AnyMouseButtonDown() && !IsInsideEntity(input.MousePosition))
                {
                    if (!IsInsideEntity(input.MousePosition))
                    ListVisible = false;
                }
            }

            // call base do-before-update
            base.DoAfterUpdate(input);
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
        override public bool IsNaturallyInteractable()
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
