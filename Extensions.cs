using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace youtube
{
    internal static class Extensions
    {
        public static StringBuilder AppendSpace(this StringBuilder sb, string str)
        {
            return sb.Append($"{str} ");
        }
    }
}
