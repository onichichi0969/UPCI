using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class Route : Base
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string DownstreamPathTemplate { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string DownstreamScheme { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string AuthenticationProviderKey { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string DownstreamHostAndPorts { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string UpstreamPathTemplate { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string UpstreamHttpMethod { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string ClientWhitelist { get; set; } = string.Empty;

        public bool EnableRateLimiting { get; set; } = false;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(10)]
        public string RatePeriod { get; set; } = string.Empty;

        public int RatePeriodTimespan { get; set; } = 0;
        public int RateLimit { get; set; } = 0;

        [Required(AllowEmptyStrings = true)]
        public string IPBlockedList { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string IPAllowedList { get; set; } = string.Empty;

        public bool ExcludeAllowedFromBlocked { get; set; } = false;
        public bool EnableTimeLimit { get; set; } = false;
        public string TimeFrom { get; set; } = string.Empty;
        public string TimeTo { get; set; } = string.Empty;
    }

}
