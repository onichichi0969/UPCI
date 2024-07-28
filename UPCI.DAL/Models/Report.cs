using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.Models
{
    public class ReportLogHttp:Base
    {
        public string? Name { get; set; } = string.Empty;
        public string? Company { get; set; } = string.Empty;
        public string? Client { get; set; } = string.Empty;
        public string? TraceId { get; set; } = string.Empty;
        public string? Referrer { get; set; } = string.Empty;
        public string? Agent { get; set; } = string.Empty;
        public string? HttpVersion { get; set; } = string.Empty;
        public string? HttpMethod { get; set; } = string.Empty;
        public string? UpstreamURL { get; set; } = string.Empty;
        public string? Request { get; set; } = string.Empty;
        public string? Response { get; set; } = string.Empty;
        public string? ResponseCode { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; } = default;
        public TimeSpan? TimeTaken { get; set; }
    }
    public class ReportLogTransaction
    {
        public string? Name { get; set; } = string.Empty;
        public string? Company { get; set; } = string.Empty;
        public string? Client { get; set; } = string.Empty;
        public string? TraceId { get; set; } = string.Empty;
        public string? Referrer { get; set; } = string.Empty;
        public string? Agent { get; set; } = string.Empty;
        public string? HttpVersion { get; set; } = string.Empty;
        public string? HttpMethod { get; set; } = string.Empty;
        public string? UpstreamURL { get; set; } = string.Empty;
        public string? Request { get; set; } = string.Empty;
        public string? Response { get; set; } = string.Empty;
        public string? ResponseCode { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public DateTime? RequestDate { get; set; } = default;
        public DateTime? ResponseDate { get; set; } = default;
        public TimeSpan? TimeTaken { get; set; }
    }
}
