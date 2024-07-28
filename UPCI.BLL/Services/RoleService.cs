using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Transactions;

namespace UPCI.BLL.Services
{
    public class RoleService(IConfiguration configuration, ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Role> RoleRepository, IRepository<RoleModule> RoleModuleRepository, ILogService logService) : IRoleService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Role> _RoleRepository = RoleRepository;
        private readonly IRepository<RoleModule> _RoleModuleRepository = RoleModuleRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        //IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly string _moduleName = "Role";
        private readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;
        public async Task<List<UPCI.DAL.DTO.Response.Role>> Get()
        {
            try
            {
                var result = _applicationDbContext.Set<Role>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Role>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }

        public async Task<UPCI.DAL.DTO.Response.Role> ById(string id)
        {
            try
            {
                int idx = Convert.ToInt32(UPCI.DAL.Helpers.StringManipulation.Decrypt(id, _encryptionKey));
                var result = _applicationDbContext.Role!.FirstOrDefault(b => b.Id == idx && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.Role>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }

        }
        public async Task<List<UPCI.DAL.DTO.Response.Module>> ByCode(string code)
        {
            try
            {
                var query = _applicationDbContext.RoleModule!.Where(b => b.RoleModuleCode == code).AsQueryable();
                var modules = _applicationDbContext.Module!.Where(m => m.Deleted != true).AsQueryable();

                var result = (from q in query
                              join m in modules on q.ModuleCode equals m.Code
                              select new UPCI.DAL.DTO.Response.Module
                              {
                                  Id = m.Id.ToString(),
                                  Code = q.ModuleCode,
                                  Name = m.Name,
                                  ModuleType = m.ModuleType,
                                  ParentId = m.ParentId.ToString(),
                                  Action = q.Actions,
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
        public async Task<UPCI.DAL.DTO.Response.VRole> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.Role>(model.SortColumn);

                UPCI.DAL.DTO.Response.VRole vRole = new(); 
                
                var query = _applicationDbContext.Set<DAL.Models.Role>().Include(rm => rm.RoleModule).ThenInclude(m => m.Module).AsQueryable();
                //var query = _applicationDbContext.Role!.AsQueryable(); 
                //query = query.Where(e => e.Name.Contains(model.Prefix) && e.Deleted != true);
                //model.Filters.Add(new DAL.Models.Filter { 
                //    Property = "Code",
                //    Operator = "NOTEQUALS",
                //    Value = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("UserRole")),
                //});

                if (model.Filters != null && model.Filters.Count != 0)
                    query = query.Where(ExpressionBuilder.GetExpression<DAL.Models.Role>(model.Filters));


                if (model.Descending)
                {
                    query = query.OrderByDescending(propertySelector);
                }
                else
                {
                    query = query.OrderBy(propertySelector);
                }

                vRole.CurrentPage = model.PageNum;
                vRole.TotalRecord = query.Count();
                vRole.TotalPage = (int)Math.Ceiling((double)vRole.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = query.Skip(recordsToSkip).Take(model.PageSize);

                //vRole.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FRole>>(pagedQuery.ToList());
                var result = (from u in pagedQuery
                              select new UPCI.DAL.DTO.Response.FRole
                              {
                                  EncryptedId = UPCI.DAL.Helpers.StringManipulation.Encrypt(u.Id.ToString(), configuration["AppContext:EncryptionKey"]!),
                                  Code = u.Code,
                                  Name = u.Name,
                                  Description = u.Description, 
                                  Deleted = u.Deleted
                              }
                ).ToList();

                vRole.Data = result;

                return await Task.FromResult(vRole);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Role model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var action = "ADD";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Username.ToString().ToUpper();

                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = action
                };

                var data = _applicationDbContext.Role!.FirstOrDefault(d => d.Code == model.Code);

                if (data == null)
                { 
                    data = new()
                    {
                        Code = model.Code,
                        Name = model.Name,
                        Description = model.Description,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _RoleRepository.AddAsync(data);

                    //Delete RoleModules
                    var dataToDelete = _applicationDbContext.RoleModule!.Where(d => d.RoleModuleCode == model.Code);
                   
                    if (dataToDelete != null)
                        await _RoleModuleRepository.RemoveRangeAsync(dataToDelete);

                    //Add RoleModules
                    List<RoleModule> roleModule = new();
                    if (model.Modules != null)
                    {
                        foreach (var modules in model.Modules)
                        {
                            roleModule.Add(
                                new RoleModule
                                {
                                    RoleModuleCode = model.Code,
                                    ModuleCode = modules.Code,
                                    Actions = modules.Action, 
                                }
                            );
                        }
                        if (roleModule.Count > 0)
                            await _RoleModuleRepository.AddRangeAsync(roleModule);
                    }
                    

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
                    result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("{0} already exist.", _moduleName) };
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

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Role model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var action = "EDIT";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Username;

                int updateStatus = 0;

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = action
                };

                int id = Convert.ToInt32(UPCI.DAL.Helpers.StringManipulation.Decrypt(model.EncryptedId!, _encryptionKey));
                var data = _applicationDbContext.Role!.FirstOrDefault(l => l.Id == id);


                if (data != null)
                {
                    if (data.Id == id && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.Role!.Any(d => d.Code == model.Code);

                        if (!x)
                            updateStatus = 1;
                        else
                        {
                            updateStatus = -1;
                            result.Status = "FAILED";
                            result.Message = string.Format("{0} [code: {1}] already exist.", _moduleName, model.Name);
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

                    data!.Code = model.Code;
                    data!.Name = model.Name;
                    data.Description = model.Description;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _RoleRepository.UpdateAsync(data);

                    //Delete RoleModules
                    var dataToDelete = _applicationDbContext.RoleModule!.Where(d => d.RoleModuleCode == model.Code);
                    if (dataToDelete != null)
                        await _RoleModuleRepository.RemoveRangeAsync(dataToDelete);

                    //Add RoleModules
                    List<RoleModule> roleModule = new();
                    if (model.Modules != null)
                    {
                        foreach (var modules in model.Modules)
                        {
                            roleModule.Add(
                                new RoleModule
                                {
                                    RoleModuleCode = model.Code,
                                    ModuleCode = modules.Code,
                                    Actions = modules.Action, 
                                }
                            );
                        }
                        if (roleModule.Count > 0)
                            await _RoleModuleRepository.AddRangeAsync(roleModule);
                    }

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

                    activityLog.Details = string.Format("[code: {0}] updated.", model.Name);
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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Role model)
        {
            UPCI.DAL.DTO.Response.Result result = new();
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "DELETE";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Username;



                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = _action
                };

                var data = _applicationDbContext.Role!.FirstOrDefault(l => l.Id == Convert.ToInt32(UPCI.DAL.Helpers.StringManipulation.Decrypt(model.EncryptedId!, _encryptionKey)));

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _RoleRepository.DeleteAsync(data);

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

                    activityLog.Details = string.Format("[code: {0}] deleted.", data.Name);
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
    }
}
