using System;
using System.Linq;

namespace Utf8Utility.Benchmarks.Helpers
{
    static class StringHelper
    {
        static readonly Random Random = new();

        public static string RandomString(int length)
        {
            const string Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var array = Enumerable.Repeat(Table, length)
                .Select(x => x[Random.Next(x.Length)])
                .ToArray();
            return new string(array);
        }
    }
}