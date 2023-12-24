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
using System.Xml.Serialization;


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
    public class UserInterface : System.IDisposable
    {
        /// <summary>Current GeonBit.UI version identifier.</summary>
        public const string VERSION = "4.3.0.2";

        /// <summary>
        /// The currently active user interface instance.
        /// </summary>
        public static UserInterface Active = null!;

        /// <summary>
        /// The object that provide mouse input for GeonBit UI.
        /// By default it uses internal implementation that uses MonoGame mouse input.
        /// If you want to use things like Touch input, you can override and replace this instance
        /// with your own object that emulates mouse input from different sources.
        /// </summary>
        public IMouseInput MouseInputProvider;

        /// <summary>
        /// The object that provide keyboard and typing input for GeonBit UI.
        /// By default it uses internal implementation that uses MonoGame keyboard input.
        /// If you want to use alternative typing methods, you can override and replace this instance
        /// with your own object that emulates keyboard input.
        /// </summary>
        public IKeyboardInput KeyboardInputProvider;

        /// <summary>
        /// Get current game time value.
        /// </summary>
        public GameTime CurrGameTime { get; private set; }

        // content manager
        static ContentManager _content;

        // the main render target we render everything on
        RenderTarget2D _renderTarget = null;

        // are we currently in use-render-target mode
        private bool _useRenderTarget = false;

        // are we currently during deserialization phase?
        internal bool _isDeserializing = false;

        /// <summary>
        /// If true, GeonBit.UI will not raise exceptions on sanity checks, validations, and errors which are not critical.
        /// For example, trying to select a value that doesn't exist from a list would do nothing instead of throwing exception.
        /// </summary>
        public bool SilentSoftErrors = false;
        
        /// <summary>
        /// If true, will add debug rendering to UI.
        /// </summary>
        public bool DebugDraw = false;

        /// <summary>
        /// Create a default paragraph instance.
        /// GeonBit.UI entities use this method when need to create a paragraph, so you can override this to change which paragraph type the built-in
        /// entities will use by-default (for example Buttons text, SelectList items, etc.).
        /// </summary>
        static public DefaultParagraphGenerator DefaultParagraph =
            (string text, Anchor anchor, Color? color, float? scale, Vector2? size, Vector2? offset) => {
                if (color != null)
                {
                    return new RichParagraph(text, anchor, color.Value, scale, size, offset);
                }
                return new RichParagraph(text, anchor, size, offset, scale);
            };

        /// <summary>
        /// If true, will draw the UI on a render target before drawing on screen.
        /// This mode is required for some of the features.
        /// </summary>
        public bool UseRenderTarget
        {
            get { return _useRenderTarget; }
            set { _useRenderTarget = value; DisposeRenderTarget(); }
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
            set { if (_scale != value) { _scale = value; Root.MarkAsDirty(); } }
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
        public Entity ActiveEntity { get; internal set; } = null!;

        /// <summary>The current target entity, eg what cursor points on. Can be null if cursor don't point on any entity.</summary>
        public Entity TargetEntity { get; private set; } = null!;

        /// <summary>Callback to execute when mouse button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnMouseDown = null;

        /// <summary>Callback to execute when right mouse button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnRightMouseDown = null;

        /// <summary>Callback to execute when mouse button is released over an entity (called once when button is released).</summary>
        public EventCallback OnMouseReleased = null;

        /// <summary>Callback to execute every frame while mouse button is pressed over an entity.</summary>
        public EventCallback WhileMouseDown = null;

        /// <summary>Callback to execute every frame while right mouse button is pressed over an entity.</summary>
        public EventCallback WhileRightMouseDown = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity, unless mouse button is down.</summary>
        public EventCallback WhileMouseHover = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity, even if mouse button is down.</summary>
        public EventCallback WhileMouseHoverOrDown = null;

        /// <summary>Callback to execute when user clicks on an entity (eg release mouse over it).</summary>
        public EventCallback OnClick = null;

        /// <summary>Callback to execute when user clicks on an entity with right mouse button (eg release mouse over it).</summary>
        public EventCallback OnRightClick = null;

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

        // current tooltip target entity (eg entity we point on with tooltip).
        Entity _tooltipTargetEntity;

        /// <summary>
        /// How long to wait before showing tooltip texts.
        /// </summary>
        public static float TimeToShowTooltipText = 2f;

        /// <summary>Whether or not to draw the cursor.</summary>
        public bool ShowCursor = true;

        /// <summary>
        /// Optional transformation matrix to apply when drawing with render targets.
        /// </summary>
        public Matrix? RenderTargetTransformMatrix = null;

        /// <summary>
        /// If using render targets, should the curser be rendered inside of it?
        /// If false, cursor will draw outside the render target, when presenting it.
        /// </summary>
        public bool IncludeCursorInRenderTarget = true;

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
            Resources.Reset();
            Resources.Instance.LoadContent(_content, theme);

            // create a default active user interface
            Active = new UserInterface();
        }

        /// <summary>
        /// Dispose unmanaged resources of this user interface.
        /// </summary>
        public void Dispose()
        {
            DisposeRenderTarget();
        }

        /// <summary>
        /// UserInterface destructor.
        /// </summary>
        ~UserInterface()
        {
            Dispose();
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
            tooltip.BackgroundColor = Color.Black;

            // add callback to update tooltip position
            tooltip.BeforeDraw += (Entity ent) =>
            {
                // get dest rect and calculate tooltip position based on size and mouse position
                var destRect = tooltip.GetActualDestRect();
                var position = UserInterface.Active.GetTransformedCursorPos(new Vector2(-destRect.Width / 2, -destRect.Height - 20));

                // make sure tooltip is not out of screen boundaries
                var screenBounds = Active.Root.GetActualDestRect();
                if (position.Y < screenBounds.Top) position.Y = screenBounds.Top;
                if (position.Y > screenBounds.Bottom - destRect.Height) position.Y = screenBounds.Bottom - destRect.Height;
                if (position.X < screenBounds.Left) position.X = screenBounds.Left;
                if (position.X > screenBounds.Right - destRect.Width) position.X = screenBounds.Right - destRect.Width;

                // update tooltip position
                tooltip.SetAnchorAndOffset(Anchor.TopLeft, position / Active.GlobalScale);
            };
            tooltip.CalcTextActualRectWithWrap();
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

            // create default input providers
            MouseInputProvider = new DefaultInputProvider();
            KeyboardInputProvider = new DefaultInputProvider();

            // create draw utils
            DrawUtils = new DrawUtils();

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
            DataTypes.CursorTextureData data = Resources.Instance.CursorsData[(int)type];
            SetCursor(Resources.Instance.Cursors[type], data.DrawWidth, new Point(data.OffsetX, data.OffsetY));
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
        public void DrawCursor(SpriteBatch spriteBatch)
        {
            // start drawing for cursor
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            // calculate cursor size
            float cursorSize = CursorScale * GlobalScale * ((float)_cursorWidth / (float)_cursorTexture.Width);

            // get cursor position and draw it
            Vector2 cursorPos = MouseInputProvider.MousePosition;
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
        public Entity AddEntity(Entity entity)
        {
            return Root.AddChild(entity);
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
            // store current time
            CurrGameTime = gameTime;

            // update input managers
            MouseInputProvider.Update(gameTime);
            if (MouseInputProvider != KeyboardInputProvider) { KeyboardInputProvider.Update(gameTime); }

            // unset the drag target if the mouse was released
            if (_dragTarget != null && !MouseInputProvider.MouseButtonDown(MouseButton.Left)) {
              _dragTarget = null;
            }

            // update root panel
            Entity target = null;
            bool wasEventHandled = false;
            Root.Update(ref target, ref _dragTarget, ref wasEventHandled, Point.Zero);

            // set active entity
            if (MouseInputProvider.MouseButtonDown(MouseButton.Left))
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
            // fix tooltip target to be an actual entity
            while (target != null && target._hiddenInternalEntity)
                target = target.Parent;

            // if target entity changed, zero time to show tooltip text
            if (_tooltipTargetEntity != target || target == null)
            {
                // zero time until showing tooltip text
                _timeUntilTooltip = 0f;

                // if we currently have a tooltip we show, remove it
                if (_tooltipEntity != null && _tooltipEntity.Parent != null)
                {
                    _tooltipEntity.RemoveFromParent();
                    _tooltipEntity = null;
                }
            }

            // set current tooltip target
            _tooltipTargetEntity = target;

            // if we currently not showing any tooltip entity
            if (_tooltipEntity == null)
            {
                // decrease time until showing tooltip
                _timeUntilTooltip += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // if its time to show tooltip text, create it.
                // note: we create even if the target have no tooltip text, to allow our custom function to create default tooltip or generate based on entity type.
                // if the entity should not show tooltip text, the function to generate it should just return null.
                if (_timeUntilTooltip > TimeToShowTooltipText)
                {
                    // create tooltip text entity
                    _tooltipEntity = GenerateTooltipFunc(_tooltipTargetEntity);

                    // if got a result lock it and add to UI
                    if (_tooltipEntity != null)
                    {
                        _tooltipEntity.Locked = true;
                        _tooltipEntity.ClickThrough = true;
                        AddEntity(_tooltipEntity);
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the render target (only if use) and set it to null.
        /// </summary>
        private void DisposeRenderTarget()
        {
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
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
                    // recreate render target
                    DisposeRenderTarget();
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

            // draw cursor (unless using render targets and should draw cursor outside of it)
            if (ShowCursor && (IncludeCursorInRenderTarget || !UseRenderTarget))
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
                // draw render target
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: RenderTargetTransformMatrix);
                spriteBatch.Draw(RenderTarget, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
                spriteBatch.End();
            }

            // draw cursor
            if (ShowCursor && !IncludeCursorInRenderTarget)
            {
                DrawCursor(spriteBatch);
            }
        }

        /// <summary>
        /// Get transformed cursoer position for collision detection.
        /// If have transform matrix and curser is included in render target, will transform cursor position too.
        /// If don't use transform matrix or drawing cursor outside, will not transform cursor position.
        /// </summary>
        /// <returns>Transformed cursor position.</returns>
        public Vector2 GetTransformedCursorPos(Vector2? addVector)
        {
            // default add vector
            addVector = addVector ?? Vector2.Zero;

            // return transformed cursor position
            if (UseRenderTarget && RenderTargetTransformMatrix != null && !IncludeCursorInRenderTarget)
            {
                var matrix = Matrix.Invert(RenderTargetTransformMatrix.Value);
                return MouseInputProvider.TransformMousePosition(matrix) + Vector2.Transform(addVector.Value, matrix);
            }

            // return raw cursor pos
            return MouseInputProvider.MousePosition + addVector.Value;
        }

        /// <summary>
        /// Get xml serializer.
        /// </summary>
        /// <returns>XML serializer instance.</returns>
        virtual protected XmlSerializer GetXmlSerializer()
        {
            return new XmlSerializer(Root.GetType(), Entity._serializableTypes.ToArray());
        }

        /// <summary>
        /// Serialize the whole UI to stream.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="stream">Stream to serialize to.</param>
        public void Serialize(System.IO.Stream stream)
        {
            var writer = GetXmlSerializer();
            writer.Serialize(stream, Root);
        }

        /// <summary>
        /// Deserialize the whole UI from stream.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="stream">Stream to deserialize from.</param>
        public void Deserialize(System.IO.Stream stream)
        {
            // started deserializing..
            _isDeserializing = true;

            // do deserialize
            try
            {
                var reader = GetXmlSerializer();
                Root = (RootPanel)reader.Deserialize(stream);
            }
            // handle errors
            catch
            {
                _isDeserializing = false;
                throw;
            }

            // init after finish deserializing
            _isDeserializing = false;
            Root.InitAfterDeserialize();
        }

        /// <summary>
        /// Serialize the whole UI to filename.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="path">Filename to serialize into.</param>
        public void Serialize(string path)
        {
            System.IO.FileStream file = System.IO.File.Create(path);
            Serialize(file);
            file.Close();
        }

        /// <summary>
        /// Deserialize the whole UI from filename.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="path">Filename to deserialize from.</param>
        public void Deserialize(string path)
        {
            System.IO.FileStream file = System.IO.File.OpenRead(path);
            Deserialize(file);
            file.Close();
        }

    }
}
