namespace UPCI.BLL.Services.IService
{
    public interface IUserService
    {
        //Task<UPCI.DAL.DTO.Response.Result> Validate(UPCI.DAL.DTO.Request.Credential model);
        Task<DAL.DTO.Response.User> Login(DAL.DTO.Request.User user);
        Task<List<UPCI.DAL.DTO.Response.ActivityLog>> RecentActivity(string username);
        Task<bool> CheckSession(DAL.DTO.Request.User User);
        Task<bool> CheckActiveSession(DAL.DTO.Request.User User);
        Task<UPCI.DAL.DTO.Response.Result> RefreshAttempt(string username);
        Task Logout(string username);
        Task<UPCI.DAL.DTO.Response.User> ByUserName(string username);
        Task<List<UPCI.DAL.DTO.Response.User>> Get();
        Task<UPCI.DAL.DTO.Response.VUser> Filter(UPCI.DAL.DTO.Request.FParam model);
        Task<UPCI.DAL.DTO.Response.Result> ChangePassword(UPCI.DAL.DTO.Request.UserPassword model);
        Task<UPCI.DAL.DTO.Response.Result> ChangeProfileImage(UPCI.DAL.DTO.Request.User model);
        Task<UPCI.DAL.DTO.Response.Result> UnlockUser(UPCI.DAL.DTO.Request.User model);
        Task<UPCI.DAL.DTO.Response.Result> ResetPassword(UPCI.DAL.DTO.Request.User model);
        Task<UPCI.DAL.DTO.Response.Result> Create(UPCI.DAL.DTO.Request.User model);
        Task<UPCI.DAL.DTO.Response.Result> Update(UPCI.DAL.DTO.Request.User model);
        Task<UPCI.DAL.DTO.Response.Result> Delete(UPCI.DAL.DTO.Request.User model);
    }
}
