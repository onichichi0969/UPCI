namespace UPCI.BLL.Services.IService
{
    public interface IReportService
    {
        Task<UPCI.DAL.DTO.Response.VReportLogHTTP> LogHTTP(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.VReportLogHTTP> LogHTTPListOnly(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.VReportLogTransaction> LogTransaction(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.VReportLogTransaction> LogTransactionListOnly(UPCI.DAL.DTO.Request.FParam model);
    }
}
