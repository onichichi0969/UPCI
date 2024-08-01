using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("User")]
    public class User : Base
    {
        [Key]
        public long? Id { get; set; }
        public string? Username { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public int? RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        public int? PasswordAttempt { get; set; } = 0;
        public DateTime? PasswordExpirationDate { get; set; }
        public DateTime? PasswordLastChange { get; set; }
        public bool? DefaultPassword { get; set; } = false;
        public byte[]? ImageContent { get; set; } 
        public string? ImageType { get; set; } = string.Empty;
        public int? Status { get; set; } = 0;

       
        
    }
    [Table("ActiveUser")]
    public class ActiveUser  
    {
        [Key]
        public long? Id { get; set; } 
        public string? Username { get; set; } = string.Empty; 
        public string? Terminal { get; set; } = string.Empty;
        public DateTime? ActivityDate { get; set; }

    }
 
}
