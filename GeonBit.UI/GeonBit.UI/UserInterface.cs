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
    }

    /// <summary>
    /// Main GeonBit.UI class that manage and draw all the UI entities.
    /// This is the main manager you use to update, draw, and add entities to.
    /// </summary>
    public static class UserInterface
    {
        // input manager
        static internal InputHelper _input;

        // content manager
        static ContentManager _content;

        // the main render target we render everything on
        static RenderTarget2D _renderTarget = null;

        // are we currently in use-render-target mode
        static private bool _useRenderTarget = false;

        /// <summary>
        /// If true, will draw the UI on a render target before drawing on screen.
        /// This mode is required for some of the features.
        /// </summary>
        static public bool UseRenderTarget
        {
            get { return _useRenderTarget; }
            set { _useRenderTarget = value; _renderTarget = null; }
        }

        /// <summary>
        /// Get the main render target all the UI draws on.
        /// </summary>
        public static RenderTarget2D RenderTarget
        {
            get { return _renderTarget; }
        }

        /// <summary>Current GeonBit.UI version identifier.</summary>
        public const string VERSION = "2.0.2.0";

        // root panel that covers the entire screen and everything is added to it
        static RootPanel _root;

        /// <summary>
        /// Get the root entity.
        /// </summary>
        static RootPanel Root
        {
            get { return _root; }
        }

        // the entity currently being dragged
        static Entity _dragTarget;

        // current global scale
        static private float _scale = 1f;

        /// <summary>Scale the entire UI and all the entities in it. This is useful for smaller device screens.</summary>
        static public float GlobalScale
        {
            get { return _scale; }
            set { _scale = value; _root.MarkAsDirty(); }
        }

        /// <summary>Cursor rendering size.</summary>
        static public float CursorScale = 1f;

        /// <summary>Screen width.</summary>
        static public int ScreenWidth = 0;

        /// <summary>Screen height.</summary>
        static public int ScreenHeight = 0;

        /// <summary>Draw utils helper. Contain general drawing functionality and handle effects replacement.</summary>
        static public DrawUtils DrawUtils = null;

        /// <summary>Current active entity, eg last entity user interacted with.</summary>
        static public Entity ActiveEntity = null;

        /// <summary>The current target entity, eg what cursor points on. Can be null if cursor don't point on any entity.</summary>
        static public Entity TargetEntity { get; private set; }

        /// <summary>Callback to execute when mouse button is pressed over an entity (called once when button is pressed).</summary>
        static public EventCallback OnMouseDown = null;

        /// <summary>Callback to execute when mouse button is released over an entity (called once when button is released).</summary>
        static public EventCallback OnMouseReleased = null;

        /// <summary>Callback to execute every frame while mouse button is pressed over an entity.</summary>
        static public EventCallback WhileMouseDown = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity.</summary>
        static public EventCallback WhileMouseHover = null;

        /// <summary>Callback to execute when user clicks on an entity (eg release mouse over it).</summary>
        static public EventCallback OnClick = null;

        /// <summary>Callback to execute when any entity value changes (relevant only for entities with value).</summary>
        static public EventCallback OnValueChange = null;

        /// <summary>Callback to execute when mouse start hovering over an entity (eg enters its region).</summary>
        static public EventCallback OnMouseEnter = null;

        /// <summary>Callback to execute when mouse stop hovering over an entity (eg leaves its region).</summary>
        static public EventCallback OnMouseLeave = null;

        /// <summary>Callback to execute when mouse wheel scrolls and an entity is the active entity.</summary>
        static public EventCallback OnMouseWheelScroll = null;

        /// <summary>Called when entity starts getting dragged (only if draggable).</summary>
        static public EventCallback OnStartDrag = null;

        /// <summary>Called when entity stop getting dragged (only if draggable).</summary>
        static public EventCallback OnStopDrag = null;

        /// <summary>Called every frame while entity is being dragged.</summary>
        static public EventCallback WhileDragging = null;

        /// <summary>Callback to execute every frame before entity update.</summary>
        static public EventCallback BeforeUpdate = null;

        /// <summary>Callback to execute every frame after entity update.</summary>
        static public EventCallback AfterUpdate = null;

        /// <summary>Callback to execute every frame before entity is rendered.</summary>
        static public EventCallback BeforeDraw = null;

        /// <summary>Callback to execute every frame after entity is rendered.</summary>
        static public EventCallback AfterDraw = null;

        /// <summary>Callback to execute every time the visibility property of an entity change.</summary>
        static public EventCallback OnVisiblityChange = null;

        /// <summary>Callback to execute every time a new entity is spawned (note: spawn = first time Update() is called on this entity).</summary>
        static public EventCallback OnEntitySpawn = null;

        // cursor draw settings
        static Texture2D _cursorTexture = null;
        static int _cursorWidth = 32;
        static Point _cursorOffset = Point.Zero;

        /// <summary>Weather or not to draw the cursor.</summary>
        static public bool ShowCursor = true;

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        /// <param name="theme">Which UI theme to use (see options in Content/GeonBit.UI/themes/). This affect the appearance of all textures and effects.</param>
        static public void Initialize(ContentManager contentManager, string theme = "hd")
        {
            // create draw utils
            DrawUtils = new DrawUtils();

            // store the content manager
            _content = contentManager;

            // create input helper
            _input = new InputHelper();

            // create the root panel
            _root = new RootPanel();

            // load textures etc
            Resources.LoadContent(_content, theme);

            // set default cursor
            SetCursor(CursorType.Default);
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
        /// Set cursor style.
        /// </summary>
        /// <param name="type">What type of cursor to show.</param>
        static public void SetCursor(CursorType type)
        {
            int typeI = (int)type;
            DataTypes.CursorTextureData data = Resources.CursorsData[typeI];
            SetCursor(Resources.Cursors[typeI], data.DrawWidth, new Point(data.OffsetX, data.OffsetY));
        }

        /// <summary>
        /// Set cursor graphics from a custom texture.
        /// </summary>
        /// <param name="texture">Texture to use for cursor.</param>
        /// <param name="drawWidth">Width, in pixels to draw the cursor. Height will be calculated automatically to fit texture propotions.</param>
        /// <param name="offset">Cursor offset from mouse position (if not provided will draw cursor with top-left corner on mouse position).</param>
        static public void SetCursor(Texture2D texture, int drawWidth = 32, Point? offset = null)
        {
            _cursorTexture = texture;
            _cursorWidth = drawWidth;
            _cursorOffset = offset ?? Point.Zero;
        }

        /// <summary>
        /// Draw the cursor.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw the cursor.</param>
        static private void DrawCursor(SpriteBatch spriteBatch)
        {
            // start drawing for cursor
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

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
        static public void AddEntity(Entity entity)
        {
            _root.AddChild(entity);
        }

        /// <summary>
        /// Remove an entity from screen.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        static public void RemoveEntity(Entity entity)
        {
            _root.RemoveChild(entity);
        }

        /// <summary>
        /// Update the UI manager. This function should be called from your Game 'Update()' function, as early as possible (eg before you update your game state).
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        static public void Update(GameTime gameTime)
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
            _root.Update(_input, ref target, ref _dragTarget, ref wasEventHandled);

            // set active entity
            if (_input.MouseButtonDown(MouseButton.Left))
            {
                ActiveEntity = target;
            }

            // default active entity is root panel
            ActiveEntity = ActiveEntity ?? _root;

            // set current target entity
            TargetEntity = target;
        }

        /// <summary>
        /// Draw the UI. This function should be called from your Game 'Draw()' function.
        /// Note: if UseRenderTarget is true, this function should be called FIRST in your draw function.
        /// If UseRenderTarget is false, this function should be called LAST in your draw function.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        static public void Draw(SpriteBatch spriteBatch)
        {
            int newScreenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int newScreenHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            // update screen size
            if (ScreenWidth != newScreenWidth || ScreenHeight != newScreenHeight)
            {
                ScreenWidth = newScreenWidth;
                ScreenHeight = newScreenHeight;
                _root.MarkAsDirty();
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
            _root.Draw(spriteBatch);

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
        static public void DrawMainRenderTarget(SpriteBatch spriteBatch)
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
