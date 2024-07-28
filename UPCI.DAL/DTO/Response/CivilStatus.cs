using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    public class VCivilStatus : ListBase
    {
        public List<FCivilStatus> Data { get; set; } = [];
    }

    public class FCivilStatus : CivilStatus
    {
        public bool Deleted { get; set; }
    }
    public class CivilStatus : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
    }
}
