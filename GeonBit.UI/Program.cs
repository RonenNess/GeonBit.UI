using System;

namespace GeonBit.UI
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GeonBit.UI.Example.GeonBitUI_Examples())
                game.Run();
        }
    }
}
