using UPCI.DAL.DTO.Response;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{ 
    [Table("Module")]
    public class Module : Base
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? ModuleType { get; set; } = string.Empty;
        public int? DisplayOrder { get; set; }
        public int? ParentId { get; set; }
        
        public string? Description { get; set; } = string.Empty;
        public string? Url { get; set; } = string.Empty;
        public string? Icon { get; set; } = string.Empty;
        public string? Action { get; set; } = string.Empty;
        public string? AuditContent { get; set; } = string.Empty;
        public bool Show { get; set; } = false;

        [ForeignKey("ParentId")]
        public Module? Parent { get; set; } 
        public ICollection<RoleModule> RoleModules { get; set; }
    }
     

    [Table("ModuleAction")]
    public class ModuleAction : Base
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty; 
    }

    
    //public class ModuleAccess
    //{
    //    public long Id { get; set; }
    //    public string? Code { get; set; } = string.Empty;
    //    public string? Name { get; set; } = string.Empty;
    //    public string? ModuleType { get; set; } = string.Empty;
    //    public int? DisplayOrder { get; set; }
    //    public int? ParentId { get; set; }

    //    public string? Description { get; set; } = string.Empty;
    //    public string? Url { get; set; } = string.Empty;
    //    public string? Icon { get; set; } = string.Empty;
    //    public string? Action { get; set; } = string.Empty;
    //    public string? AuditContent { get; set; } = string.Empty;
    //    public bool Show { get; set; } = false;
    //}
}
