using UPCI.DAL.Models;

namespace UPCI.DAL.DTO.Response
{
    public class VCell: ListBase
    {
        public List<FCell> Data { get; set; } = [];
    }

    public class FCell : Ministry
    {
        public bool Deleted { get; set; }
    }
    public class Cell : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
        public string FormDate { get; set; }  
    }
}
