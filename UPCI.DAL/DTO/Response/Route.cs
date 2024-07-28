using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.DTO.Response
{
    public class VRoute : ListBase
    {
        public List<FRoute> Data { get; set; } = [];
    }

    public class FRoute : Route
    {
        public bool Deleted { get; set; }
    }
    public class Route : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DownstreamPathTemplate { get; set; } = string.Empty;
        public string DownstreamScheme { get; set; } = string.Empty;
        public string AuthenticationProviderKey { get; set; } = string.Empty;
        public string DownstreamHostAndPorts { get; set; } = string.Empty;
        public string UpstreamPathTemplate { get; set; } = string.Empty;
        public string UpstreamHttpMethod { get; set; } = string.Empty;
        public string ClientWhitelist { get; set; } = string.Empty;
        public string ClientWhitelistId { get; set; } = string.Empty;
        public bool EnableRateLimiting { get; set; } = false;
        public string RatePeriod { get; set; } = string.Empty;
        public int RatePeriodTimespan { get; set; } = 0;
        public int RateLimit { get; set; } = 0;
        public string IPBlockedList { get; set; } = string.Empty;
        public string IPAllowedList { get; set; } = string.Empty;
        public bool ExcludeAllowedFromBlocked { get; set; } = false;
        public bool EnableTimeLimit { get; set; } = false;
        public string TimeFrom { get; set; } = string.Empty;
        public string TimeTo { get; set; } = string.Empty;
    }
}
