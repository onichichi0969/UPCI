using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL; 
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static NPOI.HSSF.Util.HSSFColor;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace UPCI.BLL.Services
{
    public class ReportService(IConfiguration configuration, ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Route> routeRepository, IRepository<HttpLog> logHttpRepository, IRepository<ApiClient> apiClientRepository, IRepository<Company> companyRepository, ILogService logService): IReportService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Route> _routeRepository = routeRepository;
        private readonly IRepository<HttpLog> _logHttpRepository = logHttpRepository;
        private readonly IRepository<ApiClient> _apiClientRepository = apiClientRepository;
        private readonly IRepository<Company> _companyRepository = companyRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "Report";
        private readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;

        public async Task<UPCI.DAL.DTO.Response.VReportLogHTTP> LogHTTP(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                UPCI.DAL.DTO.Response.VReportLogHTTP reportHTTPLogSummary = new();
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.ReportLogHttp>(model.SortColumn);

                var route = _applicationDbContext.Route!.AsQueryable();
                var logHttp = _applicationDbContext.HttpLog!.AsQueryable();
                var apiClient = _applicationDbContext.ApiClient!.AsQueryable();
                var company = _applicationDbContext.Company!.AsQueryable();

                var query = (
                             from l in logHttp
                             join r in route on l.RouteId.ToString() equals r.Id.ToString() into lGroup 
                             from r in lGroup.DefaultIfEmpty()
                             join ac in apiClient on l.ClientId equals ac.Id.ToString() into acGroup
                             from ac in acGroup.DefaultIfEmpty()
                             join c in company on ac.CompanyId equals c.Id into cGroup
                             from c in cGroup.DefaultIfEmpty()
                             where r.DownstreamPathTemplate != "/api/auth/authenticate"


                             select new DAL.Models.ReportLogHttp
                             {
                                 Name = r.Name,
                                 Company = c.Description,
                                 Client = ac.Description,
                                 TraceId = l.TraceId,
                                 Referrer = l.Referrer,
                                 Agent = l.UserAgent,
                                 HttpVersion = l.HttpVersion,
                                 HttpMethod = l.HttpMethod,
                                 UpstreamURL = l.Uri,
                                 Request = l.RequestData,
                                 Response = l.ResponseData,
                                 ResponseCode = l.ResponseCode,
                                 Status = l.ResponseCode == "200" ? "SUCCESS" :
                                          l.ResponseCode == "401" ? "BLOCKED" :
                                          l.ResponseCode == "400" ? "ERROR" :
                                          l.ResponseCode == "503" ? "SERVICE UNAVAILABLE" :
                                          string.IsNullOrEmpty(l.ResponseCode) ? "NO RESPONSE" : "UNKNOWN",
                                 RequestDate = l.RequestDate! == null ? Convert.ToDateTime("1900-01-01"): l.RequestDate!, //l.RequestDate!,
                                 ResponseDate = l.ResponseDate!,
                                 TimeTaken = (l.ResponseDate! - (DateTime)l.RequestDate!),
                                 CreatedDate = r.CreatedDate
                             }
                             ).AsQueryable();


                if (model.Filters != null && model.Filters.Count != 0)
                {
                    var dateRange = model.Filters.Where(f => f.IsDate == true).FirstOrDefault();
                    Expression<Func<DAL.Models.ReportLogHttp,bool>> exp = null;
                    if (dateRange != null)
                    { 
                        exp = (t=> t.RequestDate >= Convert.ToDateTime(dateRange.Value) && t.RequestDate <= Convert.ToDateTime(dateRange.Value2));
                        model.Filters.Remove(dateRange); 
                    }
                    if (model.Filters.Count != 0)
                    {
                        if (exp != null)
                            exp = exp.And(ExpressionBuilder.GetExpression<DAL.Models.ReportLogHttp>(model.Filters));
                        else
                            exp = ExpressionBuilder.GetExpression<DAL.Models.ReportLogHttp>(model.Filters);
                    } 
                    query = query.Where(exp); 
                }
                    


                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }
                
                reportHTTPLogSummary.CurrentPage = model.PageNum;
                reportHTTPLogSummary.TotalRecord = query.Count();
                reportHTTPLogSummary.TotalPage = (int)Math.Ceiling((double)reportHTTPLogSummary.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = query.Skip(recordsToSkip).Take(model.PageSize);

                var result = (from q in pagedQuery
                              select new UPCI.DAL.DTO.Response.ReportLogHTTP
                              {
                                  Name = q.Name!,
                                  Company = q.Company!,
                                  Client =  q.Client!,
                                  TraceId = q.TraceId!,  
                                  Referrer = q.Referrer!,
                                  Agent = q.Agent!,
                                  HttpMethod = q.HttpMethod!,
                                  HttpVersion = q.HttpVersion!,  
                                  UpstreamURL = q.UpstreamURL!,
                                  Request = q.Request!,
                                  Response = q.Response!,
                                  ResponseCode = q.ResponseCode!,    
                                  Status = q.Status!,
                                  RequestDate = q.RequestDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                  ResponseDate = q.ResponseDate! == null ? "" : ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  //TimeTaken = TimeSpan.FromSeconds(Convert.ToDouble(q.TimeTaken)).ToString(@"hh\:mm\:ss"),
                                  TimeTaken = q.TimeTaken! == null ? "" :  ((TimeSpan)q.TimeTaken!).ToString(@"hh\:mm\:ss"),
                              }).ToList();
                reportHTTPLogSummary.Data = result;
                return reportHTTPLogSummary;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }

        }
        public async Task<UPCI.DAL.DTO.Response.VReportLogHTTP> LogHTTPListOnly(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                UPCI.DAL.DTO.Response.VReportLogHTTP reportHTTPLogSummary = new();
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.ReportLogHttp>(model.SortColumn);

                var route = _applicationDbContext.Route!.AsQueryable();
                var logHttp = _applicationDbContext.HttpLog!.AsQueryable();
                var apiClient = _applicationDbContext.ApiClient!.AsQueryable();
                var company = _applicationDbContext.Company!.AsQueryable();

                var query = (from l in logHttp
                             join r in route on l.RouteId.ToString() equals r.Id.ToString() into lGroup
                             from r in lGroup.DefaultIfEmpty()
                             join ac in apiClient on l.ClientId equals ac.Id.ToString() into acGroup
                             from ac in acGroup.DefaultIfEmpty()
                             join c in company on ac.CompanyId equals c.Id into cGroup
                             from c in cGroup.DefaultIfEmpty()
                             where r.DownstreamPathTemplate != "/api/auth/authenticate"
                             select new DAL.Models.ReportLogHttp
                             {
                                 Name = r.Name,
                                 Company = c.Description,
                                 Client = ac.Description,
                                 TraceId = l.TraceId,
                                 Referrer = l.Referrer,
                                 Agent = l.UserAgent,
                                 HttpVersion = l.HttpVersion,
                                 HttpMethod = l.HttpMethod,
                                 UpstreamURL = l.Uri,
                                 Request = l.RequestData,
                                 Response = l.ResponseData,
                                 ResponseCode = l.ResponseCode,
                                 Status = l.ResponseCode == "200" ? "SUCCESS" :
                                          l.ResponseCode == "401" ? "BLOCKED" :
                                          l.ResponseCode == "400" ? "ERROR" :
                                          l.ResponseCode == "503" ? "SERVICE UNAVAILABLE" :
                                          string.IsNullOrEmpty(l.ResponseCode) ? "NO RESPONSE" : "UNKNOWN",
                                 RequestDate = l.RequestDate! == null ? Convert.ToDateTime("1900-01-01") : l.RequestDate!, //l.RequestDate!,
                                 ResponseDate = l.ResponseDate!,
                                 TimeTaken = (l.ResponseDate! - (DateTime)l.RequestDate!),
                                 CreatedDate = r.CreatedDate
                             }
                             ).AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                {
                    var dateRange = model.Filters.Where(f => f.IsDate == true).FirstOrDefault();
                    Expression<Func<DAL.Models.ReportLogHttp, bool>> exp = null;
                    if (dateRange != null)
                    {
                        exp = (t => t.RequestDate >= Convert.ToDateTime(dateRange.Value) && t.RequestDate <= Convert.ToDateTime(dateRange.Value2));
                        model.Filters.Remove(dateRange);
                    }
                    if (model.Filters.Count != 0)
                    {
                        if (exp != null)
                            exp = exp.And(ExpressionBuilder.GetExpression<DAL.Models.ReportLogHttp>(model.Filters));
                        else
                            exp = ExpressionBuilder.GetExpression<DAL.Models.ReportLogHttp>(model.Filters);
                    }
                    query = query.Where(exp);
                }


                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }
                 
                var result = (from q in query
                              select new UPCI.DAL.DTO.Response.ReportLogHTTP
                              {
                                  Name = q.Name,
                                  Company = q.Company,
                                  Client = q.Client,
                                  TraceId = q.TraceId,
                                  Referrer = q.Referrer,
                                  Agent = q.Agent,
                                  HttpMethod = q.HttpMethod,
                                  HttpVersion = q.HttpVersion,
                                  UpstreamURL = q.UpstreamURL,
                                  Request = q.Request,
                                  Response = q.Response,
                                  ResponseCode = q.ResponseCode,
                                  Status = q.Status,
                                  RequestDate = ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  ResponseDate = q.ResponseDate! == null ? "" : ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  TimeTaken = q.TimeTaken! == null ? "" : ((TimeSpan)q.TimeTaken!).ToString(@"hh\:mm\:ss"),
                              }).ToList();
                reportHTTPLogSummary.Data = result;
                return reportHTTPLogSummary;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }


        public async Task<UPCI.DAL.DTO.Response.VReportLogTransaction> LogTransaction(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                UPCI.DAL.DTO.Response.VReportLogTransaction reportLogTransaction = new();
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.ReportLogTransaction>(model.SortColumn);

                var route = _applicationDbContext.Route!.AsQueryable();
                var logHttp = _applicationDbContext.HttpLog!.AsQueryable();
                var apiClient = _applicationDbContext.ApiClient!.AsQueryable();
                var company = _applicationDbContext.Company!.AsQueryable();
                var transaction = _applicationDbContext.TransactionLog!.AsQueryable();

                var query = (from r in route
                             join l in logHttp on r.Id.ToString() equals l.RouteId.ToString() into lGroup
                             from l in lGroup.DefaultIfEmpty()
                             join ac in apiClient on l.ClientId equals ac.Id.ToString() into acGroup
                             from ac in acGroup.DefaultIfEmpty()
                             join c in company on ac.CompanyId equals c.Id into cGroup
                             from c in cGroup.DefaultIfEmpty()
                             join tran in transaction on l.TraceId equals tran.Traceid into tranGroup
                             from tran in tranGroup.DefaultIfEmpty()

                             where r.DownstreamPathTemplate != "/api/auth/authenticate"
                                 && tran.Traceid != null && tran.Traceid != ""


                             select new DAL.Models.ReportLogTransaction
                             {
                                 Name = r.Name,
                                 Company = c.Description,
                                 Client = ac.Description,
                                 TraceId = l.TraceId,
                                 Referrer = l.Referrer,
                                 Agent = l.UserAgent,
                                 HttpVersion = l.HttpVersion,
                                 HttpMethod = l.HttpMethod,
                                 UpstreamURL = l.Uri,
                                 Request = l.RequestData,
                                 Response = l.ResponseData,
                                 ResponseCode = l.ResponseCode,
                                 Status = l.ResponseCode == "200" ? "SUCCESS" :
                                          l.ResponseCode == "401" ? "BLOCKED" :
                                          l.ResponseCode == "400" ? "ERROR" :
                                          l.ResponseCode == "503" ? "SERVICE UNAVAILABLE" :
                                          string.IsNullOrEmpty(l.ResponseCode) ? "NO RESPONSE" : "UNKNOWN",
                                 RequestDate = l.RequestDate!,
                                 ResponseDate = l.ResponseDate!,
                                 TimeTaken = (l.ResponseDate! - l.RequestDate!)

                             }
                             ).AsQueryable();


                if (model.Filters != null && model.Filters.Count != 0)
                    query = query.Where(ExpressionBuilder.GetExpression<DAL.Models.ReportLogTransaction>(model.Filters));


                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }

                reportLogTransaction.CurrentPage = model.PageNum;
                reportLogTransaction.TotalRecord = query.Count();
                reportLogTransaction.TotalPage = (int)Math.Ceiling((double)reportLogTransaction.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = query.Skip(recordsToSkip).Take(model.PageSize);

                var result = (from q in pagedQuery
                              select new UPCI.DAL.DTO.Response.ReportLogTransaction
                              {
                                  Name = q.Name,
                                  Company = q.Company,
                                  Client = q.Client,
                                  TraceId = q.TraceId,
                                  Referrer = q.Referrer,
                                  Agent = q.Agent,
                                  HttpMethod = q.HttpMethod,
                                  HttpVersion = q.HttpVersion,
                                  UpstreamURL = q.UpstreamURL,
                                  Request = q.Request,
                                  Response = q.Response,
                                  ResponseCode = q.ResponseCode,
                                  Status = q.Status,
                                  RequestDate = ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  ResponseDate = q.ResponseDate! == null ? "" : ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  //TimeTaken = TimeSpan.FromSeconds(Convert.ToDouble(q.TimeTaken)).ToString(@"hh\:mm\:ss"),
                                  TimeTaken = q.TimeTaken! == null ? "" : ((TimeSpan)q.TimeTaken!).ToString(@"hh\:mm\:ss"),
                              }).ToList();
                reportLogTransaction.Data = result;
                return reportLogTransaction;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }

        }

        public async Task<UPCI.DAL.DTO.Response.VReportLogTransaction> LogTransactionListOnly(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                UPCI.DAL.DTO.Response.VReportLogTransaction reportLogTransaction = new();
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.ReportLogTransaction>(model.SortColumn);

                var route = _applicationDbContext.Route!.AsQueryable();
                var logHttp = _applicationDbContext.HttpLog!.AsQueryable();
                var apiClient = _applicationDbContext.ApiClient!.AsQueryable();
                var company = _applicationDbContext.Company!.AsQueryable();
                var transaction = _applicationDbContext.TransactionLog!.AsQueryable();

                var query = (from r in route
                             join l in logHttp on r.Id.ToString() equals l.RouteId.ToString() into lGroup
                             from l in lGroup.DefaultIfEmpty()
                             join ac in apiClient on l.ClientId equals ac.Id.ToString() into acGroup
                             from ac in acGroup.DefaultIfEmpty()
                             join c in company on ac.CompanyId equals c.Id into cGroup
                             from c in cGroup.DefaultIfEmpty()
                             join tran in transaction on l.TraceId equals tran.Traceid into tranGroup
                             from tran in tranGroup.DefaultIfEmpty()
                             where r.DownstreamPathTemplate != "/api/auth/authenticate"
                                && tran.Traceid != null && tran.Traceid != ""
                             select new DAL.Models.ReportLogTransaction
                             {
                                 Name = r.Name,
                                 Company = c.Description,
                                 Client = ac.Description,
                                 TraceId = l.TraceId,
                                 Referrer = l.Referrer,
                                 Agent = l.UserAgent,
                                 HttpVersion = l.HttpVersion,
                                 HttpMethod = l.HttpMethod,
                                 UpstreamURL = l.Uri,
                                 Request = l.RequestData,
                                 Response = l.ResponseData,
                                 ResponseCode = l.ResponseCode,
                                 Status = l.ResponseCode == "200" ? "SUCCESS" :
                                          l.ResponseCode == "401" ? "BLOCKED" :
                                          l.ResponseCode == "400" ? "ERROR" :
                                          l.ResponseCode == "503" ? "SERVICE UNAVAILABLE" :
                                          string.IsNullOrEmpty(l.ResponseCode) ? "NO RESPONSE" : "UNKNOWN",
                                 RequestDate = (DateTime)l.RequestDate!,
                                 ResponseDate = (DateTime)l.ResponseDate!,
                                 TimeTaken = (l.ResponseDate! - l.RequestDate!)
                             }
                             ).AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                    query = query.Where(ExpressionBuilder.GetExpression<DAL.Models.ReportLogTransaction>(model.Filters));


                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }

                var result = (from q in query
                              select new UPCI.DAL.DTO.Response.ReportLogTransaction
                              {
                                  Name = q.Name,
                                  Company = q.Company,
                                  Client = q.Client,
                                  TraceId = q.TraceId,
                                  Referrer = q.Referrer,
                                  Agent = q.Agent,
                                  HttpMethod = q.HttpMethod,
                                  HttpVersion = q.HttpVersion,
                                  UpstreamURL = q.UpstreamURL,
                                  Request = q.Request,
                                  Response = q.Response,
                                  ResponseCode = q.ResponseCode,
                                  Status = q.Status,
                                  RequestDate = ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  ResponseDate = q.ResponseDate! == null ? "" : ((DateTime)q.RequestDate!).ToString("yyyy-MM-dd HH:mm:ss"),
                                  TimeTaken = q.TimeTaken! == null ? "" : ((TimeSpan)q.TimeTaken!).ToString(@"hh\:mm\:ss"),
                              }).ToList();
                reportLogTransaction.Data = result;
                return reportLogTransaction;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
    }
}
