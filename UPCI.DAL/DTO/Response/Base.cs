using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.DTO.Response
{
    public class Result
    {
        public string? Status { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty; 
    }

    public class Base
    {
        public string? CreatedBy { get; set; } = default!;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; } = default!;
        public DateTime? UpdatedDate { get; set; }
    }

    public class ListBase
    {
        public int TotalPage { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
        public int TotalRecord { get; set; } = 0;
    }
}
