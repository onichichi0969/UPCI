﻿using System.ComponentModel.DataAnnotations;

namespace UPCI.DAL.DTO.Request
{
    public class Ministry : Base
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

    }
}