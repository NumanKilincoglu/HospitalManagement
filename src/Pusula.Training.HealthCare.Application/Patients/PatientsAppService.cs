﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using MiniExcelLibs;
using Pusula.Training.HealthCare.Exceptions;
using Pusula.Training.HealthCare.GlobalExceptions;
using Pusula.Training.HealthCare.Permissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Content;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Users;

namespace Pusula.Training.HealthCare.Patients
{
    [RemoteService(IsEnabled = false)]
    [Authorize(HealthCarePermissions.Patients.Default)]
    public class PatientsAppService(IPatientRepository patientRepository, PatientManager patientManager,
        IDistributedCache<PatientDownloadTokenCacheItem, string> downloadTokenCache,
        IDistributedEventBus distributedEventBus, ICurrentUser currentUser) : HealthCareAppService, IPatientsAppService
    {
        public virtual async Task<PagedResultDto<PatientDto>> GetListAsync(GetPatientsInput input)
        {
            var totalCount = await patientRepository.GetCountAsync(input.FilterText, input.PatientNumber, input.FirstName, input.LastName, input.IdentityNumber, input.PassportNumber,
                input.Nationality, input.BirthDateMin, input.BirthDateMax, input.EmailAddress, input.MobilePhoneNumber, input.PatientType, input.DiscountGroup, input.Gender);
            var items = await patientRepository.GetListAsync(input.FilterText, input.PatientNumber, input.FirstName, input.LastName, input.IdentityNumber, input.PassportNumber,
                input.Nationality, input.BirthDateMin, input.BirthDateMax, input.EmailAddress, input.MobilePhoneNumber, input.PatientType, input.DiscountGroup, input.Gender,
                input.Sorting, input.IsDeleted, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<PatientDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Patient>, List<PatientDto>>(items)
            };
        }

        public virtual async Task<PatientDto> GetAsync(Guid id)
        {
            await distributedEventBus.PublishAsync(new PatientViewedEto { Id = id, ViewedAt = Clock.Now },
                onUnitOfWorkComplete: false);

            var patient = await patientRepository.GetAsync(id);
            return ObjectMapper.Map<Patient, PatientDto>(patient);
        }

        public virtual async Task<PatientDto> GetPatientByNumberAsync(int number) => ObjectMapper.Map<Patient, PatientDto>(await patientRepository.GetPatientByNumberAsync(number));
        

        public virtual async Task<PatientDto> GetPatientByIdentityAsync(string number) => ObjectMapper.Map<Patient, PatientDto>(await patientRepository.GetPatientByIdentityAsync(number));


        [Authorize(HealthCarePermissions.Patients.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            var patient = await patientRepository.GetAsync(id);
            await patientRepository.DeleteAsync(id);
            await distributedEventBus.PublishAsync(new PatientDeletedEto 
                { PatientNumber = patient.PatientNumber, FirstName = patient.FirstName, LastName = patient.LastName, DeletedAt = Clock.Now, 
                DeletedByUserName = currentUser.UserName  }, onUnitOfWorkComplete: false);
        }

        [Authorize(HealthCarePermissions.Patients.Create)]
        public virtual async Task<PatientDto> CreateAsync(PatientCreateDto input)
        {
            HealthCareGlobalException.ThrowIf(HealthCareDomainErrorKeyValuePairs.PatientAlreadyExist,
            (await patientRepository.GetListAsync()).FirstOrDefault(p => (p.IdentityNumber == input.IdentityNumber)) is not null);
            
            int patientNumber = (await patientRepository.GetListAsync()).Count == 0
                ? 1
                : (await patientRepository.GetListAsync()).Count + 1;

            var patient = await patientManager.CreateAsync(
                patientNumber, input.FirstName, input.LastName, input.BirthDate, input.Gender, input.IdentityNumber, input.PassportNumber, input.Nationality, input.MobilePhoneNumber,
                input.PatientType, input.MothersName, input.FathersName, input.EmailAddress, input.Relative, input.RelativePhoneNumber, input.Address, input.DiscountGroup);

            return ObjectMapper.Map<Patient, PatientDto>(patient);
        }

        [Authorize(HealthCarePermissions.Patients.Edit)]
        public virtual async Task<PatientDto> UpdateAsync(Guid id, PatientUpdateDto input)
        {
            try
            {
                var patient = await patientManager.UpdateAsync(
                    id, input.FirstName, input.LastName, input.BirthDate, input.Gender, input.IdentityNumber, input.PassportNumber, input.Nationality, input.MobilePhoneNumber,
                    input.PatientType, input.MothersName, input.FathersName, input.EmailAddress, input.Relative, input.RelativePhoneNumber, input.Address, input.DiscountGroup);

                return ObjectMapper.Map<Patient, PatientDto>(patient);
            }
            catch (PatientUpdateException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        [AllowAnonymous]
        public virtual async Task<IRemoteStreamContent> GetListAsExcelFileAsync(PatientExcelDownloadDto input)
        {
            var downloadToken = await downloadTokenCache.GetAsync(input.DownloadToken);
            HealthCareGlobalException.ThrowIf(HealthCareDomainErrorKeyValuePairs.InvalidDownloadToken,
                downloadToken is null || input.DownloadToken != downloadToken.Token);

            var items = await patientRepository.GetListAsync(
                    input.FilterText, input.PatientNumber, input.FirstName, input.LastName, input.IdentityNumber, input.PassportNumber, input.Nationality,
                    input.BirthDateMin, input.BirthDateMax, input.EmailAddress, input.MobilePhoneNumber, input.PatientType, input.DiscountGroup, input.Gender);

            var memoryStream = new MemoryStream();
            await memoryStream.SaveAsAsync(ObjectMapper.Map<List<Patient>, List<PatientExcelDto>>(items));
            memoryStream.Seek(0, SeekOrigin.Begin);

            return new RemoteStreamContent(memoryStream, "Patients.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Authorize(HealthCarePermissions.Patients.Delete)]
        public virtual async Task DeleteByIdsAsync(List<Guid> patientIds) => await patientRepository.DeleteManyAsync(patientIds);
        

        [Authorize(HealthCarePermissions.Patients.Delete)]
        public virtual async Task DeleteAllAsync(GetPatientsInput input) => await patientRepository.DeleteAllAsync(
                input.FilterText, input.PatientNumber, input.FirstName, input.LastName, input.IdentityNumber, input.PassportNumber, input.Nationality,
                input.BirthDateMin, input.BirthDateMax, input.EmailAddress, input.MobilePhoneNumber, input.PatientType, input.DiscountGroup, input.Gender);
        

        public virtual async Task<Shared.DownloadTokenResultDto> GetDownloadTokenAsync()
        {
            var token = Guid.NewGuid().ToString("N");

            await downloadTokenCache.SetAsync(
                token,
                new PatientDownloadTokenCacheItem { Token = token },
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });

            return new Shared.DownloadTokenResultDto
            {
                Token = token
            };
        }
    }
}