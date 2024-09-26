namespace MakeListToNgen.Utils
{
    internal static class EnumerableExtensions
    {
        public static bool AtLeastOne<T>(this IEnumerable<T> enumerable, Predicate<T> predicate) =>
            !Equals(enumerable.FirstOrDefault(x => predicate(x)), default);
    }
}
