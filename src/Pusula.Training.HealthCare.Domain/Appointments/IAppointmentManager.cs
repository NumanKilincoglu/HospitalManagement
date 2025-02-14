using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pusula.Training.HealthCare.Appointments;

public interface IAppointmentManager
{
    Task<List<AppointmentSlotDto>> GetAppointmentSlotsAsync(
        Guid doctorId,
        Guid medicalServiceId,
        DateTime date,
        bool? excludeNotAvailable = false
    );
    
    Task<List<AppointmentDayLookupDto>> GetAvailableDaysLookupAsync(
        Guid doctorId,
        Guid medicalServiceId,
        DateTime startDate,
        int offset
    );

    Task<Appointment> CreateAsync(
        Guid doctorId, 
        Guid patientId, 
        Guid medicalServiceId, 
        Guid appointmentTypeId,
        Guid departmentId,
        DateTime appointmentDate,
        DateTime startTime,
        DateTime endTime,
        bool reminderSent,
        double amount,
        string? notes = null
    );
    
    Task<Appointment> UpdateAsync(
        Guid id,
        Guid doctorId,
        DateTime appointmentDate,
        DateTime startTime,
        DateTime endTime,
        EnumAppointmentStatus status,
        bool reminderSent,
        double amount,
        string? notes = null,
        string? cancellationNotes = null
    );
    
}