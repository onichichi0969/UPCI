using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using UPCI.DAL.DTO.Response;
using UPCI.DAL.Models;
using UPCI.Portal.Helpers;

namespace UPCI.Pages
{
     
    public class ErrorModel : PageModel
    {
        [BindProperty]
        public string Actions { get; set; }
        [BindProperty]
        public string PageTitle { get; set; }
        [BindProperty]
        public string Icon { get; set; }
        [BindProperty]
        public string PageUrl { get; set; }

        [BindProperty]
        public string Code { get; set; }
        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string Message { get; set; }


        IConfiguration _configuration;
        readonly AppConfig _appConfig;
        public ErrorModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _appConfig = configuration.GetSection("AppSettings").Get<AppConfig>();
        }
        public IActionResult OnGet()
        {
            try
            {
                if (!String.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {
                    var modules = HttpContext.Session.GetObject<List<ModuleAccess>>("UserGroupModules");
                    var currentPage = HttpContext.Request.Path.Value;

                    ViewData["Navigation"] = Helper.LoadNav(modules, currentPage);

                    var error = Helper.ErrorMessage(HttpContext.Request.Query["code"]);

                    Code = error.Code;
                    Title = error.Title;
                    Message = error.Message;

                    return Page();

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

        public IActionResult OnPostReturnToLogin()
        {
            HttpContext.Session.Clear(); // Clear the session

            // Redirect to the login page or desired URL
            return RedirectToPage("/Login"); // Adjust to your actual login page URL
        }
    }

}
