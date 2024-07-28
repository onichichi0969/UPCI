using AutoMapper;
using UPCI.DAL.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace UPCI.DAL.Models
{
    public class Filter
    {
        public string Property { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public object Value2 { get; set; } // For BETWEEN operator
        public IEnumerable<object> Values { get; set; } // For IN operator 
        public Boolean IsDate { get; set; } = false;
    }

    public static class Operators
    {
        public const string Equals = "EQUALS";
        public const string GreaterThan = "GREATERTHAN";
        public const string LessThan = "LESSTHAN";
        public const string GreaterThanOrEqual = "GREATERTHANOREQUAL";
        public const string LessThanOrEqual = "LESSTHANOREQUAL";
        public const string Contains = "CONTAINS";
        public const string StartsWith = "STARTSWITH";
        public const string EndsWith = "ENDSWITH";
        public const string Between = "BETWEEN";
        public const string In = "IN";
        public const string IsNull = "ISNULL";
        public const string IsNotNull = "ISNOTEQUAL";
        public const string NotEquals = "NOTEQUALS";
    }

    public class SweetAlertMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
    }
    public class AppConfig
    {
        public string AppCode { get; set; }
        public string AppUrl { get; set; }
        public string ApiUrl { get; set; } 
    }
    public class SystemParameters
    {
        public int? PasswordMaxTry { get; set; } = 0;
        public double? SessionTimeOutMinutes { get; set; } = 0;
    }
    public class Chapter
    {
        public string? Code { get; set; } 
        public double? Description { get; set; }
    }
    public class MailSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string Username { get; set; }
        public string AccountPassword { get; set; }
        public bool IsHTML { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string AttachmentPassword { get; set; }
    }
    public class MailAttachments
    {
        public string Filename { get; set; }
        public byte[] FileBytes { get; set; }
    }
    public class MailTemplate
    { 
        public static string CreateUser(User user, string encryptionKey)
        {

            string content = null;

            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "CreateUser.txt")))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, user.FirstName, user.LastName, user.Username, StringManipulation.Decrypt(user.Password, encryptionKey) );
        }
        public static string CreateUserPasswordless(User user)
        {

            string content = null;

            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "CreateUserPasswordless.txt")))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, user.FirstName, user.LastName, user.Username);
        }

        public static string ChangePassword(User user, string encryptionKey)
        {

            string content = null;
            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "ChangePassword.txt")))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, user.FirstName, user.LastName, user.Username);
        }
        public static string ResetPassword(User user, string encryptionKey)
        {

            string content = null; 
            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "ResetPassword.txt")))
            {
                content = sr.ReadToEnd();
            }
             
            return string.Format(content, user.FirstName, user.LastName, user.Username, StringManipulation.Decrypt(user.Password, encryptionKey));
        }
        public static string UnlockUser(User user)
        {

            string content = null;

            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "UnlockUser.txt")))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, user.FirstName, user.LastName, user.Username);
        }

        public static string LogException(Exception ex)
        {

            string content = null;

            using (var sr = new StreamReader(string.Format(Directory.GetCurrentDirectory() + "{0}{1}", "/Files/Templates/Mail/", "LogException.txt")))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, ex.Message, ex.Source, ex.StackTrace, DateTime.Now);

        }

    }
}
