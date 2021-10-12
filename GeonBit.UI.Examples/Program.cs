using System;

namespace GeonBit.UI.Examples
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GeonBitUI_Examples())
            {
                game.Run();
            }
        }
    }
}
