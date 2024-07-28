using UPCI.DAL.Models;

namespace UPCI.BLL.Services.IService
{
    public interface ILogService
    {
        void LogActivity(ActivityLog model);
        void LogAudit(AuditTrail model);
        void LogException(Exception ex, string moduleName);
        void LogTransaction(TransactionLog model, string type);
        void LogHttp(HttpLog model, string type);

    }
}
