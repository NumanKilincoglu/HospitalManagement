using System;
using Volo.Abp.Application.Dtos;

namespace Pusula.Training.HealthCare.MedicalServices;

public class GetMedicalServiceInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? MedicalServiceId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Name { get; set; }
    public DateTime? ServiceDateMin { get; set; }
    public DateTime? ServiceDateMax { get; set; }
    
    public double? CostMin { get; set; }
    public double? CostMax { get; set; }

    public GetMedicalServiceInput()
    {
    }
}