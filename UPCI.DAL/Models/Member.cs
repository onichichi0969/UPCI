using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("Members")]
    public class Member : Base
    {
        [Key]
        public int Id { get; set; }
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
        public virtual List<Cell> Cells { get; set; } = new List<Cell>();
       // public virtual List<Ministry> Ministries { get; set; } = new List<Ministry>();
    }
}
