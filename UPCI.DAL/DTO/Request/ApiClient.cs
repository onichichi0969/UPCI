using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class ApiClient : Base
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CompanyId { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

    }
}
