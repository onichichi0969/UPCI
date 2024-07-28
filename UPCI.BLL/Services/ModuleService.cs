using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL; 
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.POIFS.Properties;
using System;
using System.Transactions;

namespace UPCI.BLL.Services
{
    public class ModuleService(IConfiguration configuration, ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Module> moduleRepository, ILogService logService) : IModuleService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Module> _moduleRepository = moduleRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "Module";
        private readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;

        public async Task<List<UPCI.DAL.DTO.Response.Module>> Get()
        {
            try
            {
                var result = _applicationDbContext.Set<Module>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Module>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.ModuleAction>> GetAllModuleAction()
        {
            try
            {
                
                var moduleActionList = _applicationDbContext.Set<DAL.Models.ModuleAction>().AsQueryable()
                    .Where(e => e.Deleted != true).OrderBy(e => e.Code);

                var result = (from a in moduleActionList
                              select new UPCI.DAL.DTO.Response.ModuleAction
                              {
                                  Code = a.Code,
                                  Description = a.Description,
                                  Deleted = a.Deleted
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
        public async Task<UPCI.DAL.DTO.Response.Module> ById(string id)
        {
            try
            { 
                var result = _applicationDbContext.Module!.FirstOrDefault(b => b.Id == Convert.ToInt32(id) && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.Module>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }

        }
        
        public async Task<UPCI.DAL.DTO.Response.VModule> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<DAL.Models.Module>(model.SortColumn);

                UPCI.DAL.DTO.Response.VModule vModule = new();

                //var query = _applicationDbContext.Module!.AsQueryable();

                var moduleList = _applicationDbContext.Set<DAL.Models.Module>().AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                    moduleList = moduleList.Where(ExpressionBuilder.GetExpression<DAL.Models.Module>(model.Filters));

                if (model.Descending)
                {
                    moduleList = moduleList.OrderByDescending(propertySelector);
                }
                else
                {
                    moduleList = moduleList.OrderBy(propertySelector);
                }

                vModule.CurrentPage = model.PageNum;
                vModule.TotalRecord = moduleList.Count();
                vModule.TotalPage = (int)Math.Ceiling((double)vModule.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = moduleList.Skip(recordsToSkip).Take(model.PageSize);

                var result = (from a in pagedQuery
                              select new UPCI.DAL.DTO.Response.FModule
                              {
                                  Id = a.Id.ToString(),
                                  Code = a.Code,
                                  Name = a.Name,
                                  ModuleType = a.ModuleType,
                                  ParentId = a.ParentId.ToString(),
                                  ParentName = a.Parent.Name,
                                  Description = a.Description,
                                  DisplayOrder = a.DisplayOrder.ToString(),
                                  Url = a.Url,
                                  Icon = a.Icon,
                                  Action = a.Action,
                                  AuditContent = a.AuditContent,
                                  Show = a.Show,
                                  Deleted = a.Deleted,
                                  CreatedBy = a.CreatedBy,
                                  CreatedDate = a.CreatedDate,
                                  UpdatedBy = a.UpdatedBy,
                                  UpdatedDate = a.UpdatedDate,
                              }
                               ).ToList();
                //vModule.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FModule>>(pagedQuery.ToList());
                vModule.Data = result;
                return await Task.FromResult(vModule);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.ModuleAccess>> GetUserModuleAccess(string roleModuleCode)
        {
            try
            { 
                var moduleList = _applicationDbContext.Set<DAL.Models.RoleModule>()
                    .Where(rm => rm.RoleModuleCode == roleModuleCode) 
                    .Include(m => m.Module)
                    .OrderBy(m => m.Module.DisplayOrder)
                    .AsQueryable();
                var result = (from m in moduleList
                              select new DAL.DTO.Response.ModuleAccess
                              {
                                  Id = m.Module.Id.ToString(),
                                  Code = m.ModuleCode,
                                  Name = m.Module.Name,
                                  ModuleType = m.Module.ModuleType,
                                  ParentId = m.Module.ParentId.ToString(),
                                  ParentName = m.Module.Parent.Name,
                                  Description = m.Module.Description,
                                  DisplayOrder = m.Module.DisplayOrder.ToString(),
                                  Url = m.Module.Url,
                                  Icon = m.Module.Icon,
                                  Action = m.Actions,
                                  AuditContent = m.Module.AuditContent,
                                  Show = m.Module.Show,
                                  CreatedBy = m.Module.CreatedBy,
                                  CreatedDate = m.Module.CreatedDate,
                                  UpdatedBy = m.Module.UpdatedBy,
                                  UpdatedDate = m.Module.UpdatedDate,
                                  Path = m.Id.ToString()
                              }).ToList();


                return result;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.ModuleAccess>> GetAllParent()
        {
            try
            {
                var moduleList = _applicationDbContext.Set<DAL.Models.Module>().AsQueryable();

                var rootModules = moduleList
                   .Where(m => m.ParentId == 0 && !m.Deleted)
                   .OrderBy(m => m.DisplayOrder)
                   .Select(m => new DAL.DTO.Response.ModuleAccess
                   {
                       Id = m.Id.ToString(),
                       Code = m.Code,
                       Name = m.Name,
                       ModuleType = m.ModuleType,
                       ParentId = m.ParentId.ToString(),
                       ParentName = m.Parent.Name,
                       Description = m.Description,
                       DisplayOrder = m.DisplayOrder.ToString(),
                       Url = m.Url,
                       Icon = m.Icon,
                       Action = m.Action,
                       AuditContent = m.AuditContent,
                       Show = m.Show,
                       CreatedBy = m.CreatedBy,
                       CreatedDate = m.CreatedDate,
                       UpdatedBy = m.UpdatedBy,
                       UpdatedDate = m.UpdatedDate,
                       Path = m.Id.ToString()
                   })
                   .ToList();

                var allModules = moduleList
                    .Where(m => !m.Deleted)
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();

                var moduleMap = allModules.ToDictionary(m => m.Id);

                List< DAL.DTO.Response.ModuleAccess> result = new List<DAL.DTO.Response.ModuleAccess>();
                int rowNumber = 1;
                foreach (var rootModule in rootModules)
                {
                    PopulateChildren(rootModule, moduleMap, result, rootModule.Path, rootModule.Name, true);
                    rowNumber++;
                }

                
                return result.OrderBy(m => m.Path.ToString()).ToList(); 

            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        private void PopulateChildren(DAL.DTO.Response.ModuleAccess parentModule, Dictionary<int, DAL.Models.Module> moduleMap, List<DAL.DTO.Response.ModuleAccess> result, string currentPath, string parentName, bool isParentList)
        {
            result.Add(parentModule);
            var children = moduleMap.Values
                .Where(m => m.ParentId.ToString() == parentModule.Id)
                .OrderBy(m => m.DisplayOrder)
                .ToList();

            int rowNumber = 1;
            foreach (var child in children)
            {
                var childModuleDto = new DAL.DTO.Response.ModuleAccess
                {
                    Id = child.Id.ToString(),
                    Code = child.Code,
                    Name = isParentList ? parentName + " > " + child.Name : child.Name, // Construct the name
                    ModuleType = child.ModuleType,
                    ParentId = child.ParentId.ToString(),
                    ParentName = child.Parent.Name,
                    Description = child.Description,
                    DisplayOrder = child.DisplayOrder.ToString(),
                    Url = child.Url,
                    Icon = child.Icon,
                    Action = child.Action,
                    AuditContent = child.AuditContent,
                    Show = child.Show,
                    CreatedBy = child.CreatedBy,
                    CreatedDate = child.CreatedDate,
                    UpdatedBy = child.UpdatedBy,
                    UpdatedDate = child.UpdatedDate, 
                    Path = currentPath + rowNumber // Construct the path
                };

                PopulateChildren(childModuleDto, moduleMap, result, childModuleDto.Path, childModuleDto.Name, isParentList);
                rowNumber++;
            }
        }
        

        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Module model)
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

                var data = _applicationDbContext.Module!.FirstOrDefault(d => d.Code == model.Code);

                if (data == null)
                {

                    data = new()
                    {
                        Code = model.Code.Trim(),
                        Name = model.Name.Trim(),
                        ModuleType = model.ModuleType.Trim(),
                        DisplayOrder = model.DisplayOrder,
                        ParentId = model.ParentId == "" ? 0: Convert.ToInt32(model.ParentId),
                        Description = model.Description.Trim(),
                        Url = model.Url.Trim(),
                        Icon = model.Icon.Trim(),
                        Action = model.Action.Trim(),
                        AuditContent = model.AuditContent.Trim(),
                        Show = model.Show,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _moduleRepository.AddAsync(data);

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
                return new();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Module model)
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

                int id = Convert.ToInt32(model.Id!);
                var data = _applicationDbContext.Module!.FirstOrDefault(l => l.Id == id);


                if (data != null)
                {
                    if (data.Id == id && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.Module!.Any(d => d.Name == model.Name);

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

                    data!.Code = model.Code.Trim();
                    data!.Name = model.Name.Trim();
                    data.ModuleType = model.ModuleType.Trim();
                    data.DisplayOrder = model.DisplayOrder; 
                    data.ParentId = model.ParentId == "" ? 0 : Convert.ToInt32(model.ParentId);
                    data.Description = model.Description.Trim();
                    data.Url = model.Url.Trim();
                    data.Icon = model.Icon.Trim();
                    data.Action = model.Action.Trim();
                    data.AuditContent = model.AuditContent.Trim();
                    data.Show = model.Show;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _moduleRepository.UpdateAsync(data);

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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Module model)
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

                var data = _applicationDbContext.Module!.FirstOrDefault(l => l.Id == Convert.ToInt32(model.Id!));

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _moduleRepository.DeleteAsync(data);

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
