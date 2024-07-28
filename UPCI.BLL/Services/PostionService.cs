using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using System.Transactions; 

namespace UPCI.BLL.Services
{
    public class PositionService(ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<PositionCell> positionCellRepository, IRepository<PositionMinistry> positionMinistryRepository, ILogService logService) : IPositionService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<PositionCell> _positionCellRepository = positionCellRepository;
        private readonly IRepository<PositionMinistry> _positionMinistryRepository = positionMinistryRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        private readonly string _moduleName = "Position";

        public async Task<List<UPCI.DAL.DTO.Response.PositionCell>> GetCell()
        {
            try
            {
                var result = _applicationDbContext.Set<UPCI.DAL.Models.PositionCell>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.PositionCell>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.PositionMinistry>> GetMinistry()
        {
            try
            {
                var result = _applicationDbContext.Set<UPCI.DAL.Models.PositionMinistry>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.PositionMinistry>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }
        /// <summary>
        /// //////////////////////
        /// </summary>
        public async Task<UPCI.DAL.DTO.Response.PositionCell> ByIdCell(string id)
        {
            try
            {
                var result = _applicationDbContext.Cell!.FirstOrDefault(b => b.Id.ToString().ToUpper() == id && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.PositionCell>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        } 
        public async Task<UPCI.DAL.DTO.Response.PositionMinistry> ByIdMinistry(string id)
        {
            try
            {
                var result = _applicationDbContext.Cell!.FirstOrDefault(b => b.Id.ToString().ToUpper() == id && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.PositionMinistry>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        }
        /// <summary>
        /// //////////
        /// </summary> 
        public async Task<UPCI.DAL.DTO.Response.VPositionCell> FilterCell(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<PositionCell>(model.SortColumn);

                UPCI.DAL.DTO.Response.VPositionCell vCell = new();
                 
                var cellList = _applicationDbContext.Set<PositionCell>().AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                    cellList = cellList.Where(ExpressionBuilder.GetExpression<PositionCell>(model.Filters));

                if (model.Descending)
                {
                    cellList = cellList.OrderByDescending(propertySelector);
                }
                else
                {
                    cellList = cellList.OrderBy(propertySelector);
                }

               

                vCell.CurrentPage = model.PageNum;
                vCell.TotalRecord = cellList.Count();
                vCell.TotalPage = (int)Math.Ceiling((double)vCell.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = cellList.Skip(recordsToSkip).Take(model.PageSize);
                 
                var result = (from u in pagedQuery
                              select new UPCI.DAL.DTO.Response.FPositionCell
                              {
                                  Id = u.Id.ToString(),
                                  Code = u.Code,
                                  Description = u.Description,
                                  Deleted = u.Deleted
                              }
                               ).ToList();

                vCell.Data = result;
                return await Task.FromResult(vCell);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new ();
            }
        }
        public async Task<UPCI.DAL.DTO.Response.VPositionMinistry> FilterMinistry(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<PositionMinistry>(model.SortColumn);

                UPCI.DAL.DTO.Response.VPositionMinistry vCell = new();

                var ministryList = _applicationDbContext.Set<PositionMinistry>().AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                    ministryList = ministryList.Where(ExpressionBuilder.GetExpression<PositionMinistry>(model.Filters));

                if (model.Descending)
                {
                    ministryList = ministryList.OrderByDescending(propertySelector);
                }
                else
                {
                    ministryList = ministryList.OrderBy(propertySelector);
                }



                vCell.CurrentPage = model.PageNum;
                vCell.TotalRecord = ministryList.Count();
                vCell.TotalPage = (int)Math.Ceiling((double)vCell.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = ministryList.Skip(recordsToSkip).Take(model.PageSize);

                var result = (from u in pagedQuery
                              select new UPCI.DAL.DTO.Response.FPositionMinistry
                              {
                                  Id = u.Id.ToString(),
                                  Code = u.Code,
                                  Description = u.Description,
                                  Deleted = u.Deleted
                              }
                               ).ToList();

                vCell.Data = result;
                return await Task.FromResult(vCell);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        /// <summary>
        /// //////
        /// </summary> 
        public async Task<UPCI.DAL.DTO.Response.Result> CreateCell(UPCI.DAL.DTO.Request.PositionCell model)
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

                var data = _applicationDbContext.PositionCell!.FirstOrDefault(d => d.Code == model.Code);

                if (data == null)
                {

                    data = new()
                    { 
                        Code = model.Code.Trim(),
                        Description = model.Description.Trim(), 
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _positionCellRepository.AddAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.CreatedBy!,
                        ActionDate = (DateTime)data.CreatedDate,
                        TableName = _moduleName+"Cell",
                        OldValues = "",
                        NewValues = EFramework.GetEntityProperties(data)
                    };

                    activityLog.Details = string.Format("[code: {0}] created.", data.Code);

                    result = new UPCI.DAL.DTO.Response.Result() { Status = "SUCCESS", Message = string.Format("{0} created.", _moduleName ) };

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                }
                else
                {
                    result = new UPCI.DAL.DTO.Response.Result() { Status = "FAILED", Message = string.Format("{0} already exist.", _moduleName ) };
                }

                transactionScope.Complete();

            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }
        public async Task<UPCI.DAL.DTO.Response.Result> CreateMinistry(UPCI.DAL.DTO.Request.PositionMinistry model)
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

                var data = _applicationDbContext.PositionMinistry!.FirstOrDefault(d => d.Code == model.Code);

                if (data == null)
                {

                    data = new()
                    {
                        Code = model.Code.Trim(),
                        Description = model.Description.Trim(),
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _positionMinistryRepository.AddAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.CreatedBy!,
                        ActionDate = (DateTime)data.CreatedDate,
                        TableName = _moduleName + "Ministry",
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
                _logService.LogException(ex, _moduleName + "Ministry");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Ministry");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }
        /// <summary>
        /// /////////
        /// </summary> 
        public async Task<UPCI.DAL.DTO.Response.Result> UpdateCell(UPCI.DAL.DTO.Request.PositionCell model)
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

                var data = _applicationDbContext.PositionCell!.Where(d => d.Code == model.Code!).FirstOrDefault();

                if (data != null)
                {
                    if (data.Id == Convert.ToInt32(model.Id!) && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.PositionCell!.Any(d => d.Code == model.Code);

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
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _positionCellRepository.UpdateAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName + "Cell",
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
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }
        public async Task<UPCI.DAL.DTO.Response.Result> UpdateMinistry(UPCI.DAL.DTO.Request.PositionMinistry model)
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

                var data = _applicationDbContext.PositionMinistry!.Where(d => d.Code == model.Code!).FirstOrDefault();

                if (data != null)
                {
                    if (data.Id == Convert.ToInt32(model.Id!) && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.PositionMinistry!.Any(d => d.Code == model.Code);

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
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _positionMinistryRepository.UpdateAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName + "Ministry",
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
                _logService.LogException(ex, _moduleName + "Ministry");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Ministry");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);
        }
        /// <summary>
        /// ///
        /// </summary> 
        public async Task<UPCI.DAL.DTO.Response.Result> DeleteCell(UPCI.DAL.DTO.Request.PositionCell model)
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

                var data = _applicationDbContext.PositionCell!.FirstOrDefault(l => l.Id.ToString() == model.Id);

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _positionCellRepository.DeleteAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName+"Cell",
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
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Cell");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            finally
            {

                transactionScope.Dispose();
            }

            return await Task.FromResult(result);

        }
        public async Task<UPCI.DAL.DTO.Response.Result> DeleteMinistry(UPCI.DAL.DTO.Request.PositionMinistry model)
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

                var data = _applicationDbContext.PositionMinistry!.FirstOrDefault(l => l.Id.ToString() == model.Id);

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _positionMinistryRepository.DeleteAsync(data);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = data.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = data.UpdatedBy!,
                        ActionDate = (DateTime)data.UpdatedDate,
                        TableName = _moduleName + "Ministry",
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
                _logService.LogException(ex, _moduleName + "Ministry");
                result = new UPCI.DAL.DTO.Response.Result() { Status = "ERROR", Message = "Error encountered" };
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName + "Ministry");
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
