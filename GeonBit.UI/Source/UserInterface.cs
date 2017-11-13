#region File Description
//-----------------------------------------------------------------------------
// This file define the main class that manage and draw the UI.
// To use GeonBit.UI you first need to create an instance of this class and
// update / draw it every frame.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework.Content;

namespace GeonBit.UI
{
    /// <summary>
    /// GeonBit.UI is part of the GeonBit project, and provide a simple yet extensive UI framework for MonoGame based projects.
    /// This is the main GeonBit.UI namespace. It contains the UserInterface manager and other important helpers.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// A callback function you can register on entity events, like on-click, on-mouse-leave, etc.
    /// </summary>
    /// <param name="entity">The entity instance the event came from.</param>
    public delegate void EventCallback(Entity entity);

    /// <summary>
    /// A function used to generate tooltip entity.
    /// Used when the user points on an entity with a tooltip text and show present it.
    /// </summary>
    /// <param name="entity">The entity instance the tooltip came from.</param>
    public delegate Entity GenerateTooltipFunc(Entity entity);

    /// <summary>
    /// Callback to generate default paragraph type for internal entities.
    /// </summary>
    /// <param name="text">Paragraph starting text.</param>
    /// <param name="anchor">Paragraph anchor.</param>
    /// <param name="color">Optional fill color.</param>
    /// <param name="scale">Optional scale.</param>
    /// <param name="size">Optional size.</param>
    /// <param name="offset">Optional offset</param>
    /// <returns></returns>
    public delegate Paragraph DefaultParagraphGenerator(string text, Anchor anchor, Color? color = null, float? scale = null, Vector2? size = null, Vector2? offset = null);

    /// <summary>
    /// Curser styles / types.
    /// </summary>
    public enum CursorType
    {
        /// <summary>Default cursor.</summary>
        Default,

        /// <summary>Pointing hand cursor.</summary>
        Pointer,

        /// <summary>Text-input I-beam cursor.</summary>
        IBeam,
    };

    /// <summary>
    /// Enum with all the built-in themes.
    /// </summary>
    public enum BuiltinThemes
    {
        /// <summary>
        /// Old-school style theme with hi-res textures.
        /// </summary>
        hd,

        /// <summary>
        /// Old-school style theme with low-res textures.
        /// </summary>
        lowres,

        /// <summary>
        /// Clean, editor-like theme.
        /// </summary>
        editor,
    }

    /// <summary>
    /// Main GeonBit.UI class that manage and draw all the UI entities.
    /// This is the main manager you use to update, draw, and add entities to.
    /// </summary>
    public class UserInterface
    {
        /// <summary>Current GeonBit.UI version identifier.</summary>
        public const string VERSION = "3.0.2.0";

        /// <summary>
        /// The currently active user interface instance.
        /// </summary>
        public static UserInterface Active = null;

        // input manager
        static internal InputHelper _input;

        // content manager
        static ContentManager _content;

        // the main render target we render everything on
        RenderTarget2D _renderTarget = null;

        // are we currently in use-render-target mode
        private bool _useRenderTarget = false;

        /// <summary>
        /// If true, GeonBit.UI will not raise exceptions on sanity checks, validations, and errors which are not critical.
        /// For example, trying to select a value that doesn't exist from a list would do nothing instead of throwing exception.
        /// </summary>
        public bool SilentSoftErrors = false;

        /// <summary>
        /// Create a default paragraph instance.
        /// GeonBit.UI entities use this method when need to create a paragraph, so you can override this to change which paragraph type the built-in
        /// entities will use by-default (for example Buttons text, SelectList items, etc.).
        /// </summary>
        static public DefaultParagraphGenerator DefaultParagraph =
            (string text, Anchor anchor, Color? color, float? scale, Vector2? size, Vector2? offset) => {
                if (color != null)
                {
                    return new MulticolorParagraph(text, anchor, color.Value, scale, size, offset);
                }
                return new MulticolorParagraph(text, anchor, size, offset, scale);
            };

        /// <summary>
        /// If true, will draw the UI on a render target before drawing on screen.
        /// This mode is required for some of the features.
        /// </summary>
        public bool UseRenderTarget
        {
            get { return _useRenderTarget; }
            set { _useRenderTarget = value; _renderTarget = null; }
        }

        /// <summary>
        /// Get the main render target all the UI draws on.
        /// </summary>
        public RenderTarget2D RenderTarget
        {
            get { return _renderTarget; }
        }

        /// <summary>
        /// Get the root entity.
        /// </summary>
        public RootPanel Root { get; private set; }

        /// <summary>
        /// Blend state to use when rendering UI.
        /// </summary>
        public BlendState BlendState = BlendState.AlphaBlend;

        /// <summary>
        /// Sampler state to use when rendering UI.
        /// </summary>
        public SamplerState SamplerState = SamplerState.PointClamp;

        // the entity currently being dragged
        Entity _dragTarget;

        // current global scale
        private float _scale = 1f;

        /// <summary>Scale the entire UI and all the entities in it. This is useful for smaller device screens.</summary>
        public float GlobalScale
        {
            get { return _scale; }
            set { _scale = value; Root.MarkAsDirty(); }
        }

        /// <summary>Cursor rendering size.</summary>
        public float CursorScale = 1f;

        /// <summary>Screen width.</summary>
        public int ScreenWidth = 0;

        /// <summary>Screen height.</summary>
        public int ScreenHeight = 0;

        /// <summary>Draw utils helper. Contain general drawing functionality and handle effects replacement.</summary>
        public DrawUtils DrawUtils = null;

        /// <summary>Current active entity, eg last entity user interacted with.</summary>
        public Entity ActiveEntity = null;

        /// <summary>The current target entity, eg what cursor points on. Can be null if cursor don't point on any entity.</summary>
        public Entity TargetEntity { get; private set; }

        /// <summary>Callback to execute when mouse button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnMouseDown = null;

        /// <summary>Callback to execute when mouse button is released over an entity (called once when button is released).</summary>
        public EventCallback OnMouseReleased = null;

        /// <summary>Callback to execute every frame while mouse button is pressed over an entity.</summary>
        public EventCallback WhileMouseDown = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity.</summary>
        public EventCallback WhileMouseHover = null;

        /// <summary>Callback to execute when user clicks on an entity (eg release mouse over it).</summary>
        public EventCallback OnClick = null;

        /// <summary>Callback to execute when any entity value changes (relevant only for entities with value).</summary>
        public EventCallback OnValueChange = null;

        /// <summary>Callback to execute when mouse start hovering over an entity (eg enters its region).</summary>
        public EventCallback OnMouseEnter = null;

        /// <summary>Callback to execute when mouse stop hovering over an entity (eg leaves its region).</summary>
        public EventCallback OnMouseLeave = null;

        /// <summary>Callback to execute when mouse wheel scrolls and an entity is the active entity.</summary>
        public EventCallback OnMouseWheelScroll = null;

        /// <summary>Called when entity starts getting dragged (only if draggable).</summary>
        public EventCallback OnStartDrag = null;

        /// <summary>Called when entity stop getting dragged (only if draggable).</summary>
        public EventCallback OnStopDrag = null;

        /// <summary>Called every frame while entity is being dragged.</summary>
        public EventCallback WhileDragging = null;

        /// <summary>Callback to execute every frame before entity update.</summary>
        public EventCallback BeforeUpdate = null;

        /// <summary>Callback to execute every frame after entity update.</summary>
        public EventCallback AfterUpdate = null;

        /// <summary>Callback to execute every frame before entity is rendered.</summary>
        public EventCallback BeforeDraw = null;

        /// <summary>Callback to execute every frame after entity is rendered.</summary>
        public EventCallback AfterDraw = null;

        /// <summary>Callback to execute every time the visibility property of an entity change.</summary>
        public EventCallback OnVisiblityChange = null;

        /// <summary>Callback to execute every time a new entity is spawned (note: spawn = first time Update() is called on this entity).</summary>
        public EventCallback OnEntitySpawn = null;

        /// <summary>Callback to execute every time an entity focus changes.</summary>
        public EventCallback OnFocusChange = null;

        // cursor texture.
        Texture2D _cursorTexture = null;
        
        // cursor width.
        int _cursorWidth = 32;

        // cursor offset from mouse actual position.
        Point _cursorOffset = Point.Zero;

        // time until we show tooltip text.
        private float _timeUntilTooltip = 0f;

        // the current tooltip entity.
        Entity _tooltipEntity;

        /// <summary>
        /// How long to wait before showing tooltip texts.
        /// </summary>
        public static float TimeToShowTooltipText = 2f;

        /// <summary>Weather or not to draw the cursor.</summary>
        public bool ShowCursor = true;

        /// <summary>
        /// The function used to generate tooltip text on entities.
        /// </summary>
        public GenerateTooltipFunc GenerateTooltipFunc = DefaultGenerateTooltipFunc;

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        /// <param name="theme">Which UI theme to use (see options in Content/GeonBit.UI/themes/). This affect the appearance of all textures and effects.</param>
        static public void Initialize(ContentManager contentManager, string theme = "hd")
        {
            // store the content manager
            _content = contentManager;

            // init resources (textures etc)
            Resources.LoadContent(_content, theme);

            // create a default active user interface
            Active = new UserInterface();
        }

        /// <summary>
        /// Default function we use to generate tooltip text entities.
        /// </summary>
        /// <param name="source">Source entity.</param>
        /// <returns>Entity to use for tooltip text.</returns>
        static private Entity DefaultGenerateTooltipFunc(Entity source)
        {
            // no tooltip text? return null
            if (source.ToolTipText == null) return null;

            // create tooltip paragraph
            var tooltip = new Paragraph(source.ToolTipText, size: new Vector2(500, -1));
            tooltip.AlignToCenter = true;

            // create background for tooltip text
            var background = new ColoredRectangle(new Vector2(1, 1), Anchor.TopCenter);
            tooltip.Background = background;
            background.FillColor = Color.Black;

            // add callback to update tooltip position and background
            tooltip.BeforeDraw += (Entity ent) =>
            {
                var destRect = tooltip.GetActualDestRect();
                var position = _input.MousePosition + new Vector2(-destRect.Width / 2, -destRect.Height - 20);
                tooltip.SetPosition(Anchor.TopLeft, _input.MousePosition + new Vector2(-destRect.Width / 2, -destRect.Height - 20));
                tooltip.Background.SetOffset(new Vector2(-2, -5 - tooltip.GetCharacterActualSize().Y));
            };
            tooltip.CalcTextActualRectWithWrap();
            tooltip.Background.Size = tooltip.GetActualDestRect().Size.ToVector2();
            tooltip.BeforeDraw(tooltip);

            // return tooltip object
            return tooltip;
        }

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        /// <param name="theme">Which UI theme to use. This affect the appearance of all textures and effects.</param>
        static public void Initialize(ContentManager contentManager, BuiltinThemes theme)
        {
            Initialize(contentManager, theme.ToString());
        }

        /// <summary>
        /// Create the user interface instance.
        /// </summary>
        public UserInterface()
        { 
            // sanity test
            if (_content == null)
            {
                throw new Exceptions.InvalidStateException("Cannot create a UserInterface before calling UserInterface.Initialize()!");
            }

            // create draw utils
            DrawUtils = new DrawUtils();

            // create input helper
            _input = new InputHelper();

            // create the root panel
            Root = new RootPanel();

            // set default cursor
            SetCursor(CursorType.Default);
        }

        /// <summary>
        /// Set cursor style.
        /// </summary>
        /// <param name="type">What type of cursor to show.</param>
        public void SetCursor(CursorType type)
        {
            DataTypes.CursorTextureData data = Resources.CursorsData[(int)type];
            SetCursor(Resources.Cursors[type], data.DrawWidth, new Point(data.OffsetX, data.OffsetY));
        }

        /// <summary>
        /// Set cursor graphics from a custom texture.
        /// </summary>
        /// <param name="texture">Texture to use for cursor.</param>
        /// <param name="drawWidth">Width, in pixels to draw the cursor. Height will be calculated automatically to fit texture propotions.</param>
        /// <param name="offset">Cursor offset from mouse position (if not provided will draw cursor with top-left corner on mouse position).</param>
        public void SetCursor(Texture2D texture, int drawWidth = 32, Point? offset = null)
        {
            _cursorTexture = texture;
            _cursorWidth = drawWidth;
            _cursorOffset = offset ?? Point.Zero;
        }

        /// <summary>
        /// Draw the cursor.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw the cursor.</param>
        private void DrawCursor(SpriteBatch spriteBatch)
        {
            // start drawing for cursor
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            // calculate cursor size
            float cursorSize = CursorScale * GlobalScale * ((float)_cursorWidth / (float)_cursorTexture.Width);

            // get cursor position and draw it
            Vector2 cursorPos = _input.MousePosition;
            spriteBatch.Draw(_cursorTexture,
                new Rectangle(
                    (int)(cursorPos.X + _cursorOffset.X * cursorSize), (int)(cursorPos.Y + _cursorOffset.Y * cursorSize),
                    (int)(_cursorTexture.Width * cursorSize), (int)(_cursorTexture.Height * cursorSize)),
                Color.White);

            // end drawing
            spriteBatch.End();
        }

        /// <summary>
        /// Add an entity to screen.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(Entity entity)
        {
            Root.AddChild(entity);
        }

        /// <summary>
        /// Remove an entity from screen.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void RemoveEntity(Entity entity)
        {
            Root.RemoveChild(entity);
        }

        /// <summary>
        /// Remove all entities from screen.
        /// </summary>
        public void Clear()
        {
            Root.ClearChildren();
        }

        /// <summary>
        /// Update the UI manager. This function should be called from your Game 'Update()' function, as early as possible (eg before you update your game state).
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // update input manager
            _input.Update(gameTime);

            // unset the drag target if the mouse was released
            if (_dragTarget != null && !_input.MouseButtonDown(MouseButton.Left)) {
              _dragTarget = null;
            }

            // update root panel
            Entity target = null;
            bool wasEventHandled = false;
            Root.Update(_input, ref target, ref _dragTarget, ref wasEventHandled, Point.Zero);

            // set active entity
            if (_input.MouseButtonDown(MouseButton.Left))
            {
                ActiveEntity = target;
            }

            // update tooltip
            UpdateTooltipText(gameTime, target);

            // default active entity is root panel
            ActiveEntity = ActiveEntity ?? Root;

            // set current target entity
            TargetEntity = target;
        }

        /// <summary>
        /// Update tooltip text related stuff.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="target">Current target entity.</param>
        private void UpdateTooltipText(GameTime gameTime, Entity target)
        {
            // if target entity changed, zero time to show tooltip text
            if (TargetEntity != target || target == null)
            {
                _timeUntilTooltip = 0f;
                if (_tooltipEntity != null && _tooltipEntity.Parent != null)
                {
                    _tooltipEntity.RemoveFromParent();
                    _tooltipEntity = null;
                }
            }

            // check if we need to create a tooltip text
            if (_tooltipEntity == null)
            {
                _timeUntilTooltip += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeUntilTooltip > TimeToShowTooltipText)
                {
                    _tooltipEntity = GenerateTooltipFunc(TargetEntity);
                    if (_tooltipEntity != null)
                    {
                        _tooltipEntity.Locked = true;
                        AddEntity(_tooltipEntity);
                    }
                }
            }
        }

        /// <summary>
        /// Draw the UI. This function should be called from your Game 'Draw()' function.
        /// Note: if UseRenderTarget is true, this function should be called FIRST in your draw function.
        /// If UseRenderTarget is false, this function should be called LAST in your draw function.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            int newScreenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int newScreenHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            // update screen size
            if (ScreenWidth != newScreenWidth || ScreenHeight != newScreenHeight)
            {
                ScreenWidth = newScreenWidth;
                ScreenHeight = newScreenHeight;
                Root.MarkAsDirty();
            }

            // if using rendering targets
            if (UseRenderTarget)
            {
                // check if screen size changed or don't have a render target yet. if so, create the render target.
                if (_renderTarget == null ||
                    _renderTarget.Width != ScreenWidth ||
                    _renderTarget.Height != ScreenHeight)
                {
                    if (_renderTarget != null) { _renderTarget.Dispose(); }
                    _renderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice,
                        ScreenWidth, ScreenHeight, false,
                        spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat,
                        spriteBatch.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0,
                        RenderTargetUsage.PreserveContents);
                }
                // if didn't create a new render target, clear it
                else
                {
                    spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
                    spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                }
            }

            // draw root panel
            Root.Draw(spriteBatch);

            // draw cursor
            if (ShowCursor)
            {
                DrawCursor(spriteBatch);
            }

            // reset render target
            if (UseRenderTarget)
            {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);
            }
        }

        /// <summary>
        /// Finalize the draw frame and draw all the UI on screen.
        /// This function only works if we are in UseRenderTarget mode.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        public void DrawMainRenderTarget(SpriteBatch spriteBatch)
        {
            // draw the main render target
            if (RenderTarget != null && !RenderTarget.IsDisposed)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                spriteBatch.Draw(RenderTarget, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
                spriteBatch.End();
            }
        }
    }
}
