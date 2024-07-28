﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPCI.DAL.Models
{
    [Table("PEPSOLLevel")]
    public class PEPSOLLevel : Base
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty; 
    }
}
