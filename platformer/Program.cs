using System;
using System.Text.RegularExpressions;

namespace platformer
{
#if WINDOWS || LINUX
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
            //Console.WriteLine(Regex.Replace(System.AppDomain.CurrentDomain.BaseDirectory, @"bin.*", @"levels\temp.txt"));
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
