using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utf8Utility.Helpers
{
    static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(string paramName)
            => throw new ArgumentOutOfRangeException(paramName);
    }
}
