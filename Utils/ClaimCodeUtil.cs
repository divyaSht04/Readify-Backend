namespace Backend.Utils;

public class ClaimCodeUtil
{
    public static string GenerateClaimCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = "READ-" + new string(Enumerable.Repeat(chars, 4)
                       .Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 4)
                       .Select(s => s[random.Next(s.Length)]).ToArray());
        return code;
    }
}