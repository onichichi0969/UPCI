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
        public async Task<List<UPCI.DAL.DTO.Response.Member>> GetCodeAndName(List<string> existing)
        {
            try
            {
                var data = _applicationDbContext.Set<Member>()
                    .Where(e => e.Deleted != true &&
                                e.Code!.Contains(e.Code)).ToList();
                var result = (from d in data
                              select new UPCI.DAL.DTO.Response.Member
                              {
                                  Id = Convert.ToString(d.Id),
                                  Code = d.Code!,
                                  FirstName = d.FirstName!,
                                  MiddleName = d.MiddleName!,
                                  LastName = d.LastName!
                              }).ToList();

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
                //FirstOrDefault(b => b.Id.ToString().ToUpper() == StringManipulation.Decrypt(id!, _encryptionKey) && b.Deleted != true)
                //var civilStatus = _applicationDbContext.Set<CivilStatus>().AsQueryable();
                //var memberType = _applicationDbContext.Set<MemberType>().AsQueryable();
                //var pepsolLevel = _applicationDbContext.Set<PEPSOLLevel>().AsQueryable();
                var decryptedId = StringManipulation.Decrypt(id!, _encryptionKey);
                var civilStatus = _applicationDbContext.Set<CivilStatus>().AsQueryable();
                var memberType = _applicationDbContext.Set<MemberType>().AsQueryable();
                var pepsolLevel = _applicationDbContext.Set<PEPSOLLevel>().AsQueryable();

                var result = _applicationDbContext.Member
                    .Include(m => m.MemberCell)  
                    .Include(m => m.MemberMinistry) 
                    .Where(u => u.Id.ToString().ToUpper() == decryptedId && u.Deleted != true)  
                    .GroupJoin(civilStatus,  
                        u => u.CivilStatus,
                        c => c.Code,
                        (u, civilGroup) => new { u, civilGroup })
                    .SelectMany(x => x.civilGroup.DefaultIfEmpty(),  
                        (x, c) => new { x.u, CivilStatus = c })
                    .GroupJoin(memberType,  
                        x => x.u.MemberType,
                        mt => mt.Code,
                        (x, memberTypeGroup) => new { x.u, x.CivilStatus, memberTypeGroup })
                    .SelectMany(x => x.memberTypeGroup.DefaultIfEmpty(),  
                        (x, mt) => new { x.u, x.CivilStatus, MemberType = mt })
                    .GroupJoin(pepsolLevel,  
                        x => x.u.PEPSOL,
                        pl => pl.Code,
                        (x, pepsolLevelGroup) => new { x.u, x.CivilStatus, x.MemberType, pepsolLevelGroup })
                    .SelectMany(x => x.pepsolLevelGroup.DefaultIfEmpty(), 
                        (x, pl) => new UPCI.DAL.DTO.Response.Member
                        {
                            Id = id,
                            Code = x.u.Code,
                            Sequence = Convert.ToString(x.u.Sequence),
                            Chapter = x.u.Chapter,
                            FirstName = x.u.FirstName,
                            MiddleName = x.u.MiddleName,
                            LastName = x.u.LastName,
                            Gender = x.u.Gender,
                            CivilStatus = x.u.CivilStatus,
                            CivilStatusDesc = x.CivilStatus != null ? x.CivilStatus.Description : null,
                            Address = x.u.Address,
                            Birthday = x.u.Birthday.HasValue ? x.u.Birthday.Value.ToString("yyyy-MM-dd") : "",
                            BaptismDate = x.u.BaptismDate.HasValue ? x.u.BaptismDate.Value.ToString("yyyy-MM-dd") : "",
                            FirstAttend = x.u.FirstAttend.HasValue ? x.u.FirstAttend.Value.ToString("yyyy-MM-dd") : "",
                            Baptized = x.u.Baptized ?? false,
                            InvolvedToCell = x.u.InvolvedToCell ?? false,
                            PEPSOL = x.u.PEPSOL,
                            PEPSOLDesc = pl != null ? pl.Description : null,
                            MemberType = x.u.MemberType,
                            MemberTypeDesc = x.MemberType != null ? x.MemberType.Description : null,
                            Email = x.u.Email,
                            ContactNo = x.u.ContactNo,
                            ActiveMember = x.u.ActiveMember ?? false,
                            MemberCell = x.u.MemberCell == null ? new List<UPCI.DAL.DTO.Response.MemberCell>() :
                                                   x.u.MemberCell.Select(cell => new UPCI.DAL.DTO.Response.MemberCell
                                                   {
                                                       MemberCode = x.u.Code!,
                                                       CellCode = cell.CellCode!,
                                                       CellDesc = cell.Cell.Description!,
                                                       Position = cell.Position!,
                                                       PositionDesc = cell.PositionCell.Description!,
                                                   }).ToList(),
                            MemberMinistry = x.u.MemberMinistry == null ? new List<UPCI.DAL.DTO.Response.MemberMinistry>() :
                                                           x.u.MemberMinistry.Select(ministry => new UPCI.DAL.DTO.Response.MemberMinistry
                                                           {
                                                               MemberCode = x.u.Code,
                                                               MinistryCode = ministry.MinistryCode!,
                                                               MinistryDesc = ministry.Ministry.Description,
                                                               Position = ministry.Position!,
                                                               PositionDesc = ministry.PositionMinistry.Description!,
                                                               DepartmentCode = ministry.Ministry.Department.Code,
                                                               DepartmentDesc = ministry.Ministry.Department.Description,
                                                           }).ToList(),
                            ImageContent = x.u.ImageContent,
                            ImageType = x.u.ImageType,
                            CreatedBy = x.u.CreatedBy,
                            CreatedDate = x.u.CreatedDate,
                            UpdatedBy = x.u.UpdatedBy,
                            UpdatedDate = x.u.UpdatedDate 

                            // Map other properties here
                        })
                    .FirstOrDefaultAsync();
                





                return await Task.FromResult(result.Result);
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
                 
                var memberList = _applicationDbContext.Member.Include(c => c.MemberCell).Include(m => m.MemberMinistry).
                    ProjectTo<Member>(_mapper.ConfigurationProvider).AsQueryable(); 

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

                var result = await (from u in pagedQuery
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

                                  ImageContent = u.ImageContent,
                                  ImageType = u.ImageType,
                                  CreatedBy = u.CreatedBy,
                                  CreatedDate = u.CreatedDate,
                                  UpdatedBy = u.UpdatedBy,
                                  UpdatedDate = u.UpdatedDate,
                                  Deleted = u.Deleted
                              }
                               ).ToListAsync();
                #region new code faster
                //var query = from u in pagedQuery
                //            join c in civilStatus on u.CivilStatus equals c.Code into civilGroup
                //            from c in civilGroup.DefaultIfEmpty()
                //            join mt in memberType on u.MemberType equals mt.Code into memberTypeGroup
                //            from mt in memberTypeGroup.DefaultIfEmpty()
                //            join pl in pepsolLevel on u.PEPSOL equals pl.Code into pepsolLevelGroup
                //            from pl in pepsolLevelGroup.DefaultIfEmpty()
                //            join mc in _applicationDbContext.MemberCell.AsQueryable() on u.Code equals mc.MemberCode
                //            select new
                //            {
                //                u.Id,
                //                u.Code,
                //                u.Sequence,
                //                u.Chapter,
                //                u.FirstName,
                //                u.MiddleName,
                //                u.LastName,
                //                u.Gender,
                //                u.CivilStatus,
                //                CivilStatusDesc = c.Description,
                //                u.Address,
                //                u.Birthday,
                //                u.BaptismDate,
                //                u.FirstAttend,
                //                u.Baptized,
                //                u.InvolvedToCell,
                //                u.PEPSOL,
                //                PEPSOLDesc = pl.Description,
                //                u.MemberType,
                //                MemberTypeDesc = mt.Description,
                //                u.Email,
                //                u.ContactNo,
                //                u.ActiveMember,
                //                u.ImageContent,
                //                u.ImageType,
                //                u.CreatedBy,
                //                u.CreatedDate,
                //                u.UpdatedBy,
                //                u.UpdatedDate,
                //                u.Deleted
                //            };
                //var result = query.Select(member => new UPCI.DAL.DTO.Response.FMember
                //{
                //    Id = StringManipulation.Encrypt(Convert.ToString(member.Id), _encryptionKey),
                //    Code = member.Code,
                //    Sequence = Convert.ToString(member.Sequence),
                //    Chapter = member.Chapter,
                //    FirstName = member.FirstName,
                //    MiddleName = member.MiddleName,
                //    LastName = member.LastName,
                //    Gender = member.Gender,
                //    CivilStatus = member.CivilStatus,
                //    CivilStatusDesc = member.CivilStatusDesc,
                //    Address = member.Address,
                //    Birthday = member.Birthday! == null ? "" : Convert.ToDateTime(member.Birthday!).ToString("yyyy-MM-dd"),
                //    BaptismDate = member.BaptismDate! == null ? "" : Convert.ToDateTime(member.BaptismDate!).ToString("yyyy-MM-dd"),
                //    FirstAttend = member.FirstAttend! == null ? "" : Convert.ToDateTime(member.FirstAttend!).ToString("yyyy-MM-dd"),
                //    Baptized = (bool)member.Baptized,
                //    InvolvedToCell = (bool)member.InvolvedToCell,
                //    PEPSOL = member.PEPSOL,
                //    PEPSOLDesc = member.PEPSOLDesc,
                //    MemberType = member.MemberType,
                //    MemberTypeDesc = member.MemberTypeDesc,
                //    Email = member.Email,
                //    ContactNo = member.ContactNo,
                //    ActiveMember = (bool)member.ActiveMember,
                //    ImageContent = member.ImageContent,
                //    ImageType = member.ImageType,
                //    CreatedBy = member.CreatedBy,
                //    CreatedDate = member.CreatedDate,
                //    UpdatedBy = member.UpdatedBy,
                //    UpdatedDate = member.UpdatedDate,
                //    Deleted = member.Deleted
                //}).ToList();
                #endregion
                vCell.Data = result;
                return await Task.FromResult(vCell);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new ();
            }
        }
        private static string ExtractBase64Data(string dataUrl)
        {
            const string base64Prefix = "data:image/jpeg;base64,";
            if (dataUrl.StartsWith(base64Prefix))
            {
                return dataUrl.Substring(base64Prefix.Length);
            }
            else
                return "";
        }
        public byte[] ConvertBase64ToByte(string rawString)
        {
            var base64String = ExtractBase64Data(rawString);
            if (Convert.ToString(base64String).Trim() != "")
                return Convert.FromBase64String(base64String);
            else
                return null;
            
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
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now 
                    };

                    if (model.ImageChanged && model.ImageContent != null)
                    {
                        data.ImageContent = ConvertBase64ToByte(model.ImageContent);
                        data.ImageType = model.ImageType.Trim();
                    }


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
                    if (model.ImageChanged && model.ImageContent != null)
                    {
                        data.ImageContent = ConvertBase64ToByte(model.ImageContent);
                        data.ImageType = model.ImageType.Trim();
                    }
                        
                     
                    
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
