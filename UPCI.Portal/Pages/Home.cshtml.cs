using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; 
using UPCI.Portal.Helpers;
using UPCI.DAL.Models;
using UPCI.BLL.Services.IService;
using UPCI.DAL.DTO.Request;
namespace UPCI.Portal.Pages
{
    public class HomeModel : PageModel
    { 
        IConfiguration _configuration;
        IUserService _userService;
        IRouteService _routeService;
        IReportService _reportService;
        readonly AppConfig _appConfig;

        [BindProperty]
        public string PageUrl { get; set; }

        [BindProperty]
        public string Actions { get; set; }

        public HomeModel(IConfiguration configuration, IUserService userService, IRouteService routeService, IReportService reportService)
        {
            
            _configuration = configuration;
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
            _userService = userService;
            _routeService = routeService;
            _reportService = reportService;
        }

        public IActionResult OnGet()
        {
            try
            {

                if (!String.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {

                    if (HttpContext.Session.GetString("ChangePassword") == "0")
                    {

                        var modules = HttpContext.Session.GetObject<List<DAL.DTO.Response.ModuleAccess>>("Modules");
                        var currentPage = HttpContext.Request.Path.Value;
                        var hasAccess = false;
                        if (modules != null)
                        {
                            ViewData["Navigation"] = Helper.LoadNav(modules, currentPage, _configuration);
                            hasAccess = Helper.HasAccess(modules, HttpContext.Request.Path);
                        }
                        else
                            ViewData["Navigation"] = null;

                        if (hasAccess)
                        {
                            Actions = (Helper.GetActions(modules, HttpContext.Request.Path));

                            //var x = _reportService.HTTPLogSummary().Result;
                            return Page();
                        }
                        else
                            return Redirect(HttpContext.Session.GetString("AppUrl") + "/Error?code=403");
                    }
                    else
                    {
                        //original
                        //return Redirect(HttpContext.Session.GetString("AppUrl") + "/Error?code=801");
                        HttpContext.Session.Clear();
                        return Redirect(HttpContext.Session.GetString("AppUrl"));

                    } 
                }
                else
                {
                    return Redirect(_appConfig.AppUrl);
                }
            }
            catch (Exception ex)
            {
                return Redirect(_appConfig.AppUrl);
            }
        } 
        public async Task<IActionResult> OnGetUserProfileImage()
        {
            try
            {
                var username = Convert.ToString(HttpContext.Session.GetString("Username")); 
                var user = await _userService.ByUserName(username);

                if (user != null)
                    return File(user.ImageContent, user.ImageType);
                else
                    return File("~/Assets/Images/default-user.jpg", "image/jpg");
            }
            catch(Exception ex)
            {
                return File("~/Assets/Images/default-user.jpg", "image/jpg");
            }

        }
        public async Task<JsonResult> OnPostChangeProfileImage(List<IFormFile> images)
        {

            try
            {
                var user = new UPCI.DAL.DTO.Request.User();
                if (images.Count > 0)
                {
                    MemoryStream imageMS = new MemoryStream();
                    images[0].CopyTo(imageMS);


                    user.Username = HttpContext.Session.GetString("Username");
                    user.OpUser = HttpContext.Session.GetString("Username");
                    user.ImageContent = imageMS.ToArray();
                    user.ImageType = images[0].ContentType; 
                }
                var result = await _userService.ChangeProfileImage(user);
                return new JsonResult(result);
            }
            catch (Exception ex)
            { 
                return new JsonResult(ex.ToString());
            }
        }

        public JsonResult OnGetSessionExpired()
        {
            try
            { 
                var userRequest = new DAL.DTO.Request.User() 
                { 
                    Username = Convert.ToString(HttpContext.Session.GetString("Username")), 
                    Terminal = Convert.ToString(HttpContext.Connection.RemoteIpAddress!) 
                };
                var result = _userService.CheckSession(userRequest).Result; 

                if (result == false)
                {
                    HttpContext.Session.Clear();
                    return new JsonResult("true"); 
                }
                else
                    return new JsonResult("false");
            }
            catch(Exception ex)
            {
                HttpContext.Session.Clear();
                return new JsonResult("true");
            }

        }
        public async Task<JsonResult> OnGetLogout()
        {
            await _userService.Logout(Convert.ToString(HttpContext.Session.GetString("Username")));
            HttpContext.Session.Clear();
            return new JsonResult("logout");
        }
    }
}
