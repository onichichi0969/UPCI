using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("Role")]
    public class Role : Base
    {
        [Key]
        public int Id { get; set; } = 0;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
        public ICollection<RoleModule> RoleModule { get; set; }
    }

    [Table("RoleModule")]
    public class RoleModule
    {
        [Key]
        public long Id { get; set; } 
        public string? RoleModuleCode { get; set; } = string.Empty;
        public string? ModuleCode { get; set; } = string.Empty;
        public string? Actions { get; set; } = string.Empty;  

        [ForeignKey("RoleModuleCode")]
        public Role Role { get; set; }
         
        [ForeignKey("ModuleCode")]
        public Module Module { get; set; }
    }
}
