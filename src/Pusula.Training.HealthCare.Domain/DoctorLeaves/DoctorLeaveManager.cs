﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Services;

namespace Pusula.Training.HealthCare.DoctorLeaves;

public class DoctorLeaveManager(IDoctorLeaveRepository repo) : DomainService, IDoctorLeaveManager
{
     public virtual async Task<DoctorLeave> CreateAsync( Guid doctorId, DateTime startDate, DateTime endDate, string? reason=null )
    {
            Check.NotNull(doctorId, nameof(doctorId));
            Check.NotNull(startDate, nameof(startDate));
            Check.NotNull(endDate, nameof(endDate));

            if (startDate > endDate)
                throw new BusinessException("InvalidDateRange",
                        "The start date cannot be greater than the end date.").WithData("StartDate", startDate)
                    .WithData("EndDate", endDate);

            var leaves = new DoctorLeave(
                GuidGenerator.Create(), doctorId, startDate, endDate, reason);

            return await repo.InsertAsync(leaves);
    
   
    }

    public virtual async Task<DoctorLeave> UpdateAsync( Guid id, Guid doctorId,
        DateTime startDate, DateTime endDate, string? reason = null,  [CanBeNull] string? concurrencyStamp = null)
    {
        
        try
        {
            Check.NotNull(doctorId, nameof(doctorId));
             Check.NotNull(startDate, nameof(startDate));
             Check.NotNull(endDate, nameof(endDate));
        
             if (startDate > endDate)
                   throw new BusinessException("InvalidDateRange",
                     "The start date cannot be greater than the end date.").WithData("StartDate", startDate)
                    .WithData("EndDate", endDate)
                    .GetBaseException();
        
             var leaves = await repo.GetAsync(id);
        
             leaves.SetDoctorId(doctorId);
             leaves.SetStartDate(startDate);
             leaves.SetEndDate(endDate);
             leaves.SetReason(reason);
        
             leaves.SetConcurrencyStampIfNotNull(concurrencyStamp);
             return await repo.UpdateAsync(leaves);
        }
        catch (Exception e)
        {
            throw new UserFriendlyException($"An unexpected error occurred during update leave : {e.Message}" );
        
        }
       
    }
        

}