namespace UPCI.DAL.DTO.Response
{
    public class VModule : ListBase
    {
        public List<FModule> Data { get; set; } = [];
    }

    public class FModule : Module
    {
        public bool Deleted { get; set; }
    }
    public class Module : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModuleType { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string DisplayOrder { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string AuditContent { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool Show { get; set; } 

    }
    public class ModuleAccess : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModuleType { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string DisplayOrder { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string AuditContent { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool Show { get; set; }

    }

    public class ModuleAction : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Deleted { get; set; }

    } 
}
