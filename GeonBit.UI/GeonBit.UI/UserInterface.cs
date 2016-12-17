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
    /// Curser styles / types.
    /// </summary>
    public enum CursorType
    {
        /// <summary>Default cursor.</summary>
        Default,

        /// <summary>Pointing hand cursor.</summary>
        Pointer
    };

    /// <summary>
    /// Main GeonBit.UI class that manage and draw all the UI entities.
    /// This is the main manager you use to update, draw, and add entities to.
    /// </summary>
    public class UserInterface
    {
        // input manager
        InputHelper _input;

        // content manager
        ContentManager _content;

        /// <summary>Current GeonBit.UI version identifier.</summary>
        public const string VERSION = "1.0.0";

        // root panel that covers the entire screen and everything is added to it
        Panel _root;

        /// <summary>Scale the entire UI and all the entities in it. This is useful for smaller device screens.</summary>
        static public float SCALE = 1.0f;

        /// <summary>Cursor rendering size.</summary>
        static public Vector2 CURSOR_SIZE = new Vector2(36, 36);

        /// <summary>Screen width.</summary>
        static public int ScreenWidth = 0;
        
        /// <summary>Screen height.</summary>
        static public int ScreenHeight = 0;

        /// <summary>Current active entity, eg last entity user interacted with.</summary>
        static public Entity ActiveEntity = null;

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

        // cursor pointer and X offset
        Texture2D _cursor;
        int _cursorOffset;

        /// <summary>Weather or not to draw the cursor.</summary>
        public bool ShowCursor = true;

        /// <summary>
        /// Create the UI manager.
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        public UserInterface(ContentManager contentManager)
        {
            // store the content manager
            _content = contentManager;

            // create input helper
            _input = new InputHelper();

            // create the root panel
            _root = new RootPanel();
        }

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="theme">Which UI theme to use (see options in Content/GeonBit.UI/themes/). This basically affect the appearance of all textures and effects.</param>
        public void Initialize(string theme = "hd")
        {
            // load textures etc
            Resources.LoadContent(_content, theme);

            // set default cursor
            SetCursor(CursorType.Default);
        }

        /// <summary>
        /// Set cursor style.
        /// </summary>
        /// <param name="type">What type of cursor to show.</param>
        public void SetCursor(CursorType type)
        {
            switch (type)
            {
                case CursorType.Default:
                    _cursor = Resources.CursorDefault;
                    _cursorOffset = 0;
                    break;

                case CursorType.Pointer:
                    _cursor = Resources.CursorHand;
                    _cursorOffset = -10;
                    break;
            }
        }

        /// <summary>
        /// Draw the cursor.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw the cursor.</param>
        private void DrawCursor(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            Vector2 cursorPos = _input.MousePosition;
            spriteBatch.Draw(_cursor, 
                                new Rectangle((int)cursorPos.X + _cursorOffset, (int)cursorPos.Y, (int)CURSOR_SIZE.X, (int)CURSOR_SIZE.Y), 
                                Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Add an entity to screen.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(Entity entity)
        {
            _root.AddChild(entity);
        }

        /// <summary>
        /// Remove an entity from screen.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void RemoveEntity(Entity entity)
        {
            _root.RemoveChild(entity);
        }

        /// <summary>
        /// Update the UI manager. This function should be called from your Game 'Update()' function, as early as possible (eg before you update your game state).
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // update input manager
            _input.Update(gameTime);

            // update root panel
            Entity dragTarget = null;
            Entity target = null;
            bool wasEventHandled = false;
            _root.Update(_input, ref target, ref dragTarget, ref wasEventHandled);

            // set active entity
            if (_input.MouseButtonDown(MouseButton.Left))
            {
                ActiveEntity = target;
            }

            // default active entity is root panel
            ActiveEntity = ActiveEntity ?? _root;
        }

        /// <summary>
        /// Draw the UI. This function should be called from your Game 'Draw()' function, as late as possible (eg after you draw all your game graphics).
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // update screen size
            ScreenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            ScreenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            
            // draw root panel
            _root.Draw(spriteBatch);

            // draw cursor
            if (ShowCursor)
            {
                DrawCursor(spriteBatch);
            }
        }
    }
}
