using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.Models
{
    [Table("log_transaction")]
    public class TransactionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Traceid { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string UrlEndpoint { get; set; } = string.Empty;
        public string? Request { get; set; } = string.Empty;
        public DateTime? RequestDate { get; set; } = DateTime.Now;
        public string? Response { get; set; } = string.Empty;
        public DateTime? ResponseDate { get; set; } = DateTime.Now;
        public string? Status { get; set; } = string.Empty;
        public string? CheckSum { get; set; } = string.Empty;

    }
}
