namespace UPCI.DAL.DTO.Response
{
    public class VDepartment : ListBase
    {
        public List<FDepartment> Data { get; set; } = [];
    }

    public class FDepartment : Department
    {
        public bool Deleted { get; set; }
    }
    public class Department : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
        public string HeadCode { get; set; } = string.Empty;
        public string HeadDesc { get; set; } = string.Empty;
    }
}
