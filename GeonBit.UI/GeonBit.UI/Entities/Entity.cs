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
using System.Reflection;
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
    /// Static strings with all common style property names, to reduce string creations.
    /// </summary>
    internal static class StylePropertyIds
    {
        public static readonly string SpaceAfter = "SpaceAfter";
        public static readonly string SpaceBefore = "SpaceBefore";
        public static readonly string FillColor = "FillColor";
        public static readonly string Scale = "Scale";
        public static readonly string Padding = "Padding";
        public static readonly string ShadowColor = "ShadowColor";
        public static readonly string ShadowScale = "ShadowScale";
        public static readonly string ShadowOffset = "ShadowOffset";
        public static readonly string OutlineColor = "OutlineColor";
        public static readonly string OutlineWidth = "OutlineWidth";
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
        public static readonly Vector2 USE_DEFAULT_SIZE = new Vector2(-1, -1);

        /// <summary>The direct parent of this entity.</summary>
        protected Entity _parent = null;

        /// <summary>Index inside parent.</summary>
        protected int _indexInParent;

        /// <summary>Is the entity currently interactable.</summary>
        protected bool _isInteractable = false;

        /// <summary>Optional identifier you can attach to entities so you can later search and retrieve by.</summary>
        public string Identifier = "";

        /// <summary>
        /// If this boolean is true, events will just "go through" this entity to its children or entities behind it.
        /// This bool comes to solve conditions where you have two panels without skin that hide each other but you want
        /// users to be able to click on the bottom panel through the upper panel, provided it doesn't hit any of the first
        /// panel's children.
        /// </summary>
        public bool ClickThrough = false;

        /// <summary>If in promiscuous mode, mouse button is pressed *outside* the entity and then released on the entity, click event will be fired.
        /// If false, in order to fire click event the mouse button must be pressed AND released over this entity (but can travel outside while being
        /// held down, as long as its released inside).
        /// Note: Windows default behavior is non promiscuous mode.</summary>
        public bool PromiscuousClicksMode = false;

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

        // mark the first update call on this entity.
        private bool _isFirstUpdate = true;

        /// <summary>
        /// Mark if this entity is dirty and need to recalculate its destination rect.
        /// </summary>
        private bool _isDirty = true;

        // entity current style properties
        private StyleSheet _style = new StyleSheet();

        /// <summary>
        /// Every time we update destination rect and internal destination rect view the update function, we increase this counter.
        /// This is so our children will know we did an update and they need to update too.
        /// </summary>
        private uint _destRectVersion = 0;

        /// <summary>
        /// The last known version we have of the parent dest rect version.
        /// If this number does not match our parent's _destRectVersion, we will recalculate destination rect.
        /// </summary>
        private uint _parentLastDestRectVersion = 0;

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

        /// <summary>Callback to execute every time the visibility of this entity changes (also invokes when parent becomes invisible / visible again).</summary>
        public EventCallback OnVisiblityChange = null;

        /// <summary>Is mouse currently pointing on this entity.</summary>
        protected bool _isMouseOver = false;

        /// <summary>If true, this entity and its children will be drawn in greyscale effect and will not respond to events.</summary>
        public bool Disabled = false;

        /// <summary>If true, this entity and its children will not respond to events (but will be drawn normally, unlike when disabled).</summary>
        public bool Locked = false;

        /// <summary>Is the entity currently visible.</summary>
        private bool _visible = true;

        /// <summary>Is this entity currently disabled?</summary>
        private bool _isCurrentlyDisabled = false;

        /// <summary>Current entity state.</summary>
        protected EntityState _entityState = EntityState.Default;

        /// <summary>Does this entity or one of its children currently focused?</summary>
        public bool IsFocused = false;

        /// <summary>Currently calculated destination rect (eg the region this entity is drawn on).</summary>
        internal Rectangle _destRect;

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
        public static Vector2 DefaultSize = Vector2.Zero;

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
            // set as dirty (eg need to recalculate destination rect)
            MarkAsDirty();

            // store size, anchor and offset
            Vector2 defaultSize = EntityDefaultSize;
            _size = size ?? defaultSize;
            _offset = offset ?? Vector2.Zero;
            _anchor = anchor;

            // set basic default style
            UpdateStyle(DefaultStyle);

            // check default size on specific axises
            if (_size.X == -1) { _size.X = defaultSize.X; }
            if (_size.Y == -1) { _size.Y = defaultSize.Y; }
        }

        /// <summary>
        /// Return the default size for this entity.
        /// </summary>
        public Vector2 EntityDefaultSize
        {
            get
            {
                // get static field from class type
                System.Type type = GetType();
                FieldInfo field = type.GetField("DefaultSize", BindingFlags.Public | BindingFlags.Static);

                // if not found, return the static default size of the base entity
                if (field == null) { return DefaultSize; }

                // return default size
                return (Vector2)(field.GetValue(null));
            }
        }

        /// <summary>
        /// Call this function when the first update occures.
        /// </summary>
        protected virtual void DoOnFirstUpdate()
        {
            // call the spawn event
            UserInterface.OnEntitySpawn?.Invoke(this);

            // make parent dirty
            if (_parent != null) { _parent.MarkAsDirty(); }
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
        /// <param name="markAsDirty">If true, will mark this entity as dirty after this style change.</param>
        public void SetStyleProperty(string property, StyleProperty value, EntityState state = EntityState.Default, bool markAsDirty = true)
        {
            _style.SetStyleProperty(property, value, state);
            if (markAsDirty) { MarkAsDirty(); }
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
            MarkAsDirty();
        }

        /// <summary>Get extra space after with current UI scale applied. </summary>
        protected Vector2 _scaledSpaceAfter { get { return SpaceAfter * UserInterface.GlobalScale; } }

        /// <summary>Get extra space before with current UI scale applied. </summary>
        protected Vector2 _scaledSpaceBefore { get { return SpaceBefore * UserInterface.GlobalScale; } }

        /// <summary>Get size with current UI scale applied. </summary>
        protected Vector2 _scaledSize { get { return _size * UserInterface.GlobalScale; } }

        /// <summary>Get offset with current UI scale applied. </summary>
        protected Vector2 _scaledOffset { get { return _offset * UserInterface.GlobalScale; } }

        /// <summary>Get offset with current UI scale applied. </summary>
        protected Vector2 _scaledPadding { get { return Padding * UserInterface.GlobalScale; } }

        /// <summary>
        /// Set / get visibility.
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; DoOnVisibilityChange(); }
        }

        /// <summary>
        /// Return entity priority in drawing order and event handling.
        /// </summary>
        public virtual int Priority
        {
            get { return _indexInParent; }
        }

        /// <summary>
        /// Get if this entity needs to recalculate destination rect.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
        }

        /// <summary>
        /// Is the entity draggable (eg can a user grab it and drag it around).
        /// </summary>
        public bool Draggable
        {
            get { return _draggable; }
            set { _needToSetDragOffset = _draggable != value; _draggable = value; MarkAsDirty(); }
        }

        /// <summary>
        /// Optional background entity that will not respond to events and will always be rendered right behind this entity.
        /// </summary>
        public Entity Background
        {
            get { return _background; }
            set {
                if (value != null && value._parent != null)
                {
                    throw new System.Exception("Cannot set background entity that have a parent!");
                }
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
        /// Find and return first occurance of a child entity with a given identifier and specific type.
        /// </summary>
        /// <typeparam name="T">Entity type to get.</typeparam>
        /// <param name="identifier">Identifier to find.</param>
        /// <param name="recursive">If true, will search recursively in children of children. If false, will search only in direct children.</param>
        /// <returns>First found entity with given identifier and type, or null if nothing found.</returns>
        public T Find<T> (string identifier, bool recursive = false) where T : Entity
        {
            // iterate children
            foreach (Entity child in _children)
            {
                // check if identifier and type matches - if so, return it
                if (child.Identifier == identifier && child.GetType() == typeof(T))
                {
                    return (T)child;
                }

                // if recursive, search in child
                if (recursive)
                {
                    // search in child
                    T ret = child.Find<T>(identifier, recursive);

                    // if found return it
                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }

            // not found?
            return null;
        }

        /// <summary>
        /// Find and return first occurance of a child entity with a given identifier.
        /// </summary>
        /// <param name="identifier">Identifier to find.</param>
        /// <param name="recursive">If true, will search recursively in children of children. If false, will search only in direct children.</param>
        /// <returns>First found entity with given identifier, or null if nothing found.</returns>
        public Entity Find(string identifier, bool recursive = false)
        {
            return Find<Entity>(identifier, recursive);
        }

        /// <summary>
        /// Iterate over children and call 'callback' for every direct child of this entity.
        /// </summary>
        /// <param name="callback">Callback function to call with every child of this entity.</param>
        public void IterateChildren(EventCallback callback)
        {
            foreach (Entity child in _children)
            {
                callback(child);
            }
        }

        /// <summary>
        /// Entity current size property.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set { if (_size != value) { _size = value; MarkAsDirty(); } }
        }

        /// <summary>
        /// Extra space (in pixels) to reserve *after* this entity when using Auto Anchors.
        /// </summary>
        public Vector2 SpaceAfter
        {
            set { SetStyleProperty(StylePropertyIds.SpaceAfter, new StyleProperty(value)); }
            get { return GetActiveStyle(StylePropertyIds.SpaceAfter).asVector; }
        }

        /// <summary>
        /// Extra space (in pixels) to reserve *before* this entity when using Auto Anchors.
        /// </summary>
        public Vector2 SpaceBefore
        {
            set { SetStyleProperty(StylePropertyIds.SpaceBefore, new StyleProperty(value)); }
            get { return GetActiveStyle(StylePropertyIds.SpaceBefore).asVector; }
        }

        /// <summary>
        /// Entity fill color - this is just a sugarcoat to access the default fill color style property.
        /// </summary>
        public Color FillColor
        {
            set { SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.FillColor).asColor; }
        }

        /// <summary>
        /// Entity fill color opacity - this is just a sugarcoat to access the default fill color alpha style property.
        /// </summary>
        public byte Opacity
        {
            set
            {
                Color col = FillColor;
                col.A = value;
                SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(col), markAsDirty: false);
            }
            get
            {
                return FillColor.A;
            }
        }

        /// <summary>
        /// Entity padding - this is just a sugarcoat to access the default padding style property.
        /// </summary>
        public Vector2 Padding
        {
            set { SetStyleProperty(StylePropertyIds.Padding, new StyleProperty(value)); }
            get { return GetActiveStyle(StylePropertyIds.Padding).asVector; }
        }

        /// <summary>
        /// Entity shadow color - this is just a sugarcoat to access the default shadow color style property.
        /// </summary>
        public Color ShadowColor
        {
            set { SetStyleProperty(StylePropertyIds.ShadowColor, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.ShadowColor).asColor; }
        }

        /// <summary>
        /// Entity shadow scale - this is just a sugarcoat to access the default shadow scale style property.
        /// </summary>
        public float ShadowScale
        {
            set { SetStyleProperty(StylePropertyIds.ShadowScale, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.ShadowScale).asFloat; }
        }

        /// <summary>
        /// Entity shadow offset - this is just a sugarcoat to access the default shadow offset style property.
        /// </summary>
        public Vector2 ShadowOffset
        {
            set { SetStyleProperty(StylePropertyIds.ShadowOffset, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.ShadowOffset).asVector; }
        }

        /// <summary>
        /// Entity scale - this is just a sugarcoat to access the default scale style property.
        /// </summary>
        public float Scale
        {
            set { SetStyleProperty(StylePropertyIds.Scale, new StyleProperty(value)); }
            get { return GetActiveStyle(StylePropertyIds.Scale).asFloat; }
        }

        /// <summary>
        /// Entity outline color - this is just a sugarcoat to access the default outline color style property.
        /// </summary>
        public Color OutlineColor
        {
            set { SetStyleProperty(StylePropertyIds.OutlineColor, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.OutlineColor).asColor; }
        }

        /// <summary>
        /// Entity outline width - this is just a sugarcoat to access the default outline color style property.
        /// </summary>
        public int OutlineWidth
        {
            set { SetStyleProperty(StylePropertyIds.OutlineWidth, new StyleProperty(value), markAsDirty: false); }
            get { return GetActiveStyle(StylePropertyIds.OutlineWidth).asInt; }
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
            SetAnchor(anchor);
            SetOffset(offset);
        }

        /// <summary>
        /// Set the anchor of this entity.
        /// </summary>
        /// <param name="anchor">New anchor to set.</param>
        public void SetAnchor(Anchor anchor)
        {
            _anchor = anchor;
            MarkAsDirty();
        }

        /// <summary>
        /// Set the offset of this entity.
        /// </summary>
        /// <param name="offset">New offset to set.</param>
        public void SetOffset(Vector2 offset)
        {
            // if currently dragged:
            if (_isBeingDragged)
            {
                _dragOffset = offset;
                MarkAsDirty();
            }
            // if not dragged, set regular offset
            else if (_offset != offset)
            {
                _offset = offset;
                MarkAsDirty();
            }
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
        /// Update dest rect and internal dest rect.
        /// This is called internally whenever a change is made to the entity or its parent.
        /// </summary>
        virtual public void UpdateDestinationRects()
        {
            // update dest and internal dest rects
            _destRect = CalcDestRect();
            _destRectInternal = CalcInternalRect();

            // mark as no longer dirty
            _isDirty = false;

            // increase dest rect version and update parent last known
            _destRectVersion++;
            if (_parent != null) { _parentLastDestRectVersion = _parent._destRectVersion; }
        }

        /// <summary>
        /// Update dest rect and internal dest rect, but only if needed (eg if something changed since last update).
        /// </summary>
        virtual public void UpdateDestinationRectsIfDirty()
        {
            // if dirty, update destination rectangles
            if (_parent != null && (_isDirty || (_parentLastDestRectVersion != _parent._destRectVersion)))
            {
                UpdateDestinationRects();
            }
        }

        /// <summary>
        /// Draw this entity and its children.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
        virtual public void Draw(SpriteBatch spriteBatch)
        {
            // if not visible skip
            if (!Visible)
            {
                return;
            }

            // update if disabled
            _isCurrentlyDisabled = IsDisabled();

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

            // calc desination rects (if needed)
            UpdateDestinationRectsIfDirty();

            // draw shadow
            DrawEntityShadow(spriteBatch);

            // draw entity outline
            DrawEntityOutline(spriteBatch);

            // draw the entity itself
            UserInterface.DrawUtils.StartDraw(spriteBatch, _isCurrentlyDisabled);
            DrawEntity(spriteBatch);
            UserInterface.DrawUtils.EndDraw(spriteBatch);

            // do stuff before drawing children
            BeforeDrawChildren(spriteBatch);

            // get sorted children list
            List<Entity> childrenSorted = GetSortedChildren();

            // draw all children
            foreach (Entity child in childrenSorted)
            {
                child.Draw(spriteBatch);
            }

            // do stuff after drawing children
            AfterDrawChildren(spriteBatch);

            // do after draw event
            OnAfterDraw(spriteBatch);
        }

        /// <summary>
        /// Called before drawing child entities of this entity.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
        protected virtual void BeforeDrawChildren(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Called after drawing child entities of this entity.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
        protected virtual void AfterDrawChildren(SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// Draw entity shadow (if defined shadow).
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        virtual protected void DrawEntityShadow(SpriteBatch spriteBatch)
        {
            // store current 'is-dirty' flag, because it changes internally while drawing shadow
            bool isDirty = _isDirty;

            // get current shadow color and if transparent skip
            Color shadowColor = ShadowColor;
            if (shadowColor.A == 0) { return; }

            // get shadow scale
            float shadowScale = ShadowScale;

            // update position to draw shadow
            _destRect.X += (int)ShadowOffset.X;
            _destRect.Y += (int)ShadowOffset.Y;

            // store previous state and colors
            Color oldFill = FillColor;
            Color oldOutline = OutlineColor;
            float oldScale = Scale;
            int oldOutlineWidth = OutlineWidth;
            EntityState oldState = _entityState;

            // set default colors and state for shadow pass
            FillColor = shadowColor;
            OutlineColor = Color.Transparent;
            OutlineWidth = 0;
            Scale = shadowScale;
            _entityState = EntityState.Default;

            // if disabled, turn color into greyscale
            if (_isCurrentlyDisabled)
            {
                FillColor = new Color(Color.White * (((shadowColor.R + shadowColor.G + shadowColor.B) / 3f) / 255f), shadowColor.A);
            }

            // draw with shadow effect
            UserInterface.DrawUtils.StartDrawSilhouette(spriteBatch);
            DrawEntity(spriteBatch);
            UserInterface.DrawUtils.EndDraw(spriteBatch);

            // return position and colors back to what they were
            _destRect.X -= (int)ShadowOffset.X;
            _destRect.Y -= (int)ShadowOffset.Y;
            FillColor = oldFill;
            Scale = oldScale;
            OutlineColor = oldOutline;
            OutlineWidth = oldOutlineWidth;
            _entityState = oldState;

            // restore is-dirty flag
            _isDirty = isDirty;
        }

        /// <summary>
        /// Draw entity outline.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        virtual protected void DrawEntityOutline(SpriteBatch spriteBatch)
        {
            // get outline width and if 0 return
            int outlineWidth = OutlineWidth;
            if (OutlineWidth == 0) { return; }

            // get outline color
            Color outlineColor = OutlineColor;

            // if disabled, turn outline to grey
            if (_isCurrentlyDisabled)
            {
                outlineColor = new Color(Color.White * (((outlineColor.R + outlineColor.G + outlineColor.B) / 3f) / 255f), outlineColor.A);
            }

            // store previous fill color
            Color oldFill = FillColor;

            // store original destination rect
            Rectangle originalDest = _destRect;
            Rectangle originalIntDest = _destRectInternal;

            // store entity previous state
            EntityState oldState = _entityState;

            // set fill color
            SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(outlineColor), oldState, markAsDirty: false);

            // draw the entity outline
            UserInterface.DrawUtils.StartDrawSilhouette(spriteBatch);
            _destRect.Location = originalDest.Location + new Point(-outlineWidth, 0);
            DrawEntity(spriteBatch);
            _destRect.Location = originalDest.Location + new Point(0, -outlineWidth);
            DrawEntity(spriteBatch);
            _destRect.Location = originalDest.Location + new Point(outlineWidth, 0);
            DrawEntity(spriteBatch);
            _destRect.Location = originalDest.Location + new Point(0, outlineWidth);
            DrawEntity(spriteBatch);
            UserInterface.DrawUtils.EndDraw(spriteBatch);

            // turn back to previous fill color
            SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(oldFill), oldState, markAsDirty: false);

            // return to the original destination rect
            _destRect = originalDest;
            _destRectInternal = originalIntDest;
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
        /// <param name="index">If provided, will be the index in the children array to push the new entity.</param>
        public void AddChild(Entity child, bool inheritParentState = false, int index = -1)
        {
            // make sure don't already have a parent
            if (child._parent != null)
            {
                throw new System.Exception("Child element to add already got a parent!");
            }

            // set inherit parent mode
            child.InheritParentState = inheritParentState;

            // set child's new parent
            child._parent = this;

            // if index is -1 or out of range, set to last item in childrens list
            if (index == -1 || index >= _children.Count)
            {
                index = _children.Count;
            }
            
            // add child at index
            child._indexInParent = index;
            _children.Insert(index, child);

            // update any siblings which need their index updating
            for (int i = index + 1; i < _children.Count; i++)
            {
                _children[i]._indexInParent += 1;
            }

            // reset child parent dest rect version
            child._parentLastDestRectVersion = _destRectVersion - 1;

            // mark child as dirty
            child.MarkAsDirty();
            MarkAsDirty();
        }

        /// <summary>
        /// Bring this entity to be on front (inside its parent).
        /// </summary>
        public void BringToFront()
        {
            Entity parent = _parent;
            parent.RemoveChild(this);
            parent.AddChild(this);
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

            // mark child and self as dirty
            child.MarkAsDirty();
            MarkAsDirty();
        }

        /// <summary>
        /// Remove all children entities.
        /// </summary>
        public void ClearChildren()
        {
            // remove all children
            foreach (Entity child in _children)
            {
                child._parent = null;
                child._indexInParent = -1;
                child.MarkAsDirty();
            }
            _children.Clear();

            // mark self as dirty
            MarkAsDirty();
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

            // no parent? stop here and return empty rect
            if (_parent == null)
            {
                return ret;
            }

            // get parent internal destination rectangle
            _parent.UpdateDestinationRectsIfDirty();
            Rectangle parentDest = _parent._destRectInternal;

            // set size:
            // 0: takes whole parent size.
            // 0.0 - 1.0: takes percent of parent size.
            // > 1.0: size in pixels.
            Vector2 size = _scaledSize;
            ret.Width  = (size.X == 0f ? parentDest.Width  : (size.X > 0f && size.X < 1f ? (int)(parentDest.Width  * _size.X) : (int)size.X));
            ret.Height = (size.Y == 0f ? parentDest.Height : (size.Y > 0f && size.Y < 1f ? (int)(parentDest.Height * _size.Y) : (int)size.Y));

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
                offset = _dragOffset;
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
                Entity prevEntity = GetPreviousEntity(true);

                // only if found align based on it
                if (prevEntity != null)
                {
                    // make sure sibling is up-to-date
                    prevEntity.UpdateDestinationRectsIfDirty();

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
        /// Return the relative offset, in pixels, from parent top-left corner.
        /// </summary>
        /// <remarks>
        /// This return the offset between the top left corner of this entity regardless of anchor type.
        /// </remarks>
        /// <returns>Calculated offset from parent top-left corner.</returns>
        public Vector2 GetRelativeOffset()
        {
            Rectangle dest = GetActualDestRect();
            Rectangle parentDest = _parent.GetActualDestRect();
            return new Vector2(dest.X - parentDest.X, dest.Y - parentDest.Y);
        }

        /// <summary>
        /// Return the entity before this one in parent container, aka the next older sibling.
        /// </summary>
        /// <returns>Entity before this in parent, or null if first in parent or if orphan entity.</returns>
        /// <param name="skipInvisibles">If true, will skip invisible entities, eg will return the first visible older sibling.</param>
        protected Entity GetPreviousEntity(bool skipInvisibles = false)
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

                // if older sibling is invisible, skip it
                if (skipInvisibles && !sibling.Visible)
                {
                    continue;
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
        /// Called every time the visibility property of this entity changes.
        /// </summary>
        virtual protected void DoOnVisibilityChange()
        {
            // invoke callbacks
            OnVisiblityChange?.Invoke(this);
            UserInterface.OnVisiblityChange?.Invoke(this);
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
        /// Mark that this entity boundaries or style changed and it need to recalculate cached destination rect and other things.
        /// </summary>
        public void MarkAsDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Remove the IsDirty flag.
        /// </summary>
        /// <param name="updateChildren">If true, will set a flag that will still make children update.</param>
        internal void ClearDirtyFlag(bool updateChildren = false)
        {
            _isDirty = false;
            if (updateChildren)
            {
                _destRectVersion++;
            }
        }

        /// <summary>
        /// Called every frame to update the children of this entity.
        /// </summary>
        /// <param name="input">Input helper.</param>
        /// <param name="targetEntity">The deepest child entity with highest priority that we point on and can be interacted with.</param>
        /// <param name="dragTargetEntity">The deepest child dragable entity with highest priority that we point on and can be drag if mouse down.</param>
        /// <param name="wasEventHandled">Set to true if current event was already handled by a deeper child.</param>
        virtual protected void UpdateChildren(InputHelper input, ref Entity targetEntity, ref Entity dragTargetEntity, ref bool wasEventHandled)
        {
            // update all children (note: we go in reverse order so that entities on front will receive events before entites on back.
            List<Entity> childrenSorted = GetSortedChildren();
            for (int i = childrenSorted.Count - 1; i >= 0; i--)
            {
                childrenSorted[i].Update(input, ref targetEntity, ref dragTargetEntity, ref wasEventHandled);
            }
        }

        /// <summary>
        /// Called every frame to update entity state and call events.
        /// </summary>
        /// <param name="input">Input helper.</param>
        /// <param name="targetEntity">The deepest child entity with highest priority that we point on and can be interacted with.</param>
        /// <param name="dragTargetEntity">The deepest child dragable entity with highest priority that we point on and can be drag if mouse down.</param>
        /// <param name="wasEventHandled">Set to true if current event was already handled by a deeper child.</param>
        virtual public void Update(InputHelper input, ref Entity targetEntity, ref Entity dragTargetEntity, ref bool wasEventHandled)
        {
            // check if should invoke the spawn effect
            if (_isFirstUpdate)
            {
                DoOnFirstUpdate();
                _isFirstUpdate = false;
            }

            // if inherit parent state just copy it and stop
            if (InheritParentState)
            {
                _entityState = _parent._entityState;
                _isMouseOver = _parent._isMouseOver;
                IsFocused = _parent.IsFocused;
                _isCurrentlyDisabled = _parent._isCurrentlyDisabled;
                return;
            }

            // get if disabled
            _isCurrentlyDisabled = IsDisabled();

            // if disabled, invisible, or locked - skip
            if (_isCurrentlyDisabled || IsLocked() || !IsVisible())
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

                // if was previously interactable, we might need to trigger some events
                if (_isInteractable)
                {
                    // if mouse was over, trigger mouse leave event
                    if (_entityState == EntityState.MouseHover)
                    {
                        DoOnMouseLeave(input);
                    }
                    // if mouse was down, trigger mouse up and leave events
                    else if (_entityState == EntityState.MouseDown)
                    {
                        DoOnMouseReleased(input);
                        DoOnMouseLeave(input);
                    }
                }

                // set to default and return
                _isInteractable = false;
                _entityState = EntityState.Default;
                return;
            }

            // if click-through is true, update children and stop here
            if (ClickThrough)
            {
                UpdateChildren(input, ref targetEntity, ref dragTargetEntity, ref wasEventHandled);
                return;
            }

            // update dest rect if needed (dest rect is calculated before draw, but is used for mouse collision detection as well,
            // so its better to calculate it here too in case something changed).
            UpdateDestinationRectsIfDirty();
            
            // set if interactable
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

                        // mouse is over entity
                        _isMouseOver = true;

                        // update mouse state
                        _entityState = (IsFocused || PromiscuousClicksMode || input.MouseButtonPressed()) && 
                            input.MouseButtonDown() ? EntityState.MouseDown : EntityState.MouseHover;
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

            // update all children
            UpdateChildren(input, ref targetEntity, ref dragTargetEntity, ref wasEventHandled);

            // check dragging after children so that the most nested entity gets priority
            if ((_draggable || IsNaturallyInteractable()) && dragTargetEntity == null && _isMouseOver && input.MouseButtonPressed(MouseButton.Left))
            {
                dragTargetEntity = this;
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
                    if (input.MouseButtonPressed())
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

            // if draggable, and after calling all the children target is self, it means we are being dragged!
            if (_draggable && (dragTargetEntity == this) && IsFocused)
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
                    _dragOffset += input.MousePositionDiff;
                    DoWhileDragging(input);
                }
            }
            // if not currently dragged but was dragged last frane, call the end dragging event
            else if (_isBeingDragged)
            {
                _isBeingDragged = false;
                DoOnStopDrag(input);
                MarkAsDirty();
            }

            // if being dragged, mark as dirty
            if (_isBeingDragged)
            {
                MarkAsDirty();
            }

            // do after-update events
            DoAfterUpdate(input);
        }
    }
}
