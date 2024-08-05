using UPCI.DAL.DTO.Response;
using Newtonsoft.Json;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace UPCI.Portal.Helpers
{
    public class Helper
    {
        public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
        {
            try
            {
                using (var inputStream = new MemoryStream(imageBytes))
                {
                    // Load the image from byte array
                    using (var image = Image.FromStream(inputStream))
                    {
                        // Create a new bitmap with the desired size
                        using (var resizedImage = new Bitmap(width, height))
                        {
                            using (var graphics = Graphics.FromImage(resizedImage))
                            {
                                // Set the interpolation mode to high quality
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                // Draw the original image onto the resized bitmap
                                graphics.DrawImage(image, 0, 0, width, height);
                            }

                            // Save the resized image to a byte array
                            using (var outputStream = new MemoryStream())
                            {
                                resizedImage.Save(outputStream, ImageFormat.Jpeg); // Change the format if needed
                                return outputStream.ToArray();
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                return null;
            }
        }
        public static string ConvertBytesToBase64(byte[] imageBytes, string imageType)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                //throw new ArgumentException("Image bytes cannot be null or empty.");
                return "";

            string base64String = Convert.ToBase64String(imageBytes);
            return $"data:{imageType};base64,{base64String}";
        }
        public static async Task<byte[]> ConvertVirtualFileToBytesAsync(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                //throw new ArgumentException("Virtual path cannot be null or empty.");
                return null;

            // Resolve the physical path from the virtual path
            var webRootPath = Directory.GetCurrentDirectory(); // Typically, wwwroot is in the root directory
            var physicalPath = Path.Combine(webRootPath, "wwwroot", virtualPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(physicalPath))
                //throw new FileNotFoundException("File not found.", physicalPath);
                return null;

            return await System.IO.File.ReadAllBytesAsync(physicalPath);
        }
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
