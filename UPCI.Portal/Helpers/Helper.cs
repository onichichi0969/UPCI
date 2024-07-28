﻿using UPCI.DAL.DTO.Response;
using Newtonsoft.Json;
using System.Text;

namespace UPCI.Portal.Helpers
{
    public class Helper
    {
        public static bool HasAccess(List<ModuleAccess> modules, string path)
        {
            var result = false;

            foreach (ModuleAccess item in modules)
            {
                if (item.Url.Equals(path))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static ModuleAccess GetPageProperty(List<ModuleAccess> modules, string path)
        {

            ModuleAccess moduleAccess = new ModuleAccess();

            foreach (ModuleAccess item in modules)
            {
                if (item.Url.Equals(path))
                {
                    moduleAccess.Url = item.Url;
                    moduleAccess.Action = item.Action;
                    moduleAccess.Name = item.Name;
                    moduleAccess.Icon = item.Icon;
                    break;
                }
            }
            return moduleAccess;
        }
        public static string GetActions(List<ModuleAccess> modules, string path)
        {
            var result = "";

            foreach (ModuleAccess item in modules)
            {
                if (item.Url.Contains(path))
                {
                    result = item.Action;
                    break;
                }
            }
            return result;
        }
        public static string LoadNav(List<ModuleAccess> modules, string currentPage, IConfiguration configuration)
        {

            StringBuilder sb = new StringBuilder();
          
            try
            {
                if (modules != null)
                {
                    int child = 0;

                    foreach (var item in modules)
                    {

                        if (item.Show)
                        {
                            if (item.ParentId == "0")
                            {
                                child = 0;

                                foreach (var i in modules)
                                {
                                    if (i.ParentId == item.Id && i.Show)
                                    {
                                        child++;
                                    }
                                }

                                sb.Append(string.Format("<li class=\"nav-item {0} {1}\">",
                                    ((child > 0) ? "has-treeview" : ""), (string.IsNullOrEmpty(item.Url) && currentPage.Contains(item.Name) ? "menu-open" : "")
                                    ));
                                //sb.Append(string.Format("<a href=\"{0}\" class=\"nav-link {1}\"><i class=\"nav-icon {2}\"></i>",
                                //    (string.IsNullOrEmpty(item.Url) ? "#" : item.Url), (item.Url == currentPage || String.IsNullOrEmpty(item.Url) && currentPage.Contains(item.Name) ? "active" : ""), (string.IsNullOrEmpty(item.Icon) ? "" : "fas " + item.Icon)
                                //));
                                sb.Append(string.Format("<a href=\"{0}\" class=\"nav-link {1}\"><i class=\"nav-icon {2}\"></i>",
                                    (string.IsNullOrEmpty(item.Url) ? "#" : item.Url), ((currentPage.Contains(item.Url) || string.IsNullOrEmpty(item.Url)) && currentPage.Contains(item.Name) ? "active" : ""), (string.IsNullOrEmpty(item.Icon) ? "" : "fas " + item.Icon)
                                ));

                                if (child > 0)
                                    sb.Append(string.Format("<p> {0} {1}</p></a>{2}", item.Name, "<i class=\"right fas fa-angle-left\"></i>", "<ul class=\"nav nav-treeview\">" + LoadChildNav(modules, currentPage, item.Id) + "</ul>"));
                                else
                                    sb.Append(string.Format("<p> {0} </p></a>", item.Name));

                                sb.Append("</li>");
                            }
                        }
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return sb.ToString();
            }


        }
        private static string LoadChildNav(List<DAL.DTO.Response.ModuleAccess> modules, string currentPage, string encryptedParentId)
        {
            StringBuilder sb = new StringBuilder();
            try
            {

                int child = 0;

                foreach (var item in modules)
                {
                    if (item.Show)
                    {

                        child = 0;

                        if (item.ParentId == encryptedParentId)
                        {
                            foreach (var i in modules) { if (i.ParentId == item.Id && i.Show) { { child++; } } }

                            sb.Append(string.Format("<li class=\"nav-item {0} {1}\">",
                                 ((child > 0) ? "has-treeview" : ""), (currentPage.Contains(item.Name) ? "menu-open" : "")
                            ));
                            //sb.Append(string.Format("<a href=\"{0}\" class=\"nav-link {1}\"><i class=\"nav-icon {2}\"></i>",
                            //    (string.IsNullOrEmpty(item.Url) ? "#" : item.Url), (item.Url == currentPage && item.EncryptedParentId == encryptedParentId ? "active" : ""), (string.IsNullOrEmpty(item.Icon) ? "" : "fas " + item.Icon)
                            //));

                            sb.Append(string.Format("<a href=\"{0}\" class=\"nav-link {1}\"><i class=\"nav-icon {2}\"></i>",
                              (string.IsNullOrEmpty(item.Url) ? "#" : item.Url), (currentPage == item.Url || string.IsNullOrEmpty(item.Url) ? "active" : ""), (string.IsNullOrEmpty(item.Icon) ? "" : "fas " + item.Icon)
                          ));

                            if (child > 0)
                                sb.Append(string.Format("<p> {0} {1}</p></a>{2}", item.Name, "<i class=\"right fas fa-angle-left\"></i>", "<ul class=\"nav nav-treeview\">" + LoadChildNav(modules, currentPage, item.Id) + "</ul>"));
                            else
                                sb.Append(string.Format("<p> {0} </p></a>", item.Name));

                            sb.Append("</li>");
                        }
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return sb.ToString();
            }
        }


    }

    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}