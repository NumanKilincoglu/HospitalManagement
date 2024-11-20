using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pusula.Training.HealthCare.DoctorWorkingHours;
using Pusula.Training.HealthCare.Exceptions;
using Pusula.Training.HealthCare.MedicalServices;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Pusula.Training.HealthCare.Appointments;

public class AppointmentManager(
    IAppointmentRepository appointmentRepository,
    IRepository<DoctorWorkingHour> doctorWorkingHourRepository,
    IMedicalServiceRepository medicalServiceRepository) : DomainService, IAppointmentManager
{
    public virtual async Task<List<AppointmentSlotDto>> GetAppointmentSlotsAsync(
        Guid doctorId, 
        Guid medicalServiceId,
        DateTime date)
    {
        
        //Check doctor's working hours on the selected date
        var workingHour = await doctorWorkingHourRepository
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId 
                                      && date.DayOfWeek == x.DayOfWeek);

        if (workingHour == null)
        {
            throw new DoctorNotWorkingException();
        }

        //Check if medical service exist
        var medicalService = await medicalServiceRepository
            .FirstOrDefaultAsync(x => x.Id == medicalServiceId);

        if (medicalService == null)
        {
            throw new MedicalServiceNotFoundException();
        }

        //Check doctor's appointments on the selected date
        var doctorAppointments = await appointmentRepository
            .GetListAsync(x => x.DoctorId == doctorId 
                               && x.AppointmentDate.Date == date.Date);

        //Retrieve doctor's appointment times on the selected date
        var doctorAppointmentTimes = doctorAppointments.Select(x => new 
        {
            StartTime = x.StartTime.TimeOfDay,
            EndTime = x.EndTime.TimeOfDay
        }).ToList();
        
        var startTime = workingHour.StartHour;
        var endTime = workingHour.EndHour;
        var serviceDuration = TimeSpan.FromMinutes(medicalService.Duration);

        var availableSlots = new List<AppointmentSlotDto>();

        for (var appointmentTime = startTime;
             appointmentTime + serviceDuration <= endTime;
             appointmentTime += serviceDuration)
        {
            
            // Skip creating slots for past times on the current date
            if (date.Date == DateTime.Now.Date && appointmentTime < DateTime.Now.TimeOfDay)
            {
                continue;
            }

            //Check if doctor has another appointment at the same time
            var isAvailable = !doctorAppointmentTimes.Any(slot =>
                appointmentTime >= slot.StartTime && appointmentTime < slot.EndTime);

            availableSlots.Add(new AppointmentSlotDto
            {
                Date = date,
                StartTime = appointmentTime.ToString(@"hh\:mm"),
                EndTime = appointmentTime.Add(serviceDuration).ToString(@"hh\:mm"),
                DoctorId = doctorId,
                MedicalServiceId = medicalService.Id,
                AvailabilityValue = isAvailable,
            });
        }

        return availableSlots;
    }

    public virtual async Task<Appointment> CreateAsync(Guid doctorId, Guid patientId, Guid medicalServiceId,
        DateTime appointmentDate, DateTime startTime,
        DateTime endTime, bool reminderSent, double amount, string? notes = null)
    {
        Check.NotNull(doctorId, nameof(doctorId));
        Check.NotNull(patientId, nameof(patientId));
        Check.NotNull(medicalServiceId, nameof(doctorId));
        Check.NotNull(appointmentDate, nameof(appointmentDate));
        Check.NotNull(startTime, nameof(startTime));
        Check.NotNull(endTime, nameof(endTime));
        Check.NotNull(reminderSent, nameof(reminderSent));
        Check.NotNull(amount, nameof(amount));
        Check.Range(amount, nameof(amount), MedicalServiceConsts.CostMinValue, MedicalServiceConsts.CostMaxValue);

        if (startTime < DateTime.Now || endTime < DateTime.Now || startTime >= endTime)
        {
            throw new AppointmentDateNotValidException();
        }

        var isAppointmentTaken = await appointmentRepository
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId
                                      && x.AppointmentDate.Date == appointmentDate.Date
                                      && x.StartTime == startTime);

        if (isAppointmentTaken != null)
        {
            throw new AppointmentAlreadyTakenException();
        }

        var appointment = new Appointment(
            id: GuidGenerator.Create(),
            doctorId: doctorId,
            patientId: patientId,
            medicalServiceId: medicalServiceId,
            appointmentDate: appointmentDate,
            startTime: startTime,
            endTime: endTime,
            EnumAppointmentStatus.Scheduled,
            notes: notes,
            reminderSent: reminderSent,
            amount: amount
        );

        return await appointmentRepository.InsertAsync(appointment);
    }

    public async Task<Appointment> UpdateAsync(Guid id, DateTime appointmentDate, DateTime startTime,
        DateTime endTime, EnumAppointmentStatus status, bool reminderSent, double amount,
        [CanBeNull] string? notes = null, [CanBeNull] string? concurrencyStamp = null)
    {
        Check.NotNull(appointmentDate, nameof(appointmentDate));
        Check.NotNull(startTime, nameof(startTime));
        Check.NotNull(endTime, nameof(endTime));
        Check.NotNull(reminderSent, nameof(reminderSent));
        Check.NotNull(amount, nameof(amount));
        Check.Range(amount, nameof(amount), MedicalServiceConsts.CostMinValue, MedicalServiceConsts.CostMaxValue);

        var appointment = await appointmentRepository.GetAsync(id);

        appointment.SetAppointmentDate(appointmentDate);
        appointment.SetStartTime(startTime);
        appointment.SetEndTime(endTime);
        appointment.SetStatus(status);
        appointment.SetNotes(notes);
        appointment.SetReminderSent(reminderSent);
        appointment.SetAmount(amount);
        appointment.SetConcurrencyStampIfNotNull(concurrencyStamp);

        return await appointmentRepository.UpdateAsync(appointment);
    }
}