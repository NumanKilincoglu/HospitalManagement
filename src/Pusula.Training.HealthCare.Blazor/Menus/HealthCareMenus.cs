﻿namespace Pusula.Training.HealthCare.Blazor.Menus;

public class HealthCareMenus
{
    private const string Prefix = "HealthCare";
    public const string Home = Prefix + ".Home";

    //Add your menu items here...
    public const string Patients = Prefix + ".Patients";
    public const string Protocols = Prefix + ".Protocols";
    public const string ReportsDoctor = Protocols + ".Doctor_Reports";
    public const string ReportsDepartment = Protocols + ".Department-Reports";
    public const string Departments = Prefix + ".Departments";
    public const string MedicalServices = Prefix + ".MedicalServices";
    public const string Doctors = Prefix + ".Doctors";
    public const string TestApprovalPanel = Prefix + ".TestApprovalPanel";
    public const string LaboratoryTechnician = Prefix + ".LaboratoryTechnician";
    public const string MyPatients = Prefix + ".MyPatients";
    public const string MedicalStaff = Prefix + ".MedicalStaff";
    public const string Laboratory = Prefix + ".Laboratory";
    
    public const string Appointments = Prefix + ".Appointments";
    public const string AppointmentTypes = Prefix + ".AppointmentTypes";
    public const string AppointmentList = Prefix + ".AppointmentList";
    public const string AppointmentsOverview = Prefix + ".AppointmentsOverview";
    public const string DoctorLeaves = Prefix + ".DoctorLeaves";
    public const string Treatment = Prefix + ".Treatment";
    public const string Insurances = Prefix + ".Insurances";
    public const string Icds = Treatment + ".Icds";
    public const string MyProtocols = Treatment + ".MyProtocols";
    public const string IcdReport = Treatment + ".IcdReport";
    public const string ProtocolType = Prefix + ".ProtocolTypes";
}
