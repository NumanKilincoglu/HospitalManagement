using System;
using System.Text.Json;
using System.Threading.Tasks;
using Pusula.Training.HealthCare.Appointments;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using Volo.Abp;

namespace Pusula.Training.HealthCare.Blazor.Services;

public class AppointmentAdaptor(IAppointmentAppService appointmentAppService) : DataAdaptor
{
    public override async Task<object> ReadAsync(
        DataManagerRequest dataManagerRequest,
        string? additionalParam = null)
    {
        try
        {
            
            var filterJson = dataManagerRequest?.Params?["Filter"]?.ToString();
            
            var filter = string.IsNullOrEmpty(filterJson)
                ? new GetAppointmentsInput { MaxResultCount = dataManagerRequest!.Take, SkipCount = dataManagerRequest!.Skip }
                : JsonSerializer.Deserialize<GetAppointmentsInput>(filterJson);
    
            filter!.Sorting = dataManagerRequest!.Sorted is { Count: > 0 }
                ? $"{dataManagerRequest.Sorted[0].Name} {dataManagerRequest.Sorted[0].Direction}"
                : null;
            
            filter!.SkipCount = dataManagerRequest!.Skip;
            filter.MaxResultCount = dataManagerRequest.Take;
            var result = await appointmentAppService.GetListAsync(filter!);

            var dataResult = new DataResult
            {
                Result = result.Items,
                Count = (int)result.TotalCount
            };
            return dataResult;
        }
        catch (Exception e)
        {
            throw new UserFriendlyException(e.Message);
        }
        
    }
}