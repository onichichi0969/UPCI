using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    public class Base
    {
        public string? CreatedBy { get; set; } = default!; 
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; } = default!; 
        public DateTime? UpdatedDate { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
