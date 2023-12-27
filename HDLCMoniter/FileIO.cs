using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HDLCMoniter
{
    internal static class FileIO
    {
        public static void Write(string data)
        {
            File.AppendAllText(@"./log.txt", data + "\n\n");
        }
    }
}
