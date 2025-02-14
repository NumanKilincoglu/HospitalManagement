using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using MiniExcelLibs;
using Pusula.Training.HealthCare.GlobalExceptions;
using Pusula.Training.HealthCare.Permissions;
using Pusula.Training.HealthCare.Shared;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Pusula.Training.HealthCare.AppointmentTypes;

[RemoteService(IsEnabled = false)]
[Authorize(HealthCarePermissions.AppointmentTypes.Default)]
public class AppointmentTypesAppService(
    IAppointmentTypeRepository appointmentTypeRepository,
    IAppointmentTypeManager appointmentTypeManager,
    IDistributedCache<AppointmentTypesDownloadTokenCacheItem, string> downloadTokenCache,
    IDistributedCache<AppointmentTypeCacheItem> appointmentTypeCache)
    : HealthCareAppService, IAppointmentTypesAppService
{
    public virtual async Task<PagedResultDto<AppointmentTypeDto>> GetListAsync(GetAppointmentTypesInput input)
    {
        var totalCount = await appointmentTypeRepository.GetCountAsync(input.FilterText, input.Name);
        var items = await appointmentTypeRepository.GetListAsync(input.FilterText, input.Name, input.Sorting,
            input.MaxResultCount, input.SkipCount);

        return new PagedResultDto<AppointmentTypeDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<AppointmentType>, List<AppointmentTypeDto>>(items)
        };
    }

    public virtual async Task<AppointmentTypeDto> GetAsync(Guid id)
    {
        var cacheToken = $"AppointmentType:{id}";
        var cacheResult = await appointmentTypeCache.GetAsync(cacheToken);
        if (cacheResult != null)
        {
            return ObjectMapper.Map<AppointmentTypeCacheItem, AppointmentTypeDto>(cacheResult);
        }

        var type = await appointmentTypeRepository.GetAsync(id);
        
        await appointmentTypeCache.SetAsync(cacheToken,
            ObjectMapper.Map<AppointmentType, AppointmentTypeCacheItem>(type),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return ObjectMapper.Map<AppointmentType, AppointmentTypeDto>(type);
    }

    [Authorize(HealthCarePermissions.AppointmentTypes.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await appointmentTypeRepository.DeleteAsync(id);
    }

    [Authorize(HealthCarePermissions.AppointmentTypes.Create)]
    public virtual async Task<AppointmentTypeDto> CreateAsync(AppointmentTypeCreateDto input)
    {
        HealthCareGlobalException.ThrowIf(HealthCareDomainErrorKeyValuePairs.NameAlreadyExists,
            await appointmentTypeRepository.FirstOrDefaultAsync(x => x.Name == input.Name) is not null);

        var department = await appointmentTypeManager.CreateAsync(
            input.Name
        );

        return ObjectMapper.Map<AppointmentType, AppointmentTypeDto>(department);
    }

    [Authorize(HealthCarePermissions.AppointmentTypes.Edit)]
    public virtual async Task<AppointmentTypeDto> UpdateAsync(Guid id, AppointmentTypeUpdateDto input)
    {
        HealthCareGlobalException.ThrowIf(HealthCareDomainErrorKeyValuePairs.NameAlreadyExists,
            await appointmentTypeRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.Id != id) is not null);

        var department = await appointmentTypeManager.UpdateAsync(
            id,
            input.Name, input.ConcurrencyStamp
        );

        return ObjectMapper.Map<AppointmentType, AppointmentTypeDto>(department);
    }

    [Authorize(HealthCarePermissions.AppointmentTypes.Delete)]
    public virtual async Task DeleteByIdsAsync(List<Guid> appointmentTypeIds)
    {
        await appointmentTypeRepository.DeleteManyAsync(appointmentTypeIds);
    }

    [AllowAnonymous]
    public virtual async Task<IRemoteStreamContent> GetListAsExcelFileAsync(AppointmentTypeExcelDownloadDto input)
    {
        var downloadToken = await downloadTokenCache.GetAsync(input.DownloadToken);
        if (downloadToken == null || input.DownloadToken != downloadToken.Token)
        {
            throw new AbpAuthorizationException("Invalid download token: " + input.DownloadToken);
        }

        var items = await appointmentTypeRepository.GetListAsync(input.FilterText, input.Name);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(ObjectMapper.Map<List<AppointmentType>, List<AppointmentTypeExcelDto>>(items));
        memoryStream.Seek(0, SeekOrigin.Begin);

        return new RemoteStreamContent(memoryStream, "AppointmentTypes.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    public virtual async Task<DownloadTokenResultDto> GetDownloadTokenAsync()
    {
        var token = Guid.NewGuid().ToString("N");

        await downloadTokenCache.SetAsync(
            token,
            new AppointmentTypesDownloadTokenCacheItem { Token = token },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

        return new DownloadTokenResultDto
        {
            Token = token
        };
    }
}