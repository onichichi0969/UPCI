using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UPCI.BLL.Services
{
    public class ApiClientService(ApplicationDbContext applicationDbContext, IConfiguration configuration, IMapper mapper, IRepository<ApiClient> apiClientRepository, ILogService logService) : IApiClientService
    {

        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        //private readonly IConfiguration _configuration = configuration;
        private readonly IRepository<ApiClient> _apiClientRepository = apiClientRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "ApiUser";
        private readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;

        public async Task<UPCI.DAL.DTO.Response.ApiClient> Authenticate(string username, string password)
        {
            try
            {
                var apiUser = _applicationDbContext.ApiClient!.FirstOrDefault(u => u.Username == username
                    && u.Password == StringManipulation.Encrypt(password, _encryptionKey)
                    && u.Deleted != true
                );

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.ApiClient>(apiUser));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        }

        public async Task<UPCI.DAL.DTO.Response.ApiClient> ByApiKey(string key)
        {
            try
            { 
                var result = _applicationDbContext.Set<ApiClient>()!.FirstOrDefault(b => b.ApiKey.ToString() == key && b.Deleted != true);



                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.ApiClient>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.ApiClient>?> GetAll(bool includeDeleted)
        {
            try
            {
                var clientList = _applicationDbContext.Set<ApiClient>(); 

                if (!includeDeleted)
                    clientList.Where(e => e.Deleted != true);


                var companyList = _applicationDbContext.Set<Company>();

                var result = (from u in clientList
                                join company in companyList on u.CompanyId equals company.Id
                                select new UPCI.DAL.DTO.Response.ApiClient
                                {
                                    Id = u.Id.ToString().ToUpper(),
                                    Username = u.Username,
                                    CompanyId = u.CompanyId.ToString(),
                                    CompanyName = company.Name,
                                    CompanyDescription = company.Description,
                                    Description = u.Description,
                                    ApiKey = u.ApiKey,
                                    ApiSecret = u.ApiSecret,
                                    Deleted = u.Deleted
                                }
                                ).ToList();
  
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }

        public async Task<UPCI.DAL.DTO.Response.ApiClient> ById(string id)
        {
            try
            {
                var result = _applicationDbContext.ApiClient!.FirstOrDefault(b => b.Id.ToString().ToUpper() == id && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.ApiClient>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }

        }
        public async Task<UPCI.DAL.DTO.Response.APISecurityResult> Reset(UPCI.DAL.DTO.Request.ApiClient model)
        {
            DAL.DTO.Response.APISecurityResult result = new();
            string newKey = StringManipulation.Random(16), newSecret = StringManipulation.Random(16);
            try
            {
                var action = "RESET";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Id.ToString().ToUpper();

                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = action
                };

                var client = _applicationDbContext.ApiClient!.FirstOrDefault(b => b.Id.ToString().ToUpper() == model.Id );
                if (client != null)
                {

                    client.ApiKey = newKey;
                    client.ApiSecret = newSecret;
                    await _apiClientRepository.UpdateAsync(client);
                    result.Status = "SUCCESS";
                    result.Key = newKey;
                    result.Secret = newSecret;

                }
                else 
                {
                    newKey = "";
                    newSecret = "";
                    result.Status = "FAILED";
                    result.Key = "";
                    result.Secret = "";
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                result.Status = "ERROR";
                result.Key = "";
                result.Secret = "";
                return new();
            }

        }
        public async Task<UPCI.DAL.DTO.Response.VApiClient> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<ApiClient>(model.SortColumn);

                UPCI.DAL.DTO.Response.VApiClient data = new();

                //var query = _applicationDbContext.ApiClient!.AsQueryable();

                var clientList = _applicationDbContext.Set<ApiClient>().AsQueryable();

                
                if (model.Filters != null && model.Filters.Count != 0)
                    clientList = clientList.Where(ExpressionBuilder.GetExpression<ApiClient>(model.Filters));

                //var companyList = _applicationDbContext.Set<Company>();

                if (model.Descending)
                {
                    clientList = clientList.OrderByDescending(propertySelector);
                }
                else
                {
                    clientList = clientList.OrderBy(propertySelector);
                }
                 
                data.CurrentPage = model.PageNum;
                data.TotalRecord = clientList.Count();
                data.TotalPage = (int)Math.Ceiling((double)data.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = clientList.Skip(recordsToSkip).Take(model.PageSize);

                //data.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FApiClient>>(pagedQuery.ToList());


                var result = (from u in pagedQuery
                              //join company in companyList on u.CompanyId equals company.Id
                              select new UPCI.DAL.DTO.Response.FApiClient
                              {
                                  Id = u.Id.ToString().ToUpper(),
                                  Username = u.Username,
                                  CompanyId = u.CompanyId.ToString().ToUpper(),
                                  CompanyName = u.Company.Name,
                                  CompanyDescription = u.Company.Description,
                                  Description = u.Description,
                                  ApiKey = u.ApiKey,
                                  ApiSecret = u.ApiSecret,
                                  Deleted = u.Deleted
                              }
                               ).ToList();
                data.Data = result;
                return await Task.FromResult(data);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.ApiClient model)
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

                var data = _applicationDbContext.ApiClient!.FirstOrDefault(u => u.Username == model.Username);

                if (data == null)
                {

                    data = new()
                    {
                        Id = Guid.NewGuid(),
                        Username = model.Username,
                        Password = StringManipulation.Encrypt(StringManipulation.Random(8), _encryptionKey),
                        //Password = StringManipulation.Encrypt("password", _encryptionKey),
                        CompanyId = Guid.Parse(model.CompanyId),
                        Description = model.Description,
                        ApiKey = StringManipulation.Random(16),
                        ApiSecret = StringManipulation.Random(16),
                        CreatedBy = model.OpUser,
                        CreatedDate = DateTime.Now
                    };

                    await _apiClientRepository.AddAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.Id.ToString()!,
                        ActionDate = (DateTime)data.CreatedDate,
                        TableName = _moduleName,
                        OldValues = "",
                        NewValues = EFramework.GetEntityProperties(data)
                    };

                    activityLog.Details = string.Format("[Username: {0}] created.", data.Username);

                    result = new UPCI.DAL.DTO.Response.Result() { Status = "SUCCESS", Message = string.Format("{0} created.", _moduleName) };
                 
                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);
                }
                else
                {
                    result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("Username: {0} already exist.", _moduleName) };
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
        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.ApiClient model)
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

                var data = _applicationDbContext.ApiClient!.FirstOrDefault(u => u.Username == model.Username);

                if (data != null)
                {
                    if (data.Id.ToString().ToUpper() == model.Id && data.Username == model.Username)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.ApiClient!.Any(d => d.Username == model.Username);

                        if (!x)
                            updateStatus = 1;
                        else
                        {
                            updateStatus = -1;
                            result.Status = "FAILED";
                            result.Message = string.Format("{0} [Username: {1}] already exist.", _moduleName, model.Username);
                        }
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

                    data!.Username = model.Username;
                    data!.CompanyId = Guid.Parse(model.CompanyId);
                    data.Description = model.Description;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _apiClientRepository.UpdateAsync(data);

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

                    activityLog.Details = string.Format("[Username: {0}] updated.", model.Username);
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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.ApiClient model)
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

                var data = _applicationDbContext.ApiClient!.FirstOrDefault(u => u.Username == model.Username);

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _apiClientRepository.DeleteAsync(data);

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

                    activityLog.Details = string.Format("[Username: {0}] deleted.", data.Username);
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

        //reset api key

        //reset password
    }
}
