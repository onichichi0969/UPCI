using UPCI.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class Base
    {
        [Required]
        [MaxLength(20)]
        public string? OpUser { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string? Terminal { get; set; } = string.Empty;
    }
    public class FParam:Base
    {
        [Required]
        public int PageNum { get; set; } = 0;
        [Required]
        public int PageSize { get; set; } = 0;
        [Required(AllowEmptyStrings = true)]
        public string Prefix { get; set; } = default!;
        [Required]
        public string SortColumn { get; set; } = default!;
        [Required]
        public bool Descending { get; set; } = false!;

        public List<Filter> Filters { get; set; }
    }
 
      
}
