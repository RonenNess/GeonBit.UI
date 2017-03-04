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
    public class DropDown : SelectList
    {
        /// <summary>Default text to show when no value is selected from the list.</summary>
        public string DefaultText = "Click to Select";

        /// <summary>Default style for the dropdown itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default styling for dropdown labels. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default styling for the dropdown currently-selected label. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultSelectedParagraphStyle = new StyleSheet();

        // internal panel and paragraph to show selected value.
        Panel _selectedTextPanel;
        Paragraph _selectedTextParagraph;
        Image _arrowDownImage;

        /// <summary>
        /// Get the selected text panel (what's shown when DropDown is closed).
        /// </summary>
        public Panel SelectedTextPanel
        {
            get { return _selectedTextPanel; }
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

        // set temporarily to true while we render dropdown outlines.
        private bool _isOutlinePass = false;

        // is the list part currently visible or not.
        bool _isListVisible = false;

        /// <summary>
        /// Create the DropDown list.
        /// </summary>
        /// <param name="size">List size (refers to the whole size of the list + the header when dropdown list is opened).</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">Panel skin to use for this DropDown list and header.</param>
        public DropDown(Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground) :
            base(size, anchor, offset, skin)
        {
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

            // setup the callback to show / hide the list when clicking the dropbox
            _selectedTextPanel.OnClick = (Entity self) =>
            {
                _isListVisible = !_isListVisible;
                OnResize();
            };

            // update styles
            UpdateStyle(DefaultStyle);

            // make sure default state is without children, eg list is hidden
            ClearChildren();
        }
        
        /// <summary>
        /// Is the DropDown list currentle opened (visible).
        /// </summary>
        public bool ListVisible {
            get {return _isListVisible;}
            set {_isListVisible = value;}
        }

        /// <summary>
        /// Handle when current value changes. DropDown entity override this to turn the list invisible.
        /// </summary>
        override protected void DoOnValueChange()
        {
            _isListVisible = false;
            base.DoOnValueChange();
        }

        /// <summary>
        /// Get actual destination rect for positioning, which is the size of the DropDown entity when list is closed.
        /// </summary>
        /// <returns>Actual destination rect when DropDown list is closed.</returns>
        override public Rectangle GetActualDestRect()
        {
            // to fix the bug that dropdown mess up auto positions for first frame.
            if (_destRect.Height == 0)
            {
                _destRect = CalcDestRect();
            }

            // get dest rect and fix height value
            Rectangle ret = _destRect;
            ret.Height = (int)(SelectedPanelHeight * UserInterface.GlobalScale);
            return ret;
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
        /// Called when DropDown is resized.
        /// DropDown entity override this function to handle the list when opened.
        /// </summary>
        /// <param name="recalcDestRect"></param>
        protected override void OnResize(bool recalcDestRect = true)
        {
            // if drop down is currently shown:
            if (_isListVisible)
            {
                // recalculate destination rect height
                int extraY = (int)(_selectedTextPanel.Size.Y * UserInterface.GlobalScale);
                _destRect.Height -= extraY;
                _destRectInternal.Height -= extraY;
                
                // call base resize function
                base.OnResize(false);

                // scroll list to selected item
                ScrollToSelected();
            }
            // if drop down is not shown (drop down is closed)
            else
            {
                base.OnResize(false);
            }
        }

        /// <summary>
        /// Draw entity outline. Override in dropdown to indicate when we are in outline rendering pass.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntityOutline(SpriteBatch spriteBatch)
        {
            _isOutlinePass = true;
            base.DrawEntityOutline(spriteBatch);
            _isOutlinePass = false;
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // draw the list only when visible
            if (_isListVisible)
            {
                if (!_isOutlinePass)
                {
                    // first move the dest rect down below the selected item text
                    int extraY = (int)(_selectedTextPanel.Size.Y * UserInterface.GlobalScale);
                    _destRect.Y += extraY;
                    _destRectInternal.Y += extraY;

                    // also remove the selected part height from the total height, so the element size.y will be absolute
                    _destRect.Height -= extraY;
                    _destRectInternal.Height -= extraY;
                }

                // now draw the list part
                base.DrawEntity(spriteBatch);
            }
            // if not currently visible, make sure all the paragraphs and scrollbar are removed so they won't be drawn
            // note: but still calculate dest rect
            else
            {
                ClearChildren();
            }
        }

        /// <summary>
        /// Called for every new paragraph entity created as part of the list, to allow children classes
        /// to add extra processing etc to list labels.
        /// </summary>
        /// <param name="paragraph">The newly created paragraph once ready (after added to list container).</param>
        protected override void OnCreatedListParagraph(Paragraph paragraph)
        {
            paragraph.UpdateStyle(DefaultParagraphStyle);
        }

        /// <summary>
        /// Called after drawing the entity, every frame.
        /// DropDown entity override this function to draw the extra panel with the currently selected value.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        override protected void OnAfterDraw(SpriteBatch spriteBatch)
        {
            // return destination rect to its original position
            _destRect = CalcDestRect();
            _destRectInternal = CalcInternalRect();

            // call base on-after-draw function
            base.OnAfterDraw(spriteBatch);

            // now draw the selected text part...

            // first add the selected text panel as a child
            AddChild(_selectedTextPanel);

            // remove padding and recalculate internal rect
            Vector2 originalPadding = Padding;
            Padding = Vector2.Zero;
            _destRectInternal = CalcInternalRect();

            // set selected text
            _selectedTextParagraph.Text = (SelectedValue ?? DefaultText);

            // draw selected text panel
            _selectedTextPanel.Draw(spriteBatch);

            // return padding to normal and remove selected text panel
            Padding = originalPadding;
            RemoveChild(_selectedTextPanel);
        }

        /// <summary>
        /// Called every frame before update.
        /// DropDown entity override this function to add the selected item panel before doing any updates.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoBeforeUpdate(InputHelper input)
        {
            // add selection panel to self
            AddChild(_selectedTextPanel, false);

            // call base do-before-update
            base.DoBeforeUpdate(input);
        }

        /// <summary>
        /// Called every frame after update.
        /// DropDown entity override this function to close the list if necessary and to remove the selected item panel from self.
        /// </summary>
        /// <param name="input">Input helper instance.</param>
        override protected void DoAfterUpdate(InputHelper input)
        {
            // if list currently visible we want to check if we need to close it
            if (_isListVisible)
            {
                // first calculate dest rect
                _destRect = CalcDestRect();

                // temporarily set to use dest rect and not actual dest rect
                bool prevUseActualSizeForCollision = UseActualSizeForCollision;
                UseActualSizeForCollision = false;

                // check if mouse down and not inside list
                if (input.AnyMouseButtonDown() && !IsInsideEntity(input.MousePosition))
                {
                    _isListVisible = false;
                }

                // return use actual rect state back to normal
                UseActualSizeForCollision = prevUseActualSizeForCollision;
            }

            // remove selection panel from self
            if (_selectedTextPanel.Parent != null)
            {
                RemoveChild(_selectedTextPanel);
            }

            // call base do-before-update
            base.DoAfterUpdate(input);
        }
    }
}
