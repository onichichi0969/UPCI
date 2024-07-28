using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.Models
{
    [Table("log_exception")]
    public class ExceptionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string? ModuleName { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public string? Source { get; set; } = string.Empty;
        public string? StackTrace { get; set; } = string.Empty;
        public string? InnerException { get; set; } = string.Empty;
        public DateTime LogDate { get; set; } = DateTime.Now;
    }
}
