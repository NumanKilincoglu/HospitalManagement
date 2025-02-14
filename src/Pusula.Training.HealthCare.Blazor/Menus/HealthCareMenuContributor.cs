﻿using System.Threading.Tasks;
using Pusula.Training.HealthCare.Localization;
using Pusula.Training.HealthCare.MultiTenancy;
using Pusula.Training.HealthCare.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;

namespace Pusula.Training.HealthCare.Blazor.Menus;

public class HealthCareMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<HealthCareResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                HealthCareMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        ConfigureTenantMenu(administration, MultiTenancyConsts.IsEnabled);


        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        context.Menu.AddItem(
            new ApplicationMenuItem(
                HealthCareMenus.Departments,
                l["Menu:Departments"],
                url: "/departments",
                icon: "fa fa-file-alt",
                requiredPermissionName: HealthCarePermissions.Departments.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                    name: HealthCareMenus.Doctors,
                    displayName: l["Menu:Doctors"],
                    icon: "fa-solid fa-user-md",
                    requiredPermissionName: HealthCarePermissions.Doctors.Default
                )
                .AddItem(new ApplicationMenuItem(
                    name: HealthCareMenus.Doctors,
                    displayName: l["Menu:DoctorList"],
                    url: "doctors/list",
                    icon: "fa-solid fa-list")
                )
                .AddItem(new ApplicationMenuItem(
                    name: HealthCareMenus.DoctorLeaves,
                    displayName: l["Menu:DoctorLeaves"],
                    url: "doctors/leaves",
                    icon: "fa-solid fa-calendar")
                )
        );
        
                context.Menu.AddItem(
            new ApplicationMenuItem(
                    name: "Hasta Kabul",
                    displayName: l["Menu:HastaKabul"],
                    icon: "fa fa-users",
                    requiredPermissionName: HealthCarePermissions.Patients.Default
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Definitions",
                            displayName: l["Menu:Definitions"],
                            icon: "fa-solid fa-folder"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.ProtocolType,
                            displayName: l["Menu:ProtocolType"],
                            url: "/protocol-types",
                            icon: "fa fa-file-alt",
                            requiredPermissionName: HealthCarePermissions.ProtocolTypes.Default )
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.Insurances,
                            displayName: l["Menu:Insurance"],
                            url: "/insurances",
                            icon: "fa fa-file-alt",
                            requiredPermissionName: HealthCarePermissions.Insurances.Default)
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Operations",
                            displayName: l["Menu:Operations"],
                            icon: "fa-solid fa-cogs"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.Protocols,
                            displayName: l["Menu:Protocols"],
                            url: "/protocols",
                            icon: "fa fa-file-alt",
                            requiredPermissionName: HealthCarePermissions.Protocols.Default)
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.Patients,
                            displayName: l["Menu:Patients"],
                            url: "/patients",
                            icon: "fa fa-file-alt",
                            requiredPermissionName: HealthCarePermissions.Patients.Default)
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                        name: "Reports",
                        displayName: l["Menu:Reports"],
                        icon: "fa-solid fa-chart-bar"
                    )
                    .AddItem(new ApplicationMenuItem(
                        name: HealthCareMenus.ReportsDepartment,
                        displayName: l["Menu:Department-Reports"],
                        url: "/protocols-report-department",
                        icon: "fa fa-file-alt",
                        requiredPermissionName: HealthCarePermissions.Protocols.Default)
                    )
                    .AddItem(new ApplicationMenuItem(
                        name: HealthCareMenus.ReportsDoctor,
                        displayName: l["Menu:Doctor-Reports"],
                        url: "/protocols-report-doctor",
                        icon: "fa fa-file-alt",
                        requiredPermissionName: HealthCarePermissions.Protocols.Default)
                    )
                ));
        
        context.Menu.AddItem(
            new ApplicationMenuItem(
                    name: HealthCareMenus.Appointments,
                    displayName: l["Menu:Appointment"],
                    icon: "fa-solid fa-calendar-check",
                    requiredPermissionName: HealthCarePermissions.Appointments.Default
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Definitions",
                            displayName: l["Menu:Definitions"],
                            icon: "fa-solid fa-folder"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.AppointmentTypes,
                            displayName: l["Menu:AppointmentTypes"],
                            url: "appointments/definitions/types",
                            icon: "fa-solid fa-calendar-days")
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.MedicalServices,
                            displayName: l["Menu:MedicalServices"],
                            url: "appointments/definitions/medical-services",
                            icon: "fa-solid fa-notes-medical")
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Operations",
                            displayName: l["Menu:Operations"],
                            icon: "fa-solid fa-cogs"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.Appointments,
                            displayName: l["Menu:ManageAppointment"],
                            url: "/appointments/operations/create",
                            icon: "fa-solid fa-calendar-check")
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Reports",
                            displayName: l["Menu:Reports"],
                            url: "/appointments/reports",
                            icon: "fa-solid fa-chart-bar"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.AppointmentList,
                            displayName: l["Menu:AppointmentList"],
                            url: "appointments/reports/list",
                            icon: "fa-solid fa-list")
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.AppointmentsOverview,
                            displayName: l["Menu:AppointmentsOverview"],
                            url: "/appointments/reports/overview",
                            icon: "fa-solid fa-pie-chart")
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.AppointmentsOverview,
                            displayName: l["Menu:DoctorAppointmentsOverview"],
                            url: "/appointments/reports/doctor-appointments",
                            icon: "fa-solid fa-calendar")
                        )
                )
        );
        
        context.Menu.AddItem(
            new ApplicationMenuItem(
                    name: HealthCareMenus.Laboratory,
                    displayName: l["Menu:Laboratory"],
                    icon: "fa-solid fa-flask-vial",
                    requiredPermissionName: HealthCarePermissions.BloodTests.Create
                )
                .AddItem(
                    new ApplicationMenuItem(
                        HealthCareMenus.MyPatients,
                        l["Menu:MyPatients"],
                        url: "/doctor/my-patients",
                        icon: "fa-solid fa-receipt",
                        requiredPermissionName: HealthCarePermissions.Doctors.Default)
                )
                
                .AddItem(
                new ApplicationMenuItem(
                    HealthCareMenus.TestApprovalPanel,
                    l["Menu:TestApprovalPanel"],
                    url: "/TestApprovalPanel",
                    icon: "fa-solid fa-list-check",
                    requiredPermissionName: HealthCarePermissions.BloodTests.Edit)
                )
                
                .AddItem(
                    new ApplicationMenuItem(
                        HealthCareMenus.TestApprovalPanel,
                        l["Menu:LaboratoryTechnician"],
                        url: "/laboratorytechnician",
                        icon: "fa-solid fa-flask-vial",
                        requiredPermissionName: HealthCarePermissions.BloodTests.Edit)
                )
                );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                HealthCareMenus.MedicalStaff,
                l["Menu:MedicalStaff"],
                url: "/medical-staff",
                icon: "fa fa-user-md",
                requiredPermissionName: HealthCarePermissions.MedicalStaff.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                    name: HealthCareMenus.Treatment,
                    displayName: l["Menu:Treatment"],
                    icon: "fa-solid fa-medkit",
                    requiredPermissionName: HealthCarePermissions.Treatment.Default
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Definitions",
                            displayName: l["Menu:Definitions"],
                            icon: "fa-solid fa-folder"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.Icds,
                            displayName: l["Menu:Icds"],
                            url: "/icds",
                            icon: "fa-solid fa-plus-square")
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Operations",
                            displayName: l["Menu:Operations"],
                            icon: "fa-solid fa-cogs"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.MyProtocols,
                            displayName: l["Menu:MyProtocols"],
                            url: "/my-protocols",
                            icon: "fa-solid fa-plus-square")
                        )
                )
                .AddItem(
                    new ApplicationMenuItem(
                            name: "Reports",
                            displayName: l["Menu:Reports"],
                            icon: "fa-solid fa-chart-bar"
                        )
                        .AddItem(new ApplicationMenuItem(
                            name: HealthCareMenus.IcdReport,
                            displayName: l["Menu:IcdReport"],
                            url: "/icd-report",
                            icon: "fa-solid fa-plus-square")
                        )
                ));
        return Task.CompletedTask;
    }

    private static void ConfigureTenantMenu(ApplicationMenuItem? item, bool isMultiTenancyEnabled)
    {
        if (isMultiTenancyEnabled)
        {
            item?.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            item?.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }
    }
}