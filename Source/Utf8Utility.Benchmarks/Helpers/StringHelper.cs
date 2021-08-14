namespace Utf8Utility.Benchmarks.Helpers;

static class StringHelper
{
    public static string RandomString(int length)
    {
        const string Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var array = Enumerable.Repeat(Table, length)
            .Select(x => x[Random.Shared.Next(x.Length)])
            .ToArray();
        return new(array);
    }
}