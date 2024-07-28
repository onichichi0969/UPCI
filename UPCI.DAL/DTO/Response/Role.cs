namespace UPCI.DAL.DTO.Response
{
    public class VRole : ListBase
    {
        public List<FRole> Data { get; set; } = [];
    }

    public class FRole : Role
    {
        public bool Deleted { get; set; }
    }
    public class Role : Base
    {
        public string EncryptedId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class RoleModule
    {
        public string EncryptedId { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string EncryptedParentId { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string Icon { get; set; } = default!;
        public string Actions { get; set; } = default!;
        public bool Show { get; set; }

    }
}
