using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class Member : Base
    { 
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Id { get; set; }
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Chapter { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Sequence { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string MiddleName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string CivilStatus { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(150)]
        public string Address { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Birthday { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string BaptismDate { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string FirstAttend { get; set; } = string.Empty;
        public bool ActiveMember { get; set; } = false;
        public bool Baptized { get; set; } = false;
        public bool InvolvedToCell { get; set; } = false;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string PEPSOL { get; set; } = string.Empty;


        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string MemberType { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string ContactNo { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string? ImageContent { get; set; }
        public string ImageType { get; set; } = string.Empty;
        public bool ImageChanged { get; set; } = false;

        public bool CellChanged { get; set; } = false;
        public bool MinistryChanged { get; set; } = false;
        public ICollection<MemberCells> Cells { get; set; }
        public ICollection<MemberMinistries> Ministries { get; set; }

    }
    public class MemberCells
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string CellCode { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string PositionCellCode { get; set; } = string.Empty;
    }
    public class MemberMinistries
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string MinistryCode { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string PositionMinistryCode { get; set; } = string.Empty;
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;
    }
}
