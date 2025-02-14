﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Training.HealthCare.Treatment.Icds;

public class IcdUpdateDto
{
    [Required]
    public Guid Id { get; set; } = default!;
    [Required]
    [StringLength(IcdConsts.CodeNumberMaxLength, MinimumLength = IcdConsts.CodeNumberMinLength)]
    public string CodeNumber { get; set; } = null!;
    [Required]
    [StringLength(IcdConsts.DetailMaxLength, MinimumLength = IcdConsts.DetailMinLength)]
    public string Detail { get; set; } = null!;
}