using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace UPCI.BLL.Services
{
    public class MinistryService(ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Ministry> ministryRepository, IRepository<MemberMinistry> memberMinistryRepository, ILogService logService) : IMinistryService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Ministry> _ministryRepository = ministryRepository;
        private readonly IRepository<MemberMinistry> _memberMinistryRepository = memberMinistryRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "Ministry";
        public async Task<UPCI.DAL.DTO.Response.Result> SaveMinistryMembers(UPCI.DAL.DTO.Request.MinistryMembersList model)
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
                if (!model.isMembersChanged)
                {
                    result.Status = "SUCCESS";
                    result.Message = "No changes has been made"; 
                }

                var existingMinistries = _applicationDbContext.MemberMinistry!.Where(d => d.MinistryCode == model.MinistryCode!).ToList(); 
                if (existingMinistries.Count > 0)
                    //delete
                    await _memberMinistryRepository.RemoveRangeAsync(existingMinistries);

                var memberMinistryList = new List<MemberMinistry>();

                if (model.MinistryMembers != null && model.MinistryMembers.Count != 0)
                {
                    memberMinistryList = model.MinistryMembers.Select(member => new MemberMinistry
                    {
                        MemberCode = member.MemberCode,
                        MinistryCode = member.MinistryCode,
                        Position = member.PositionMinistryCode,
                    }).ToList();
                    await _memberMinistryRepository.AddRangeAsync(memberMinistryList);
                }

                var auditTrail = new AuditTrail()
                {
                    RecordId = model.MinistryCode,
                    Terminal = model.Terminal!,
                    Action = action,
                    UserId = userId!,
                    ActionDate = DateTime.Now,
                    TableName = _moduleName,
                    OldValues = "",
                    NewValues = ""
                };

                activityLog.Details = string.Format("[code: {0}] updated.", model.MinistryCode);
                result.Status = "SUCCESS";
                result.Message = string.Format("{0} updated.", _moduleName);

                _logService.LogActivity(activityLog);
                _logService.LogAudit(auditTrail!);
             

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
        public async Task<List<UPCI.DAL.DTO.Response.Ministry>> Get(string departmentCode)
        {
            try
            {
                var result = _applicationDbContext.Set<Ministry>()
                    .Where(e => e.Deleted != true && e.DepartmentCode == departmentCode).ProjectTo<UPCI.DAL.DTO.Response.Ministry>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }

        public async Task<UPCI.DAL.DTO.Response.Ministry> ById(string id)
        {
            try
            {
                var result = _applicationDbContext.Ministry!.FirstOrDefault(b => b.Id.ToString().ToUpper() == id && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.Ministry>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        }
        public async Task<UPCI.DAL.DTO.Response.VMinistry> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<Ministry>(model.SortColumn);

                UPCI.DAL.DTO.Response.VMinistry vMinistry = new();

                var MinistryList = _applicationDbContext.Ministry!.Include(d => d.Department).Include(m => m.MemberMinistry).ThenInclude(m => m.Member).AsQueryable();
                    //.ProjectTo<Ministry>(_mapper.ConfigurationProvider).AsQueryable();  
                
                if (model.Filters != null && model.Filters.Count != 0)
                    MinistryList = MinistryList.Where(ExpressionBuilder.GetExpression<Ministry>(model.Filters));

                if (model.Descending)
                {
                    MinistryList = MinistryList.OrderByDescending(propertySelector);
                }
                else
                {
                    MinistryList = MinistryList.OrderBy(propertySelector);
                }

               

                vMinistry.CurrentPage = model.PageNum;
                vMinistry.TotalRecord = MinistryList.Count();
                vMinistry.TotalPage = (int)Math.Ceiling((double)vMinistry.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = MinistryList.Skip(recordsToSkip).Take(model.PageSize);

                //vMinistry.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FMinistry>>(pagedQuery.ToList()); 

                var result = (from u in pagedQuery
                              select new UPCI.DAL.DTO.Response.FMinistry
                              {
                                  Id = u.Id.ToString(),
                                  Code = u.Code,
                                  Description = u.Description,
                                  DepartmentCode = u.DepartmentCode,
                                  DepartmentDesc = u.Department.Description,
                                  //MemberCount = u.MemberCount,
                                  MemberMinistry = u.MemberMinistry.Select(member => new UPCI.DAL.DTO.Response.MemberMinistry
                                                    {
                                                          MemberCode = member.MemberCode!,
                                                          MemberDesc = Convert.ToString(member.Member.FirstName) + " " + Convert.ToString(member.Member.MiddleName) + " " + Convert.ToString(member.Member.LastName),
                                                          MinistryCode = member.MinistryCode!,
                                                          MinistryDesc = member.Ministry.Description,
                                                          Position = member.Position!,
                                                          PositionDesc = member.PositionMinistry.Description!, 
                                                    }).ToList(),
                                  Deleted = u.Deleted
                              }).ToList();
                vMinistry.Data = result;
                return await Task.FromResult(vMinistry);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new ();
            }
        }
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Ministry model)
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

                var data = _applicationDbContext.Ministry!.FirstOrDefault(d => d.Code == model.Code);

                if (data == null)
                {

                    data = new()
                    { 
                        Code = model.Code.Trim(),
                        Description = model.Description.Trim(),
                        DepartmentCode = model.DepartmentCode.Trim(),
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _ministryRepository.AddAsync(data);

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

                    activityLog.Details = string.Format("[code: {0}] created.", data.Code);

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

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Ministry model)
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

                var data = _applicationDbContext.Ministry!.Where(d => d.Code == model.Code!).FirstOrDefault();

                if (data != null)
                {
                    if (data.Id == Convert.ToInt32(model.Id!) && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.Ministry!.Any(d => d.Code == model.Code);

                        if (!x)
                            updateStatus = 1;
                        else
                        {
                            updateStatus = -1;
                            result.Status = "FAILED";
                            result.Message = string.Format("{0} [code: {1}] already exist.", _moduleName, model.Code);
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
                    data.Description = model.Description.Trim();
                    data.DepartmentCode = model.DepartmentCode.Trim();
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _ministryRepository.UpdateAsync(data);

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

                    activityLog.Details = string.Format("[code: {0}] updated.", model.Code);
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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Ministry model)
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

                var data = _applicationDbContext.Ministry!.FirstOrDefault(l => l.Id.ToString() == model.Id);

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _ministryRepository.DeleteAsync(data);

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

                    activityLog.Details = string.Format("[code: {0}] deleted.", data.Code);
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
