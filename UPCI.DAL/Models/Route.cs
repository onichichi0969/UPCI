using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace UPCI.DAL.Models
{
    [Table("master_route")]
    public class Route : Base
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DownstreamPathTemplate { get; set; } = string.Empty;
        public string DownstreamScheme { get; set; } = string.Empty;
        public string DownstreamHostAndPorts { get; set; } = string.Empty;
        public string AuthenticationProviderKey { get; set; } = string.Empty;
        public string UpstreamPathTemplate { get; set; } = string.Empty;
        public string UpstreamHttpMethod { get; set; } = string.Empty;
        //public string ClientWhitelist { get; set; } = string.Empty;  
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

        public virtual ICollection<MapRouteClient> MapRouteClients { get; set; } = new List<MapRouteClient>();
        public virtual ICollection<MapRouteIp> MapRouteIps { get; set; } = new List<MapRouteIp>();
    }


    public class RouteLogSummary
    {
        public string? Name { get; set; } = string.Empty;
        public string? DownstreamPathTemplate { get; set; } = string.Empty;
        public string? UpstreamPathTemplate { get; set; } = string.Empty;
        public int? Unauthorized { get; set; } = 0; 
        public int? Success { get; set; } = 0;
        public int? Failed { get; set; } = 0;
        public int? Total { get; set; } = 0;
        public DateTime? CreatedDate { get; set; }
    }
    [Table("map_route_client")]
    public class MapRouteClient 
    {
        public int Id { get; set; } = 0;
        public Guid? RouteId { get; set; }
        public string ClientId { get; set; } = string.Empty;

        public virtual Route Route { get; set; }
    }
    [Table("map_route_ip")]
    public class MapRouteIp
    {
        public int Id { get; set; } = 0;
        public Guid? RouteId { get; set; } 
        public string Ip { get; set; } = string.Empty;
    }
}
