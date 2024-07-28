namespace UPCI.DAL.DTO.Response
{
    public class OcelotConfig
    {
        public GlobalConfiguration? GlobalConfiguration { get; set; }
        public List<ORoute>? Routes { get; set; }
    }

    public class GlobalConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
    }


    public class ORoute
    {
        public string _comment { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string DownstreamPathTemplate { get; set; } = string.Empty;
        public string DownstreamScheme { get; set; } = string.Empty;
        public List<DownstreamHostAndPorts>? DownstreamHostAndPorts { get; set; }
        public OAuthenticationOptions? AuthenticationOptions { get; set; }
        public string UpstreamPathTemplate { get; set; } = string.Empty;
        public List<string>? UpstreamHttpMethod { get; set; }
        public RateLimit? RateLimitOptions { get; set; }
        public SecurityOptions? SecurityOptions { get; set; }
        public List<string>? Client { get; set; }
        public TimeLimit? TimeLimit { get; set; }
    }

    public class DownstreamHostAndPorts
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }

    public class OAuthenticationOptions
    {
        public string AuthenticationProviderKey { get; set; } = "Bearer";
        public List<string>? AllowedScopes { get; set; } = default!;
    }
    public class RateLimit
    {
        public List<string>? ClientWhitelist { get; set; } = default!;
        public bool EnableRateLimiting { get; set; } = false;
        public string Period { get; set; } = string.Empty;
        public int PeriodTimespan { get; set; }
        public int Limit { get; set; }
    }

    public class SecurityOptions
    {
        public List<string>? IPBlockedList { get; set; } = default!;
        public List<string>? IPAllowedList { get; set; } = default!;
        public bool ExcludeAllowedFromBlocked { get; set; } = false;

    }
    public class TimeLimit
    {
        public bool EnableTimeLimit { get; set; } = false;
        public string? TimeFrom { get; set; } = default!;
        public string? TimeTo { get; set; } = default!;

    }



}