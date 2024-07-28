namespace UPCI.DAL.DTO.Response
{
    public class APISecurityResult
    {
        public string? Status { get; set; } = string.Empty;
        public string? Key { get; set; } = string.Empty;
        public string? Secret { get; set; } = string.Empty;
    }
    public class VApiClient : ListBase
    {
        public List<FApiClient> Data { get; set; } = [];
    }

    public class FApiClient : ApiClient
    {
        public bool Deleted { get; set; }
    }
    public class ApiClient : Base 
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public bool Deleted { get; set; } = false;
    }
}


