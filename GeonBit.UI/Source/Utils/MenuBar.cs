#region File Description
//-----------------------------------------------------------------------------
// Generate menu bar layout.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;


namespace GeonBit.UI.Utils
{
    /// <summary>
    /// A helper class to generate a simple menu bar using panels and dropdown entities.
    /// </summary>
    public static class MenuBar
    {
        /// <summary>
        /// Struct to store params for when a menu item triggers its callback.
        /// </summary>
        public struct MenuCallbackContext
        {
            /// <summary>
            /// Selected menu item index.
            /// </summary>
            public int ItemIndex;

            /// <summary>
            /// Selected menu item text.
            /// </summary>
            public string ItemText;

            /// <summary>
            /// Menu dropdown entity.
            /// </summary>
            public Entities.DropDown Entity;
        }

        /// <summary>
        /// Class used to define the menu layout.
        /// </summary>
        public class MenuLayout
        {
            /// <summary>
            /// A single menu in the menu bar navbar.
            /// </summary>
            internal class Menu
            {
                /// <summary>
                /// Menu title.
                /// </summary>
                public string Title;

                /// <summary>
                /// Menu width.
                /// </summary>
                public float Width;

                /// <summary>
                /// Items under this menu.
                /// </summary>
                public List<string> Items = new List<string>();

                /// <summary>
                /// Actions attached to menu.
                /// </summary>
                public List<System.Action<MenuCallbackContext>> Actions = new List<System.Action<MenuCallbackContext>>();
            }

            /// <summary>
            /// Menus layout.
            /// </summary>
            internal List<Menu> Layout { get; private set; } = new List<Menu>();

            /// <summary>
            /// Add a top menu.
            /// </summary>
            /// <param name="title">Menu title.</param>
            /// <param name="width">Menu width.</param>
            public void AddMenu(string title, float width)
            {
                var newMenu = new Menu()
                {
                    Title = title,
                    Width = width
                };
                Layout.Add(newMenu);
            }

            /// <summary>
            /// Adds an item to a menu.
            /// </summary>
            /// <param name="menuTitle">Menu title to add item to.</param>
            /// <param name="item">Item text.</param>
            /// <param name="onClick">On-click action.</param>
            public void AddItemToMenu(string menuTitle, string item, System.Action onClick)
            {
                AddItemToMenu(menuTitle, item, (MenuCallbackContext ctx) => { onClick(); });
            }

            /// <summary>
            /// Adds an item to a menu with advanced callback.
            /// </summary>
            /// <param name="menuTitle">Menu title to add item to.</param>
            /// <param name="item">Item text.</param>
            /// <param name="onClick">On-click action.</param>
            public void AddItemToMenu(string menuTitle, string item, System.Action<MenuCallbackContext> onClick)
            {
                foreach (var menu in Layout)
                {
                    if (menu.Title == menuTitle)
                    {
                        menu.Items.Add(item);
                        menu.Actions.Add(onClick);
                        return;
                    }
                }
                throw new Exceptions.NotFoundException("Menu with title '" + menuTitle + "' is not defined!");
            }
        }

        /// <summary>
        /// Create the menu bar and return the root panel.
        /// The result would be a panel containing a group of dropdown entities, which implement the menu layout.
        /// The id of every dropdown is "menu-[menu-title]".
        /// Note: the returned menu bar panel comes without parent, you need to add it to your UI tree manually.
        /// </summary>
        /// <param name="layout">Layout to create menu bar with.</param>
        /// <param name="skin">Skin to use for panels and dropdown of this menu.</param>
        /// <returns>Menu root panel.</returns>
        static public Entities.Panel Create(MenuLayout layout, Entities.PanelSkin skin = Entities.PanelSkin.Simple)
        {
            // create the root panel
            var height = (int)Entities.DropDown.DefaultSelectedPanelStyle.GetStyleProperty("DefaultSize", EntityState.Default).asVector.Y;
            if (height <= 1) { height = Entities.DropDown.DefaultSelectedTextPanelHeight; }
            var rootPanel = new Entities.Panel(new Vector2(0, height), skin, Entities.Anchor.TopLeft);
            rootPanel.PriorityBonus = 10000;
            rootPanel.Padding = Vector2.Zero;

            // create menus
            foreach (var menu in layout.Layout)
            {
                // create dropdown and all its items
                var dropdown = new Entities.DropDown(new Vector2(menu.Width, -1), Entities.Anchor.AutoInline, null, Entities.PanelSkin.None, skin, false);
                rootPanel.AddChild(dropdown);
                foreach (var item in menu.Items)
                {
                    dropdown.AddItem(item);
                }
                dropdown.AutoSetListHeight = true;

                // set menu title and id
                dropdown.DefaultText = menu.Title;
                dropdown.Identifier = "menu-" + menu.Title;

                // disable dropdown selection (will only trigger event and unselect)
                dropdown.DontKeepSelection = true;

                // set callbacks
                for (int i = 0; i < menu.Items.Count; ++i)
                {
                    var callback = menu.Actions[i];
                    if (callback != null)
                    {
                        var context = new MenuCallbackContext() { ItemIndex = i, ItemText = menu.Items[i], Entity = dropdown };
                        dropdown.OnSelectedSpecificItem(menu.Items[i], () => { callback(context); });
                    }
                }
            }

            // return the root panel
            return rootPanel;
        }
    }
}
