using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using UPCI.BLL.Services.IService;
using UPCI.DAL; 
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Transactions; 

namespace UPCI.BLL.Services
{
    public class UserService(ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<User> userRepository, IRepository<ActiveUser> activeUserRepository, ILogService logService, IConfiguration configuration, IMailerService mailerService) : IUserService
    {
        readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        readonly IMapper _mapper = mapper;
        readonly IRepository<User> _userRepository = userRepository;
        readonly IRepository<ActiveUser> _activeUserRepository = activeUserRepository;
        readonly ILogService _logService = logService;
        readonly string _moduleName = "User";
        readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;
        readonly MailSettings mailSettings = configuration.GetSection("MailSettings").Get<MailSettings>()!;
        readonly IMailerService _mailerService = mailerService;
        readonly SystemParameters _systemParameters = configuration.GetSection("SystemParameters").Get<SystemParameters>()!;
        public async Task<DAL.DTO.Response.User> Login(DAL.DTO.Request.User user)
        {
            //var x = StringManipulation.Encrypt("1", configuration["AppContext:EncryptionKey"]!);
            try
            {
                var result = _applicationDbContext.User!.Include(u => u.Role).FirstOrDefault(
                    u => u.Username!.Trim().ToUpper() == user.Username!.Trim().ToUpper()
                    && u.Password!.Trim() == StringManipulation.Encrypt(user.Password, _encryptionKey)
                    && u.Deleted != true); 

                var userInfo = new DAL.DTO.Response.User();
                if (result != null)
                {
                    userInfo.Username = result.Username;
                    userInfo.FirstName = result.FirstName;
                    userInfo.MiddleName = result.MiddleName;
                    userInfo.LastName = result.LastName;
                    userInfo.Email = result.Email;
                    userInfo.EncryptedRoleId = StringManipulation.Encrypt(result.RoleId.ToString(), configuration["AppContext:EncryptionKey"]!);
                    userInfo.RoleDescription = result.Role.Description;
                    userInfo.RoleCode = result.Role.Code;
                    userInfo.PasswordAttempt = result.PasswordAttempt;
                    userInfo.PasswordExpirationDate = result.PasswordExpirationDate;
                    userInfo.PasswordLastChange = result.PasswordLastChange;
                    userInfo.DefaultPassword = result.DefaultPassword;
                    userInfo.Deleted = result.Deleted;
                    userInfo.IsLocked = Convert.ToBoolean(result.Status);

                    await _activeUserRepository.AddAsync(new ActiveUser 
                    {
                        Username = user.Username,
                        Terminal = user.Terminal.ToUpper(),
                        ActivityDate = DateTime.Now
                    });
                }
                else
                { 
                    // adding passwordattempt
                    var invalidUser = _applicationDbContext.User!.FirstOrDefault(
                    u => u.Username!.Trim().ToUpper() == user.Username!.Trim().ToUpper()
                    && u.Deleted != true);

                    if (invalidUser != null)
                    {
                        if (_systemParameters != null)
                        {

                            if (invalidUser.PasswordAttempt > (_systemParameters.PasswordMaxTry <= 0 ? 3: _systemParameters.PasswordMaxTry))
                                invalidUser.Status = 1;
                            invalidUser.PasswordAttempt += 1;
                        }
                        else
                        {
                            if (invalidUser.PasswordAttempt > 3)
                                invalidUser.Status = 1;
                            invalidUser.PasswordAttempt += 1;
                        }
                       

                        await _userRepository.UpdateAsync(invalidUser);
                    }
                }
                return await Task.FromResult(userInfo);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<bool> CheckSession(DAL.DTO.Request.User User)
        {
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                double? sessionTimeoutMinutes = 0;
                if (_systemParameters != null)
                {
                    sessionTimeoutMinutes = _systemParameters.SessionTimeOutMinutes <= 0 ? 3 : _systemParameters.SessionTimeOutMinutes;
                }
                else
                {
                    sessionTimeoutMinutes = 3;
                }

                var userActivity = await _applicationDbContext.ActiveUser!
                        .Where(u => u.Username == User.Username)
                        .GroupBy(u => u.Username)
                        .Select(g => new
                        {
                            Count = g.Count(),
                            LatestActivity = g.Max(u => u.ActivityDate)
                        })
                        .FirstOrDefaultAsync();

                if (userActivity != null && userActivity.Count > 0)
                {
                    if (DateTime.Now.Subtract((DateTime)userActivity.LatestActivity).TotalMinutes >= sessionTimeoutMinutes)
                    {
                        return await Task.FromResult(false);
                    }
                    else
                    {
                        return await Task.FromResult(true);
                    }
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName);
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return await Task.FromResult(false);
            }
            finally
            {
                transactionScope.Dispose();
            }
        }
        public async Task<bool> CheckActiveSession(DAL.DTO.Request.User User)
        { 
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try 
            {
                double? sessionTimeoutMinutes = 0;
                if (_systemParameters != null)
                {
                    sessionTimeoutMinutes = _systemParameters.SessionTimeOutMinutes <= 0 ? 3 : _systemParameters.SessionTimeOutMinutes;
                }
                else 
                {
                    sessionTimeoutMinutes = 3;
                }
                
                var userActivity = await _applicationDbContext.ActiveUser!
                    .Where(u => u.Username.ToUpper() == User.Username.ToUpper())
                    .GroupBy(u => u.Username)
                    .Select(g => new
                    {
                        Count = g.Count(),
                        LatestActivity = g.Max(u => u.ActivityDate),
                        Terminal = g.OrderByDescending(u => u.ActivityDate).Select(u => u.Terminal).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (userActivity != null && userActivity.Count > 0)
                {
                    if (userActivity.Terminal != User.Terminal)
                    {
                        if (DateTime.Now.Subtract((DateTime)userActivity.LatestActivity!).TotalMinutes >= sessionTimeoutMinutes)
                        {
                            return await Task.FromResult(false);
                        }
                        else
                        {
                            return await Task.FromResult(true);
                        }
                    }
                    else
                    {
                        return await Task.FromResult(false);
                    }
                }
                else
                {
                    return await Task.FromResult(false);
                } 
            }
            catch (TransactionAbortedException ex)
            {
                _logService.LogException(ex, _moduleName);
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return await Task.FromResult(false);
            }
            finally
            {
                transactionScope.Dispose(); 
            } 
        }
        public async Task<UPCI.DAL.DTO.Response.Result> RefreshAttempt(string username)
        {
            UPCI.DAL.DTO.Response.Result result = new();
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                
                
                var userInfo = _applicationDbContext.User!.Include(u => u.Role).FirstOrDefault(
                        u => u.Username!.Trim().ToUpper() == username!.Trim().ToUpper()
                        && u.Deleted != true);

                if (userInfo != null)
                {
                    userInfo.PasswordAttempt = 0;
                }

                await _userRepository.UpdateAsync(userInfo);

                result.Status = "SUCCESS";
                result.Message = string.Format("{0} refreshed.", "User password attempts");
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
                return new();
            }
            finally
            {
                transactionScope.Dispose();
            }
            return await Task.FromResult(result);
        }
        public async Task Logout(string username)
        {
            UPCI.DAL.DTO.Response.Result result = new();
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {

                var activeUser = _applicationDbContext.ActiveUser!.Where(
                        u => u.Username!.Trim().ToUpper() == username!.Trim().ToUpper()).AsQueryable();

                if (activeUser != null)
                {
                    await _activeUserRepository.RemoveRangeAsync(activeUser); 
                }
                 
                result.Status = "SUCCESS";
                result.Message = string.Format("{0} deleted", "Active User");
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
        }
        public async Task<UPCI.DAL.DTO.Response.User> ByUserName(string username)
        {
            try
            {
                var result = _applicationDbContext.User!.Include(u => u.Role).FirstOrDefault(
                    u => u.Username!.Trim().ToUpper() == username!.Trim().ToUpper()
                    && u.Deleted != true);

                var userInfo = new DAL.DTO.Response.User();
                if (result != null)
                {
                    userInfo.Username = result.Username;
                    userInfo.FirstName = result.FirstName;
                    userInfo.MiddleName = result.MiddleName;
                    userInfo.LastName = result.LastName;
                    userInfo.Email = result.Email;
                    userInfo.EncryptedRoleId = StringManipulation.Encrypt(result.RoleId.ToString(), configuration["AppContext:EncryptionKey"]!);
                    userInfo.RoleDescription = result.Role.Description;
                    userInfo.RoleCode = result.Role.Code;
                    userInfo.PasswordAttempt = result.PasswordAttempt;
                    userInfo.PasswordExpirationDate = result.PasswordExpirationDate;
                    userInfo.PasswordLastChange = result.PasswordLastChange;
                    userInfo.DefaultPassword = result.DefaultPassword;
                    userInfo.Deleted = result.Deleted;
                    userInfo.IsLocked = Convert.ToBoolean(result.Status); 
                    userInfo.ImageContent = result.ImageContent;
                    userInfo.ImageType = result.ImageType;
                }
                 
                return await Task.FromResult(userInfo);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }

        public async Task<List<UPCI.DAL.DTO.Response.ActivityLog>> RecentActivity(string username)
        {
            try
            {
                var query = _applicationDbContext.ActivityLog!.Where(u => u.UserId == username ).OrderByDescending(a => a.LogDate).Take(10).AsQueryable();
                var result = (from q in query
                              select new UPCI.DAL.DTO.Response.ActivityLog
                              { 
                                Id = q.Id,
                                UserId = q.UserId,
                                ModuleName = q.ModuleName,
                                Action = q.Action,
                                Details = q.Details,
                                LogDate = Convert.ToDateTime(q.LogDate).ToString("yyyy-MM-dd-HH:mm:ss"),    
                              });

                return await Task.FromResult(result.ToList());
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        }
        public async Task<List<UPCI.DAL.DTO.Response.User>> Get()
        {
            try
            {
                var result = _applicationDbContext.Set<User>()
                    .Where(e => e.Deleted != true).ProjectTo<UPCI.DAL.DTO.Response.User>(_mapper.ConfigurationProvider)!.ToList();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        } 
        public async Task<UPCI.DAL.DTO.Response.VUser> Filter(UPCI.DAL.DTO.Request.FParam model)
        {
            try
            {
                var propertySelector = EFramework.BuildPropertySelector<User>(model.SortColumn);

                UPCI.DAL.DTO.Response.VUser vUser = new();
                model.Filters.AddRange(new List<DAL.Models.Filter>
                {
                    new DAL.Models.Filter
                    {
                        Property = "Deleted",
                        Operator = "NOTEQUALS",
                        Value = true
                    },
                    new DAL.Models.Filter
                    {
                        Property = "Username",
                        Operator = "NOTEQUALS",
                        Value = model.OpUser
                    }
                });
                //var query = _applicationDbContext.User!.AsQueryable();

                var userList = _applicationDbContext.Set<User>().AsQueryable();
                //var roleList = _applicationDbContext.Set<Role>().AsQueryable();

               
                if (model.Filters != null && model.Filters.Count != 0)
                    userList = userList.Where(ExpressionBuilder.GetExpression<User>(model.Filters));

                //query = query.Where(e => (e.FirstName.Contains(model.Prefix) || e.LastName!.Contains(model.Prefix) || e.Username.Contains(model.Prefix)) && e.Deleted != true);

                if (model.Descending)
                {
                    userList = userList.OrderByDescending(propertySelector);
                }
                else
                {
                    userList = userList.OrderBy(propertySelector);
                }
                
                vUser.CurrentPage = model.PageNum;
                vUser.TotalRecord = userList.Count();
                vUser.TotalPage = (int)Math.Ceiling((double)vUser.TotalRecord / model.PageSize);

                int recordsToSkip = (model.PageNum - 1) * model.PageSize;
                var pagedQuery = userList.Skip(recordsToSkip).Take(model.PageSize);

                //vUser.Data = _mapper.Map<List<UPCI.DAL.DTO.Response.FUser>>(pagedQuery.ToList());
                var result = (from u in pagedQuery
                              //join r in roleList on u.RoleId equals r.Id
                              select new UPCI.DAL.DTO.Response.FUser
                              {
                                  Id = u.Id.ToString(),
                                  Username = u.Username,
                                  FirstName = u.FirstName,
                                  MiddleName = u.MiddleName,
                                  LastName = u.LastName,
                                  EncryptedRoleId = UPCI.DAL.Helpers.StringManipulation.Encrypt(u.RoleId.ToString(), configuration["AppContext:EncryptionKey"]!),
                                  RoleDescription = u.Role.Name,
                                  RoleCode = u.Role.Code,
                                  IsLocked  = Convert.ToBoolean(u.Status), 
                                  ImageContent = u.ImageContent,
                                  Email = u.Email,
                                  //ImageContentBase64 = u.ImageContent == null? "" : Convert.ToBase64String(u.ImageContent),
                                  ImageType = u.ImageType,
                                  Deleted = u.Deleted
                              }
                               ).ToList();
                vUser.Data = result;
                return await Task.FromResult(vUser);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, _moduleName);
                return new();
            }
        } 
        
        
        public async Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var action = "ADD";
                var userId = _applicationDbContext.User!.FirstOrDefault(u => u.Username == model.OpUser)!.Username;

                var activityLog = new ActivityLog()
                {
                    UserId = userId.ToString(),
                    ModuleName = _moduleName,
                    Action = action
                };


                var data = _applicationDbContext.User!.FirstOrDefault(l => l.Username == model.Username);

                if (data == null)
                {
                    var randomPassword = StringManipulation.Random(10);
                    User user = new()
                    {
                        Username = model.Username.Trim().ToUpper(),
                        Password = UPCI.DAL.Helpers.StringManipulation.Encrypt(randomPassword, _encryptionKey),
                        //Password = UPCI.DAL.Helpers.StringManipulation.Encrypt("123456", _encryptionKey),
                        FirstName = model.FirstName.Trim(),
                        MiddleName = model.MiddleName.Trim(),
                        LastName = model.LastName.Trim(),
                        Email = model.Email.Trim(),
                        RoleId = int.Parse(UPCI.DAL.Helpers.StringManipulation.Decrypt(model.EncryptedRoleId!, _encryptionKey)),
                        Status = 0,
                        PasswordExpirationDate = DateTime.Now.AddMonths(3),
                        DefaultPassword = true,
                        Deleted = false,
                        CreatedBy = userId.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _userRepository.AddAsync(user); 

                    //mailer 
                    //_mailerService.Send(mailSettings, user.Email, "EQUICOM SAVINGS BANK Gateway Portal Notification : User Created", MailTemplate.CreateUser(user,_encryptionKey), true);
                    _mailerService.Send(mailSettings, user.Email, "EQUICOM SAVINGS BANK Gateway Portal Notification : User Created", MailTemplate.CreateUserPasswordless(user), true);

                    

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = action,
                        UserId = user.Id.ToString()!,
                        ActionDate = (DateTime)user.CreatedDate,
                        TableName = _moduleName,
                        OldValues = "",
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[code: {0}] created.", user.Username);

                    result = new UPCI.DAL.DTO.Response.Result() { Status = "SUCCESS", Message = string.Format("{0} created.", _moduleName) };

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);
                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} already exist.", _moduleName);
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

        public async Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "EDIT";
                int _updateStatus = 0;

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };

                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Id.ToString().Trim() == model.Id.Trim())!;

                if (user != null)
                {
                    if (user.Id.ToString().ToUpper().Trim() == model.Id.Trim() && user.Username == model.Username)
                    {
                        _updateStatus = 1;
                    }
                    else
                    {
                        var x = _applicationDbContext.User!.Any(d => d.Username == model.Username);

                        if (!x)
                            _updateStatus = 1;
                        else
                        {
                            _updateStatus = -1;
                            result.Status = "FAILED";
                            result.Message = string.Format($"{_moduleName} [username: {model.Username}] already exist.");
                        }
                    }
                }
                else
                {
                    _updateStatus = 0;
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }

                if (_updateStatus == 1)
                {

                    var oldValue = EFramework.GetEntityProperties(user!);

                    user!.FirstName = model.FirstName.Trim();
                    user.MiddleName = model.MiddleName.Trim();
                    user.LastName = model.LastName.Trim();
                    user.RoleId = int.Parse(UPCI.DAL.Helpers.StringManipulation.Decrypt(model.EncryptedRoleId!, _encryptionKey));
                    user.UpdatedBy = model.OpUser.Trim();
                    user.UpdatedDate = DateTime.Now;

                    await _userRepository.UpdateAsync(user);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.Id.ToString()!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[user: {0}] updated.", user.Username);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} updated.", _moduleName);

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

        public async Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();
            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "DELETE";

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };

                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Id.ToString().Trim() == model.Id.Trim())!;

                if (user != null)
                {
                    var oldValue = EFramework.GetEntityProperties(user!);

                    user.Deleted = true;
                    user.UpdatedBy = model.OpUser;
                    user.UpdatedDate = DateTime.Now;

                    await _userRepository.DeleteAsync(user);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.UpdatedBy!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = "[deleted :false]",
                        NewValues = "[deleted :true]"
                    };

                    activityLog.Details = string.Format("[code: {0}] deleted.", user.Username);
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
        public async Task<UPCI.DAL.DTO.Response.Result> ChangePassword(UPCI.DAL.DTO.Request.UserPassword model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "CHANGEPASS";

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };


                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Username.ToString().Trim().ToUpper() == model.Username.Trim().ToUpper())!;

                if (user != null)
                {
                    if (StringManipulation.Decrypt(user.Password, _encryptionKey) != model.Current)
                    {
                        result.Status = "Invalid";
                        result.Message = @"Invalid current password!";
                        return result;
                    }
                    if (model.New != model.Confirm)
                    {
                        result.Status = "Invalid";
                        result.Message = @"New password does not match with Confirm password!";
                        return result;
                    }

                    var oldValue = EFramework.GetEntityProperties(user!);

                    user.Password = StringManipulation.Encrypt(model.New, _encryptionKey);
                    user.PasswordLastChange = DateTime.Now;
                    user.PasswordExpirationDate = DateTime.Now.AddMonths(3);
                    user.PasswordAttempt = 0;
                    user.Status = 0;
                    user.UpdatedBy = model.Username;
                    user.UpdatedDate = DateTime.Now;

                    await _userRepository.UpdateAsync(user);
                    //mailer 
                    _mailerService.Send(mailSettings, user.Email, "EQUICOM SAVINGS BANK Gateway Portal Notification : Unlock User", MailTemplate.ChangePassword(user, _encryptionKey), true);
                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.UpdatedBy!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[user: {0}] password updated.", user.Username);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} updated.", _moduleName);

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                    transactionScope.Complete();
                    return result;
                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }



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
        public async Task<UPCI.DAL.DTO.Response.Result> ResetPassword(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "RESETPASS";

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };


                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Username.ToString().Trim().ToUpper() == model.Username.Trim().ToUpper())!;

                if (user != null)
                {

                    var oldValue = EFramework.GetEntityProperties(user!);
                    var randomPassword = StringManipulation.Random(10);
                    user.Password = StringManipulation.Encrypt(randomPassword, _encryptionKey);
                    user.PasswordExpirationDate = DateTime.Now.AddDays(1);
                    user.Status = 0;
                    user.PasswordAttempt = 0;
                    user.DefaultPassword = true;
                    user.UpdatedBy = model.Username;
                    user.UpdatedDate = DateTime.Now;


                    await _userRepository.UpdateAsync(user);
                    //mailer 
                    _mailerService.Send(mailSettings, user.Email, "EQUICOM SAVINGS BANK Gateway Portal Notification : Password Reset", MailTemplate.ResetPassword(user, _encryptionKey), true);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.UpdatedBy!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[user: {0}] password updated.", user.Username);
                    result.Status = "SUCCESS";
                    result.Message = randomPassword;

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                    transactionScope.Complete();
                    return result;
                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }



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
        public async Task<UPCI.DAL.DTO.Response.Result> UnlockUser(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "UNLOCKUSER";

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };


                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Username.ToString().Trim().ToUpper() == model.Username.Trim().ToUpper())!;

                if (user != null)
                {
                    var oldValue = EFramework.GetEntityProperties(user!);
                    user.PasswordAttempt = 0;
                    user.Status = 0;
                    user.UpdatedBy = model.Username;
                    user.UpdatedDate = DateTime.Now;

                    await _userRepository.UpdateAsync(user);
                    //mailer 
                    _mailerService.Send(mailSettings, user.Email, "EQUICOM SAVINGS BANK Gateway Portal Notification : Unlock User", MailTemplate.UnlockUser(user), true);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.UpdatedBy!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[user: {0}] password updated.", user.Username);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} updated.", _moduleName);

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                    transactionScope.Complete();
                    return result;
                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }



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
        public async Task<UPCI.DAL.DTO.Response.Result> ChangeProfileImage(UPCI.DAL.DTO.Request.User model)
        {
            UPCI.DAL.DTO.Response.Result result = new();

            TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var _action = "CHANGEPROFILEIMAGE";

                var activityLog = new ActivityLog()
                {
                    UserId = model.OpUser!,
                    ModuleName = _moduleName,
                    Action = _action
                };


                var user = _applicationDbContext.User!.FirstOrDefault(l => l.Username.ToString().Trim().ToUpper() == model.Username.Trim().ToUpper())!;

                if (user != null)
                {
                    var oldValue = EFramework.GetEntityProperties(user!);
                    user.ImageContent = model.ImageContent;
                    user.ImageType = model.ImageType;
                    user.UpdatedBy = model.Username;
                    user.UpdatedDate = DateTime.Now;


                    await _userRepository.UpdateAsync(user);

                    var auditTrail = new AuditTrail()
                    {
                        RecordId = user.Id.ToString(),
                        Terminal = model.Terminal!,
                        Action = _action,
                        UserId = user.UpdatedBy!,
                        ActionDate = (DateTime)user.UpdatedDate,
                        TableName = _moduleName,
                        OldValues = oldValue,
                        NewValues = EFramework.GetEntityProperties(user)
                    };

                    activityLog.Details = string.Format("[user: {0}] profile image updated.", user.Username);
                    result.Status = "SUCCESS";
                    result.Message = string.Format("{0} updated.", _moduleName);

                    _logService.LogActivity(activityLog);
                    _logService.LogAudit(auditTrail!);

                    transactionScope.Complete();
                    return result;
                }
                else
                {
                    result.Status = "FAILED";
                    result.Message = string.Format("{0} not exist.", _moduleName);
                }



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
