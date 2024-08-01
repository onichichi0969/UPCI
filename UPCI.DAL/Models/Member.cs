using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace UPCI.DAL.Models
{
    [Table("Members")]
    public class Member : Base
    {
        [Key]
        public long Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Chapter { get; set; } = string.Empty;
        public int? Sequence { get; set; } = 0;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Gender { get; set; } = string.Empty;
        public string? CivilStatus { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; } = default; 
        public DateTime? BaptismDate { get; set; } = default; 
        public DateTime? FirstAttend { get; set; } = default;
        public bool? Baptized { get; set; } = false;
        public bool? InvolvedToCell { get; set; } = false;
        public bool? ActiveMember { get; set; } = false; 
        public string? PEPSOL { get; set; } = string.Empty;
        public string? MemberType { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? ContactNo { get; set; } = string.Empty; 
        public byte[]? ImageContent { get; set; }
        public string? ImageType { get; set; } = string.Empty;
         
        public virtual ICollection<MemberCell> MemberCell { get; set; } = new List<MemberCell>();
        public virtual ICollection<MemberMinistry> MemberMinistry { get; set; } = new List<MemberMinistry>();
    }
    [Table("MemberCell")]
    public class MemberCell
    {
        [Key] 
        public long Id { get; set; } 
        public string? MemberCode { get; set; } = string.Empty;
        public string? CellCode { get; set; } = string.Empty;
        public string? Position { get; set; } = string.Empty;

        [ForeignKey("CellCode")]
        public virtual Cell Cell { get; set; }

        [ForeignKey("Position")]
        public virtual PositionCell PositionCell { get; set; }
        public virtual Member Member { get; set; } 
    }
    [Table("MemberMinistry")]
    public class MemberMinistry
    {
        [Key]
        public long Id { get; set; }
        public string? MemberCode { get; set; } = string.Empty;
        public string? MinistryCode { get; set; } = string.Empty;
        public string? Position { get; set; } = string.Empty; 

        [ForeignKey("MinistryCode")]
        public virtual Ministry Ministry { get; set; }

        [ForeignKey("Position")]
        public virtual PositionMinistry PositionMinistry { get; set; }
        public virtual Member Member { get; set; }
    }
}
