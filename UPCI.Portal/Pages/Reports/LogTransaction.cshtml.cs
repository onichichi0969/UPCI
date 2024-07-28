using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UPCI.BLL.Services.IService;
using UPCI.DAL.DTO.Request;
using UPCI.DAL.Models;
using UPCI.Portal.Helpers;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using NPOI.Util.Collections;
using System.Reflection;
using OfficeOpenXml.Style;
using NPOI.XWPF.UserModel;

namespace UPCI.Portal.Pages.Reports.LogTransactionModel
{
    public class LogTransactionModel : PageModel
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
        IReportService _reportService;
        readonly AppConfig _appConfig;
        public LogTransactionModel(IConfiguration configuration, IReportService reportService)
        {
            _configuration = configuration;
            _reportService = reportService;
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
        public JsonResult OnPostFilter([FromBody] FParam fparam)
        {
             
            fparam.OpUser = HttpContext.Session.GetString("Username");
            fparam.Terminal = HttpContext.Session.GetString("Terminal"); 
            var result = _reportService.LogTransaction(fparam).Result; 

            return new JsonResult(result);
        }
        public IActionResult OnPostDownload([FromBody] FParam fparam)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "Report_Log_Transaction (" + DateTime.UtcNow.ToString("MM-dd-yyyy") + ").xlsx";
            fparam.OpUser = HttpContext.Session.GetString("Username");
            fparam.Terminal = HttpContext.Session.GetString("Terminal");
            var result = _reportService.LogTransactionListOnly(fparam).Result;
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("HTTP Log Summary");

                    var properties = typeof(UPCI.DAL.DTO.Response.ReportLogTransaction).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    // Create header row
                    for (int i = 0; i < properties.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = properties[i].Name;
                    }

                    // Populate data rows
                    for (int i = 0; i < result.Data.Count; i++)
                    {
                        for (int j = 0; j < properties.Length; j++)
                        {
                            ws.Cells[i + 2, j + 1].Value = properties[j].GetValue(result.Data[i]);
                        }
                    }
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    if (ws.Dimension != null)
                    {
                        var range = ws.Cells[ws.Dimension.Address];
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin; 
                    }
                    using (var stream = new MemoryStream())
                    {
                        package.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                 
                

                return null;
            } 
        }
    }
}