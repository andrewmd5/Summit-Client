using System;
using System.IO;
using System.Text;

namespace CodeUSAClient
{
    internal static class Debug
    {
        private static FileStream debugLogStream = new FileStream("./DebugLog.log", FileMode.Create);

        public static void PrintLine(string text)
        {
            text = "[" + DateTime.Now.ToShortTimeString() + "] " + text;
            Console.WriteLine(text);
            if (Settings.GetValue<bool>("Client.EnableDebug"))
            {
                var bytes = Encoding.ASCII.GetBytes(text + "\n");
                debugLogStream.Write(bytes, 0, bytes.Length);
                debugLogStream.Flush();
            }
        }
    }
}