﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("Department")]
    public class Department : Base
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Leader { get; set; } = string.Empty;

        [ForeignKey("Leader")]
        public Member Member { get; set; }
        public ICollection<Ministry> Ministries { get; set; } = new List<Ministry>();

    }
}
