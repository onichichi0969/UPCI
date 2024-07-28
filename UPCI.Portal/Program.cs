using UPCI.BLL;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Models;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var systemParameters = new SystemParameters();
systemParameters = builder.Configuration.GetSection("SystemParameters").Get<SystemParameters>();
// Add services to the container.

//add this to enable (save->hot reload) OR (save "CTRL+S") in Web page changes, you must include  Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation in .WEB Project
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// fix sa valuekin false:"false" issue, serializing issue
builder.Services.AddControllers().AddNewtonsoftJson();
//
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.RegisterDAL(builder.Configuration);
builder.Services.RegisterBLL(); 
builder.Services.AddSingleton<IFileProvider>(
               new PhysicalFileProvider(
                   Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
            );

builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "EQB.Session";
    if (systemParameters != null) 
        options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToDouble(systemParameters.SessionTimeOutMinutes));
  
    //options.IdleTimeout = TimeSpan.FromSeconds(10);
    //options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    //options.Cookie.Domain = "ucpbsavings.com";
    //options.Cookie.Path = "/";
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //options.Cookie.HttpOnly = true;
});
builder.Services.AddMvc().AddRazorPagesOptions(
                options =>
                {
                    options.Conventions.AddPageRoute("/Login", "");
                }).AddSessionStateTempDataProvider();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    //Para sa camelcasing, uncomment para pwede naka Capital ang return ng json
    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePages(async context => {
    if (context.HttpContext.Response.StatusCode == 400)
    {
        context.HttpContext.Response.Redirect("/Error?code=400");
    }
    else if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Error?code=404");
    }
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting(); 
app.UseAuthorization(); 

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    context.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
    context.Response.Headers.Add("Pragma", "no-cache");
    context.Response.Headers.Add("Expires", "0");
    await next();
});
app.MapRazorPages(); 

app.Run();
