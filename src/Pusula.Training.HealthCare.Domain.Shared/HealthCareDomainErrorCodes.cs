﻿namespace Pusula.Training.HealthCare;

public static class HealthCareDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    
    public const string GuidNotValid = "HealthCareError:GuidNotValid";
    public const string PatientAlreadyExist = "HealthCareError:PatientAlreadyExist";

    public const string DoctorNotFound = "HealthCareError:DoctorNotFound";
    public const string DoctorWorkingHourNotFound = "HealthCareError:DoctorWorkingHourNotFound";
    public const string DoctorNotWorking = "HealthCareError:DoctorNotWorking";
    public const string DoctorRequired = "HealthCareError:DoctorRequired";
    public const string DoctorAlreadyHasLeave = "HealthCareError:DoctorAlreadyHasLeave";

    public const string MedicalServiceNotFound = "HealthCareError:MedicalServiceNotFound";

    public const string GuidEmpty = "HealthCareError:GuidEmpty";
    public const string GuidRequired = "HealthCareError:GuidRequired";
    public const string NameExists = "HealthCareError:NameAlreadyExist";
    public const string GroupNameNotValid = "HealthCareError:GroupNameNotValid";

    public const string TextLenghtExceeded = "HealthCareError:TextLenghtExceeded";
    public const string GenderNotValid = "HealthCareError:GenderNotValid";
    public const string ValueExceedLimit = "HealthCareError:ValueExceedLimit";

    public const string RestrictionAlreadyExists = "HealthCareError:RestrictionAlreadyExists";
    public const string DoctorRestrictionAlreadyExists = "HealthCareError:DoctorRestrictionAlreadyExists";
    public const string AppointmentAlreadyTaken = "HealthCareError:AppointmentAlreadyTaken";
    public const string AppointmentNotFound = "HealthCareError:AppointmentNotFound";
    public const string AppointmentDateNotValid = "HealthCareError:AppointmentDateNotValid";
    public const string AlreadyHaveAppointmentExactTime = "HealthCareError:AlreadyHaveAppointmentExactTime";

    public const string RestrictionNotFound = "HealthCareError:RestrictionNotFound";
    public const string DoctorHasLeave = "HealthCareError:DoctorHasLeave";

    public const string InvalidDepartments = "HealthCareError:InvalidDepartments";
    public const string DepartmentsNotFound = "HealthCareError:DepartmentsNotFound";

    public const string DateNotInPastException = "HealthCareError:DateNotInPast";

    public const string DoctorInformationsRequired = "HealthCareError:DoctorInformationsRequired";
    public const string PatientInformationsRequired = "HealthCareError:PatientInformationsRequired";
    public const string CategoryInformationsRequired = "HealthCareError:CategoryInformationsRequired";
    public const string TestCategoryInformationsRequired = "HealthCareError:TestCategoryInformationsRequired";
    public const string TestInformationsRequired = "HealthCareError:TestInformationsRequired";
    public const string BloodTestInformationsRequired = "HealthCareError:BloodTestInformationsRequired";
    public const string PatientNotFound = "HealthCareError:PatientNotFound";

    
    //Examination
    public const string ExaminationIcdExists = "HealthCareError:ExaminationIcdAlreadyExist";
    public const string ExaminationIcdNotFound = "HealthCareError:ExaminationIcdNotFound";
   

    /*  ERROR MESSAGES */
    public const string InvalidDateRange_MESSAGE = "InvalidDateRange";
    public const string InvalidDownloadToken_MESSAGE = "The provided download token is invalid.";
    public const string HealthcareError_MESSAGE = "Healthcare error occured!!";
    public const string InvalidNoteLength_MESSAGE = "Notes must be between {1} and {100} characters.";
    public const string ProtocolUpdate_MESSAGE = "Protocol not found with the given ID.";
    public const string PatientNotFoundByNumber_CODE = "HealthCare:002";
    public const string PatientNotFoundByNumber_MESSAGE = "Patient with number {0} could not be found.";
    

    /*  CODES */
    public const string ValidationError_CODE = "400";
    public const string Forbidden_CODE = "403";
    public const string InternalServerError_CODE = "500";
    public const string InvalidDateRange_CODE = "400";
    public const string InvalidDownloadToken_CODE = "INVALID_DOWNLOAD_TOKEN";
    public const string HealthcareError_CODE = "500";
    public const string InvalidNoteLength_CODE = "Invalid note length!";
    public const string ProtocolUpdate_CODE = "ProtocolNotFound";
}