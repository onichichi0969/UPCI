using UPCI.BLL.Services;
using UPCI.BLL.Services.IService;
using UPCI.DAL.Models;
using UPCI.Portal.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; 
using Newtonsoft.Json; 

namespace UPCI.Portal.Pages
{
    public class ActiveSessionModel : PageModel
    {

        [BindProperty]
        public string Actions { get; set; }

        IConfiguration _configuration;
        IModuleService _moduleService;
        IUserService _userService;
        readonly AppConfig _appConfig;

        public ActiveSessionModel(IConfiguration configuration, IModuleService moduleService, IUserService userService)
        {
            _configuration = configuration; 
            _moduleService = moduleService;
            _userService = userService;
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
        }

        public IActionResult OnGet()
        {
            try
            {

                if (!String.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {

                    if (HttpContext.Session.GetString("ActiveSession") == "1")
                    {
                        return Page();
                    }
                    else
                    {
                        var modules = _moduleService.GetUserModuleAccess(Convert.ToString(HttpContext.Session.GetString("UserRole"))).Result;
                        HttpContext.Session.SetObject("Modules", modules); 

                        return Redirect(HttpContext.Session.GetString("AppUrl") + "/Home");
                    }

                }
                else
                {
                    return Redirect(_appConfig.AppUrl);
                }
            }
            catch
            {
                return Redirect(_appConfig.AppUrl);
            }
        }

        public IActionResult OnPostContinue()
        { 
            HttpContext.Session.SetString("ActiveSession", "0");

            if (HttpContext.Session.GetString("ChangePassword") == "0")
            {
                var modules = _moduleService.GetUserModuleAccess(Convert.ToString(HttpContext.Session.GetString("UserRole"))).Result;
                 
                HttpContext.Session.SetObject("Modules", modules);

                return Redirect(_appConfig.AppUrl + "/Home");

            }
            else
                return Redirect(_appConfig.AppUrl + "/ChangePassword");

        }
        public IActionResult OnPostLogout()
        {
            _userService.Logout(Convert.ToString(HttpContext.Session.GetString("Username")));
            HttpContext.Session.Clear();
            return Redirect(_appConfig.AppUrl);
        }

    }
}
