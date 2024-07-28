using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("Cell")]
    public class Cell : Base
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Leader { get; set; } = string.Empty;
        public DateTime? FormDate { get; set; } = default;
    }
}
