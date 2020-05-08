using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class SystemExtensions
    {
        public static bool Almost(this decimal me, decimal other, decimal precision)
        {
            if (Math.Abs(me - other) > precision)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
