using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    public class VPEPSOLLevel : ListBase
    {
        public List<FPEPSOLLevel> Data { get; set; } = [];
    }

    public class FPEPSOLLevel : PEPSOL
    {
        public bool Deleted { get; set; }
    }
    public class PEPSOL : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
    }
}
