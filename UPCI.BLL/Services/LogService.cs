using UPCI.BLL.Services.IService;
using UPCI.DAL.Models;
using UPCI.DAL;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.Security.AccessControl;

namespace UPCI.BLL.Services
{
    public class LogService(ApplicationDbContext applicationDbContext) : ILogService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

        public void LogActivity(ActivityLog model)
        {
            _applicationDbContext.ActivityLog!.Add(model);
            _applicationDbContext.SaveChanges();
        }

        public void LogAudit(AuditTrail model)
        {
            _applicationDbContext.AuditTrail!.Add(model);
            _applicationDbContext.SaveChanges();
        }
        public void LogException(Exception ex, string moduleName)
        {
            try
            {
                ExceptionLog model = new()
                {
                    ModuleName = moduleName,
                    Message = ex.Message.ToString(),
                    Source = ex.Source!.ToString(),
                    InnerException = (ex.InnerException! == null ? "" : ex.InnerException!.ToString().Replace("'", "''")),
                    StackTrace = (ex.StackTrace! == null ? "" : ex.StackTrace!.ToString()),
                    LogDate = DateTime.Now
                };

                _applicationDbContext.ExceptionLog!.Add(model);
                _applicationDbContext.SaveChanges();
            }
            catch (Exception exs)
            { 

            }

        }

        public void LogTransaction(TransactionLog model, string type)
        {
            try
            {
                if (type == "REQUEST")
                {
                    _applicationDbContext.TransactionLog!.Add(model);
                    _applicationDbContext.SaveChanges();

                }
                else
                {
                    var data = _applicationDbContext.TransactionLog!.FirstOrDefault(l => l.TransactionId.Trim() == model.TransactionId.Trim())!;
                
                    data.Response = model.Response;
                    data.ResponseDate = model.ResponseDate;
                    data.Status = model.Status;
                    data.CheckSum = model.CheckSum;

                    _applicationDbContext.TransactionLog!.Update(data);
                    _applicationDbContext.SaveChanges();

                }
            }
            catch (Exception exs)
            {

            }
        }

        public void LogHttp(HttpLog model, string type)
        {

            if (type == "REQUEST")
            {
                _applicationDbContext.HttpLog!.Add(model);
                _applicationDbContext.SaveChanges();

            }
            else
            {
                var data = _applicationDbContext.HttpLog!.FirstOrDefault(l => l.TraceId!.Trim() == model.TraceId!.Trim())!;
                data.ResponseData = model.ResponseData;
                data.ResponseDate = model.ResponseDate;
                data.ResponseCode = model.ResponseCode;

                _applicationDbContext.HttpLog!.Update(data);
                _applicationDbContext.SaveChanges();

            }
        }




    }
}
