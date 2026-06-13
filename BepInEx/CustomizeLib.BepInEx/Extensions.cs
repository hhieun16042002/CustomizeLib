using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public static class Ext
    {
        public static void Swap<T>(ref T a, ref T b) =>
            (b, a) = (a, b);
    }
}
