using System;

namespace Pusula.Training.HealthCare.Patients;

public class PatientExcelDto
{
    public int PatientNumber { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MothersName { get; set; }
    public string? FathersName { get; set; }
    public string IdentityNumber { get; set; } = null!;
    public string? PassportNumber { get; set; } 
    public string? Nationality { get; set; } 
    public DateTime BirthDate { get; set; }
    public string? EmailAddress { get; set; }
    public string? MobilePhoneNumber { get; set; } 
    public EnumRelative? Relative { get; set; }
    public string? RelativePhoneNumber { get; set; }
    public EnumPatientTypes? PatientType { get; set; }
    public string? Address { get; set; }
    public EnumDiscountGroup? DiscountGroup { get; set; }
    public EnumGender Gender { get; set; } 
}