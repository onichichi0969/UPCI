using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    //cell
    public class VPositionCell : ListBase
    {
        public List<FPositionCell> Data { get; set; } = [];
    }

    public class FPositionCell : PositionCell
    {
        public bool Deleted { get; set; }
    }
    public class PositionCell : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;  
    }

    //Ministry
    public class VPositionMinistry : ListBase
    {
        public List<FPositionMinistry> Data { get; set; } = [];
    }

    public class FPositionMinistry : PositionMinistry
    {
        public bool Deleted { get; set; }
    }
    public class PositionMinistry : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
