using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class User : Base
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string? Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string MiddleName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string EncryptedRoleId { get; set; } = string.Empty;

        public byte[]? ImageContent { get; set; }
        public string? ImageType { get; set; } = string.Empty;
    }

    public class UserPassword : Base
    {
        public string Username { get; set; }
        public string New { get; set; }
        public string Current { get; set; }
        public string Confirm { get; set; }

    }
}
