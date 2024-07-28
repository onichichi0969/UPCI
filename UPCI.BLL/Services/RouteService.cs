 using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL; 
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace UPCI.BLL.Services
{
    public class RouteService(ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Route> routeRepository, IRepository<MapRouteClient> mapRouteClientRepository, IRepository<MapRouteIp> mapRouteIpRepository, ILogService logService) : IRouteService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Route> _routeRepository = routeRepository;
        private readonly IRepository<MapRouteClient> _mapRouteClientRepository = mapRouteClientRepository;
        private readonly IRepository<MapRouteIp> _mapRouteIpRepository = mapRouteIpRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "Route";

        public async Task<List<UPCI.DAL.DTO.Response.Route>> GetAll()
        {
            try
            {
                //List<Filter> filters = new List<Filter>()
                //{
                //    new Filter
                //    {
                //        Property = "Name",
                //        Value = "req",
                //        Operator = "Contains" 

                //    },
                //    new Filter
                //    {
                //        Property = "UpstreamPathTemplate",
                //        Value = "auth",
                //        Operator = "Contains"
                //    },
                //    new Filter
                //    {
                //        Property = "Deleted",
                //        Value = 1,
                //        Operator = "NotEquals"
                //    },
                //};
                //string sqlWhereClause = "Name LIKE '%Req%' AND DownstreamPathTemplate LIKE '%http%' OR [Deleted] <> 1";

                //List<Filter> filterss = SQLWhereClauseParser.Parse(sqlWhereClause); 

                //var expression = PredicateBuilder.And(ExpressionBuilder.GetExpression<Route>(filters)) ;

                //var result = _applicationDbContext.Set<Route>()
                //    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Route>(_mapper.ConfigurationProvider)!.ToList();

                //var expression = ExpressionBuilder.GetExpression<Route>(filters);
                //var result = _applicationDbContext.Set<Route>()
                //    .Where(expression).ProjectTo<UPCI.DAL.DTO.Response.Route>(_mapper.ConfigurationProvider)!.ToList();
                var result = _applicationDbContext.Set<Route>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Route>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }

        public async Task<UPCI.DAL.DTO.Response.Route> ById(string id)
        {
            try
            {
                var result = _applicationDbContext.Company!.FirstOrDefault(b => b.Id.ToString().ToUpper() == id && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.Route>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        }
        public async Task<UPCI.DAL.DTO.Response.VRoute> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<DAL.DTO.Response.FRoute>(model.SortColumn);

                UPCI.DAL.DTO.Response.VRoute vRoute = new();

                var apiClientsList = _applicationDbContext.ApiClient!
                    .Select(ac => new { ac.Id, ac.Username })
                    .ToList();
                var apiClients = apiClientsList.ToDictionary(ac => ac.Id, ac => ac.Username);
                var routes = _applicationDbContext.Route.Include(route => route.MapRouteClients).Include(route => route.MapRouteIps)!.ToList();

                var query = routes
                    .Select(u => new UPCI.DAL.DTO.Response.FRoute
                    {
                        Id = u.Id.ToString(),
                        Name = u.Name,
                        DownstreamScheme = u.DownstreamScheme,
                        DownstreamPathTemplate = u.DownstreamPathTemplate,
                        DownstreamHostAndPorts = u.DownstreamHostAndPorts,
                        AuthenticationProviderKey = u.AuthenticationProviderKey,
                        UpstreamPathTemplate = u.UpstreamPathTemplate,
                        UpstreamHttpMethod = u.UpstreamHttpMethod,
                        ClientWhitelist = string.Join("|", u.MapRouteClients
                                            .Select(mrc =>
                                            {
                                                Guid clientId;
                                                return Guid.TryParse(mrc.ClientId, out clientId) && apiClients.ContainsKey(clientId) ? apiClients[clientId] : mrc.ClientId;
                                            })),
                        ClientWhitelistId = string.Join("|", u.MapRouteClients
                                          .Select(mrc =>
                                          {
                                              return mrc.ClientId;
                                          })),
                        EnableRateLimiting = u.EnableRateLimiting,
                        RatePeriod = u.RatePeriod,
                        RatePeriodTimespan = u.RatePeriodTimespan,
                        RateLimit = u.RateLimit,
                        IPBlockedList = u.IPBlockedList,
                        //IPAllowedList = u.IPAllowedList,

                        IPAllowedList = string.Join("|", u.MapRouteIps
                                          .Select(mrc =>
                                          {
                                              return mrc.Ip;
                                          })),

                        ExcludeAllowedFromBlocked = u.ExcludeAllowedFromBlocked,
                        EnableTimeLimit = u.EnableTimeLimit,
                        TimeFrom = u.TimeFrom,
                        TimeTo = u.TimeTo,
                        CreatedBy = u.CreatedBy,
                        CreatedDate = u.CreatedDate,
                        UpdatedBy = u.UpdatedBy,
                        UpdatedDate = u.UpdatedDate,
                        Deleted = u.Deleted
                    }).AsQueryable();
                
                

                if (model.Filters != null && model.Filters.Count != 0)
                    query = query.Where(ExpressionBuilder.GetExpression<UPCI.DAL.DTO.Response.FRoute>(model.Filters)); 
          

                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }

                vRoute.CurrentPage = model.PageNum;
                vRoute.TotalRecord = query.Count();
                vRoute.TotalPage = (int)Math.Ceiling((double)vRoute.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = query.Skip(recordsToSkip).Take(model.PageSize);

                var result = (from u in pagedQuery
                                  //join company in companyList on u.CompanyId equals company.Id
                              select new UPCI.DAL.DTO.Response.FRoute
                              {
                                  Id = u.Id.ToString(),
                                  Name = u.Name,
                                  DownstreamScheme = u.DownstreamScheme,
                                  DownstreamPathTemplate = u.DownstreamPathTemplate,
                                  DownstreamHostAndPorts = u.DownstreamHostAndPorts,
                                  AuthenticationProviderKey = u.AuthenticationProviderKey,
                                  UpstreamPathTemplate = u.UpstreamPathTemplate,
                                  UpstreamHttpMethod = u.UpstreamHttpMethod,
                                  ClientWhitelist = u.ClientWhitelist,
                                  ClientWhitelistId = u.ClientWhitelistId,
                                  EnableRateLimiting = u.EnableRateLimiting,
                                  RatePeriod = u.RatePeriod, 
                                  RatePeriodTimespan = u.RatePeriodTimespan,
                                  RateLimit = u.RateLimit,
                                  IPBlockedList = u.IPBlockedList,
                                  IPAllowedList = u.IPAllowedList,
                                  ExcludeAllowedFromBlocked = u.ExcludeAllowedFromBlocked,
                                  EnableTimeLimit = u.EnableTimeLimit,
                                  TimeFrom = u.TimeFrom,
                                  TimeTo = u.TimeTo,
                                  CreatedBy = u.CreatedBy,
                                  CreatedDate = u.CreatedDate,
                                  UpdatedBy = u.UpdatedBy,
                                  UpdatedDate = u.UpdatedDate,
                              }
                               ).ToList();

                vRoute.Data = result;

                return await Task.FromResult(vRoute);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }
        public async Task<List<DAL.Models.RouteLogSummary>> RouteSummary()
        {
            try
            {
               
                UPCI.DAL.DTO.Response.VRoute vRoute = new();

                var routes = _applicationDbContext.Route!.AsQueryable();
                var logHttp = _applicationDbContext.HttpLog!.AsQueryable();

                var query = from r in routes
                            join http in logHttp on r.Id.ToString() equals http.RouteId into httpGroup
                            from http in httpGroup.DefaultIfEmpty()
                            group http by new { r.Id, r.Name, r.DownstreamPathTemplate, r.UpstreamPathTemplate, r.CreatedDate } into g 
                            orderby g.Key.CreatedDate ascending
                            select new RouteLogSummary
                            {
                                Name = g.Key.Name,
                                DownstreamPathTemplate = g.Key.DownstreamPathTemplate,
                                UpstreamPathTemplate = g.Key.UpstreamPathTemplate, 
                                Unauthorized = g.Count(x => x != null && x.ResponseCode == "401"),
                                Success = g.Count(x => x != null && x.ResponseCode == "200"),
                                Failed = g.Count(x => x != null && x.ResponseCode != "200" && x.ResponseCode != "401"),
                                Total = g.Count(x => x != null),
                                CreatedDate = g.Key.CreatedDate
                            };

                return await Task.FromResult(query.ToList()); 

            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Route model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var action = "ADD";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Id.ToString().ToUpper();

                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = action
                };

                var data = _applicationDbContext.Route!.FirstOrDefault(d => d.Name.Trim() == model.Name.Trim() || d.UpstreamPathTemplate.Trim() == model.UpstreamPathTemplate.Trim() || d.DownstreamPathTemplate.Trim() == model.DownstreamPathTemplate.Trim());
               
                if (data == null)
                {

                    data = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        DownstreamPathTemplate = model.DownstreamPathTemplate,
                        DownstreamScheme = model.DownstreamScheme,
                        DownstreamHostAndPorts = model.DownstreamHostAndPorts,
                        AuthenticationProviderKey = model.AuthenticationProviderKey,
                        UpstreamPathTemplate = model.UpstreamPathTemplate,
                        UpstreamHttpMethod = model.UpstreamHttpMethod,
                        //ClientWhitelist = model.ClientWhitelist,
                        EnableRateLimiting = model.EnableRateLimiting,
                        RatePeriod = model.RatePeriod,
                        RatePeriodTimespan = model.RatePeriodTimespan,
                        RateLimit = model.RateLimit,
                        IPBlockedList = model.IPBlockedList,
                        IPAllowedList = model.IPAllowedList,
                        ExcludeAllowedFromBlocked = model.ExcludeAllowedFromBlocked,
                        EnableTimeLimit = model.EnableTimeLimit,
                        TimeFrom = model.TimeFrom,
                        TimeTo=model.TimeTo,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    var mapRouteClientsToDelete = _applicationDbContext.MapRouteClient
                       .Where(mrc => mrc.RouteId == data.Id) // Apply the condition
                       .ToList(); // Execute the query and get the entities in memory
                    if (mapRouteClientsToDelete.Any())
                        await _mapRouteClientRepository.RemoveRangeAsync(mapRouteClientsToDelete);

                    var splitClientWhitelist = model.ClientWhitelist.Split('|');
                    var mapRouteClientList = new List<MapRouteClient>();

                    if (splitClientWhitelist.Any())
                    {
                        mapRouteClientList = splitClientWhitelist.Select(clientId => new MapRouteClient
                        {
                            RouteId = data.Id,
                            ClientId = clientId
                        }).ToList();
                    }

                    
                    await _routeRepository.AddAsync(data);
                    await _mapRouteClientRepository.AddRangeAsync(mapRouteClientList);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.CreatedBy!,
                        ActionDate = (DateTime)data.CreatedDate,
                        TableName = _moduleName,
                        OldValues = "",
                        NewValues = EFramework.GetEntityProperties(data)
                    };

                    activityLog.Details = string.Format("[code: {0}] created.", data.Name);

                    result = new UPCI.DAL.DTO.Response.Result() { Status = "SUCCESS", Message = string.Format("{0} created.", _moduleName) };

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                }
                else
                {
                    if(data.Name.Trim() == model.Name)
                        result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("{0} already exist.", model.Name) };
                    else if(data.UpstreamPathTemplate == model.UpstreamPathTemplate)
                        result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("{0} already exist.", model.UpstreamPathTemplate) };
                    else
                        result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("{0} already exist.", model.DownstreamPathTemplate) };
                }

                transactionScope.Complete();

            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Route model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var action = "EDIT";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Id;

                int updateStatus = 0;

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = action
                };

                var data = _applicationDbContext.Route!.Where(d => d.Id == Guid.Parse(model.Id!)).FirstOrDefault();

                if (data != null)
                {
                    var previousName = data.Name.Trim();
                    var previousPath = data.UpstreamPathTemplate.Trim();

                     
                    bool upPathExists = _applicationDbContext.Route!.Any(d => d.Id != data.Id && d.UpstreamPathTemplate.Trim().ToUpper() == model.UpstreamPathTemplate.Trim().ToUpper());
                    bool downPathExists = _applicationDbContext.Route!.Any(d => d.Id != data.Id && d.DownstreamPathTemplate.Trim().ToUpper() == model.DownstreamPathTemplate.Trim().ToUpper());
                    bool nameExists = _applicationDbContext.Route!.Any(d => d.Id != data.Id &&  d.Name.Trim().ToUpper() == model.Name.Trim().ToUpper());
                    updateStatus = 1;

                    if (upPathExists)
                    {
                        updateStatus = -1;
                        result.Status = "FAILED";
                        result.Message = string.Format("{0} [{1}] already exist.", "Upstream Path", model.UpstreamPathTemplate);
                    }
                    if (downPathExists)
                    {
                        updateStatus = -1;
                        result.Status = "FAILED";
                        result.Message = string.Format("{0} [{1}] already exist.", "Downstream Path", model.DownstreamPathTemplate);
                    }
                    if (nameExists)
                    {
                        updateStatus = -1;
                        result.Status = "FAILED";
                        result.Message = string.Format("{0} [{1}] already exist.", "Name", model.Name);
                    }
                        
                }
                else
                {
                    updateStatus = 0;
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }


                if (updateStatus == 1)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data!.Name = model.Name;
                    data.DownstreamPathTemplate = model.DownstreamPathTemplate;
                    data.DownstreamScheme = model.DownstreamScheme;
                    data.DownstreamHostAndPorts = model.DownstreamHostAndPorts;
                    data.AuthenticationProviderKey = model.AuthenticationProviderKey;
                    data.UpstreamPathTemplate = model.UpstreamPathTemplate;
                    data.UpstreamHttpMethod = model.UpstreamHttpMethod;
                    //data.ClientWhitelist = model.ClientWhitelist;
                    data.EnableRateLimiting = model.EnableRateLimiting;
                    data.RatePeriod = model.RatePeriod;
                    data.RatePeriodTimespan = model.RatePeriodTimespan;
                    data.RateLimit = model.RateLimit;
                    data.IPBlockedList = model.IPBlockedList;
                    //data.IPAllowedList = model.IPAllowedList;
                    data.ExcludeAllowedFromBlocked = model.ExcludeAllowedFromBlocked;
                    data.EnableTimeLimit = model.EnableTimeLimit;
                    data.TimeFrom = model.TimeFrom;
                    data.TimeTo = model.TimeTo;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    //Client
                    var mapRouteClientsToDelete = _applicationDbContext.MapRouteClient
                      .Where(mrc => mrc.RouteId == data.Id) // Apply the condition
                      .ToList(); // Execute the query and get the entities in memory
                    if (mapRouteClientsToDelete.Any())
                        await _mapRouteClientRepository.RemoveRangeAsync(mapRouteClientsToDelete);

                    var splitClientWhitelist = model.ClientWhitelist.Split('|');
                    var mapRouteClientList = new List<MapRouteClient>();

                    if (splitClientWhitelist.Any())
                    {
                        mapRouteClientList = splitClientWhitelist.Select(clientId => new MapRouteClient
                        {
                            RouteId = data.Id,
                            ClientId = clientId
                        }).ToList();
                    }

                    //IP
                    var mapRouteIpsToDelete = _applicationDbContext.MapRouteIp
                     .Where(mrc => mrc.RouteId == data.Id) // Apply the condition
                     .ToList(); // Execute the query and get the entities in memory
                    if (mapRouteIpsToDelete.Any())
                        await _mapRouteIpRepository.RemoveRangeAsync(mapRouteIpsToDelete);

                    var splitIp = model.IPAllowedList.Split('|');
                    var mapRouteIp = new List<MapRouteIp>();

                    if (splitIp.Any())
                    {
                        mapRouteIp = splitIp.Select(Ip => new MapRouteIp
                        {
                            RouteId = data.Id,
                            Ip = Ip
                        }).ToList();
                    }



                    //CRUD

                    await _routeRepository.UpdateAsync(data);
                    await _mapRouteClientRepository.AddRangeAsync(mapRouteClientList);
                    await _mapRouteIpRepository.AddRangeAsync(mapRouteIp);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(data)
                    };

                    activityLog.Details = string.Format("[{0}] updated.", model.Name);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} updated.", _moduleName);

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);
                }

                transactionScope.Complete();

            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Route model)
        {
            UPCI.DAL.DTO.Response.Result result = new();
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled); 
            try
            {
                var _action = "DELETE";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Id;

                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = _action
                };

                var data = _applicationDbContext.Route!.FirstOrDefault(l => l.Id.ToString() == model.Id);

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _routeRepository.DeleteAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = "[deleted :false]",
                        NewValues = "[deleted :true]"
                    };

                    activityLog.Details = string.Format("[{0}] deleted.", data.Name);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} deleted.", _moduleName);

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }

                transactionScope.Complete();

            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);

        }


        
        #region Ocelot
       
        public async Task<UPCI.DAL.DTO.Response.OcelotConfig> Config()
        {

            try
            {
                //var result = _applicationDbContext.Set<Route>()
                //    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Route>(_mapper.ConfigurationProvider)!.ToList();

                var result = _applicationDbContext.Set<Route>().Include(c => c.MapRouteClients).Include(i => i.MapRouteIps)
                   .Where(e => e.Deleted != true).ToList();

                UPCI.DAL.DTO.Response.OcelotConfig ocelot = new()
                {
                    GlobalConfiguration = new DAL.DTO.Response.GlobalConfiguration() { BaseUrl = "localhost" }, //must be in appsettings
                    Routes = []
                };

                foreach (var item in result)
                {
                    DAL.DTO.Response.ORoute oRoute = new()
                    {
                        _comment = item.Name,
                        Id = Convert.ToString(item.Id), 
                        DownstreamPathTemplate = item.DownstreamPathTemplate,
                        DownstreamScheme = item.DownstreamScheme,
                        DownstreamHostAndPorts = OcelotHost(item.DownstreamHostAndPorts),
                        AuthenticationOptions = new DAL.DTO.Response.OAuthenticationOptions { AuthenticationProviderKey = item.AuthenticationProviderKey } ,
                        RateLimitOptions = OcelotRateLimit(item.EnableRateLimiting, item.RatePeriod, item.RatePeriodTimespan, item.RateLimit),
                        UpstreamPathTemplate = item.UpstreamPathTemplate,
                        UpstreamHttpMethod = OcelotMethod(item.UpstreamHttpMethod),
                        //SecurityOptions = OcelotSecurityOptions(item.ExcludeAllowedFromBlocked, item.IPBlockedList, item.IPAllowedList),
                        //Client = OcelotClient(item.ClientWhitelist),
                        SecurityOptions = OcelotSecurityOptions(item.ExcludeAllowedFromBlocked, item.IPBlockedList, item.MapRouteIps),
                        
                        Client = OcelotClient(item.MapRouteClients),
                        TimeLimit = OcelotTimeLimit(item.EnableTimeLimit, item.TimeFrom, item.TimeTo),
                    };

                    ocelot.Routes!.Add(oRoute);

                }

                return await Task.FromResult(ocelot);

            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }

        private static List<string>? OcelotMethod(string httpMethod)
        {
            List<string> methods = [];

            if (!String.IsNullOrEmpty(httpMethod))
            {
                foreach (var m in httpMethod.Split("|"))
                {
                    methods.Add(m);
                }

                return methods;
            }
            else
            {
                return null;
            }
        }
        private static DAL.DTO.Response.RateLimit? OcelotRateLimit(bool enableRateLimiting, string period, int timeSpan, int limit)
        {
            DAL.DTO.Response.RateLimit rateLimit;

            if (enableRateLimiting)
            {
                rateLimit = new()
                {
                    EnableRateLimiting = enableRateLimiting,
                    Period = period,
                    PeriodTimespan = timeSpan,
                    Limit = limit
                };
                return rateLimit;
            }
            else
            {
                return null;
            }

        }
        private static List<DAL.DTO.Response.DownstreamHostAndPorts>? OcelotHost(string downstream)
        {
            List<DAL.DTO.Response.DownstreamHostAndPorts> hosts = [];

            if (!String.IsNullOrEmpty(downstream))
            {
                foreach (var h in downstream.Split("|"))
                {
                    DAL.DTO.Response.DownstreamHostAndPorts hp1 = new();
                    try
                    {
                        hp1.Host = h.Split(":")[0];
                        hp1.Port = int.Parse(h.Split(":")[1]);
                    }
                    catch (Exception ex)
                    {
                        hp1.Host = h.Split(":")[0];
                        hp1.Port = 443;
                    }

                    hosts.Add(hp1);
                }
                return hosts;
            }
            else
            {
                return null;
            }

        }
        //private static DAL.DTO.Response.SecurityOptions? OcelotSecurityOptions(bool excluedFromBlocked, string blockList, string whiteList)
        //{
        //    DAL.DTO.Response.SecurityOptions securityOptions;

        //    if (string.IsNullOrEmpty(blockList) && string.IsNullOrEmpty(whiteList))
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        securityOptions = new()
        //        {
        //            ExcludeAllowedFromBlocked = excluedFromBlocked,
        //        };

        //        List<string> white = [];
        //        List<string> block = [];

        //        if (!string.IsNullOrEmpty(whiteList))
        //        {

        //            foreach (var w in whiteList.Split("|"))
        //            {
        //                white.Add(w);
        //            }

        //            securityOptions.IPAllowedList = white;

        //        }

        //        if (!string.IsNullOrEmpty(blockList))
        //        {
        //            foreach (var b in blockList.Split("|"))
        //            {
        //                block.Add(b);
        //            }

        //            securityOptions.IPBlockedList = block;
        //        }

        //        return securityOptions;
        //    }
        //}

        //private static List<string>? OcelotClient(string client)
        //{
        //    List<string> clients = [];

        //    if (!String.IsNullOrEmpty(client))
        //    {
        //        foreach (var m in client.Split("|"))
        //        {
        //            clients.Add(m);
        //        }

        //        return clients;
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}
        private static DAL.DTO.Response.SecurityOptions? OcelotSecurityOptions(bool excluedFromBlocked, string blockList, ICollection<MapRouteIp> whiteList)
        {
            DAL.DTO.Response.SecurityOptions securityOptions;

            if (string.IsNullOrEmpty(blockList) && whiteList == null && whiteList.Count == 0)
            {
                return null;
            }
            else
            {
                securityOptions = new()
                {
                    ExcludeAllowedFromBlocked = excluedFromBlocked,
                };

                List<string> white = [];
                List<string> block = [];

                if (whiteList != null && whiteList.Count != 0)
                {

                    foreach (var w in whiteList)
                    {
                        white.Add(w.Ip);
                    }

                    securityOptions.IPAllowedList = white;

                }

                if (!string.IsNullOrEmpty(blockList))
                {
                    foreach (var b in blockList.Split("|"))
                    {
                        block.Add(b);
                    }

                    securityOptions.IPBlockedList = block;
                }

                return securityOptions;
            }
        }
        private static List<string>? OcelotClient(ICollection<MapRouteClient> mapRouteClients)
        {
            List<string> clients = new();
            if (mapRouteClients != null && mapRouteClients.Count != 0)
            {
                foreach (MapRouteClient routeClient in mapRouteClients) 
                    clients.Add(routeClient.ClientId); 
            }
             
            return clients; 
        }
        private static DAL.DTO.Response.TimeLimit? OcelotTimeLimit(bool enableTimeLimit, string timeFrom, string timeTo)
        {
            DAL.DTO.Response.TimeLimit timeLimit;

            if (enableTimeLimit)
            {
                timeLimit = new()
                {
                    EnableTimeLimit = enableTimeLimit,
                    TimeFrom = timeFrom,
                    TimeTo = timeTo
                };

                return timeLimit;
            }
            else
            {
                return null;
            }
        }

        
        #endregion


    }
}
