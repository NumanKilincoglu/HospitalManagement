﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Training.HealthCare.Treatment.Examinations.Backgrounds;

public class BackgroundCreateDto
{
    [StringLength(BackgroundConsts.AllergiesMaxLength)]
    public string? Allergies { get; set; }
    [StringLength(BackgroundConsts.MedicationsMaxLength)]
    public string? Medications { get; set; }
    [StringLength(BackgroundConsts.HabitsMaxLength)]
    public string? Habits { get; set; }
    [Required]
    public Guid ExaminationId { get; set; }
}