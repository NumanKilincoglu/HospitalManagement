﻿using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Pusula.Training.HealthCare.Countries;
using Pusula.Training.HealthCare.Patients;
using Pusula.Training.HealthCare.Permissions;
using Pusula.Training.HealthCare.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Pusula.Training.HealthCare.Blazor.Components.Modals;
using Pusula.Training.HealthCare.Protocols;
using Pusula.Training.HealthCare.Shared;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
    
namespace Pusula.Training.HealthCare.Blazor.Components.Pages.Patient;

public partial class Patients
{
    [CreatePhoneNumberValidator]
    public string MobilePhoneNumber { get; set; } 

    [CreatePhoneNumberValidator]
    public string RelativePhoneNumber { get; set; } 

    protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems;
    protected PageToolbar Toolbar { get; } 
    protected bool ShowAdvancedFilters { get; set; }
    private IReadOnlyList<PatientDto> PatientList { get; set; }
    private IReadOnlyList<LookupDto<Guid>> DepartmentsCollection { get; set; } 

    private IReadOnlyList<LookupDto<Guid>> InsuranceCollections { get; set; } 
    private IReadOnlyList<LookupDto<Guid>> DoctorsCollection { get; set; } 
    private IReadOnlyList<LookupDto<Guid>> ProtocolTypesCollection { get; set; } 
    private int PageSize { get; } 
    private int CurrentPage { get; set; } 
    private int TotalCount { get; set; }
    private string CurrentSorting { get; set; } 
    public string? MainCountryCode { get; set; }
    public string? RelativeCountryCode { get; set; }
    private string FoundPatientName { get; set; } = string.Empty; // Bulunan hastanın adı (eğer varsa)
    private bool AllPatientsSelected { get; set; }
    private bool CanCreatePatient { get; set; }
    private bool CanEditPatient { get; set; }
    private bool CanDeletePatient { get; set; }
    private bool IsLookupsLoaded { get; set; }
    private int LookupPageSize { get; }
    private ProtocolCreateDto _newProtocol;
    private GenericModal<ProtocolCreateDto> _createModal;
    private PatientCreateDto NewPatient { get; set; } 
    private PatientUpdateDto EditingPatient { get; set; }
    private Validations NewPatientValidations { get; set; } 
    private Validations EditingPatientValidations { get; set; } 
    private Guid EditingPatientId { get; set; }
    private Modal CreatePatientModal { get; set; } 
    private Modal EditPatientModal { get; set; } 
    private GetPatientsInput Filter { get; set; }
    private List<PatientDto> SelectedPatients { get; set; }
    private IEnumerable<CountryPhoneCodeDto> Nationalities;
    private IEnumerable<KeyValuePair<int, string>> Genders;
    private IEnumerable<KeyValuePair<int, string>> Relatives;
    private IEnumerable<KeyValuePair<int, string>> PationTypes;
    private IEnumerable<KeyValuePair<int, string>> DiscountGroups;

    public Patients()
    {
        NewPatient = new();
        CreatePatientModal = new();
        EditPatientModal = new();
        EditingPatientValidations = new();
        EditingPatient = new PatientUpdateDto();
        _createModal = new GenericModal<ProtocolCreateDto>();
        _newProtocol = new ProtocolCreateDto();
        NewPatientValidations = new();
        Filter = new GetPatientsInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            Sorting = CurrentSorting
        };
        DepartmentsCollection = [];
        InsuranceCollections = [];
        DoctorsCollection = [];
        ProtocolTypesCollection = [];
        PatientList = [];
        Nationalities = [];
        Genders = [];
        Relatives = [];
        PationTypes = [];
        DiscountGroups = [];
        SelectedPatients = [];
        BreadcrumbItems = [];
        LookupPageSize = 100;
        CurrentSorting = string.Empty;
        FoundPatientName = string.Empty;
        MobilePhoneNumber = string.Empty;
        RelativePhoneNumber = string.Empty;
        PageSize = LimitedResultRequestDto.DefaultMaxResultCount;
        Toolbar = new PageToolbar();
        CurrentPage = 1;
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            DepartmentsCollection =
                (await LookupAppService.GetDepartmentLookupAsync(new LookupRequestDto
                    { MaxResultCount = LookupPageSize }))
                .Items;

            InsuranceCollections =
                (await LookupAppService.GetInsuranceLookupAsync(new LookupRequestDto
                    { MaxResultCount = LookupPageSize }))
                .Items;

            DoctorsCollection = (await LookupAppService.GetDoctorLookupAsync(new LookupRequestDto
                    { MaxResultCount = LookupPageSize }))
                .Items;
            ProtocolTypesCollection =
                (await LookupAppService.GetProtocolTypeLookupAsync(new LookupRequestDto
                    { MaxResultCount = LookupPageSize }))
                .Items;
        }
        catch (Exception e)
        {
            await UiMessageService.Error(e.Message);
        }
    }

    private async Task ShowCreateModal(PatientDto patientDto)
    {

        _newProtocol = new ProtocolCreateDto();
        FoundPatientName = patientDto.FirstName + " " + patientDto.LastName;

        _newProtocol.PatientId = patientDto.Id;
        
        if (!IsLookupsLoaded) 
        {
            await LoadLookupsAsync();
            IsLookupsLoaded = true; 
        }
        _createModal.Show(); 
    }

    private async Task CreateProtocolTypeAsync(ProtocolCreateDto protocol)
    {
        try
        {
            protocol = _newProtocol;
            await ProtocolsAppService.CreateAsync(protocol);

            await UiMessageService.Success(L["Protocol created successfully"] );
            
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(@L["An error occurred while creating the Protocol."] + ex.Message);
            throw new UserFriendlyException(ex.Message);
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
       
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            await SetBreadcrumbItemsAsync();
            await SetToolbarItemsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    protected virtual ValueTask SetBreadcrumbItemsAsync()
    {
        BreadcrumbItems.Add(new Volo.Abp.BlazoriseUI.BreadcrumbItem(L["Patients"]));
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask SetToolbarItemsAsync()
    {
        Toolbar.AddButton(L["ExportToExcel"], DownloadAsExcelAsync, IconName.Download);
        Toolbar.AddButton(L["NewPatient"], OpenCreatePatientModalAsync, IconName.Add, requiredPolicyName: HealthCarePermissions.Patients.Create);
        return ValueTask.CompletedTask;
    }

    private async Task SetPermissionsAsync()
    {
        CanCreatePatient = await AuthorizationService
            .IsGrantedAsync(HealthCarePermissions.Patients.Create);
        CanEditPatient = await AuthorizationService
                        .IsGrantedAsync(HealthCarePermissions.Patients.Edit);
        CanDeletePatient = await AuthorizationService
                        .IsGrantedAsync(HealthCarePermissions.Patients.Delete);
    }

    private async Task GetPatientsAsync()
    {
        Filter.MaxResultCount = PageSize;
        Filter.SkipCount = (CurrentPage - 1) * PageSize;
        Filter.Sorting = CurrentSorting;

        var result = await PatientsAppService.GetListAsync(Filter);
        PatientList = result.Items;
        TotalCount = (int)result.TotalCount;

        await ClearSelection();
    }

    // for navigate to the detail-page
    private void NavigateToDetail(PatientDto patientDto)
    {
        //seçili hastayı servise kaydediyoruz 
        //   patientService.SetPatient(patientDto);
        NavigationManager.NavigateTo($"/patients/{patientDto.PatientNumber}/detail");
    }
    
    protected virtual async Task SearchAsync()
    {
        CurrentPage = 1;
        await GetPatientsAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task DownloadAsExcelAsync()
    {
        var token = (await PatientsAppService.GetDownloadTokenAsync()).Token;
        var remoteService = await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("HealthCare") ?? await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");
        var culture = CultureInfo.CurrentUICulture.Name ;
        if (!culture.IsNullOrEmpty())
        {
            culture = "&culture=" + culture;
        }
        await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");
        NavigationManager.NavigateTo($"{remoteService?.BaseUrl.EnsureEndsWith('/') ?? string.Empty}api/app/patients/as-excel-file?DownloadToken={token}&FilterText={HttpUtility.UrlEncode(Filter.FilterText)}" +
            $"{culture}&FirstName={HttpUtility.UrlEncode(Filter.FirstName)}&LastName={HttpUtility.UrlEncode(Filter.LastName)}&BirthDateMin={Filter.BirthDateMin?.ToString("O")}" +
            $"&BirthDateMax={Filter.BirthDateMax?.ToString("O")}&IdentityNumber={HttpUtility.UrlEncode(Filter.IdentityNumber)}&PassportNumber={HttpUtility.UrlEncode(Filter.PassportNumber)}" +
            $"&EmailAddress={HttpUtility.UrlEncode(Filter.EmailAddress)}" +$"&MobilePhoneNumber={HttpUtility.UrlEncode(Filter.MobilePhoneNumber)}&Gender={Filter.Gender}", forceLoad: true);
    }

    private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<PatientDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");
        CurrentPage = e.Page;
        await GetPatientsAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OpenCreatePatientModalAsync()
    {
        await GetNationalitiesListAsync();
        NewPatient = new PatientCreateDto
        {
            BirthDate = DateTime.Now,
            Gender = EnumGender.NONE,
            Nationality = Nationalities.First().Country!,
            Relative = EnumRelative.NONE,
        };
        await GetGendersListAsync();
        await GetRelativesListAsync();
        await GetPationTypesListAsync();
        await GetDiscountGroupsListAsync();
        await NewPatientValidations.ClearAll();
        await ClearPhone();
        await CreatePatientModal.Show();
    }

    private async Task OpenEditPatientModalAsync(PatientDto input)
    {
        await GetNationalitiesListAsync();
        await GetGendersListAsync();
        await GetRelativesListAsync();
        await GetPationTypesListAsync();
        await GetDiscountGroupsListAsync();
        await EditingPatientValidations.ClearAll();
        var patient = await PatientsAppService.GetAsync(input.Id);

        EditingPatientId = patient.Id;
        EditingPatient = ObjectMapper.Map<PatientDto, PatientUpdateDto>(patient);

        await EditPatientModal.Show();
    }

    private async Task CloseCreatePatientModalAsync()
    {
        MainCountryCode = string.Empty;
        await ClearPhone();
        await CreatePatientModal.Hide();
    }

    private async Task CloseEditPatientModalAsync()
    {
        await EditPatientModal.Hide();
    }

    private async Task DeletePatientAsync(PatientDto input)
    {

        var confirmed = await UiMessageService.Confirm($"Are you sure you want to delete {input.FirstName} {input.LastName}?");
        if (!confirmed) return;

        await PatientsAppService.DeleteAsync(input.Id);
        await GetPatientsAsync();
    }

    // revert delete function
    private async Task RevertPatientAsync(PatientDto input)
    {

        var confirmed = await UiMessageService.Confirm($"Are you sure you want to undelete {input.FirstName} {input.LastName}?");
        if (!confirmed) return;

        var revertPatient = ObjectMapper.Map<PatientDto, PatientUpdateDto>(input);
        revertPatient.IsDeleted = !input.IsDeleted;
        await PatientsAppService.UpdateAsync(input.Id, revertPatient);
        await GetPatientsAsync();
    }

    private async Task CreatePatientAsync()
    {
        try
        {
            if (await NewPatientValidations.ValidateAll() == false)
            {
                return;
            }
            NewPatient.MobilePhoneNumber = string.IsNullOrWhiteSpace(MobilePhoneNumber)
                ? ""
                : $"+{MainCountryCode}{MobilePhoneNumber}";

            NewPatient.RelativePhoneNumber = NewPatient.RelativePhoneNumber != null ? $"+{RelativeCountryCode}{RelativePhoneNumber}" : "";
            await PatientsAppService.CreateAsync(NewPatient);
            await GetPatientsAsync();
            await CloseCreatePatientModalAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    private async Task UpdatePatientAsync()
    {
        try
        {
            if (await EditingPatientValidations.ValidateAll() == false)
            {
                return;
            }

            await PatientsAppService.UpdateAsync(EditingPatientId, EditingPatient);
            await GetPatientsAsync();
            await EditPatientModal.Hide();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    protected virtual async Task OnDeletedData(bool? isDeleted)
    {
        Filter.IsDeleted = isDeleted;
        await SearchAsync();
    }

    protected virtual async Task OnFirstNameChangedAsync(string? firstName)
    {
        Filter.FirstName = firstName;
        await SearchAsync();
    }

    protected virtual async Task OnLastNameChangedAsync(string? lastName)
    {
        Filter.LastName = lastName;
        await SearchAsync();
    }

    protected virtual async Task OnIdentityNumberChangedAsync(string? identityNumber)
    {
        Filter.IdentityNumber = identityNumber;
        await SearchAsync();
    }

    protected virtual async Task OnPassportNumberChangedAsync(string? passportNumber)
    {
        Filter.PassportNumber = passportNumber;
        await SearchAsync();
    }

    protected virtual void OnNationalityChangedAsync(ChangeEventArgs e)
    {
        var nationality = e.Value?.ToString();

        MainCountryCode = Nationalities?
            .FirstOrDefault(x => string.Equals(x.Country, nationality, StringComparison.OrdinalIgnoreCase))
            ?.Code ?? string.Empty;
        NewPatient.Nationality = nationality;
    }

    protected virtual async Task OnBirthDateMinChangedAsync(DateTime? birthDateMin)
    {
        Filter.BirthDateMin = birthDateMin.HasValue ? birthDateMin.Value.Date : birthDateMin;
        await SearchAsync();
    }

    protected virtual async Task OnBirthDateMaxChangedAsync(DateTime? birthDateMax)
    {
        Filter.BirthDateMax = birthDateMax.HasValue ? birthDateMax.Value.Date.AddDays(1).AddSeconds(-1) : birthDateMax;
        await SearchAsync();
    }

    protected virtual async Task OnEmailAddressChangedAsync(string? emailAddress)
    {
        Filter.EmailAddress = emailAddress;
        await SearchAsync();
    }

    protected virtual async Task OnMobilePhoneNumberChangedAsync(string? mobilePhoneNumber)
    {
        Filter.MobilePhoneNumber = mobilePhoneNumber;
        await SearchAsync();
    }

    protected virtual async Task OnGenderChangedAsync(int? gender)
    {
        await SearchAsync();
    }

    private void OnRelativePhoneNumberChanged(ChangeEventArgs e)
    {
        NewPatient.RelativePhoneNumber = e.Value?.ToString();
    }

    private Task SelectAllItems()
    {
        AllPatientsSelected = true;

        return Task.CompletedTask;
    }

    private Task ClearSelection()
    {
        AllPatientsSelected = false;
        SelectedPatients.Clear();

        return Task.CompletedTask;
    }

    private Task ClearPhone()
    {

        MobilePhoneNumber = string.Empty;
        RelativePhoneNumber = string.Empty;
        RelativeCountryCode = "";
        return Task.CompletedTask;
    }

    private Task SelectedPatientRowsChanged()
    {
        if (SelectedPatients.Count != PageSize)
        {
            AllPatientsSelected = false;
        }

        return Task.CompletedTask;
    }

    private async Task DeleteSelectedPatientsAsync()
    {
        var message = AllPatientsSelected ? L["DeleteAllRecords"].Value : L["DeleteSelectedRecords", SelectedPatients.Count].Value;

        if (!await UiMessageService.Confirm(message))
        {
            return;
        }

        if (AllPatientsSelected)
        {
            await PatientsAppService.DeleteAllAsync(Filter);
        }
        else
        {
            await PatientsAppService.DeleteByIdsAsync(SelectedPatients.Select(x => x.Id).ToList());
        }

        SelectedPatients.Clear();
        AllPatientsSelected = false;

        await GetPatientsAsync();
    }

    private async Task GetNationalitiesListAsync()
    {
        Nationalities = await CountryAppService.GetCountryPhoneCodesAsync();
    }

    private Task GetGendersListAsync()
    {
        Genders = Enum.GetValues(typeof(EnumGender))
                      .Cast<EnumGender>()
                      .Select(e => new KeyValuePair<int, string>((int)e, e.ToString()))
                      .ToList();
        return Task.CompletedTask;
    }

    private Task GetRelativesListAsync()
    {
        Relatives = Enum.GetValues(typeof(EnumRelative))
                     .Cast<EnumRelative>()
                     .Select(e => new KeyValuePair<int, string>((int)e, e.ToString()))
                     .ToList();

        return Task.CompletedTask;
    }

    private Task GetPationTypesListAsync()
    {
        PationTypes = Enum.GetValues(typeof(EnumPatientTypes))
                             .Cast<EnumPatientTypes>()
                             .Select(e => new KeyValuePair<int, string>((int)e, e.ToString()))
                             .ToList();
        return Task.CompletedTask;
    }

    private Task GetDiscountGroupsListAsync()
    {
        DiscountGroups = Enum.GetValues(typeof(EnumDiscountGroup))
                     .Cast<EnumDiscountGroup>()
                     .Select(e => new KeyValuePair<int, string>((int)e, e.ToString()))
                     .ToList();
        return Task.CompletedTask;
    }
}
