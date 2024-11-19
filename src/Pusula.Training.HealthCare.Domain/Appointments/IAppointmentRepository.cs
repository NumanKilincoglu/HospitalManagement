using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories; 

namespace Pusula.Training.HealthCare.Appointments;

public interface IAppointmentRepository : IRepository<Appointment, Guid>
{
    Task DeleteAllAsync(
        Guid? doctorId = null, 
        Guid? patientId = null, 
        Guid? medicalServiceId = null, 
        DateTime? appointmentDate = null, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        EnumAppointmentStatus? status = null,
        bool? reminderSent = null, 
        double? minAmount = null, 
        double? maxAmount = null, 
        CancellationToken cancellationToken = default);
    
    Task<List<Appointment>> GetListAsync(
        Guid? doctorId = null, 
        Guid? patientId = null, 
        Guid? medicalServiceId = null, 
        DateTime? appointmentDate = null, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        EnumAppointmentStatus? status = null,
        bool? reminderSent = null, 
        double? minAmount = null, 
        double? maxAmount = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);
    
    Task<List<AppointmentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
        Guid? doctorId = null, 
        Guid? patientId = null, 
        Guid? medicalServiceId = null, 
        string? patientName = null,
        string? doctorName = null,
        string? serviceName = null,
        int? patientNumber = null,
        DateTime? appointmentDate = null, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        EnumAppointmentStatus? status = null,
        bool? reminderSent = null, 
        double? minAmount = null, 
        double? maxAmount = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);
    
    Task<long> GetCountAsync(
        Guid? doctorId = null, 
        Guid? patientId = null, 
        Guid? medicalServiceId = null, 
        DateTime? appointmentDate = null, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        EnumAppointmentStatus? status = null,
        bool? reminderSent = null, 
        double? minAmount = null, 
        double? maxAmount = null,
        CancellationToken cancellationToken = default);
    
    Task<long> GetCountByNavigationPropertiesAsync(
        Guid? doctorId = null,
        Guid? patientId = null,
        Guid? medicalServiceId = null,
        string? patientName = null,
        string? doctorName = null,
        string? serviceName = null,
        int? patientNumber = null,
        DateTime? appointmentDate = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        EnumAppointmentStatus? status = null,
        bool? reminderSent = null,
        double? minAmount = null,
        double? maxAmount = null,
        CancellationToken cancellationToken = default);

}