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
using Microsoft.Identity.Client;


namespace UPCI.Portal.Pages.Membership
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
        IMemberService _memberService;
        readonly AppConfig _appConfig;

        public IndexModel(IConfiguration configuration, IMemberService memberService)
        {
            _configuration = configuration;
            _memberService = memberService;
            _appConfig = _configuration.GetSection("AppSettings").Get<AppConfig>()!;
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
                            //Actions = (Helper.GetActions(modules, HttpContext.Request.Path));
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
        public async Task<IActionResult> OnPostMemberProfileImage([FromBody] string id)
        {
            try
            {
                var member = await _memberService.ById(id);

                if (member != null)
                {
                    if (member.ImageContent.Length > 0)
                        return File(member.ImageContent, member.ImageType);
                    else
                        return File("~/Assets/Images/default-user.jpg", "image/jpg");
                }
                return File("~/Assets/Images/default-user.jpg", "image/jpg");
            }
            catch (Exception ex)
            {
                return File("~/Assets/Images/default-user.jpg", "image/jpg");
            }

        } 
        public JsonResult OnPostFilter([FromBody] FParam fparam)
        { 
            fparam.OpUser = HttpContext.Session.GetString("Username");
            fparam.Terminal = HttpContext.Session.GetString("Terminal");
            var items = _memberService.Filter(fparam).Result;
            var defaultFile = Helper.ConvertVirtualFileToBytesAsync("~/Assets/Images/default-user.jpg").Result;
            var defaultImage = Helper.ConvertBytesToBase64(defaultFile, "image/jpg");

            try
            {
                if (items.Data != null)
                {
                    foreach (var item in items.Data)
                    {
                        if (item.ImageContent != null)
                            item.ImageDataString = Helper.ConvertBytesToBase64(item.ImageContent, item.ImageType);
                        else
                            item.ImageDataString = defaultImage;
                    }
                }
            }
            catch (Exception ex)
            { 
            
            }
            return new JsonResult(items);
        }
        public async Task<JsonResult> OnPostSave([FromBody] UPCI.DAL.DTO.Request.Member model)
        {
            model.OpUser = HttpContext.Session.GetString("Username");
            model.Terminal = HttpContext.Session.GetString("Terminal");
            var result = new DAL.DTO.Response.Result();

            if (model.Id.Trim() != "")
                result = await _memberService.Update(model);
            else
                result = await _memberService.Create(model);

            return new JsonResult(result);
        }
        public async Task<JsonResult> OnPostDelete([FromBody] UPCI.DAL.DTO.Request.Member model)
        {
            model.OpUser = HttpContext.Session.GetString("Username");
            model.Terminal = HttpContext.Session.GetString("Terminal");
            var result = new DAL.DTO.Response.Result();
            result = await _memberService.Delete(model);

            return new JsonResult(result);
        }

        

    }
}