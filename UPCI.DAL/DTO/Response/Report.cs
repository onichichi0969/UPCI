namespace UPCI.DAL.DTO.Response
{
    public class VReportLogHTTP : ListBase
    {
        public List<ReportLogHTTP> Data { get; set; } = [];
    }
    public class VReportLogTransaction : ListBase
    {
        public List<ReportLogTransaction> Data { get; set; } = [];
    }

    public class ReportLogHTTP
    {
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public string Referrer { get; set; } = string.Empty;
        public string Agent { get; set; } = string.Empty;
        public string HttpVersion { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string UpstreamURL { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestDate { get; set; } = string.Empty;
        public string ResponseDate { get; set; } = string.Empty;
        public string TimeTaken { get; set; } = string.Empty;
    }

    public class ReportLogTransaction
    {
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public string Referrer { get; set; } = string.Empty;
        public string Agent { get; set; } = string.Empty;
        public string HttpVersion { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string UpstreamURL { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestDate { get; set; } = string.Empty;
        public string ResponseDate { get; set; } = string.Empty;
        public string TimeTaken { get; set; } = string.Empty;
    }
}
