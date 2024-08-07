using System.ComponentModel.DataAnnotations.Schema;
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
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;
        public string CivilStatusDesc { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty;
        public string BaptismDate { get; set; } = string.Empty;
        public string FirstAttend { get; set; } = string.Empty;
        public bool Baptized { get; set; } = false; 
        public bool ActiveMember { get; set; } = false; 
        public string PEPSOL { get; set; } = string.Empty;
        public string PEPSOLDesc { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public string MemberTypeDesc { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty; 
        public byte[]? ImageContent { get; set; } 
        public string ImageType { get; set; } = string.Empty;
        public string ImageDataString { get; set; } = string.Empty;
        public ICollection<MemberCell> MemberCell { get; set; }
        public ICollection<MemberMinistry> MemberMinistry { get; set; }
    }
    public class MemberCell
    {
        public string Id { get; set; } = string.Empty;
        public string MemberCode { get; set; } = string.Empty;
        public string MemberDesc { get; set; } = string.Empty;
        public string CellCode { get; set; } = string.Empty;
        public string CellDesc { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PositionDesc { get; set; } = string.Empty;
    }

    public class MemberMinistry
    {
        public string Id { get; set; } = string.Empty;
        public string MemberCode { get; set; } = string.Empty; 
        public string MemberDesc {  get; set; } = string.Empty;
        public string MinistryCode { get; set; } = string.Empty;
        public string MinistryDesc { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PositionDesc { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentDesc { get; set; } = string.Empty;
    }

    /// <summary>
    /// /
    /// </summary>
    public class MemberStatistics
    { 
        public TotalMember Total { get; set; }
        public Age Age { get; set; }
        public ActiveMember ActiveMember { get; set; }
        public Gender Gender { get; set; }
        public InvolveCell InvolveCell { get; set; }
        public InvolveMinistry InvolveMinistry { get; set; }
    }
    public class TotalMember
    {
        public int Total { get; set; } = 0;
    }
    public class Age 
    { 
        public int Children { get; set; } = 0;
        public int Youth { get; set; } = 0;
        public int Adult { get; set; } = 0;
        public int Senior { get; set; } = 0;
    }
    public class ActiveMember
    {
        public int Active { get; set; } = 0;
        public int Inactive { get; set; } = 0;
    }
    public class Gender
    {
        public int Male { get; set; } = 0;
        public int Female { get; set; } = 0;
    }
    public class InvolveCell
    {
        public int Yes { get; set; } = 0;
        public int No { get; set; } = 0;
    }
    public class InvolveMinistry
    {
        public int Yes { get; set; } = 0;
        public int No { get; set; } = 0;
    }
}
