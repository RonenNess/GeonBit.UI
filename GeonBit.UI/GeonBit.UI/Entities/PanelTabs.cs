#region File Description
//-----------------------------------------------------------------------------
// Panel Tabs is a collection of buttons that attach themselves to the top of the 
// parent panel, and automatically create toggle buttons that switch between sub
// panels. This is a helper function that help to quickly implement UI tabs.
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
    /// A graphical panel or form you can create and add entities to.
    /// Used to group together entities with common logic.
    /// </summary>
    public class PanelTabs : Entity
    {
        /// <summary>Default styling for panel buttons. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultButtonStyle = new StyleSheet();

        /// <summary>Default styling for panel buttons paragraphs. Note: loaded from UI theme xml file.</summary>
        public static StyleSheet DefaultButtonParagraphStyle = new StyleSheet();

        /// <summary>Contains the button and panel of a single tab in the PanelTabs.</summary>
        public class TabData
        {
            /// <summary>The tab panel.</summary>
            public Panel panel;

            /// <summary>The tab top button.</summary>
            public Button button;

            /// <summary>Tab identifier / name.</summary>
            readonly public string name;

            /// <summary>
            /// Create the new tab type.
            /// </summary>
            /// <param name="tabName">Tab name / identifier.</param>
            /// <param name="tabPanel">Tab panel.</param>
            /// <param name="tabButton">Tab button.</param>
            public TabData(string tabName, Panel tabPanel, Button tabButton)
            {
                // store name, panel and button
                name = tabName;
                panel = tabPanel;
                button = tabButton;
            }
        }

        /// <summary>List of tabs data currently in panel tabs.</summary>
        private List<TabData> _tabs = new List<TabData>();

        /// <summary>A special internal panel to hold all the tab buttons.</summary>
        private Panel _buttonsPanel;

        /// <summary>A special internal panel to hold all the panels.</summary>
        private Panel _panelsPanel;

        /// <summary>Internal panel that contains everything.</summary>
        private Panel _internalPanel;

        /// <summary>Currently active tab.</summary>
        TabData _activeTab = null;

        /// <summary>
        /// Create the panel tabs.
        /// </summary>
        public PanelTabs() : base(new Vector2(0, 0), Anchor.TopCenter, Vector2.Zero)
        {
            // update style
            UpdateStyle(DefaultStyle);

            // remove self padding
            Padding = Vector2.Zero;

            // create the internal panel that contains everything
            _internalPanel = new Panel(Vector2.Zero, PanelSkin.None, Anchor.Center);
            _internalPanel.Padding = Vector2.Zero;
            AddChild(_internalPanel);

            // create the panel to hold the tab buttons
            _buttonsPanel = new Panel(Vector2.Zero, PanelSkin.None, Anchor.TopCenter);
            _buttonsPanel.SpaceBefore = _buttonsPanel.SpaceAfter = _buttonsPanel.Padding = Vector2.Zero;
            _internalPanel.AddChild(_buttonsPanel);

            // create the panel to hold the tab panels
            _panelsPanel = new Panel(Vector2.Zero, PanelSkin.None, Anchor.TopCenter, new Vector2(0, 0));
            _panelsPanel.SpaceBefore = _panelsPanel.SpaceAfter = _panelsPanel.Padding = Vector2.Zero;
            _internalPanel.AddChild(_panelsPanel);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch)
        {
            // negate parent's padding
            _internalPanel.Padding = -Parent.Padding;

            // recalculate the size of the panel containing the internal panels
            if (_tabs.Count > 0)
            {
                float buttonsHeight = _tabs[0].button.GetActualDestRect().Height / UserInterface.GlobalScale;
                _buttonsPanel.SetOffset(new Vector2(0, -buttonsHeight));
            }

            // call base draw function
            base.DrawEntity(spriteBatch);
        }

        /// <summary>
        /// Get the currently active tab.
        /// </summary>
        public TabData ActiveTab
        {
            get { return _activeTab; }
        }

        /// <summary>
        /// Select tab to be the currently active tab.
        /// </summary>
        /// <param name="name">Tab identifier to select.</param>
        public void SelectTab(string name)
        {
            // find the right tab and select it
            foreach (TabData tab in _tabs)
            {
                if (tab.name == name)
                {
                    tab.button.Checked = true;
                    return;
                }
            }

            // tab not found?
            throw new System.Exception("Tab not found!");
        }

        /// <summary>
        /// Calculate and return the destination rectangle, eg the space this entity is rendered on.
        /// </summary>
        /// <returns>Destination rectangle.</returns>
        override public Rectangle CalcDestRect()
        {
            // call base calculate dest rect
            Rectangle ret = base.CalcDestRect();

            // make sure the panel tabs are not out of screen boundaries
            if (_tabs.Count > 0)
            {
                // get first button
                Button btn = _tabs[0].button;

                // calculate button dest rect
                Rectangle buttonRect = btn.GetActualDestRect();

                // make sure there's enough room for panel buttons
                if (ret.Y < buttonRect.Height)
                {
                    ret.Y += buttonRect.Height;
                }
            }

            // return updated rectangle
            _destRect = ret;
            return ret;
        }

        /// <summary>
        /// Add a new tab to the panel tabs.
        /// </summary>
        /// <param name="name">Tab name (also what will appear on the panel button).</param>
        /// <param name="panelSkin">Panel skin to use for this panel.</param>
        /// <returns>The new tab we created - contains the panel and the button to switch it.</returns>
        public TabData AddTab(string name, PanelSkin panelSkin = PanelSkin.None)
        {
            // create new panel and button
            Panel newPanel = new Panel(Vector2.Zero, panelSkin, Anchor.TopCenter);
            Button newButton = new Button(name, ButtonSkin.Default, Anchor.AutoInline, new Vector2(-1, -1));

            // create the new tab data
            TabData newTab = new TabData(name, newPanel, newButton);

            // link tab data to panel
            newTab.panel.AttachedData = newTab;

            // set button styles
            newTab.button.UpdateStyle(DefaultButtonStyle);
            newTab.button.ButtonParagraph.UpdateStyle(DefaultButtonParagraphStyle);

            // add new tab to tabs list
            _tabs.Add(newTab);

            // update all button sizes
            float width = 1f / (float)_tabs.Count;
            if (width == 1) { width = 0; }
            foreach (TabData data in _tabs)
            {
                data.button.Size = new Vector2(width, data.button.Size.Y);
            }

            // set button to togglemode and unchecked
            newTab.button.ToggleMode = true;
            newTab.button.Checked = false;

            // set identifiers for panel and button
            newTab.button.Identifier = "tab-button-" + name;
            newTab.panel.Identifier = "tab-panel-" + name;

            // by default all panels are hidden
            newTab.panel.Visible = false;

            // attach callback to newly created button
            newTab.button.OnValueChange = (Entity entity) => 
            {
                // get self as a button
                Button self = (Button)(entity);

                // clear the currently active panel
                Panel prevActive = _activeTab != null ? _activeTab.panel : null;
                _activeTab = null;

                // if we were checked, uncheck all the other buttons
                if (self.Checked)
                {
                    // un-toggle all the other buttons
                    foreach (TabData data in _tabs)
                    {
                        Button iterButton = data.button;
                        if (iterButton != self && iterButton.Checked)
                        {
                            iterButton.Checked = false;
                        }
                    }
                }

                // get the panel associated with this tab button.
                Panel selfPanel = _panelsPanel.Find<Panel>("tab-panel-" + name);

                // show / hide the panel
                selfPanel.Visible = self.Checked;

                // if our new value is checked, set as the currently active tab
                if (self.Checked)
                {
                    _activeTab = (TabData)selfPanel.AttachedData;
                }
                
                // if at this phase there's no active panel, revert by checking self again
                // it could happen if user click the same tab button twice or via code.
                if (_activeTab == null && prevActive == selfPanel)
                {
                    self.Checked = true;
                }

                // invoke change event
                if (self.Checked)
                {
                    DoOnValueChange();
                }
            };

            // add button and panel to their corresponding containers
            _panelsPanel.AddChild(newTab.panel);
            _buttonsPanel.AddChild(newTab.button);

            // if its first button, set it as checked
            if (_tabs.Count == 1)
            {
                newTab.button.Checked = true;
            }

            // set as dirty to recalculate destination rect
            MarkAsDirty();

            // return the newly created tab data (panel + button)
            return newTab;
        }
    }
}
