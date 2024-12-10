using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Pusula.Training.HealthCare.Appointments;
using Pusula.Training.HealthCare.Blazor.Models;
using Pusula.Training.HealthCare.Doctors;
using Pusula.Training.HealthCare.MedicalServices;
using Pusula.Training.HealthCare.Patients;
using Pusula.Training.HealthCare.Shared;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Navigations;
using Volo.Abp;
using Doctor = Pusula.Training.HealthCare.Blazor.Models.Doctor;

namespace Pusula.Training.HealthCare.Blazor.Components.Pages;

public partial class Appointments : HealthCareComponentBase
{
#pragma warning disable BL0005
    [Parameter] public int PatientNo { get; set; }

    #region Stepper

    private AppointmentStepperModel StepperModel { get; set; }
    private SfStepper Stepper { get; set; } = null!;
    private StepperStep SelectServiceStepper { get; set; } = null!;
    private StepperStep ScheduleStepper { get; set; } = null!;
    private StepperStep ConfirmationStepper { get; set; } = null!;
    private StepperStep ResultStepper { get; set; } = null!;
    private int ActiveStep { get; set; }

    #endregion

    private bool IsFinalResultSuccess { get; set; }

    #region MedicalServiceFilters

    private IReadOnlyList<MedicalServiceWithDepartmentsDto> MedicalServiceWithDepartmentsList { get; set; } = null!;
    private GetMedicalServiceInput MedicalServiceFilter { get; set; }
    private int ServicePageSize { get; } = 50;
    private int ServiceCurrentPage { get; set; } = 1;
    private string ServiceCurrentSorting { get; set; } = string.Empty;
    private bool IsServiceListLoading { get; set; }

    #endregion

    #region DoctorFilters

    private List<Doctor> DoctorsList { get; set; }
    private bool IsDoctorListLoading { get; set; }
    private int DoctorLoadingShimmerCount { get; set; } = 5;
    private GetDoctorsWithDepartmentIdsInput DoctorsWithDepartmentIdsInput { get; set; }
    private int DoctorPageSize { get; } = 50;
    private int TypePageSize { get; } = 50;
    private int DoctorCurrentPage { get; set; } = 1;
    private string DoctorCurrentSorting { get; set; } = string.Empty;

    #endregion

    #region AppointmentFilters

    private GetAppointmentSlotInput GetAppointmentSlotFilter { get; set; }

    #endregion

    private PatientDto Patient { get; set; }
    private IReadOnlyList<LookupDto<Guid>> AppointmentTypesCollection { get; set; } = [];
    private List<SelectionItem> ServicesList { get; set; }
    private SfListBox<SelectionItem[], SelectionItem> SelectServiceDropdown { get; set; } = null!;
    private AppointmentCreateDto NewAppointment { get; set; }
    private List<AppointmentSlotItem> AppointmentSlots { get; set; }
    private List<AppointmentDayItemLookupDto> DaysLookupList { get; set; }
    private GetAppointmentsLookupInput DaysLookupFilter { get; set; }
    private int LoadCount { get; set; } = 14;
    private double ScreenWidth { get; set; }
    private int AvailableSlotCount { get; set; }
    private int LoadingShimmerCount { get; set; } = 24;
    private string AppointmentId { get; set; } = "ER123456";
    private string PaymentId { get; set; } = "PAY987654";
    private bool SlotsLoading { get; set; }
    private bool SlotDaysLoading { get; set; }
    private bool IsUserNavigatingReverse { get; set; }
    private bool IsCurrentStepValid { get; set; }

    private bool IsFirstStepValid =>
        !string.IsNullOrEmpty(StepperModel.MedicalServiceName) &&
        !string.IsNullOrEmpty(StepperModel.DoctorName) &&
        !string.IsNullOrEmpty(StepperModel.DepartmentName);

    private bool IsSecondStepValid =>
        IsFirstStepValid &&
        !string.IsNullOrEmpty(StepperModel.AppointmentDisplayTime) &&
        !string.IsNullOrEmpty(StepperModel.AppointmentDisplayDate);

    private bool IsThirdStepValid =>
        IsSecondStepValid &&
        !string.IsNullOrEmpty(StepperModel.PatientName) &&
        IsAppointmentTypeIdValid(StepperModel.AppointmentTypeId) &&
        !string.IsNullOrEmpty(StepperModel.HospitalName);

    private static bool IsAppointmentTypeIdValid(Guid? appointmentTypeId) =>
        appointmentTypeId.HasValue && appointmentTypeId.Value != Guid.Empty;

    public Appointments()
    {
        Patient = new PatientDto();
        GetAppointmentSlotFilter = new GetAppointmentSlotInput();
        StepperModel = new AppointmentStepperModel();
        NewAppointment = new AppointmentCreateDto();
        MedicalServiceFilter = new GetMedicalServiceInput
        {
            Name = "",
            MaxResultCount = ServicePageSize,
            SkipCount = (ServiceCurrentPage - 1) * ServicePageSize,
            Sorting = ServiceCurrentSorting
        };
        DoctorsWithDepartmentIdsInput = new GetDoctorsWithDepartmentIdsInput
        {
            Name = "",
            MaxResultCount = DoctorPageSize,
            SkipCount = (DoctorCurrentPage - 1) * DoctorPageSize,
            Sorting = DoctorCurrentSorting
        };
        DaysLookupFilter = new GetAppointmentsLookupInput
        {
            Offset = LoadCount,
            StartDate = DateTime.Now
        };

        DoctorsList = [];
        ServicesList = [];
        DaysLookupList = [];
        AppointmentSlots = [];
        IsFinalResultSuccess = false;
        IsDoctorListLoading = false;
        IsServiceListLoading = true;
        IsUserNavigatingReverse = false;
        SlotDaysLoading = false;
        IsCurrentStepValid = false;
        ActiveStep = 0;
        ScreenWidth = 1100;
    }

    protected override async Task OnInitializedAsync()
    {
        await GetPatient();
        await GetServices();
        await GetAppointmentTypes();
        SetDayLoadCount();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return Task.CompletedTask;
    }

    private async Task CancelProcess()
    {
        await OnStepperReset();
        ResetAppointmentInfo();
        StateHasChanged();
    }

    #region GenerateAppointmentDays

    private void SetDayLoadCount()
    {
        LoadCount = ScreenWidth switch
        {
            < 768 => 7,
            < 1024 => 14,
            _ => 14
        };
    }

    private async Task GetAppointmentDays()
    {
        try
        {
            SlotDaysLoading = true;
            DaysLookupList.Clear();
            
            var days = 
                (await AppointmentAppService.GetAvailableDaysLookupAsync(DaysLookupFilter))
                .Items
                .ToList();

            DaysLookupList = ObjectMapper.Map<List<AppointmentDayLookupDto>, List<AppointmentDayItemLookupDto>>(days);
        }
        catch (Exception e)
        {
            DaysLookupList = [];
            await UiMessageService.Error(e.Message);
        }
        finally
        {
            SlotDaysLoading = false;
        }
    }

    private async Task OnLoadDaysDaysLeft()
    {
        var newStartDate = DaysLookupFilter.StartDate.AddDays(-LoadCount);
        DaysLookupFilter.StartDate = newStartDate < DateTime.Now.Date ? DateTime.Now.Date : newStartDate;
        await GetAppointmentDays();
    }

    private async Task OnLoadDaysRight()
    {
        DaysLookupFilter.StartDate = DaysLookupFilter.StartDate.AddDays(LoadCount);
        await GetAppointmentDays();
    }

    #endregion

    #region Selections

    private async Task OnServiceChange(ListBoxChangeEventArgs<SelectionItem[], SelectionItem> args)
    {
        //Reset stepper model
        await OnStepperReset();
        //Reset appointment times
        ResetAppointmentInfo();

        if (args.Value.Length > 0)
        {
            var selectedService = args.Value[0];

            StepperModel.MedicalServiceId = selectedService.Id;
            StepperModel.MedicalServiceName = selectedService.DisplayName;
            StepperModel.Amount = selectedService.Cost;
            DaysLookupFilter.MedicalServiceId = selectedService.Id;
            GetAppointmentSlotFilter.MedicalServiceId = selectedService.Id;
        }

        await GetDoctorsList();
    }

    private void OnDoctorSelect(Doctor item)
    {
        if (item.IsSelected)
        {
            DoctorsList.ForEach(e => e.IsSelected = false);
        }
        else
        {
            DoctorsList.ForEach(e => e.IsSelected = false);
            item.IsSelected = true;
        }

        if (item.IsSelected)
        {
            StepperModel.DoctorName = item.Name;
            StepperModel.DoctorId = item.Id;
            StepperModel.DepartmentId = item.DepartmentId;
            StepperModel.DepartmentName = item.Department;
            GetAppointmentSlotFilter.DoctorId = item.Id;
            DaysLookupFilter.DoctorId = item.Id;
        }

        StateHasChanged();
    }

    #endregion

    #region API Fetch

    private async Task GetPatient()
    {
        try
        {
            var patient = await PatientsAppService.GetPatientByNumberAsync(PatientNo);
            Patient = patient;
            StepperModel.PatientId = Patient.Id;
            StepperModel.PatientName = Patient.FirstName + " " + Patient.LastName;
        }
        catch (Exception e)
        {
            await UiMessageService.Error(e.Message);
        }
    }

    private async Task GetAppointmentTypes(string? newValue = null)
    {
        try
        {
            IsServiceListLoading = true;
            AppointmentTypesCollection =
                (await LookupAppService.GetAppointmentTypeLookupAsync(new LookupRequestDto
                    { Filter = newValue, MaxResultCount = TypePageSize }))
                .Items;

            StateHasChanged();
        }
        catch (Exception e)
        {
            AppointmentTypesCollection = [];
            await UiMessageService.Error(e.Message);
        }
    }

    private async Task GetServices()
    {
        try
        {
            IsServiceListLoading = true;

            await ClearServiceSelection();
            ServicesList = [];

            MedicalServiceWithDepartmentsList =
                (await MedicalServiceAppService
                    .GetMedicalServiceWithDepartmentsAsync(MedicalServiceFilter))
                .Items
                .ToList();

            ServicesList = MedicalServiceWithDepartmentsList.Select(x => x.MedicalService)
                .Select(y => new SelectionItem
                {
                    DisplayName = y.Name,
                    Id = y.Id,
                    Cost = y.Cost,
                    IsSelected = false
                }).ToList();

            StateHasChanged();
        }
        catch (Exception e)
        {
            await UiMessageService.Error(e.Message);
        }
        finally
        {
            IsServiceListLoading = false;
        }
    }

    private async Task GetDoctorsList()
    {
        try
        {
            IsDoctorListLoading = true;
            var deptIds = GetRelevantDepartmentIds(StepperModel.MedicalServiceId);

            DoctorsWithDepartmentIdsInput.DepartmentIds = deptIds;
            var doctors = (await DoctorsAppService.GetByDepartmentIdsAsync(DoctorsWithDepartmentIdsInput)).Items;

            if (!doctors.Any())
            {
                DoctorsList = [];
                DoctorsWithDepartmentIdsInput = new GetDoctorsWithDepartmentIdsInput();
                IsDoctorListLoading = false;
                return;
            }

            DoctorsList = doctors
                .Select(x => new Doctor
                {
                    Id = x.Doctor.Id,
                    DepartmentId = x.Doctor.DepartmentId,
                    Name = $"{x.Title.TitleName} {x.Doctor.FirstName} {x.Doctor.LastName}",
                    Department = x.Department.Name,
                    Gender = x.Doctor.Gender,
                    IsAvailable = true,
                    IsSelected = false
                })
                .ToList();
        }
        catch (Exception e)
        {
            await UiMessageService.Error(e.Message);
        }
        finally
        {
            IsDoctorListLoading = false;
        }
    }

    private List<Guid> GetRelevantDepartmentIds(Guid? medicalServiceId) =>
        MedicalServiceWithDepartmentsList
            .Where(x => x.MedicalService.Id == medicalServiceId)
            .SelectMany(x => x.Departments)
            .Select(dept => dept.Id)
            .ToList();

    private async Task GetAvailableSlots()
    {
        try
        {
            SlotsLoading = true;

            var slots =
                (await AppointmentAppService.GetAvailableSlotsAsync(GetAppointmentSlotFilter)).Items;

            if (!slots.Any())
            {
                AppointmentSlots = [];
                return;
            }

            AppointmentSlots = slots.Select(x => new AppointmentSlotItem
            {
                DoctorId = x.DoctorId,
                MedicalServiceId = x.MedicalServiceId,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                AvailabilityValue = x.AvailabilityValue,
                IsSelected = false
            }).ToList();

            AvailableSlotCount = AppointmentSlots.Count(e => e.AvailabilityValue);
        }
        catch (BusinessException)
        {
            AppointmentSlots = [];
            AvailableSlotCount = 0;
        }
        finally
        {
            SlotsLoading = false;
        }
    }

    #endregion

    #region SearchData

    private async Task OnTextChanged(string newText)
    {
        MedicalServiceFilter.Name = newText;
        await GetServices();
    }

    private async Task OnDoctorSearchChanged(string? newText)
    {
        DoctorsWithDepartmentIdsInput.FilterText = newText ?? string.Empty;
        await GetDoctorsList();
    }

    #endregion

    #region ClearingHandlers

    private async void ClearServiceSearch()
    {
        MedicalServiceFilter.Name = "";
        await GetServices();
    }

    private async Task ClearServiceSelection()
    {
        await SelectServiceDropdown.SelectItemsAsync(SelectServiceDropdown.Value, false);
    }

    private void ResetAppointmentInfo()
    {
        AvailableSlotCount = 0;
        AppointmentSlots.Clear();
        DaysLookupList.ForEach(e => e.IsSelected = false);
        DoctorsList.ForEach(e => e.IsSelected = false);
        ServicesList.ForEach(e => e.IsSelected = false);
    }

    #endregion

    #region StyleHandler

    private string GetDoctorCardClass(Doctor doctor)
    {
        var classes = new List<string>
        {
            "rounded-1",
            "shadow",
            "p-2",
            "m-0",
            "bg-white",
            "rounded",
            "doctor-card"
        };

        if (doctor.IsSelected)
        {
            classes.Add("doctor-card-active");
        }

        if (!doctor.IsAvailable)
        {
            classes.Add("disabled-card");
        }

        return string.Join(" ", classes);
    }

    #endregion

    private async Task OnSelectAppointmentDay(AppointmentDayItemLookupDto item)
    {
        DaysLookupList.ForEach(e => e.IsSelected = false);
        item.IsSelected = true;
        StepperModel.AppointmentDisplayDate = item.Date.ToShortDateString();
        StepperModel.AppointmentDate = item.Date;
        GetAppointmentSlotFilter.Date = item.Date;
        await GetAvailableSlots();
    }

    private void SelectAppointmentSlot(AppointmentSlotItem appointmentSlot)
    {
        if (!appointmentSlot.AvailabilityValue)
        {
            return;
        }

        if (appointmentSlot.IsSelected)
        {
            AppointmentSlots.ForEach(e => e.IsSelected = false);
            StepperModel.AppointmentDisplayTime = null!;
            StepperModel.StartTime = null!;
            StepperModel.EndTime = null!;
        }
        else
        {
            AppointmentSlots.ForEach(e => e.IsSelected = false);
            appointmentSlot.IsSelected = true;
            StepperModel.AppointmentDisplayTime =
                $"{appointmentSlot.StartTime.ToString(CultureInfo.CurrentCulture)} - {appointmentSlot.EndTime.ToString(CultureInfo.CurrentCulture)}";
            StepperModel.StartTime = appointmentSlot.StartTime;
            StepperModel.EndTime = appointmentSlot.EndTime;
        }
    }

    private Task OnReminderSettingChanged(bool val)
    {
        StepperModel.ReminderSent = val;
        return Task.CompletedTask;
    }

    private async Task CreateAppointment()
    {
        if (!IsThirdStepValid)
        {
            return;
        }

        try
        {
            var baseDate = StepperModel.AppointmentDate;
            var parsedStartDateTime = ConvertToDateTime(baseDate, StepperModel.StartTime);
            var parsedEndDateTime = ConvertToDateTime(baseDate, StepperModel.EndTime);

            //New appointment object mapping
            NewAppointment.DoctorId = StepperModel.DoctorId;
            NewAppointment.DepartmentId = StepperModel.DepartmentId;
            NewAppointment.PatientId = StepperModel.PatientId;
            NewAppointment.MedicalServiceId = StepperModel.MedicalServiceId;
            NewAppointment.AppointmentDate = StepperModel.AppointmentDate;
            NewAppointment.StartTime = parsedStartDateTime;
            NewAppointment.EndTime = parsedEndDateTime;
            NewAppointment.Amount = StepperModel.Amount;
            NewAppointment.Notes = StepperModel.Note;
            NewAppointment.ReminderSent = StepperModel.ReminderSent;
            NewAppointment.AppointmentTypeId = StepperModel.AppointmentTypeId;

            var message = L["ConfirmMessage"];
            var confirm = L["Confirm"];

            if (!await UiMessageService.Confirm(message, confirm, options =>
                {
                    options.ConfirmButtonText = L["Yes"];
                    options.CancelButtonText = L["No"];
                }))
            {
                IsFinalResultSuccess = false;
                return;
            }

            await AppointmentAppService.CreateAsync(NewAppointment);
            IsFinalResultSuccess = true;
            await OnNextStep();
        }
        catch (Exception)
        {
            IsFinalResultSuccess = false;
            await OnNextStep();
        }
    }

    private static DateTime ConvertToDateTime(DateTime appointmentDate, string timeString)
    {
        if (TimeSpan.TryParse(timeString, out var time))
        {
            return appointmentDate.Date.Add(time);
        }

        throw new FormatException("Invalid time format. Expected format is 'hh:mm'.");
    }

    #region StepHandlers

    private async Task OnStepperReset()
    {
        ActiveStep = 0;
        await Stepper.ResetAsync();
        SelectServiceStepper.IsValid = null;
        ConfirmationStepper.IsValid = null;
        ScheduleStepper.IsValid = null;
        ResultStepper.IsValid = null;
        StepperModel = new AppointmentStepperModel
        {
            PatientId = Patient.Id,
            PatientName = Patient.FirstName + " " + Patient.LastName
        };
        StateHasChanged();
    }

    private async Task HandleStepChange(StepperChangeEventArgs args)
    {
        try
        {
            if (args.ActiveStep == args.PreviousStep)
            {
                return;
            }

            IsUserNavigatingReverse = args.ActiveStep < args.PreviousStep;

            if (!IsUserNavigatingReverse)
            {
                SetValidState(args);
            }
            else
            {
                for (var i = args.ActiveStep; i <= args.PreviousStep; i++)
                {
                    switch (i)
                    {
                        case 0:
                            SelectServiceStepper.IsValid = null;
                            break;
                        case 1:
                            ScheduleStepper.IsValid = null;
                            break;
                        case 2:
                            ConfirmationStepper.IsValid = null;
                            break;
                        case 3:
                            ResultStepper.IsValid = null;
                            break;
                    }
                }

                IsCurrentStepValid = true;
            }

            if (IsCurrentStepValid)
            {
                ActiveStep = args.ActiveStep;
                if (ActiveStep == 1)
                {
                    await GetAppointmentDays();
                }
            }
        }
        catch (Exception e)
        {
            await UiMessageService.Error(e.Message);
        }
    }

    private void SetValidState(StepperChangeEventArgs args)
    {
        var activeStep = Stepper.ActiveStep;
        switch (activeStep)
        {
            case 0:
            {
                IsCurrentStepValid = IsFirstStepValid;
                SelectServiceStepper.IsValid = IsCurrentStepValid;
                break;
            }
            case 1:
            {
                IsCurrentStepValid = IsSecondStepValid;
                ScheduleStepper.IsValid = IsCurrentStepValid;
                break;
            }
            case 2:
            {
                IsCurrentStepValid = IsThirdStepValid;
                ConfirmationStepper.IsValid = IsCurrentStepValid;
                break;
            }
        }

        args.Cancel = !IsCurrentStepValid;
    }

    private async Task OnNextStep()
    {
        await Stepper.NextStepAsync();
    }

    private async Task OnPreviousStep()
    {
        await Stepper.PreviousStepAsync();
    }

    #endregion

    #region Navigation

    private void NavigateToPatient()
    {
        NavigationManager.NavigateTo($"/patients/" + PatientNo + "/detail");
    }

    private async Task NavigateToFirstStep()
    {
        await OnStepperReset();
        ResetAppointmentInfo();
        StateHasChanged();
    }

    #endregion
}