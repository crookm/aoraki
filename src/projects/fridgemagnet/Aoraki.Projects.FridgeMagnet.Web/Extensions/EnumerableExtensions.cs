namespace Aoraki.Projects.FridgeMagnet.Web.Extensions;

public static class EnumerableExtensions
{
    public static IDictionary<char, int> CountChars(this IEnumerable<char> chars)
        => chars.GroupBy(c => c).ToDictionary(c => c.Key, c => c.Count());
}