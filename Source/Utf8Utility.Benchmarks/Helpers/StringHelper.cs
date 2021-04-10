using System;
using System.Linq;

namespace Utf8Utility.Benchmarks.Helpers
{
    static class StringHelper
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var array = Enumerable.Repeat(Table, length)
                .Select(x => x[random.Next(x.Length)])
                .ToArray();
            return new string(array);
        }
    }
}