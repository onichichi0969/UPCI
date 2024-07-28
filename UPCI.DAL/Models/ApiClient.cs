using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("ApiClient")]
    public class ApiClient : Base
    {
        [Key]
        public Guid? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}
