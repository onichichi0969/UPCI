using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPCI.BLL.Services.IService;
using UPCI.DAL.DTO.Request;
using UPCI.DAL.Models;
using UPCI.Portal.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 

namespace UPCI.Portal.Pages
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public string Current { get; set; }
        [BindProperty]
        public string New { get; set; }

        [BindProperty]
        public string Confirm { get; set; }
         
        [BindProperty]
        public string JavascriptToRun { get; set; }

        //Popup messages

        [BindProperty]
        public SweetAlertMessage SweetAlertMessage { get; set; }
          
        IConfiguration _configuration;
        IUserService _userService;
        readonly AppConfig _appConfig;

        static readonly string projectOrigin = "[WEB][ChangePassword]";

        public ChangePasswordModel(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
            _userService = userService;
        }

        public IActionResult OnGet()
        {

            SweetAlertMessage = new SweetAlertMessage();
             
            try
            {
                if (!String.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                {
                    var modules = HttpContext.Session.GetObject<List<DAL.DTO.Response.ModuleAccess>>("Modules");
                    var currentPage = HttpContext.Request.Path.Value;

                    HttpContext.Session.SetString("Navs", Helper.LoadNav(modules, currentPage, _configuration));
                    ViewData["Navigation"] = HttpContext.Session.GetString("Navs");
                      
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

        public async Task<IActionResult> OnPostAsync()
        {
            try 
            {
                bool process = true;
                if (Current.Trim() == "" || New.Trim() == "" || Confirm.Trim() == "")
                {
                    SweetAlertMessage = new SweetAlertMessage
                    {
                        Title = "Failed",
                        Message = "Fill out the fields",
                        MessageType = "error"
                    };
                    JavascriptToRun = "ShowPopup()";
                    process = false;
                } 
                if(process)
                {
                    UserPassword userPassword = new()
                    {
                        Current = Current,
                        New = New,
                        Confirm = Confirm,
                        Username = HttpContext.Session.GetString("Username"),
                    };

                    var result = _userService.ChangePassword(userPassword).Result;

                    if (result.Status == "SUCCESS")
                    {
                        HttpContext.Session.Clear();
                        JavascriptToRun = "PopupAndLogout()";
                    }
                    else
                    {
                        SweetAlertMessage = new SweetAlertMessage
                        {
                            Title = result.Status,
                            Message = result.Message,
                            MessageType = "error"
                        };
                        JavascriptToRun = "ShowPopup()";
                    } 
                }
                ViewData["Navigation"] = HttpContext.Session.GetString("Navs");
                return Page();
            }
            catch(Exception ex) 
            { 
                SweetAlertMessage = new SweetAlertMessage
                {
                    Title = "Error",
                    Message = "Internal server error",
                    MessageType = "error"
                };
                JavascriptToRun = "ShowPopup()";

                ViewData["Navigation"] = HttpContext.Session.GetString("Navs");
                return Page();
            }

           
            
        }
    }
}