using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.DTO.Response
{
    public class VUser: ListBase
    {
        public List<FUser> Data { get; set; } = [];
    }

    public class FUser: User
    {
        public bool Deleted { get; set; }
    }
    public class User: Base
    {
        public string Id { get; set; } = string.Empty;
        public string? Username { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? EncryptedRoleId { get; set; } = string.Empty;
        public string? RoleCode { get; set; } = string.Empty;
        public string? RoleDescription { get; set; } = string.Empty;
        public int? PasswordAttempt { get; set; } = 0;
        public DateTime? PasswordExpirationDate { get; set; } 
        public DateTime? PasswordLastChange { get; set; }
        public bool? IsLocked { get; set; } = false;
        public bool? DefaultPassword { get; set; } = false;
        public bool? Deleted { get; set; } = false;
        public byte[]? ImageContent { get; set; }
        //public string? ImageContentBase64 { get; set; }
        public string? ImageType { get; set; } = string.Empty;
    }

    
}
