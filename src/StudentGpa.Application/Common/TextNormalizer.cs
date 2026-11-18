namespace StudentGpa.Application.Common;

internal static class TextNormalizer
{
    public static string Code(string value) => value.Trim().ToUpperInvariant();
    public static string Name(string value) => value.Trim();
}
