namespace UPCI.DAL.DTO.Response
{
    public class VCompany: ListBase
    {
        public List<FCompany> Data { get; set; } = [];
    }

    public class FCompany : Company
    {
        public bool Deleted { get; set; }
    }
    public class Company : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
