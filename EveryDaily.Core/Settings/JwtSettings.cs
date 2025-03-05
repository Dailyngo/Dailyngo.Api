namespace EveryDaily.Core.Settings;

public class JwtSettings
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Secret { get; set; }
    public int Ttl { get; set; }
    public int RefreshTtl { get; set; }
}