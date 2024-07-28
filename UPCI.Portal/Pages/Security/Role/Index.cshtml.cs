using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UPCI.BLL.Services.IService;
using UPCI.DAL.DTO.Request;
using UPCI.DAL.Models;
using UPCI.BLL.Services;
using UPCI.Portal.Helpers;

namespace UPCI.Portal.Pages.Maintenance.Role
{
    public class IndexModel : PageModel
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
        public string Current { get; set; }
        [BindProperty]
        public string New { get; set; }

        [BindProperty]
        public string Confirm { get; set; }

        [BindProperty]
        public string Result { get; set; }
        [BindProperty]
        public string Message { get; set; }


        IConfiguration _configuration;
        IRoleService _roleService;
        IHttpContextAccessor _httpContextAccessor;
        readonly AppConfig _appConfig; 
        public IndexModel(IHttpContextAccessor httpContextAccessor,IConfiguration configuration, IRoleService roleService)
        {
            _configuration = configuration;
            _roleService = roleService;
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
            _httpContextAccessor = httpContextAccessor;
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
                            var pageProperty = Helper.GetPageProperty(modules, HttpContext.Request.Path);

                            Actions = pageProperty.Action;
                            PageTitle = pageProperty.Name;
                            Icon = pageProperty.Icon;
                            PageUrl = pageProperty.Url; 
                            return Page();
                        }
                        else
                            return Redirect(HttpContext.Session.GetString("AppUrl") + "/Error?code=403");
                    }
                    else
                        return Redirect(HttpContext.Session.GetString("AppUrl") + "/Error?code=801");

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
        public JsonResult OnGetAll ()
        { 
            var items = _roleService.Get().Result;

            return new JsonResult(items);
        }
        public JsonResult OnGetModuleByRoleCode (string roleCode) 
        {
            try
            {
                var result = _roleService.ByCode(roleCode).Result;
                return new JsonResult(result);
            }
            catch (Exception ex)
            { 
            return new JsonResult(ex);
            }
            
        }
        public JsonResult OnPostFilter([FromBody] FParam fparam)
        {

            fparam.OpUser = HttpContext.Session.GetString("Username");
            fparam.Terminal = HttpContext.Session.GetString("Terminal");
            var filter = new Filter()
            {
                Property = "Code",
                Operator = "NOTEQUALS",
                Value = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("UserRole")),
            };
            fparam.Filters.Add(filter);
            var items = _roleService.Filter(fparam).Result;

            return new JsonResult(items);
        }

        public async Task<JsonResult> OnPostSave([FromBody] UPCI.DAL.DTO.Request.Role model)
        {
            model.OpUser = HttpContext.Session.GetString("Username");
            model.Terminal = HttpContext.Session.GetString("Terminal");
            var result = new DAL.DTO.Response.Result();

            if (model.EncryptedId.Trim() != "")
                result = await _roleService.Update(model);
            else
                result = await _roleService.Create(model);

            return new JsonResult(result);
        }
        public async Task<JsonResult> OnPostDelete([FromBody] UPCI.DAL.DTO.Request.Role model)
        {
            model.OpUser = HttpContext.Session.GetString("Username");
            model.Terminal = HttpContext.Session.GetString("Terminal");
            var result = new DAL.DTO.Response.Result();
            result = await _roleService.Delete(model);

            return new JsonResult(result);
        }

    }
}