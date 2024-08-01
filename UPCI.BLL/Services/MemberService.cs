using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using System.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.SS.Formula.Functions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace UPCI.BLL.Services
{
    public class MemberService(IConfiguration configuration, ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<Member> memberRepository, IRepository<MemberCell> memberCellRepository, IRepository<MemberMinistry> memberMinistryRepository, ILogService logService) : IMemberService
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IRepository<Member> _memberRepository = memberRepository;
        private readonly IRepository<MemberCell> _memberCellRepository = memberCellRepository;
        private readonly IRepository<MemberMinistry> _memberMinistryRepository = memberMinistryRepository;
        private readonly ILogService _logService = logService;
        private readonly IMapper _mapper = mapper;
        IConfiguration _configuration;
        private readonly string _moduleName = "Member";
        readonly Chapter chapter = configuration.GetSection("Chapter").Get<Chapter>()!;
        readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;

        public (string memberCode,int lastSequence) GenerateMemberCode(string chapterCode)
        {
            string memberCode; 
            string year = DateTime.Now.ToString("yyyy");
            int lastSequence = LastGetSequence();
            string sequence = ("0000" + lastSequence.ToString());
            
            chapterCode = chapterCode == "" ? "PUNTURIN" : chapterCode; 
            memberCode = "UPCI" + "-" + chapterCode.ToUpper() + "-" + year + "-" + sequence.Substring(Math.Max(0, sequence.Length - 4)); ;

            return (memberCode,lastSequence);
        }
        public int LastGetSequence()
        {
            int currentYear = DateTime.Now.Year;
            int sequence = 0;
            var member = _applicationDbContext.Member!
                .Where(m => m.CreatedDate.Value.Year == currentYear)
                .OrderByDescending(m => m.Sequence).FirstOrDefault();
            if (member == null || member.Sequence == 9999)
            {
                sequence = 1;
            }
            else 
            {
                sequence = (int)member.Sequence + 1;
            }
            return sequence;
        }
        public async Task<List<UPCI.DAL.DTO.Response.Member>> Get()
        {
            try
            {
                var result = _applicationDbContext.Set<Member>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.Member>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }
        }

        public async Task<UPCI.DAL.DTO.Response.Member> ById(string id)
        {
            try
            {
                var result = _applicationDbContext.Cell!.FirstOrDefault(b => b.Id.ToString().ToUpper() == StringManipulation.Decrypt(id!, _encryptionKey) && b.Deleted != true);

                return await Task.FromResult(_mapper.Map<UPCI.DAL.DTO.Response.Member>(result));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                throw;
            }

        }
        public async Task<UPCI.DAL.DTO.Response.VMember> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<Member>(model.SortColumn);

                UPCI.DAL.DTO.Response.VMember vCell = new();
                 
                var memberList = _applicationDbContext.Member.Include(c => c.MemberCell).Include(m => m.MemberMinistry).AsQueryable(); 
                var civilStatus = _applicationDbContext.Set<CivilStatus>().AsQueryable();
                var memberType = _applicationDbContext.Set<MemberType>().AsQueryable();
                var pepsolLevel = _applicationDbContext.Set<PEPSOLLevel>().AsQueryable();

                if (model.Filters != null && model.Filters.Count != 0)
                    memberList = memberList.Where(ExpressionBuilder.GetExpression<Member>(model.Filters));

                if (model.Descending)
                {
                    memberList = memberList.OrderByDescending(propertySelector);
                }
                else
                {
                    memberList = memberList.OrderBy(propertySelector);
                }

               

                vCell.CurrentPage = model.PageNum;
                vCell.TotalRecord = memberList.Count();
                vCell.TotalPage = (int)Math.Ceiling((double)vCell.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = memberList.Skip(recordsToSkip).Take(model.PageSize);

                //vCell.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FCell>>(pagedQuery.ToList());

                var result = (from u in pagedQuery
                              join c in civilStatus on u.CivilStatus equals c.Code into civilGroup
                              from c in civilGroup.DefaultIfEmpty()
                              join mt in memberType on u.MemberType equals mt.Code into memberTypeGroup
                              from mt in memberTypeGroup.DefaultIfEmpty()
                              join pl in pepsolLevel on u.PEPSOL equals pl.Code into pepsolLevelGroup
                              from pl in pepsolLevelGroup.DefaultIfEmpty() 
                              select new UPCI.DAL.DTO.Response.FMember
                              {
                                  Id = StringManipulation.Encrypt(Convert.ToString(u.Id), _encryptionKey) ,
                                  Code = u.Code,
                                  Sequence = Convert.ToString(u.Sequence),
                                  Chapter = u.Chapter,
                                  FirstName = u.FirstName,
                                  MiddleName = u.MiddleName,
                                  LastName = u.LastName,
                                  Gender = u.Gender,
                                  CivilStatus = u.CivilStatus,
                                  CivilStatusDesc = c.Description,
                                  Address = u.Address,
                                  Birthday = u.Birthday! == null ? "" : Convert.ToDateTime(u.Birthday!).ToString("yyyy-MM-dd"),
                                  BaptismDate = u.BaptismDate! == null ? "" : Convert.ToDateTime(u.BaptismDate!).ToString("yyyy-MM-dd"),
                                  FirstAttend = u.FirstAttend! == null ? "" : Convert.ToDateTime(u.FirstAttend!).ToString("yyyy-MM-dd"),
                                  Baptized = (bool)u.Baptized,
                                  InvolvedToCell = (bool)u.InvolvedToCell,
                                  PEPSOL = u.PEPSOL,
                                  PEPSOLDesc = pl.Description,
                                  MemberType = u.MemberType,
                                  MemberTypeDesc = mt.Description,
                                  Email = u.Email,
                                  ContactNo = u.ContactNo,
                                  ActiveMember = (bool)u.ActiveMember,
                                  MemberCell = u.MemberCell == null? new() :
                                                                              u.MemberCell.Select(cell => new UPCI.DAL.DTO.Response.MemberCell
                                                                              {
                                                                                  MemberCode = u.Code!,
                                                                                  CellCode = cell.CellCode!,
                                                                                  CellDesc = cell.Cell.Description!,
                                                                                  Position = cell.Position!,
                                                                                  PositionDesc = cell.PositionCell.Description!,
                                                                              }).ToList(),
                                  MemberMinistry = u.MemberMinistry == null ? new() :
                                                                              u.MemberMinistry.Select(ministry => new UPCI.DAL.DTO.Response.MemberMinistry
                                                                              {
                                                                                  MemberCode = u.Code,
                                                                                  MinistryCode = ministry.MinistryCode!,
                                                                                  MinistryDesc = ministry.Ministry.Description,
                                                                                  Position = ministry.Position!,
                                                                                  PositionDesc = ministry.PositionMinistry.Description!,
                                                                                  DepartmentCode = ministry.Ministry.Department.Code,
                                                                                  DepartmentDesc = ministry.Ministry.Department.Description,
                                                                              }).ToList(),

                                  //ImageContent = u.ImageContent,
                                  //ImageType = u.ImageType,
                                  CreatedBy = u.CreatedBy,
                                  CreatedDate = u.CreatedDate,
                                  UpdatedBy = u.UpdatedBy,
                                  UpdatedDate = u.UpdatedDate,
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
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.Member model)
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

                var data = _applicationDbContext.Member!.FirstOrDefault(d => d.LastName.Trim().ToUpper() == model.LastName.Trim().ToUpper() 
                    && d.FirstName.Trim().ToUpper() == model.FirstName.Trim().ToUpper() 
                    && (d.Birthday! == null? Convert.ToDateTime("1900-01-01") : d.Birthday!) == Convert.ToDateTime(model.Birthday == ""? "1900-01-01" : model.Birthday).Date);

                if (data == null)
                {
                    var chapter = "PUNTURIN";
                    var memberCode = GenerateMemberCode(chapter);
                    data = new()
                    {
                        Code = memberCode.memberCode.Trim(),
                        Sequence = memberCode.lastSequence,
                        Chapter = chapter,
                        FirstName = model.FirstName.Trim(),
                        MiddleName = model.MiddleName.Trim(),
                        LastName = model.LastName.Trim(),
                        Gender = model.Gender.Trim(),
                        CivilStatus = model.CivilStatus.Trim(),
                        Address = model.Address.Trim(),
                        Birthday = Convert.ToString(model.Birthday) == string.Empty ? null : Convert.ToDateTime(model.Birthday),
                        BaptismDate = Convert.ToString(model.BaptismDate) == string.Empty ? null : Convert.ToDateTime(model.BaptismDate),
                        FirstAttend = Convert.ToString(model.FirstAttend) == string.Empty ? null : Convert.ToDateTime(model.FirstAttend),
                        PEPSOL = model.PEPSOL.Trim(),
                        Baptized = model.Baptized,
                        InvolvedToCell = model.InvolvedToCell,
                        ActiveMember = model.ActiveMember,
                        MemberType = model.MemberType.Trim(),
                        Email = model.Email.Trim(),
                        ContactNo = model.ContactNo.Trim(),
                        ImageContent = model.ImageContent,
                        ImageType = model.ImageType.Trim(),
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now 
                    };
                    await _memberRepository.AddAsync(data);
                    if (model.CellChanged)
                    {
                        var memberCellList = new List<MemberCell>();
                        if (model.Cells != null)
                        {
                            memberCellList = model.Cells.Select(cell => new MemberCell
                            {
                                MemberCode = memberCode.memberCode,
                                CellCode = cell.CellCode,
                                Position = cell.PositionCellCode,
                            }).ToList();
                            await _memberCellRepository.AddRangeAsync(memberCellList);
                        }
                    }
                    if (model.MinistryChanged)
                    {
                        var memberMinistryList = new List<MemberMinistry>();
                        if (model.Ministries != null)
                        {
                            memberMinistryList = model.Ministries.Select(ministry => new MemberMinistry
                            {
                                MemberCode = memberCode.memberCode,
                                MinistryCode = ministry.MinistryCode,
                                Position = ministry.PositionMinistryCode,
                            }).ToList();
                            await _memberMinistryRepository.AddRangeAsync(memberMinistryList);
                        }
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

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.Member model)
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

                var data = _applicationDbContext.Member!.Where(d => d.Code == model.Code!).FirstOrDefault();

                if (data != null)
                {
                    if (data.Id == Convert.ToInt32(StringManipulation.Decrypt(model.Id!, _encryptionKey)) && data.Code == model.Code)
                    {
                        updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.Cell!.Any(d => d.Code == model.Code);

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

                    data.Code = model.Code;
                    data.FirstName = model.FirstName.Trim();
                    data.MiddleName = model.MiddleName.Trim();
                    data.LastName = model.LastName.Trim();
                    data.Gender = model.Gender.Trim();
                    data.CivilStatus = model.CivilStatus.Trim();
                    data.Address = model.Address.Trim();
                    data.Birthday = Convert.ToString(model.Birthday) == string.Empty ? null : Convert.ToDateTime(model.Birthday);
                    data.BaptismDate = Convert.ToString(model.BaptismDate) == string.Empty ? null : Convert.ToDateTime(model.BaptismDate);
                    data.FirstAttend = Convert.ToString(model.FirstAttend) == string.Empty ? null : Convert.ToDateTime(model.FirstAttend);
                    data.PEPSOL = model.PEPSOL.Trim();
                    data.Baptized = model.Baptized;
                    data.InvolvedToCell = model.InvolvedToCell;
                    data.ActiveMember = model.ActiveMember;
                    data.MemberType = model.MemberType.Trim();
                    data.Email = model.Email.Trim();
                    data.ContactNo = model.ContactNo.Trim();
                    data.ImageContent = model.ImageContent;
                    data.ImageType = model.ImageType.Trim(); 
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _memberRepository.UpdateAsync(data);

                    if (model.CellChanged)
                    {
                        var memberCellToDelete = _applicationDbContext.MemberCell!.Where(d => d.MemberCode == model.Code!).ToList();
                        if (memberCellToDelete != null)
                        {
                            await _memberCellRepository.RemoveRangeAsync(memberCellToDelete);
                        }
                        var memberCellList = new List<MemberCell>();
                        if (model.Cells != null)
                        {
                            memberCellList = model.Cells.Select(cell => new MemberCell
                            {
                                MemberCode = model.Code,
                                CellCode = cell.CellCode,
                                Position = cell.PositionCellCode,
                            }).ToList();
                            await _memberCellRepository.AddRangeAsync(memberCellList);
                        }
                    }
                    if (model.MinistryChanged)
                    {
                        var memberMinistryToDelete = _applicationDbContext.MemberMinistry!.Where(d => d.MemberCode == model.Code!).ToList();
                        if (memberMinistryToDelete != null)
                        {
                            await _memberMinistryRepository.RemoveRangeAsync(memberMinistryToDelete);
                        }
                        var memberMinistryList = new List<MemberMinistry>();
                        if (model.Ministries != null)
                        {
                            memberMinistryList = model.Ministries.Select(ministry => new MemberMinistry
                            {
                                MemberCode = model.Code,
                                MinistryCode = ministry.MinistryCode,
                                Position = ministry.PositionMinistryCode,
                            }).ToList();
                            await _memberMinistryRepository.AddRangeAsync(memberMinistryList);
                        }
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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.Member model)
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

                var data = _applicationDbContext.Member!.FirstOrDefault(l => l.Id.ToString() == StringManipulation.Decrypt(model.Id!, _encryptionKey));

                if (data != null)
                {
                    var oldValue = EFramework.GetEntityProperties(data!);

                    data.Deleted = true;
                    data.UpdatedBy = userId.ToString();
                    data.UpdatedDate = DateTime.Now;

                    await _memberRepository.DeleteAsync(data);

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
