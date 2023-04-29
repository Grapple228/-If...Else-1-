using System.Net.Mail;

namespace Database.Misc;

public static class Extensions
{
    public static bool CheckForNull(this string? str)
    {
        return !(string.IsNullOrWhiteSpace(str) || str.Contains(' '));
    }

    public static bool CheckEmail(this string? str)
    {
        return MailAddress.TryCreate(str, out _);
    }
}