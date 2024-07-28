using System.ComponentModel.DataAnnotations;


namespace UPCI.DAL.DTO.Request
{
    public class Role : Base
    {
        [Required(AllowEmptyStrings = true)]
        public string EncryptedId { get; set; } = string.Empty;
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public List<Module> Modules { get; set; }
    }
}
