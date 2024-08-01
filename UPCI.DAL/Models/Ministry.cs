using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("Ministry")]
    public class Ministry : Base
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;

        [ForeignKey("DepartmentCode")]
        public Department Department { get; set; }
    }
}
