using System;
using System.ComponentModel.DataAnnotations;

namespace Pusula.Training.HealthCare.Appointments;

public class AppointmentDayLookupDto
{
    [Required] public DateTime Date { get; set; }

    [Required] public int AvailableSlotCount { get; set; } = 0;

    [Required] public bool AvailabilityValue { get; set; } = false;
}