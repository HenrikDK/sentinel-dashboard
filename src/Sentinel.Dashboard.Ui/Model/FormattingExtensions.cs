namespace Sentinel.Dashboard.Ui.Model;

public static class FormattingExtensions
{
    public static string ToMiniChart(this DateTime value, string timeSpan)
    {
        if (timeSpan.EndsWith("hours"))
        {
            return $"{value:dd MMMM HH:mm} - {value.AddHours(1):HH:mm}";
        }

        if (timeSpan.EndsWith("days"))
        {
            return $"{value:dd MMMM HH:mm} - {value.AddDays(1):dd MMMM HH:mm}";
        }

        return "";
    }
    
    public static string ToSha256Base36(this string value)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));  
        return bytes.ToBase36String();
    }
    
    public static string ToBase36String(this byte[] toConvert, bool bigEndian = false)
    {
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
        if (bigEndian) Array.Reverse(toConvert);
        var dividend = new BigInteger(toConvert);
        var builder = new StringBuilder();
        while (dividend != 0)
        {
            BigInteger remainder;
            dividend = BigInteger.DivRem(dividend, 36, out remainder);
            builder.Insert(0, alphabet[Math.Abs(((int)remainder))]);
        }    
        return builder.ToString();
    }
}
