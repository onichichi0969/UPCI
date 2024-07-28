using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.Models
{
    [Table("log_http")]
    public class HttpLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string? TraceId { get; set; } = string.Empty;
        public string? ClientId { get; set; } = string.Empty;
        public string? RouteId { get; set; } = string.Empty;
        public string? HttpMethod { get; set; } = string.Empty;
        public string? Uri { get; set; } = string.Empty;
        public string? HttpVersion { get; set; } = string.Empty;
        public string? RequestData { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = default;
        public string? Referrer { get; set; } = string.Empty;
        public string? UserAgent { get; set; } = string.Empty;
        public string? ResponseData { get; set; } = string.Empty;
        public string? ResponseCode { get; set; } = string.Empty;
        public DateTime? ResponseDate { get; set; } = default;

    }
}
