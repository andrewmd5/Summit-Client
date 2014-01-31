using System;
using System.Windows.Forms;

namespace CodeUSAClient
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Console.WriteLine(10f);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartupWindow());
        }
    }
}