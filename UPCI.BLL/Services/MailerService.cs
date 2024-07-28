using AutoMapper;
using AutoMapper.QueryableExtensions;
using UPCI.BLL.Services.IService;
using UPCI.DAL;
using UPCI.DAL.Helpers;
using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Transactions;

namespace UPCI.BLL.Services
{
    public class MailerService(ApplicationDbContext applicationDbContext, IMapper mapper, IRepository<User> userRepository, ILogService logService, IConfiguration configuration) : IMailerService
    {
        readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        readonly IMapper _mapper = mapper;
        readonly IRepository<User> _userRepository = userRepository;
        readonly ILogService _logService = logService;
        readonly string _moduleName = "Mailer";
        readonly string _encryptionKey = configuration["AppContext:EncryptionKey"]!;
        readonly MailSettings mailSettings = configuration.GetSection("MailSettings").Get<MailSettings>()!;

        public string Send(
          MailSettings config
            , string mailTo
            , string subject
            , string body
            , bool isBodyHTML
            )
        {
            return Send(config, mailTo, subject, body, isBodyHTML, null, null, false, "");
        }

        public string Send(
        MailSettings config
          , string mailTo
          , string subject
          , string body
          , bool isBodyHTML
          , string mailCc
          )
        {
            return Send(config, mailTo, subject, body, isBodyHTML, mailCc, null, false, "");
        }

        public string Send(
         MailSettings config
            , string mailTo
            , string subject
            , string body
            , bool isBodyHTML
            , string mailCc
            , string mailBcc
            )
        {
            return Send(config, mailTo, subject, body, isBodyHTML, mailCc, mailBcc, false, "");
        }

        public string Send(
            MailSettings config
            , string mailTo
            , string subject
            , string body
            , bool isBodyHTML
            , string mailCc
            , string mailBcc
            , bool hasAttachments
            , string mailAttachments
            )
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(config.Server, config.Port)
                {
                    UseDefaultCredentials = config.UseDefaultCredentials,
                    EnableSsl = config.EnableSSL,
                    Credentials = new NetworkCredential(config.Username, config.AccountPassword)
                };

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(config.From),
                    Subject = subject,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = isBodyHTML,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    Body = body
                };

                if (!String.IsNullOrEmpty(mailTo))
                {
                    for (int ctr = 0; ctr < mailTo.Split(';').Length; ctr++)
                    {
                        mail.To.Add(mailTo.Split(';')[ctr]);
                    }
                }

                if (!String.IsNullOrEmpty(mailCc))
                {
                    for (int ctr = 0; ctr < mailCc.Split(';').Length; ctr++)
                    {
                        mail.CC.Add(mailCc.Split(';')[ctr]);
                    }
                }

                if (!String.IsNullOrEmpty(mailBcc))
                {
                    for (int ctr = 0; ctr < mailBcc.Split(';').Length; ctr++)
                    {
                        mail.Bcc.Add(mailBcc.Split(';')[ctr]);
                    }
                }

                if (hasAttachments)
                {
                    for (int ctr = 0; ctr < mailAttachments.Split(';').Length; ctr++)
                    {
                        mail.Attachments.Add(new Attachment(mailAttachments.Split(';')[ctr]));
                    }
                }

                smtpClient.SendAsync(mail,null);

                return "success";
            }
            catch (SmtpException ex)
            {
                throw new ApplicationException("SmtpException has occured: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception has occured: " + ex.Message);
            }
        }


        public string Template(string path, params string[] fields)
        {
            string content = null;

            using (var sr = new StreamReader(path))
            {
                content = sr.ReadToEnd();
            }

            return string.Format(content, fields);
        }



    }
}
