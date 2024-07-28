using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.DTO.Response
{
    
    public class ActivityLog 
    {
        public long? Id { get; set; }
        public string? UserId { get; set; } = string.Empty;
        public string? ModuleName { get; set; } = string.Empty;
        public string? Action { get; set; } = string.Empty;
        public string? Details { get; set; } = string.Empty;
        public string? LogDate { get; set; } = string.Empty;
    }

    
}
