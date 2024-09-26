namespace MakeListToNgen.Utils
{
    internal static class StringUtils
    {
        public static bool EqualAsId(this string? original, string? other) =>
            string.Equals(original, other, StringComparison.OrdinalIgnoreCase);
    }
}
