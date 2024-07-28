using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    public class VMember: ListBase
    {
        public List<FMember> Data { get; set; } = [];
    }

    public class FMember : Member
    {
        public bool Deleted { get; set; }
    }
    public class Member : Base
    {
        public string Id { get; set; } 
        public string Code { get; set; } = string.Empty;
        public string Chapter { get; set; } = string.Empty;
        public string Sequence { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;
        public string CivilStatusDesc { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty;
        public string BaptismDate { get; set; } = string.Empty;
        public string FirstAttend { get; set; } = string.Empty;
        public bool Baptized { get; set; } = false;
        public bool InvolvedToCell { get; set; } = false;
        public bool ActiveMember { get; set; } = false; 
        public string PEPSOL { get; set; } = string.Empty;
        public string PEPSOLDesc { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public string MemberTypeDesc { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty; 
        public byte[]? ImageContent { get; set; } 
        public string ImageType { get; set; } = string.Empty;  
    }
}
