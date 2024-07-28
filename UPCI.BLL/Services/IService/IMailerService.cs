using UPCI.DAL.Models;

namespace UPCI.BLL.Services.IService
{
    public interface IMailerService
    {
        public string Send(
         MailSettings config
           , string mailTo
           , string subject
           , string body
           , bool isBodyHTML
           );
        public string Send(
        MailSettings config
          , string mailTo
          , string subject
          , string body
          , bool isBodyHTML
          , string mailCc
          );
        public string Send(
         MailSettings config
            , string mailTo
            , string subject
            , string body
            , bool isBodyHTML
            , string mailCc
            , string mailBcc
            );
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
            );
    }
}
