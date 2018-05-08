#region File Description
//-----------------------------------------------------------------------------
// SelectLists are lists of string values the user can pick from.
// For example, SelectList might be used to pick character class, skill, etc.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GeonBit.UI.DataTypes;

namespace GeonBit.UI.Entities
{
    // data we attach to paragraphs that are part of this selection list
    struct ParagraphData
    {
        public SelectList list;
        public int relativeIndex;
        public ParagraphData(SelectList _list, int _relativeIndex)
        {
            list = _list;
            relativeIndex = _relativeIndex;
        }
    }

    /// <summary>
    /// List of items (strings) user can scroll and pick from.
    /// </summary>
    public class SelectList : Panel
    {
        // current selected value and index
        string _value = null;
        int _index = -1;

        // store list last known internal size, so we'll know if size changed and we need to re-create the list
        Point _prevSize = Point.Zero;

        // list of paragraphs used to show the values
        List<Paragraph> _paragraphs = new List<Paragraph>();

        // scrollbar to scroll through the list
        VerticalScrollbar _scrollbar;

        // indicate that we had a resize event while not being visible
        bool _hadResizeWhileNotVisible = false;

        /// <summary>Extra space (in pixels) between items on Y axis.</summary>
        public int ExtraSpaceBetweenLines = 0;

        /// <summary>Scale items in list.</summary>
        public float ItemsScale = 1f;

        /// <summary>Special callback to execute when list size changes.</summary>
        public EventCallback OnListChange = null;

        /// <summary>
        /// If true and an item in the list is too long for its width, the list will cut its value to fit width.
        /// </summary>
        public bool ClipTextIfOverflow = true;

        /// <summary>
        /// String to append when clipping items width.
        /// </summary>
        public string AddWhenClipping = "..";

        /// <summary>When set to true, users cannot change the currently selected value.
        /// Note: unlike the basic entity "Locked" that prevent all input from entity and its children,
        /// this method of locking will still allow users to scroll through the list, thus making it useable
        /// as a read-only list entity.</summary>
        public bool LockSelection = false;

        /// <summary>Default styling for select list labels. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default styling for the select list itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>
        /// Optional dictionary of list indexes you want to lock.
        /// Every item in this dictionary set to true will be locked and user won't be able to select it.
        /// </summary>
        public Dictionary<int, bool> LockedItems = new Dictionary<int, bool>();

        // list of values
        List<string> _list = new List<string>();

        /// <summary>
        /// If true and user clicks on the item currently selected item, it will still invoke value change event as if 
        /// a new value was selected.
        /// </summary>
        public bool AllowReselectValue = false;

        /// <summary>
        /// If provided, will not be able to add any more of this number of items.
        /// </summary>
        public int MaxItems = 0;

        /// <summary>Default select list size in pixels.</summary>
        new public static Vector2 DefaultSize = new Vector2(0f, 220f);

        /// <summary>
        /// Create the select list.
        /// </summary>
        /// <param name="size">List size.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">SelectList skin, eg which texture to use.</param>
        public SelectList(Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground) :
            base(size, skin, anchor, offset)
        {
            // update style and set default padding
            UpdateStyle(DefaultStyle);

            // create the scrollbar
            _scrollbar = new VerticalScrollbar(0, 10, Anchor.CenterRight, offset: new Vector2(-8, 0));
            _scrollbar.Value = 0;
            _scrollbar.Visible = false;
            _scrollbar._hiddenInternalEntity = true;
            AddChild(_scrollbar, false);
        }

        /// <summary>
        /// Create the select list with default values.
        /// </summary>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public SelectList(Anchor anchor = Anchor.Auto, Vector2? offset = null) :
           this(USE_DEFAULT_SIZE, anchor, offset)
        {
        }

        /// <summary>
        /// Called every time the list itself changes (items added / removed).
        /// </summary>
        protected void OnListChanged()
        {
            // if already placed in a parent, call the resize event to recalculate scollbar and labels
            if (_parent != null)
            {
                OnResize();
            }

            // make sure selected index is valid
            if (SelectedIndex >= _list.Count)
            {
                Unselect();
            }

            // invoke list change callback
            OnListChange?.Invoke(this);
        }

        /// <summary>
        /// Add value to list.
        /// </summary>
        /// <remarks>Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value (will always pick the first found).</remarks>
        /// <param name="value">Value to add.</param>
        public void AddItem(string value)
        {
            if (MaxItems != 0 && Count >= MaxItems) { return; }
            _list.Add(value);
            OnListChanged();
        }

        /// <summary>
        /// Add value to list at a specific index.
        /// </summary>
        /// <remarks>Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value (will always pick the first found).</remarks>
        /// <param name="value">Value to add.</param>
        /// <param name="index">Index to insert the new item into.</param>
        public void AddItem(string value, int index)
        {
            if (MaxItems != 0 && Count >= MaxItems) { return; }
            _list.Insert(index, value);
            OnListChanged();
        }
        
        /// <summary>
        /// Remove value from the list.
        /// </summary>
        /// <param name="value">Value to remove.</param>
        public void RemoveItem(string value)
        {
            _list.Remove(value);
            OnListChanged();
        }

        /// <summary>
        /// Remove item from the list, by index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveItem(int index)
        {
            _list.RemoveAt(index);
            OnListChanged();
        }

        /// <summary>
        /// Remove all items from the list.
        /// </summary>
        public void ClearItems()
        {
            _list.Clear();
            OnListChanged();
        }

        /// <summary>
        /// How many items currently in the list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Is the list currently empty.
        /// </summary>
        public bool Empty
        {
            get { return _list.Count == 0; }
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
        /// Calculate the height of the select list to match the height of all the items in it.
        /// </summary>
        public void MatchHeightToList()
        {
            if (_list.Count == 0) return;
            if (_paragraphs.Count == 0) OnResize();
            var height = _list.Count * (_paragraphs[0].GetCharacterActualSize().Y / GlobalScale + _paragraphs[0].SpaceAfter.Y) + Padding.Y * 2;
            Size = new Vector2(Size.X, height);
        }

        /// <summary>
        /// Move scrollbar to currently selected item.
        /// </summary>
        public void ScrollToSelected()
        {
            if (_scrollbar != null && _scrollbar.Visible)
            {
                _scrollbar.Value = SelectedIndex;
            }
        }

        /// <summary>
        /// Move scrollbar to last item in list.
        /// </summary>
        public void scrollToEnd()
        {
            if (_scrollbar != null && _scrollbar.Visible)
            {
                _scrollbar.Value = _list.Count;
            }
        }

        /// <summary>
        /// Called for every new paragraph entity created as part of the list, to allow children classes
        /// to add extra processing etc to list labels.
        /// </summary>
        /// <param name="paragraph">The newly created paragraph once ready (after added to list container).</param>
        protected virtual void OnCreatedListParagraph(Paragraph paragraph)
        {
        }

        /// <summary>
        /// Called every frame before drawing is done.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        override protected void OnBeforeDraw(SpriteBatch spriteBatch)
        {
            base.OnBeforeDraw(spriteBatch);
            if (_hadResizeWhileNotVisible)
            {
                OnResize();
            }
        }

        /// <summary>
        /// When list is resized (also called on init), create the labels to show item values and init graphical stuff.
        /// </summary>
        protected virtual void OnResize()
        {
            // if not visible, skip
            if (!IsVisible())
            {
                _hadResizeWhileNotVisible = true;
                return;
            }

            // clear the _hadResizeWhileNotVisible flag
            _hadResizeWhileNotVisible = false;

            // remove all children before re-creating them
            ClearChildren();

            // remove previous paragraphs list
            _paragraphs.Clear();

            // make sure destination rect is up-to-date
            UpdateDestinationRects();

            // calculate paragraphs quantity
            int i = 0;
            while (true)
            {
                // create and add new paragraph
                Paragraph paragraph = UserInterface.DefaultParagraph(".", Anchor.Auto);
                paragraph.PromiscuousClicksMode = true;
                paragraph.WrapWords = false;
                paragraph.UpdateStyle(DefaultParagraphStyle);
                paragraph.Scale = paragraph.Scale * ItemsScale;
                paragraph.SpaceAfter = paragraph.SpaceAfter + new Vector2(0, ExtraSpaceBetweenLines - 2);
                paragraph.ExtraMargin.Y = ExtraSpaceBetweenLines / 2 + 3;
                paragraph.AttachedData = new ParagraphData(this, i++);
                paragraph.UseActualSizeForCollision = false;
                paragraph.Size = new Vector2(0, paragraph.GetCharacterActualSize().Y + ExtraSpaceBetweenLines);
                paragraph.BackgroundColorPadding = new Point((int)Padding.X, 5);
                paragraph.BackgroundColorUseBoxSize = true;
                paragraph._hiddenInternalEntity = true;
                paragraph.PropagateEventsTo(this);
                AddChild(paragraph);

                // call the callback whenever a new paragraph is created
                OnCreatedListParagraph(paragraph);

                // add to paragraphs list
                _paragraphs.Add(paragraph);

                // add callback to selection
                paragraph.OnClick += (Entity entity) =>
                {
                    ParagraphData data = (ParagraphData)entity.AttachedData;
                    if (!data.list.LockSelection)
                    {
                        data.list.Select(data.relativeIndex, true);
                    }
                };

                // to calculate paragraph actual bottom
                paragraph.UpdateDestinationRects();

                // if out of list bounderies remove this paragraph and stop
                if ((paragraph.GetActualDestRect().Bottom > _destRect.Bottom - _scaledPadding.Y) || i > _list.Count)
                {
                    RemoveChild(paragraph);
                    _paragraphs.Remove(paragraph);
                    break;
                }
            }

            // add scrollbar last, but only if needed
            if (_paragraphs.Count > 0 && _paragraphs.Count < _list.Count)
            {
                // add scrollbar to list
                AddChild(_scrollbar, false);

                // calc max scroll value
                _scrollbar.Max = (uint)(_list.Count - _paragraphs.Count);
                if (_scrollbar.Max < 2) { _scrollbar.Max = 2; }
                _scrollbar.StepsCount = _scrollbar.Max;
                _scrollbar.Visible = true;
            } 
            // if no scrollbar is needed, hide it
            else
            {
                _scrollbar.Visible = false;
                if (_scrollbar.Value > 0) { _scrollbar.Value = 0; }
            }
        }

        /// <summary>
        /// Propagate all events trigger by this entity to a given other entity.
        /// For example, if "OnClick" will be called on this entity, it will trigger OnClick on 'other' as well.
        /// </summary>
        /// <param name="other">Entity to propagate events to.</param>
        public void PropagateEventsTo(SelectList other)
        {
            PropagateEventsTo((Entity)other);
            OnListChange += (Entity entity) => { other.OnListChange?.Invoke(other); };
        }

        /// <summary>
        /// Propagate all events trigger by this entity to a given other entity.
        /// For example, if "OnClick" will be called on this entity, it will trigger OnClick on 'other' as well.
        /// </summary>
        /// <param name="other">Entity to propagate events to.</param>
        public void PropagateEventsTo(DropDown other)
        {
            PropagateEventsTo((Entity)other);
            OnListChange += (Entity entity) => { other.OnListChange?.Invoke(other); };
        }

        /// <summary>
        /// Currently selected item value (or null if none is selected).
        /// </summary>
        public string SelectedValue
        {
            get { return _value; }
            set { Select(value); }
        }

        /// <summary>
        /// Currently selected item index (or -1 if none is selected).
        /// </summary>
        public int SelectedIndex
        {
            get { return _index; }
            set { Select(value); }
        }

        /// <summary>
        /// Current scrollbar position.
        /// </summary>
        public int ScrollPosition
        {
            get { return _scrollbar.Value; }
            set { _scrollbar.Value = value; }
        }
        
        /// <summary>
        /// Clear current selection.
        /// </summary>
        public void Unselect()
        {
            Select(-1, false);
        }
        
        /// <summary>
        /// Select list item by value.
        /// </summary>
        /// <param name="value">Item value to select.</param>
        protected void Select(string value)
        {
            // value not changed? skip
            if (!AllowReselectValue && value == _value) { return; }

            // special case - value is null
            if (value == null)
            {
                _value = value;
                _index = -1;
                DoOnValueChange();
                return;
            }

            // find index in list
            _index = _list.IndexOf(value);
            if (_index == -1)
            {
                _value = null;
                if (UserInterface.Active.SilentSoftErrors) return;
                throw new Exceptions.NotFoundException("Value to set not found in list!");
            }

            // set value
            _value = value;

            // call on-value-change event
            DoOnValueChange();
        }

        /// <summary>
        /// Select list item by index.
        /// </summary>
        /// <param name="index">Item index to select.</param>
        /// <param name="relativeToScrollbar">If true, index will be relative to scrollbar current position.</param>
        protected void Select(int index, bool relativeToScrollbar = false)
        {
            // if relative to current scrollbar update index
            if (relativeToScrollbar)
            {
                index += _scrollbar.Value;
            }

            // index not changed? skip
            if (!AllowReselectValue && index == _index) { return; }

            // make sure legal index
            if (index >= -1 && index >= _list.Count)
            {
                if (UserInterface.Active.SilentSoftErrors) return;
                throw new Exceptions.NotFoundException("Invalid list index to select!");
            }

            // pick based on index
            _value = index > -1 ? _list[index] : null;
            _index = index;

            // call on-value-change event
            DoOnValueChange();
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // if size changed, update paragraphs list
            if ((_prevSize.Y != _destRectInternal.Size.Y) || _hadResizeWhileNotVisible)
            {
                OnResize();
            }

            // store last known size
            _prevSize = _destRectInternal.Size;

            // call base draw function to draw the panel part
            base.DrawEntity(spriteBatch, phase);

            // update paragraphs list values
            for (int i = 0; i < _paragraphs.Count; ++i)
            {
                // get item index
                int item_index = i + (int)_scrollbar.Value;

                // get current paragraph
                var par = _paragraphs[i];

                // if we got an item to show for this paragraph index:
                if (item_index < _list.Count)
                {
                    // set paragraph text, make visible, and remove background.
                    par.Text = _list[item_index];
                    par.BackgroundColor.A = 0;
                    par.Visible = true;

                    // check if we need to trim size
                    if (ClipTextIfOverflow)
                    {
                        // get width we need to clip and if we need to clip at all
                        var charWidth = par.GetCharacterActualSize().X;
                        var toClip = (charWidth * par.Text.Length) - _destRectInternal.Width;
                        if (toClip > 0)
                        {
                            // calc how many chars we need to remove
                            var charsToClip = (int)System.Math.Ceiling(toClip / charWidth) + AddWhenClipping.Length + 1;

                            // remove them from text
                            if (charsToClip < par.Text.Length)
                            {
                                par.Text = par.Text.Substring(0, par.Text.Length - charsToClip) + AddWhenClipping;
                            }
                            else
                            {
                                par.Text = AddWhenClipping;
                            }
                        }
                    }

                    // set locked state
                    bool isLocked = false;
                    LockedItems.TryGetValue(item_index, out isLocked);
                    par.Locked = isLocked;
                }
                // if paragraph out of range (eg more paragraphs than list items), make this paragraph invisible.
                else
                {
                    par.Visible = false;
                    par.Text = string.Empty;
                }
            }

            // highlight the currently selected item paragraph (eg the paragraph that represent the selected value, if currently visible)
            int selectedParagraphIndex = _index;
            if (selectedParagraphIndex != -1)
            {
                int i = selectedParagraphIndex - _scrollbar.Value;
                if (i >= 0 && i < _paragraphs.Count)
                {
                    // add background to selected paragraph
                    Paragraph paragraph = _paragraphs[i];
                    Rectangle destRect = paragraph.GetActualDestRect();
                    paragraph.State = EntityState.MouseDown;
                    paragraph.BackgroundColor = GetActiveStyle("SelectedHighlightColor").asColor;
                }
            }
        }
    }
}
