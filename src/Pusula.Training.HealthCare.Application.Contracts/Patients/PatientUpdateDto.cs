using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace Pusula.Training.HealthCare.Patients;

public class PatientUpdateDto : IHasConcurrencyStamp
{
    [Required]
    [StringLength(PatientConsts.NameMaxLength)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(PatientConsts.LastNameMaxLength)]
    public string LastName { get; set; } = null!;

    [StringLength(PatientConsts.NameMaxLength)]
    public string? MothersName { get; set; }

    [StringLength(PatientConsts.NameMaxLength)]
    public string? FathersName { get; set; }
    
    [RegularExpression(@"^[1-9]{1}[0-9]{9}[02468]{1}$")]
    [StringLength(PatientConsts.IdentityNumberLength)]
    public string? IdentityNumber { get; set; } = null!;
    
    public EnumNationality Nationality { get; set; }
    
    [Required]
    [StringLength(PatientConsts.PassportNumberMaxLength)]
    public string? PassportNumber { get; set; } = null!;
    
    [Required]
    public DateTime BirthDate { get; set; }
    
    [EmailAddress]
    [StringLength(PatientConsts.EmailAddressMaxLength)]
    public string EmailAddress { get; set; } = null!;

    [Required]
    [RegularExpression(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")]
    [StringLength(PatientConsts.MobilePhoneNumberMaxLength)]
    public string MobilePhoneNumber { get; set; } = null!;
    
    [Range(PatientConsts.RelativeMinValue, PatientConsts.RelativeMaxValue)]
    public EnumRelative? Relative { get; set; }

    [RegularExpression(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")]
    public string? RelativePhoneNumber { get; set; }
    
    [Required]
    [Range(PatientConsts.PatientTypeMinValue, PatientConsts.PatientTypeMaxValue)]
    public EnumPatientTypes PatientType { get; set; }
    
    [StringLength(PatientConsts.AddressMaxLength)]
    public string Address { get; set; }

    [Required]
    [Range(PatientConsts.InsuranceMinValue, PatientConsts.InsuranceMaxValue)]
    public EnumInsuranceType InsuranceType { get; set; }
    
    [Required]
    [StringLength(PatientConsts.InsuranceNumberMaxLength)]
    public string InsuranceNo { get; set; } = null!;
    
    [Range(PatientConsts.DiscountGroupMinValue, PatientConsts.DiscountGroupMaxValue)]
    public EnumDiscountGroup DiscountGroup { get; set; }
    
    [Range(PatientConsts.GenderMinValue, PatientConsts.GenderMaxValue)]
    public EnumGender Gender { get; set; }
    
    public bool IsDeleted { get; set; }

    public string ConcurrencyStamp { get; set; } = null!;
}