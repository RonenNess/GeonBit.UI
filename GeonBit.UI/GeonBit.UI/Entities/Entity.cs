#region File Description
//-----------------------------------------------------------------------------
// Base UI entity. Every widget inherit from this class.
// The base entity implement the following key functionality:
// 1. Drawing basic stuff like tiled textures with frames etc.
// 2. Positioning and calculating destination rect.
// 3. Basic events.
// 4. Visibility / Disabled / Locked modes.
// 5. Managing child entities.
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
    /// GeonBit.UI.Entities contains all the UI elements you can create and use in your layouts.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// An Anchor is a pre-defined position in parent entity that we use to position a child.
    /// For eample, we can use anchors to position an entity at the bottom-center point of its parent.
    /// Note: anchor affect both the position relative to parent and also the offset origin point of the entity.
    /// </summary>
    public enum Anchor
    {
        /// <summary>Center of parent element.</summary>
        Center,

        /// <summary>Top-Left corner of parent element.</summary>
        TopLeft,

        /// <summary>Top-Right corner of parent element.</summary>
        TopRight,

        /// <summary>Top-Center of parent element.</summary>
        TopCenter,

        /// <summary>Bottom-Left corner of parent element.</summary>
        BottomLeft,

        /// <summary>Bottom-Right corner of parent element.</summary>
        BottomRight,

        /// <summary>Bottom-Center of parent element.</summary>
        BottomCenter,

        /// <summary>Center-Left of parent element.</summary>
        CenterLeft,

        /// <summary>Center-Right of parent element.</summary>
        CenterRight,

        /// <summary>Position of the older sibling bottom, eg align this entity based on its older sibling.
        /// Use this property to place entities one after another.</summary>
        Auto,

        /// <summary>Position of the older sibling right side, or below it if not enough room in parent.
        /// In other words, this will try to put together entities on the same row until overflow parent width, in which case will
        /// go row down. Use this property to place entities one after another in the same row.</summary>
        AutoInline,

        /// <summary>Position of the older sibling bottom, eg align this entity based on its older sibling, but center on X axis.
        /// Use this property to place entities one after another but keep them aligned to center (especially paragraphs).</summary>
        AutoCenter,
    };

    /// <summary>
    /// Possible entity states and interactions with user.
    /// </summary>
    public enum EntityState
    {
        /// <summary>Default state, eg currently not interacting.</summary>
        Default = 0,

        /// <summary>Mouse is hovering over this entity.</summary>
        MouseHover = 1,

        /// <summary>Mouse button is pressed down over this entity.</summary>
        MouseDown = 2,
    };

    /// <summary>
    /// A callback function you can on entity events, like on-click, on-mouse-leave, etc.
    /// </summary>
    /// <param name="entity">The entity instance the event came from.</param>
    public delegate void EventCallback(Entity entity);

    /// <summary>
    /// Basic UI entity.
    /// All entities inherit from this class and share this API.
    /// </summary>
    public class Entity
    {
        // list of child elements
        private List<Entity> _children = new List<Entity>();

        /// <summary>
        /// A special size used value to use when you want to get the entity default size.
        /// </summary>
        public static Vector2 DEFAULT_SIZE = new Vector2(-1, -1);

        /// <summary>The direct parent of this entity.</summary>
        protected Entity _parent = null;

        /// <summary>Index inside parent.</summary>
        protected int _indexInParent;

        /// <summary>Is the entity currently interactable.</summary>
        protected bool _isInteractable = false;

        /// <summary>Is the entity currently visible.</summary>
        public bool Visible = true;

        /// <summary>
        /// If this set to true, this entity will still react to events if its direct parent is locked.
        /// This setting is mostly for scrollbars etc, that even if parent is locked should still be scrollable.
        /// </summary>
        protected bool DoEventsIfDirectParentIsLocked = false;

        /// <summary>
        /// If true, this entity will always inherit its parent state.
        /// This is useful for stuff like a paragraph that's attached to a button etc.
        /// NOTE!!! entities that inherit parent state will not trigger any events either.
        /// </summary>
        protected bool InheritParentState = false;

        // optional background object for this entity.
        // the background will be rendered on the full size of this entity, behind it, and will not respond to events etc.
        private Entity _background = null;

        // entity current style properties
        private StyleSheet _style = new StyleSheet();

        /// <summary>Optional data you can attach to this entity and retrieve later (for example when handling events).</summary>
        public object AttachedData = null;

        /// <summary>
        /// If true (default), will use the actual object size for collision detection. If false, will use the size property.
        /// This is useful for paragraphs, for example, where the actual width is based on text content and can vary and be totally
        /// different than the size set in the constructor.
        /// </summary>
        public bool UseActualSizeForCollision = true;

        /// <summary>Entity size (in pixels). Value of 0 will take parent's full size.</summary>
        protected Vector2 _size;

        /// <summary>Offset, in pixels, from the anchor position.</summary>
        protected Vector2 _offset;

        /// <summary>Anchor to position this entity based on (see Anchor enum for more info).</summary>
        protected Anchor _anchor;

        /// <summary>Basic default style that all entities share. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Callback to execute when mouse button is pressed over this entity (called once when button is pressed).</summary>
        public EventCallback OnMouseDown = null;

        /// <summary>Callback to execute when mouse button is released over this entity (called once when button is released).</summary>
        public EventCallback OnMouseReleased = null;

        /// <summary>Callback to execute every frame while mouse button is pressed over the entity.</summary>
        public EventCallback WhileMouseDown = null;

        /// <summary>Callback to execute every frame while mouse is hovering over the entity.</summary>
        public EventCallback WhileMouseHover = null;

        /// <summary>Callback to execute when user clicks on this entity (eg release mouse over it).</summary>
        public EventCallback OnClick = null;

        /// <summary>Callback to execute when entity value changes (relevant only for entities with value).</summary>
        public EventCallback OnValueChange = null;

        /// <summary>Callback to execute when mouse start hovering over this entity (eg enters its region).</summary>
        public EventCallback OnMouseEnter = null;

        /// <summary>Callback to execute when mouse stop hovering over this entity (eg leaves its region).</summary>
        public EventCallback OnMouseLeave = null;

        /// <summary>Callback to execute when mouse wheel scrolls and this entity is the active entity.</summary>
        public EventCallback OnMouseWheelScroll = null;

        /// <summary>Called when entity starts getting dragged (only if draggable).</summary>
        public EventCallback OnStartDrag = null;

        /// <summary>Called when entity stop getting dragged (only if draggable).</summary>
        public EventCallback OnStopDrag = null;

        /// <summary>Called every frame while the entity is being dragged.</summary>
        public EventCallback WhileDragging = null;

        /// <summary>Callback to execute every frame before this entity is rendered.</summary>
        public EventCallback BeforeDraw = null;

        /// <summary>Callback to execute every frame after this entity is rendered.</summary>
        public EventCallback AfterDraw = null;

        /// <summary>Callback to execute every frame before this entity updates.</summary>
        public EventCallback BeforeUpdate = null;

        /// <summary>Callback to execute every frame after this entity updates.</summary>
        public EventCallback AfterUpdate = null;

        /// <summary>Is mouse currently pointing on this entity.</summary>
        protected bool _isMouseOver = false;

        /// <summary>If true, this entity and its children will be drawn in greyscale effect and will not respond to events.</summary>
        public bool Disabled = false;

        /// <summary>If true, this entity and its children will not respond to events (but will be drawn normally, unlike when disabled).</summary>
        public bool Locked = false;

        /// <summary>Current entity state.</summary>
        protected EntityState _entityState = EntityState.Default;

        /// <summary>Does this entity or one of its children currently focused?</summary>
        protected bool IsFocused = false;

        /// <summary>Currently calculated destination rect (eg the region this entity is drawn on).</summary>
        protected Rectangle _destRect;

        /// <summary>Currently calculated internal destination rect (eg the region this entity children are positioned in).</summary>
        protected Rectangle _destRectInternal;

        // is this entity draggable?
        private bool _draggable = false;

        // do we need to init drag offset from current position?
        private bool _needToSetDragOffset = false;

        // current dragging offset.
        private Vector2 _dragOffset = Vector2.Zero;

        // true if this entity is currently being dragged.
        private bool _isBeingDragged = false;

        /// <summary>Default size this entity will have when no size is provided or when -1 is set for either width or height.</summary>
        virtual public Vector2 DefaultSize { get { return Vector2.Zero; } }

        /// <summary>If true, users will not be able to drag this entity outside its parent boundaries.</summary>
        public bool LimitDraggingToParentBoundaries = true;

        /// <summary>
        /// Create the entity.
        /// </summary>
        /// <param name="size">Entity size, in pixels.</param>
        /// <param name="anchor">Poisition anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public Entity(Vector2? size = null, Anchor anchor = Anchor.Auto, Vector2? offset = null)
        {
            // store size, anchor and offset
            _size = size ?? DefaultSize;
            _offset = offset ?? Vector2.Zero;
            _anchor = anchor;

            // set basic default style
            UpdateStyle(DefaultStyle);

            // check default size on specific axises
            if (_size.X == -1) { _size.X = DefaultSize.X; }
            if (_size.Y == -1) { _size.Y = DefaultSize.Y; }
        }

        /// <summary>
        /// Return stylesheet property for a given state.
        /// </summary>
        /// <param name="property">Property identifier.</param>
        /// <param name="state">State to get property for (if undefined will fallback to default state).</param>
        /// <param name="fallbackToDefault">If true and property not found for given state, will fallback to default state.</param>
        /// <returns>Style property value for given state or default, or null if undefined.</returns>
        public StyleProperty GetStyleProperty(string property, EntityState state = EntityState.Default, bool fallbackToDefault = true)
        {
            return _style.GetStyleProperty(property, state, fallbackToDefault);
        }

        /// <summary>
        /// Set a stylesheet property.
        /// </summary>
        /// <param name="property">Property identifier.</param>
        /// <param name="value">Property value.</param>
        /// <param name="state">State to set property for.</param>
        public void SetStyleProperty(string property, StyleProperty value, EntityState state = EntityState.Default)
        {
            _style.SetStyleProperty(property, value, state);
        }

        /// <summary>
        /// Return stylesheet property for current entity state (or default if undefined for state).
        /// </summary>
        /// <param name="property">Property identifier.</param>
        /// <returns>Stylesheet property value for current entity state, or default if not defined.</returns>
        public StyleProperty GetActiveStyle(string property)
        {
            return GetStyleProperty(property, _entityState);
        }

        /// <summary>
        /// Update the entire stylesheet from a different stylesheet.
        /// </summary>
        /// <param name="updates">Stylesheet to update from.</param>
        public void UpdateStyle(StyleSheet updates)
        {
            _style.UpdateFrom(updates);
        }

        /// <summary>Get extra space after with current UI scale applied. </summary>
        protected Vector2 _scaledSpaceAfter { get { return SpaceAfter * UserInterface.SCALE; } }

        /// <summary>Get extra space before with current UI scale applied. </summary>
        protected Vector2 _scaledSpaceBefore { get { return SpaceBefore * UserInterface.SCALE; } }

        /// <summary>Get size with current UI scale applied. </summary>
        protected Vector2 _scaledSize { get { return _size * UserInterface.SCALE; } }

        /// <summary>Get offset with current UI scale applied. </summary>
        protected Vector2 _scaledOffset { get { return _offset * UserInterface.SCALE; } }

        /// <summary>Get offset with current UI scale applied. </summary>
        protected Vector2 _scaledPadding { get { return Padding * UserInterface.SCALE; } }

        /// <summary>
        /// Return entity priority in drawing order and event handling.
        /// </summary>
        public virtual int Priority
        {
            get { return _indexInParent; }
        }

        /// <summary>
        /// Is the entity draggable (eg can a user grab it and drag it around).
        /// </summary>
        public bool Draggable
        {
            get { return _draggable; }
            set { _needToSetDragOffset = _draggable != value; _draggable = value; }
        }

        /// <summary>
        /// Optional background entity that will not respond to events and will always be rendered right behind this entity.
        /// </summary>
        public Entity Background
        {
            get { return _background; }
            set {
                if (value != null && value._parent != null) { throw new System.Exception("Cannot set background entity that have a parent!"); }
                _background = value;
            }
        }

        /// <summary>
        /// Current entity state (default / mouse hover / mouse down..).
        /// </summary>
        public EntityState State
        {
            get { return _entityState; }
            set { _entityState = value; }
        }

        /// <summary>
        /// Return all children of this entity.
        /// </summary>
        /// <returns>List with all children in entity.</returns>
        public List<Entity> GetChildren()
        {
            return _children;
        }

        /// <summary>
        /// Entity current size property.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Extra space (in pixels) to reserve *after* this entity when using Auto Anchors.
        /// </summary>
        public Vector2 SpaceAfter
        {
            set { SetStyleProperty("SpaceAfter", new StyleProperty(value)); }
            get { return GetActiveStyle("SpaceAfter").asVector; }
        }

        /// <summary>
        /// Extra space (in pixels) to reserve *before* this entity when using Auto Anchors.
        /// </summary>
        public Vector2 SpaceBefore
        {
            set { SetStyleProperty("SpaceBefore", new StyleProperty(value)); }
            get { return GetActiveStyle("SpaceBefore").asVector; }
        }

        /// <summary>
        /// Entity fill color - this is just a sugarcoat to access the default fill color style property.
        /// </summary>
        public Color FillColor
        {
            set { SetStyleProperty("FillColor", new StyleProperty(value)); }
            get { return GetActiveStyle("FillColor").asColor; }
        }

        /// <summary>
        /// Entity padding - this is just a sugarcoat to access the default padding style property.
        /// </summary>
        public Vector2 Padding
        {
            set { SetStyleProperty("Padding", new StyleProperty(value)); }
            get { return GetActiveStyle("Padding").asVector; }
        }

        /// <summary>
        /// Entity shadow color - this is just a sugarcoat to access the default shadow color style property.
        /// </summary>
        public Color ShadowColor
        {
            set { SetStyleProperty("ShadowColor", new StyleProperty(value)); }
            get { return GetActiveStyle("ShadowColor").asColor; }
        }

        /// <summary>
        /// Entity shadow offset - this is just a sugarcoat to access the default shadow offset style property.
        /// </summary>
        public Vector2 ShadowOffset
        {
            set { SetStyleProperty("ShadowOffset", new StyleProperty(value)); }
            get { return GetActiveStyle("ShadowOffset").asVector; }
        }

        /// <summary>
        /// Entity scale - this is just a sugarcoat to access the default scale style property.
        /// </summary>
        public float Scale
        {
            set { SetStyleProperty("Scale", new StyleProperty(value)); }
            get { return GetActiveStyle("Scale").asFloat; }
        }

        /// <summary>
        /// Entity outline color - this is just a sugarcoat to access the default outline color style property.
        /// </summary>
        public Color OutlineColor
        {
            set { SetStyleProperty("OutlineColor", new StyleProperty(value)); }
            get { return GetActiveStyle("OutlineColor").asColor; }
        }

        /// <summary>
        /// Entity outline width - this is just a sugarcoat to access the default outline color style property.
        /// </summary>
        public int OutlineWidth
        {
            set { SetStyleProperty("OutlineWidth", new StyleProperty(value)); }
            get { return GetActiveStyle("OutlineWidth").asInt; }
        }

        /// <summary>
        /// Return if this entity is currently disabled, due to self or one of the parents / grandparents being disabled.
        /// </summary>
        /// <returns>True if entity is disabled.</returns>
        public bool IsDisabled()
        {
            // iterate over parents until root, starting with self.
            // if any entity along the way is disabled we return true.
            Entity parent = this;
            while (parent != null)
            {
                if (parent.Disabled) { return true; }
                parent = parent._parent;
            }

            // not disabled
            return false;
        }


        /// <summary>
        /// Check if this entity is a descendant of another entity.
        /// This goes up all the way to root.
        /// </summary>
        /// <param name="other">Entity to check if this entity is descendant of.</param>
        /// <returns>True if this entity is descendant of the other entity.</returns>
        public bool IsDeepChildOf(Entity other)
        {
            // iterate over parents until root, starting with self.
            // if any entity along the way is child of 'other', we return true.
            Entity parent = this;
            while (parent != null)
            {
                if (parent._parent == other) { return true; }
                parent = parent._parent;
            }

            // not child of
            return false;
        }

        /// <summary>
        /// Return if this entity is currently locked, due to self or one of the parents / grandparents being locked.
        /// </summary>
        /// <returns>True if entity is disabled.</returns>
        public bool IsLocked()
        {
            // iterate over parents until root, starting with self.
            // if any entity along the way is locked we return true.
            Entity parent = this;
            while (parent != null)
            {
                if (parent.Locked)
                {
                    // special case - if should do events even when parent is locked and direct parent, skip
                    if (DoEventsIfDirectParentIsLocked)
                    {
                        if (parent == _parent) {
                            parent = parent._parent;
                            continue;
                        }
                    }

                    // if parent locked return true
                    return true;
                }

                // advance to next parent
                parent = parent._parent;
            }

            // not disabled
            return false;
        }

        /// <summary>
        /// Return if this entity is currently visible, eg this and all its parents and grandparents are visible.
        /// </summary>
        /// <returns>True if entity is really visible.</returns>
        public bool IsVisible()
        {
            // iterate over parents until root, starting with self.
            // if any entity along the way is not visible we return false.
            Entity parent = this;
            while (parent != null)
            {
                if (!parent.Visible) { return false; }
                parent = parent._parent;
            }

            // visible!
            return true;
        }

        /// <summary>
        /// Set the position and anchor of this entity.
        /// </summary>
        /// <param name="anchor">New anchor to set.</param>
        /// <param name="offset">Offset from new anchor position.</param>
        public void SetPosition(Anchor anchor, Vector2 offset)
        {
            _anchor = anchor;
            _offset = offset;
        }

        /// <summary>
        /// Set the anchor of this entity.
        /// </summary>
        /// <param name="anchor">New anchor to set.</param>
        public void SetAnchor(Anchor anchor)
        {
            _anchor = anchor;
        }

        /// <summary>
        /// Set the offset of this entity.
        /// </summary>
        /// <param name="offset">New offset to set.</param>
        public void SetOffset(Vector2 offset)
        {
            _offset = offset;
        }

        /// <summary>
        /// Return children in a sorted list by priority.
        /// </summary>
        /// <returns>List of children sorted by priority.</returns>
        protected List<Entity> GetSortedChildren()
        {
            // create list to sort and return
            List<Entity> ret = new List<Entity>(_children);

            // get children in a sorted list
            ret.Sort((x, y) =>
                x.Priority.CompareTo(y.Priority));

            // return the sorted list
            return ret;
        }

        /// <summary>
        /// Draw this entity and its children.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // if not visible skip
            if (!Visible)
            {
                return;
            }

            // draw background
            if (Background != null)
            {
                _background._parent = this;
                _background._indexInParent = 0;
                _background.Draw(spriteBatch);
                _background._parent = null;
            }

            // do before draw event
            OnBeforeDraw(spriteBatch);

            // calc desination rect
            _destRect = CalcDestRect();
            _destRectInternal = CalcInternalRect();

            // if there's a shadow
            if (ShadowColor.A > 0)
            {
                // update position to draw shadow
                _destRect.X += (int)ShadowOffset.X;
                _destRect.Y += (int)ShadowOffset.Y;

                // store previous state and colors
                Color oldFill = FillColor;
                Color oldOutline = OutlineColor;
                int oldOutlineWidth = OutlineWidth;
                EntityState oldState = _entityState;

                // set default colors and state for shadow pass
                FillColor = ShadowColor;
                OutlineColor = Color.Transparent;
                OutlineWidth = 0;
                _entityState = EntityState.Default;

                // if disabled, turn color into greyscale
                if (IsDisabled())
                {
                    FillColor = new Color(Color.White * (((FillColor.R + FillColor.G + FillColor.B) / 3f) / 255f), FillColor.A);
                }

                // draw with shadow effect
                DrawUtils.StartDrawSilhouette(spriteBatch);
                DrawEntity(spriteBatch);
                DrawUtils.EndDraw(spriteBatch);

                // return position and colors back to what they were
                _destRect.X -= (int)ShadowOffset.X;
                _destRect.Y -= (int)ShadowOffset.Y;
                FillColor = oldFill;
                OutlineColor = oldOutline;
                OutlineWidth = oldOutlineWidth;
                _entityState = oldState;
            }

            // draw the entity itself
            DrawUtils.StartDraw(spriteBatch, IsDisabled());
            DrawEntity(spriteBatch);
            DrawUtils.EndDraw(spriteBatch);

            // get sorted children list
            List<Entity> childrenSorted = GetSortedChildren();

            // draw all children
            foreach (Entity child in childrenSorted)
            {
                child.Draw(spriteBatch);
            }

            // do after draw event
            OnAfterDraw(spriteBatch);
        }


        /// <summary>
        /// The internal function to draw the entity itself.
        /// Implemented by inheriting entity types.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        virtual protected void DrawEntity(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Called every frame after drawing is done.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        virtual protected void OnAfterDraw(SpriteBatch spriteBatch)
        {
            AfterDraw?.Invoke(this);
            UserInterface.AfterDraw?.Invoke(this);
        }

        /// <summary>
        /// Called every frame before drawing is done.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        virtual protected void OnBeforeDraw(SpriteBatch spriteBatch)
        {
            BeforeDraw?.Invoke(this);
            UserInterface.BeforeDraw?.Invoke(this);
        }

        /// <summary>
        /// Get the direct parent of this entity.
        /// </summary>
        public Entity Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Add a child entity.
        /// </summary>
        /// <param name="child">Entity to add as child.</param>
        /// <param name="inheritParentState">If true, this entity will inherit the parent's state (set InheritParentState property).</param>
        public void AddChild(Entity child, bool inheritParentState = false)
        {
            // make sure don't already have a parent
            if (child._parent != null)
            {
                throw new System.Exception("Child element to add already got a parent!");
            }

            // set inherit parent mode
            child.InheritParentState = inheritParentState;

            // set parent and add
            child._parent = this;
            child._indexInParent = _children.Count;
            _children.Add(child);
        }

        /// <summary>
        /// Remove child entity.
        /// </summary>
        /// <param name="child">Entity to remove.</param>
        public void RemoveChild(Entity child)
        {
            // make sure don't already have a parent
            if (child._parent != this)
            {
                throw new System.Exception("Child element to remove does not belong to this entity!");
            }

            // set parent to null and remove
            child._parent = null;
            child._indexInParent = -1;
            _children.Remove(child);

            // reset index for all children
            int index = 0;
            foreach (Entity itrChild in _children)
            {
                itrChild._indexInParent = index++;
            }
        }

        /// <summary>
        /// Remove all children entities.
        /// </summary>
        public void ClearChildren()
        {
            foreach (Entity child in _children)
            {
                child._parent = null;
                child._indexInParent = -1;
            }
            _children.Clear();
        }

        /// <summary>
        /// Calculate and return the internal destination rectangle (note: this relay on the dest rect having a valid value first).
        /// </summary>
        /// <returns>Internal destination rectangle.</returns>
        public Rectangle CalcInternalRect()
        {
            // calculate the internal destination rect, eg after padding
            Vector2 padding = _scaledPadding;
            _destRectInternal = new Rectangle(_destRect.Location, _destRect.Size);
            _destRectInternal.X += (int)padding.X;
            _destRectInternal.Y += (int)padding.Y;
            _destRectInternal.Width -= (int)padding.X * 2;
            _destRectInternal.Height -= (int)padding.Y * 2;
            return _destRectInternal;
        }

        /// <summary>
        /// Calculate and return the destination rectangle, eg the space this entity is rendered on.
        /// </summary>
        /// <returns>Destination rectangle.</returns>
        virtual public Rectangle CalcDestRect()
        {
            // create new rectangle
            Rectangle ret = new Rectangle();

            // get parent internal destination rectangle
            Rectangle parentDest = _parent._destRectInternal;

            // set size (takes either this entity size, or if set to 0 take parent's size
            Vector2 size = _scaledSize;
            ret.Width   = (size.X != 0f ? (int)size.X : parentDest.Width);
            ret.Height  = (size.Y != 0f ? (int)size.Y : parentDest.Height);

            // make sure valid size
            if (ret.Width < 1) { ret.Width = 1; }
            if (ret.Height < 1) { ret.Height = 1; }

            // first calc some helpers
            int parent_left = parentDest.X;
            int parent_top = parentDest.Y;
            int parent_right = parent_left + parentDest.Width;
            int parent_bottom = parent_top + parentDest.Height;
            int parent_center_x = parent_left + parentDest.Width / 2;
            int parent_center_y = parent_top + parentDest.Height / 2;

            // get anchor and offset
            Anchor anchor = _anchor;
            Vector2 offset = _scaledOffset;

            // if we are in dragging mode we do a little hack to use top-left anchor with the dragged offset
            // note: but only if drag offset was previously set.
            if (_draggable && !_needToSetDragOffset)
            {
                anchor = Anchor.TopLeft;
                offset = _dragOffset * UserInterface.SCALE;
            }

            // calculate position based on anchor, parent and offset
            switch (anchor)
            {
                case Anchor.Auto:
                case Anchor.AutoInline:
                case Anchor.TopLeft:
                    ret.X = parent_left + (int)offset.X;
                    ret.Y = parent_top + (int)offset.Y;
                    break;

                case Anchor.TopRight:
                    ret.X = parent_right - ret.Width - (int)offset.X;
                    ret.Y = parent_top + (int)offset.Y;
                    break;

                case Anchor.TopCenter:
                case Anchor.AutoCenter:
                    ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                    ret.Y = parent_top + (int)offset.Y;
                    break;

                case Anchor.BottomLeft:
                    ret.X = parent_left + (int)offset.X;
                    ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                    break;

                case Anchor.BottomRight:
                    ret.X = parent_right - ret.Width - (int)offset.X;
                    ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                    break;

                case Anchor.BottomCenter:
                    ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                    ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                    break;

                case Anchor.CenterLeft:
                    ret.X = parent_left + (int)offset.X;
                    ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                    break;

                case Anchor.CenterRight:
                    ret.X = parent_right - ret.Width - (int)offset.X;
                    ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                    break;

                case Anchor.Center:
                    ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                    ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                    break;
            }

            // special case for auto anchors
            if ((anchor == Anchor.Auto || anchor == Anchor.AutoInline || anchor == Anchor.AutoCenter) && _parent != null)
            {
                // get previous entity before this
                Entity prevEntity = GetPreviousEntity();

                // only if found align based on it
                if (prevEntity != null)
                {
                    // handle inline align
                    if (anchor == Anchor.AutoInline)
                    {
                        ret.X = prevEntity._destRect.Right + (int)(offset.X + prevEntity._scaledSpaceAfter.X + _scaledSpaceBefore.X);
                        ret.Y = prevEntity._destRect.Y;
                    }

                    // handle inline align that ran out of width / or auto anchor not inline
                    if ((anchor == Anchor.AutoInline && ret.Right > _parent._destRectInternal.Right) ||
                        (anchor == Anchor.Auto || anchor == Anchor.AutoCenter))
                    {
                        // align x
                        if (anchor != Anchor.AutoCenter)
                        {
                            ret.X = parent_left + (int)offset.X;
                        }

                        // align y
                        ret.Y = prevEntity.GetActualDestRect().Bottom + (int)(offset.Y +
                            prevEntity._scaledSpaceAfter.Y +
                            _scaledSpaceBefore.Y);
                    }
                }
            }

            // some extra logic for draggables
            if (_draggable)
            {
                // if need to init dragged offset, set it
                // this trick is used so if an object is draggable, we first evaluate its position based on anchor etc, and we use that
                // position as starting point for the dragging
                if (_needToSetDragOffset)
                {
                    _dragOffset.X = ret.X - parent_left;
                    _dragOffset.Y = ret.Y - parent_top;
                    _needToSetDragOffset = false;
                }

                // if draggable and need to be contained inside parent, validate it
                if (LimitDraggingToParentBoundaries)
                {
                    if (ret.X < parent_left) { ret.X = parent_left; _dragOffset.X = 0; }
                    if (ret.Y < parent_top) { ret.Y = parent_top; _dragOffset.Y = 0; }
                    if (ret.Right > parent_right) { _dragOffset.X -= ret.Right - parent_right; ; ret.X -= ret.Right - parent_right; }
                    if (ret.Bottom > parent_bottom) { _dragOffset.Y -= ret.Bottom - parent_bottom; ret.Y -= ret.Bottom - parent_bottom; }
                }
            }

            // return the newly created rectangle
            _destRect = ret;
            return ret;
        }

        /// <summary>
        /// Return actual destination rectangle.
        /// This can be override and implemented by things like Paragraph, where the actual destination rect is based on
        /// text content, font and word-wrap.
        /// </summary>
        /// <returns>The actual destination rectangle.</returns>
        virtual public Rectangle GetActualDestRect()
        {
            return _destRect;
        }

        /// <summary>
        /// Remove this entity from its parent.
        /// </summary>
        public void RemoveFromParent()
        {
            if (_parent != null)
            {
                _parent.RemoveChild(this);
            }
        }

        /// <summary>
        /// Return the entity before this one in parent container, aka the next older sibling.
        /// </summary>
        /// <returns>Entity before this in parent, or null if first in parent or if orphan entity.</returns>
        protected Entity GetPreviousEntity()
        {
            // no parent? skip
            if (_parent == null) { return null; }

            // get siblings and iterate them
            List<Entity> siblings = _parent.GetChildren();
            Entity prev = null;
            foreach (Entity sibling in siblings)
            {
                // when getting to self, break the loop
                if (sibling == this)
                {
                    break;
                }

                // set prev
                prev = sibling;
            }

            // return the previous entity (or null if wasn't found)
            return prev;
        }

        /// <summary>
        /// Handle mouse down event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnMouseDown(InputHelper input)
        {
            OnMouseDown?.Invoke(this);
            UserInterface.OnMouseDown?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse up event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnMouseReleased(InputHelper input)
        {
            OnMouseReleased?.Invoke(this);
            UserInterface.OnMouseReleased?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse click event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnClick(InputHelper input)
        {
            OnClick?.Invoke(this);
            UserInterface.OnClick?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse down event, called every frame while down.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoWhileMouseDown(InputHelper input)
        {
            WhileMouseDown?.Invoke(this);
            UserInterface.WhileMouseDown?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse hover event, called every frame while hover.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoWhileMouseHover(InputHelper input)
        {
            WhileMouseHover?.Invoke(this);
            UserInterface.WhileMouseHover?.Invoke(this);
        }

        /// <summary>
        /// Handle value change event (for entities with value).
        /// </summary>
        virtual protected void DoOnValueChange()
        {
            OnValueChange?.Invoke(this);
            UserInterface.OnValueChange?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse enter event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnMouseEnter(InputHelper input)
        {
            OnMouseEnter?.Invoke(this);
            UserInterface.OnMouseEnter?.Invoke(this);
        }

        /// <summary>
        /// Handle mouse leave event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnMouseLeave(InputHelper input)
        {
            OnMouseLeave?.Invoke(this);
            UserInterface.OnMouseLeave?.Invoke(this);
        }

        /// <summary>
        /// Handle start dragging event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnStartDrag(InputHelper input)
        {
            OnStartDrag?.Invoke(this);
            UserInterface.OnStartDrag?.Invoke(this);
        }

        /// <summary>
        /// Handle end dragging event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnStopDrag(InputHelper input)
        {
            OnStopDrag?.Invoke(this);
            UserInterface.OnStopDrag?.Invoke(this);
        }

        /// <summary>
        /// Handle the while-dragging event.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoWhileDragging(InputHelper input)
        {
            WhileDragging?.Invoke(this);
            UserInterface.WhileDragging?.Invoke(this);
        }

        /// <summary>
        /// Handle when mouse wheel scroll and this entity is the active entity.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoOnMouseWheelScroll(InputHelper input)
        {
            OnMouseWheelScroll?.Invoke(this);
            UserInterface.OnMouseWheelScroll?.Invoke(this);
        }

        /// <summary>
        /// Called every frame after update.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoAfterUpdate(InputHelper input)
        {
            AfterUpdate?.Invoke(this);
            UserInterface.AfterUpdate?.Invoke(this);
        }

        /// <summary>
        /// Called every frame before update.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        virtual protected void DoBeforeUpdate(InputHelper input)
        {
            BeforeUpdate?.Invoke(this);
            UserInterface.BeforeUpdate?.Invoke(this);
        }

        /// <summary>
        /// Test if a given point is inside entity's boundaries.
        /// </summary>
        /// <remarks>This function result is affected by the 'UseActualSizeForCollision' flag.</remarks>
        /// <param name="point">Point to test.</param>
        /// <returns>True if point is in entity's boundaries (destination rectangle)</returns>
        virtual public bool IsInsideEntity(Vector2 point)
        {
            // get rectangle for the test
            Rectangle rect = UseActualSizeForCollision ? GetActualDestRect() : _destRect;
            
            // now test detection
            return (point.X >= rect.Left && point.X <= rect.Right &&
                    point.Y >= rect.Top && point.Y <= rect.Bottom);
        }

        /// <summary>
        /// Return true if this entity is naturally interactable, like buttons, lists, etc.
        /// Entities that are not naturally interactable are things like paragraph, colored rectangle, icon, etc.
        /// </summary>
        /// <remarks>This function should be overrided and implemented by different entities, and either return constant True or False.</remarks>
        /// <returns>True if entity is naturally interactable.</returns>
        virtual public bool IsNaturallyInteractable()
        {
            return false;
        }

        /// <summary>
        /// Return if the mouse is currently pressing on this entity (eg over it and left mouse button is down).
        /// </summary>
        public bool IsMouseDown { get { return _entityState == EntityState.MouseDown; } }

        /// <summary>
        /// Return if the mouse is currently over this entity (regardless of weather or not mouse button is down).
        /// </summary>
        public bool IsMouseOver { get { return _isMouseOver; } }

        /// <summary>
        /// Called every frame to update entity state and call events.
        /// </summary>
        /// <param name="input">Input helper.</param>
        /// <param name="targetEntity">The deepest child entity with highest priority that we point on and can be interacted with.</param>
        /// <param name="dragTargetEntity">The deepest child dragable entity with highest priority that we point on and can be drag if mouse down.</param>
        /// <param name="wasEventHandled">Set to true if current event was already handled by a deeper child.</param>
        virtual public void Update(InputHelper input, ref Entity targetEntity, ref Entity dragTargetEntity, ref bool wasEventHandled)
        {
            // if inherit parent state just copy it and stop
            if (InheritParentState)
            {
                _entityState = _parent._entityState;
                _isMouseOver = _parent._isMouseOver;
                IsFocused = _parent.IsFocused;
                return;
            }

            // if disabled , invisible, or locked - skip
            if (IsDisabled() || IsLocked() || !IsVisible())
            {
                // if this very entity is locked (eg not locked due to parent being locked), 
                // iterate children and invoke those with DoEventsIfDirectParentIsLocked setting
                if (Locked)
                {
                    for (int i = _children.Count - 1; i >= 0; i--)
                    {
                        if (_children[i].DoEventsIfDirectParentIsLocked)
                        {
                            _children[i].Update(input, ref targetEntity, ref dragTargetEntity, ref wasEventHandled);
                        }
                    }
                }

                // set to default and return
                _isInteractable = false;
                _entityState = EntityState.Default;
                return;
            }

            // entity is interactable!
            _isInteractable = true;

            // do before update event
            DoBeforeUpdate(input);

            // get mouse position
            Vector2 mousePos = input.MousePosition;

            // store previous state
            EntityState prevState = _entityState;

            // store previous mouse-over state
            bool prevMouseOver = _isMouseOver;

            // STEP 1: FIRST WE CALCULATE ENTITY STATE (EG MOUST HOVER / MOUSE DOWN / ..)

            // only if event was not already catched by another entity, check for events
            if (!wasEventHandled)
            {
                // if need to calculate state locally:
                if (!InheritParentState)
                {
                    // reset the mouse-over flag
                    _isMouseOver = false;
                    _entityState = EntityState.Default;

                    // set mouse state
                    if (IsInsideEntity(mousePos))
                    {
                        // set self as the current target, unless a sibling got the event first
                        if (targetEntity == null || targetEntity._parent != _parent)
                        {
                            targetEntity = this;
                        }

                        // set self as the dragging target, but only if draggable or interactive
                        if (_draggable || IsNaturallyInteractable())
                        {
                            dragTargetEntity = this;
                        }

                        // mouse is over entity
                        _isMouseOver = true;

                        // update mouse state
                        _entityState = input.MouseButtonDown() ? EntityState.MouseDown : EntityState.MouseHover;
                    }
                }

                // set if focused
                if (input.MouseButtonPressed())
                {
                    IsFocused = _isMouseOver;
                }
            }
            // if currently other entity is targeted and mouse clicked, set focused to false
            else if (input.MouseButtonClick())
            {
                IsFocused = false;
            }

            // STEP 2: NOW WE CALL ALL CHILDREN'S UPDATE

            // update all children (note: we go in reverse order so that entities on front will receive events before entites on back.
            List<Entity> childrenSorted = GetSortedChildren();
            for (int i = childrenSorted.Count - 1; i >= 0; i--)
            {
                childrenSorted[i].Update(input, ref targetEntity, ref dragTargetEntity, ref wasEventHandled);
            }

            // STEP 3: CALL EVENTS

            // if selected target is this
            if (targetEntity == this)
            {
                // handled events
                wasEventHandled = true;

                // call the while-mouse-down handler
                if (_entityState == EntityState.MouseDown)
                {
                    DoWhileMouseDown(input);
                }
                else
                {
                    DoWhileMouseHover(input);
                }

                // set mouse enter / mouse leave
                if (_isMouseOver && !prevMouseOver)
                {
                    DoOnMouseEnter(input);
                }

                // generate events
                if (prevState != _entityState)
                {
                    // mouse down
                    if (input.MouseButtonDown())
                    {
                        DoOnMouseDown(input);
                    }
                    // mouse up
                    if (input.MouseButtonReleased())
                    {
                        DoOnMouseReleased(input);
                    }
                    // mouse click
                    if (input.MouseButtonClick())
                    {
                        DoOnClick(input);
                    }
                }
            }
            // if not current target, clear entity state
            else
            {
                _entityState = EntityState.Default;
            }

            // mouse leave events
            if (!_isMouseOver && prevMouseOver)
            {
                DoOnMouseLeave(input);
            }

            // handle mouse wheel scroll over this entity
            if (targetEntity == this || UserInterface.ActiveEntity == this)
            {
                if (input.MouseWheelChange != 0)
                {
                    DoOnMouseWheelScroll(input);
                }
            }

            // STEP 4: HANDLE DRAGGING FOR DRAGABLES

            // if draggable, and after calling all the children target is still self, it means we are being dragged!
            if (_draggable && (dragTargetEntity == this) && input.MouseButtonDown(MouseButton.Left) && IsFocused)
            {
                // check if we need to start dragging the entity that was not dragged before
                if (!_isBeingDragged && input.MousePositionDiff.Length() != 0)
                {
                    // remove self from parent and add again. this trick is to keep the dragged entity always on-top
                    Entity parent = _parent;
                    RemoveFromParent();
                    parent.AddChild(this);

                    // set dragging mode = true, and call the do-start-dragging event
                    _isBeingDragged = true;
                    DoOnStartDrag(input);
                }

                // if being dragged..
                if (_isBeingDragged)
                {
                    // update drag offset and call the dragging event
                    _dragOffset += input.MousePositionDiff / UserInterface.SCALE;
                    DoWhileDragging(input);
                }
            }
            // if not currently dragged but was dragged last frane, call the end dragging event
            else if (_isBeingDragged)
            {
                _isBeingDragged = false;
                DoOnStopDrag(input);
            }

            // do after-update events
            DoAfterUpdate(input);
        }
    }
}
