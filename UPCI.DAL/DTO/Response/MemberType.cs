using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    public class VMemberType : ListBase
    {
        public List<FMemberType> Data { get; set; } = [];
    }

    public class FMemberType : MemberType
    {
        public bool Deleted { get; set; }
    }
    public class MemberType : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
    }
}
