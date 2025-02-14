using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Pusula.Training.HealthCare.Permissions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Pusula.Training.HealthCare.AppointmentTypes;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Volo.Abp;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;

namespace Pusula.Training.HealthCare.Blazor.Components.Pages.Appointment;

public partial class AppointmentTypes : HealthCareComponentBase
{
    protected PageToolbar Toolbar { get; } = new PageToolbar();
    private SfGrid<AppointmentTypeDto> Grid { get; set; }
    private int PageSize { get; } = 5;
    private string[] ToolbarItems { get; set; }
    private int CurrentPage { get; set; } = 1;
    private string CurrentSorting { get; set; } = string.Empty;
    private bool CanCreateType { get; set; }
    private bool CanEditType { get; set; }
    private bool CanDeleteType { get; set; }
    private bool IsEditDialogVisible { get; set; }
    private bool IsCreateDialogVisible { get; set; }
    private GetAppointmentTypesInput Filter { get; set; }
    private AppointmentTypeUpdateDto EditingType { get; set; }
    private Guid EditingTypeId { get; set; } = default;
    private AppointmentTypeCreateDto NewType { get; set; }
    private bool IsDeleteDialogVisible { get; set; }
    private SfDialog DeleteConfirmDialog { get; set; }
    private bool Flag { get; set; }
    private string[] PageSizes { get; set; }
    private SfToast ToastObj { get; set; }
    private string ToastContent { get; set; } = string.Empty;
    private string ToastTitle { get; set; } = "Information";
    private string ToastCssClass { get; set; } = string.Empty;

    public AppointmentTypes()
    {
        ToastObj = new SfToast();
        ToolbarItems = ["Search", "Delete"];
        DeleteConfirmDialog = new SfDialog();
        Grid = new SfGrid<AppointmentTypeDto>();
        NewType = new AppointmentTypeCreateDto();
        Filter = new GetAppointmentTypesInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            Sorting = CurrentSorting
        };

        EditingType = new AppointmentTypeUpdateDto();
        IsEditDialogVisible = false;
        IsCreateDialogVisible = false;
        IsDeleteDialogVisible = false;
        Flag = false;
        PageSizes = ["5", "10", "15", "20"];
    }

    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetToolbarItemsAsync();
            await Grid.EnableToolbarItemsAsync(["Delete"], false);
            await Grid.Refresh();
            await InvokeAsync(StateHasChanged);
        }
    }

    protected virtual ValueTask SetToolbarItemsAsync()
    {
        Toolbar.AddButton(L["ExportToExcel"], DownloadAsExcelAsync, IconName.Download);
        Toolbar.AddButton(L["NewAppointmentType"], OpenCreateTypeModalAsync, IconName.Add,
            requiredPolicyName: HealthCarePermissions.AppointmentTypes.Create);

        return ValueTask.CompletedTask;
    }

    private async Task SetPermissionsAsync()
    {
        CanCreateType = await AuthorizationService
            .IsGrantedAsync(HealthCarePermissions.AppointmentTypes.Create);
        CanEditType = await AuthorizationService
            .IsGrantedAsync(HealthCarePermissions.AppointmentTypes.Edit);
        CanDeleteType = await AuthorizationService
            .IsGrantedAsync(HealthCarePermissions.AppointmentTypes.Delete);
    }

    #region GridHandlers

    public void OnActionBegin(ActionEventArgs<AppointmentTypeDto> args)
    {
        if (args.RequestType.ToString() != "Delete" || !IsDeleteDialogVisible)
        {
            return;
        }

        args.Cancel = true;
        DeleteConfirmDialog.ShowAsync();
        Flag = false;
    }


    public void Closed()
    {
        Flag = true;
    }

    public void RowSelectHandler(RowSelectEventArgs<AppointmentTypeDto> args)
    {
        var selectedRecordCount = Grid.GetSelectedRecordsAsync().Result.Count;
        if (selectedRecordCount > 0)
        {
            Grid.EnableToolbarItemsAsync(["Delete"], true);
        }
    }

    public void RowDeselectHandler(RowDeselectEventArgs<AppointmentTypeDto> args)
    {
        var selectedRecordCount = Grid.GetSelectedRecordsAsync().Result.Count;
        if (selectedRecordCount == 0)
        {
            Grid.EnableToolbarItemsAsync(["Delete"], false);
        }
    }

    public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs args)
    {
        try
        {
            switch (args.Item.Text)
            {
                case "Delete":
                {
                    var selectedRecord = Grid.GetSelectedRecordsAsync().Result;

                    if (selectedRecord == null || selectedRecord.Count == 0)
                    {
                        return;
                    }

                    var ids = selectedRecord.Select(x => x.Id).ToList();

                    var confirmed = await UiMessageService.Confirm(@L["DeleteSelectedRecords", ids.Count]);
                    if (!confirmed)
                    {
                        return;
                    }

                    await AppointmentTypesAppService.DeleteByIdsAsync(ids);
                    break;
                }
                case "Excel Export":
                {
                    var exportProperties = new ExcelExportProperties
                    {
                        IncludeTemplateColumn = true
                    };
                    await Grid.ExportToExcelAsync(exportProperties);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            throw new UserFriendlyException(e.Message);
        }
        finally
        {
            Refresh();
        }
    }
    
    private void Refresh()
    {
        Grid.Refresh();
    }

    #endregion

    #region DialogControls

    private void OkClick()
    {
        DeleteConfirmDialog.ShowAsync();
    }

    private void CancelClick()
    {
        DeleteConfirmDialog.HideAsync();
    }

    private void OpenEditDialog(AppointmentTypeDto input)
    {
        EditingType = ObjectMapper.Map<AppointmentTypeDto, AppointmentTypeUpdateDto>(input);
        EditingTypeId = input.Id;
        IsEditDialogVisible = true;
    }

    private void CloseEditTypeModal()
    {
        EditingType = new AppointmentTypeUpdateDto();
        EditingTypeId = Guid.Empty;
        IsEditDialogVisible = false;
    }

    private Task OpenCreateTypeModalAsync()
    {
        IsCreateDialogVisible = true;
        return Task.CompletedTask;
    }

    private void CloseCreateTypeModal()
    {
        NewType = new AppointmentTypeCreateDto();
        IsCreateDialogVisible = false;
    }

    #endregion

    #region APICalls

    private async Task DeleteTypeAsync(AppointmentTypeDto input)
    {
        try
        {
            var confirmed = await UiMessageService.Confirm(@L["DeleteConfirmationMessage"]);
            if (!confirmed)
            {
                return;
            }

            await AppointmentTypesAppService.DeleteAsync(input.Id);
        }
        catch (Exception e)
        {
            throw new UserFriendlyException(e.Message);
        }
        finally
        {
            Refresh();
        }
    }

    private async Task CreateTypeAsync()
    {
        try
        {
            await AppointmentTypesAppService.CreateAsync(NewType);
            await OpenToast(content: "CreatedSuccess", title: "AppointmentTypeCreated", css: "e-toast-success");
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            CloseCreateTypeModal();
            Refresh();
        }
    }

    private async Task UpdateTypeAsync()
    {
        try
        {
            await AppointmentTypesAppService.UpdateAsync(EditingTypeId, EditingType);
            await OpenToast(content: "UpdatedSuccess", title: "AppointmentTypeUpdated", css: "e-toast-success");
            CloseEditTypeModal();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
        finally
        {
            CloseEditTypeModal();
            Refresh();
        }
    }

    private async Task DownloadAsExcelAsync()
    {
        var token = (await AppointmentTypesAppService.GetDownloadTokenAsync()).Token;
        var remoteService =
            await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("HealthCare") ??
            await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");
        var culture = CultureInfo.CurrentUICulture.Name ?? CultureInfo.CurrentCulture.Name;
        if (!culture.IsNullOrEmpty())
        {
            culture = "&culture=" + culture;
        }

        await RemoteServiceConfigurationProvider.GetConfigurationOrDefaultOrNullAsync("Default");
        NavigationManager.NavigateTo(
            $"{remoteService?.BaseUrl.EnsureEndsWith('/') ?? string.Empty}api/app/appointment-types/as-excel-file?DownloadToken={token}&FilterText={HttpUtility.UrlEncode(Filter.FilterText)}{culture}&Name={HttpUtility.UrlEncode(Filter.Name)}",
            forceLoad: true);
    }

    #endregion
    
    public static void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<AppointmentTypeDto> args)
    {
        if (args.Column.Field == "Name")
        {
            args.Cell.Value = args.Data.Name;
        }
    }

    private async Task ShowOnClick()
    {
        await ToastObj.ShowAsync();
    }

    private async Task OpenToast(string content, string title, string css)
    {
        ToastContent = @L[content];
        ToastTitle = @L[title];
        ToastCssClass = css;
        StateHasChanged();
        await ShowOnClick();
    }
}