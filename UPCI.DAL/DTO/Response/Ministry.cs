namespace UPCI.DAL.DTO.Response
{
    public class VMinistry: ListBase
    {
        public List<FMinistry> Data { get; set; } = [];
    }

    public class FMinistry : Ministry
    {
        public bool Deleted { get; set; }
    }
    public class Ministry : Base
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
