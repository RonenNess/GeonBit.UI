using System;

namespace GeonBit.UI.MG36
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new GeonBit.UI.Example.GeonBitUI_Examples())
                game.Run();
        }
    }
}
