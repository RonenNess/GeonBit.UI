using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GeonBit.UI.Utils
{
    /// <summary>
    /// Helper class to generate grid-like structure of panels.
    /// </summary>
    public static class PanelsGrid
    {
        /// <summary>
        /// Generate and return a set of panels aligned next to each other with a constant size.
        /// This is useful for cases like when you want to divide your panel into 3 colums.
        /// </summary>
        /// <param name="amount">How many panels to create.</param>
        /// <param name="parent">Optional parent entity to add panels to.</param>
        /// <param name="columnSize">Size of every column. If not set will be 1f / amount with auto-set height.</param>
        /// <param name="skin">Panels skin to use (default to None, making them invisible.</param>
        /// <returns>Array with generated panels.</returns>
        public static Panel[] GenerateColums(int amount, Entity parent, Vector2? columnSize = null, PanelSkin skin = PanelSkin.None)
        {
            // list of panels to return
            List<Panel> retList = new List<Panel>();

            // default column size
            columnSize = columnSize ?? new Vector2(1f / amount, -1);

            // create panels
            for (int i = 0; i < amount; ++i)
            {
                Panel currPanel = new Panel(columnSize.Value, skin, Anchor.AutoInlineNoBreak);
                currPanel.Padding = Vector2.Zero;
                retList.Add(currPanel);
                if (parent != null) { parent.AddChild(currPanel); }
            }

            // return result panels
            return retList.ToArray();
        }

        /// <summary>
        /// Generate and return a set of panels aligned next to each other.
        /// This is useful for cases like when you want to divide your panel into 3 colums.
        /// </summary>
        /// <param name="panelSizes">Array with panel sizes to generate (also determine how many panels to return).</param>
        /// <param name="parent">Optional parent entity to add panels to.</param>
        /// <param name="skin">Panels skin to use (default to None, making them invisible.</param>
        /// <returns>Array with generated panels.</returns>
        public static Panel[] GenerateColums(Vector2[] panelSizes, Entity parent, PanelSkin skin = PanelSkin.None)
        {
            // list of panels to return
            List<Panel> retList = new List<Panel>();

            // create panels
            foreach (var currSize in panelSizes)
            {
                Panel currPanel = new Panel(currSize, skin, Anchor.AutoInlineNoBreak);
                currPanel.Padding = Vector2.Zero;
                retList.Add(currPanel);
                if (parent != null) { parent.AddChild(currPanel); }
            }

            // return result panels
            return retList.ToArray();
        }
    }
}
