using UPCI.BLL.Services;
using UPCI.BLL.Services.IService;
using UPCI.DAL.Models; 
using UPCI.Portal.Helpers; 
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace UPCI.Portal.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool IsLogout { get; set; }

        [BindProperty]
        public string JavascriptToRun { get; set; }

        //Popup messages

        [BindProperty]
        public SweetAlertMessage SweetAlertMessage { get; set; }

        IConfiguration _configuration;
        IModuleService _moduleService;
        IUserService _userService; 
        
        readonly AppConfig _appConfig;
        public LoginModel(IConfiguration configuration, IModuleService moduleService, IUserService userService)
        {
            _configuration = configuration;
            _moduleService = moduleService;
            _userService = userService; 
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
        }

        public IActionResult OnGet()
        {  
            SweetAlertMessage = new SweetAlertMessage();
            try
            {

                if (!String.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {
                    return Redirect(_appConfig.AppUrl + "/Home");
                }
                else
                {
                    return Page();
                }
            }
            catch
            {
                return Page();
            }
        }
        public IActionResult OnPost() 
        { 
            try
            {
                var userRequest = new DAL.DTO.Request.User() { Username = Username, Password = Password, Terminal = Convert.ToString(HttpContext.Connection.RemoteIpAddress!) };
                var userInfo = _userService.Login(userRequest).Result;

                if (userInfo.Username == "")
                {
                    SweetAlertMessage = new SweetAlertMessage
                    {
                        Title = "Error",
                        Message = "Invalid Username or Password.",
                        MessageType = "error"
                    };
                    JavascriptToRun = "ShowPopup()";
                    return Page();
                }
                if ((bool)userInfo.Deleted!)
                {
                    SweetAlertMessage = new SweetAlertMessage
                    {
                        Title = "Error",
                        Message = "Your account is disabled. Please contact your system administrator.",
                        MessageType = "error"
                    };
                    JavascriptToRun = "ShowPopup()";
                    return Page();
                }
                if ((bool)userInfo.IsLocked!)
                {
                    SweetAlertMessage = new SweetAlertMessage
                    {
                        Title = "Error",
                        Message = "Your account is locked. Please contact your system administrator.",
                        MessageType = "error"
                    };
                    JavascriptToRun = "ShowPopup()";
                    return Page();
                }
                var modules = _moduleService.GetUserModuleAccess(userInfo.RoleCode).Result;

                HttpContext.Session.SetString("Username", userInfo.Username!);
                HttpContext.Session.SetString("Email", userInfo.Email!);
                HttpContext.Session.SetString("PasswordExpirationDate", Convert.ToDateTime(userInfo.PasswordExpirationDate!).ToString("yyyy-MM-dd"));
                HttpContext.Session.SetString("UserRole", userInfo.RoleCode!);
                HttpContext.Session.SetString("UserRoleDesc", userInfo.RoleDescription!);
                HttpContext.Session.SetString("FullName", userInfo.LastName!.Trim() + ", " + userInfo.FirstName!.Trim() + " " + userInfo.MiddleName!.Trim());
                HttpContext.Session.SetString("AppUrl", _appConfig.AppUrl);
                HttpContext.Session.SetString("Terminal", HttpContext.Connection.RemoteIpAddress!.ToString());
                HttpContext.Session.SetString("AppName", _configuration["AppSettings:AppName"]!);
                //set change password
                if (userInfo.PasswordExpirationDate < DateTime.Now || (bool)userInfo.DefaultPassword!)
                    HttpContext.Session.SetString("ChangePassword", "1");
                else
                    HttpContext.Session.SetString("ChangePassword", "0");

                if (_userService.CheckActiveSession(userRequest).Result == false)
                    HttpContext.Session.SetString("ActiveSession", "0");
                else
                    HttpContext.Session.SetString("ActiveSession", "1");



                if (HttpContext.Session.GetString("ActiveSession") == "0")
                {
                    if (HttpContext.Session.GetString("ChangePassword") == "1")
                        return Redirect(_appConfig.AppUrl + "/ChangePassword");
                    else
                    {
                        HttpContext.Session.SetObject("Modules", modules);
                        var refreshResult = _userService.RefreshAttempt(userInfo.Username).Result;
                        return Redirect(_appConfig.AppUrl + "/Home");
                    }
                }
                else
                {
                    return Redirect(_appConfig.AppUrl + "/ActiveSession");
                }
            }
            catch (Exception ex)
            {
                SweetAlertMessage = new SweetAlertMessage
                {
                    Title = "Error",
                    Message = "Internal server error!",
                    MessageType = "error"
                };

                JavascriptToRun = "ShowPopup()";
                return Page();
            }
        }
    }
    internal class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string Application { get; set; }
    }

}
