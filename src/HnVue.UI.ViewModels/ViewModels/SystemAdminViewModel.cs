using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the system administration screen.
/// Loading and saving system settings is restricted to users with
/// <see cref="UserRole.Admin"/> or <see cref="UserRole.Service"/> role.
/// </summary>
public sealed partial class SystemAdminViewModel : ObservableObject, ISystemAdminViewModel
{
    private readonly ISystemAdminService _systemAdminService;
    private readonly ISecurityContext _securityContext;

    /// <summary>Initialises a new instance of <see cref="SystemAdminViewModel"/>.</summary>
    /// <param name="systemAdminService">Service providing settings management operations.</param>
    /// <param name="securityContext">Provides the current user's role.</param>
    public SystemAdminViewModel(ISystemAdminService systemAdminService, ISecurityContext securityContext)
    {
        _systemAdminService = systemAdminService;
        _securityContext = securityContext;
    }

    /// <summary>
    /// Implements <see cref="IViewModelBase.IsLoading"/> by mapping to <see cref="IsBusy"/>.
    /// </summary>
    bool IViewModelBase.IsLoading => IsBusy;

    // Explicit ISystemAdminViewModel ICommand bridge — see LoginViewModel for rationale.
    ICommand ISystemAdminViewModel.LoadSettingsCommand => LoadSettingsCommand;
    ICommand ISystemAdminViewModel.SaveSettingsCommand => SaveSettingsCommand;

    /// <summary>Gets or sets the system settings loaded from the service.</summary>
    [ObservableProperty]
    private SystemSettings _settings = new();

    /// <summary>
    /// Gets a value indicating whether the current user has admin or service privileges
    /// and is therefore permitted to view and modify system settings.
    /// </summary>
    public bool IsAdminUser =>
        _securityContext.HasRole(UserRole.Admin) || _securityContext.HasRole(UserRole.Service);

    /// <summary>Gets or sets a value indicating whether a settings operation is in progress.</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Gets or sets a success or status message, or <see langword="null"/> when idle.</summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Loads the current system settings from the service.</summary>
    [RelayCommand(CanExecute = nameof(IsAdminUser))]
    private async Task LoadSettingsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var result = await _systemAdminService.GetSettingsAsync();
            if (result.IsSuccess)
            {
                Settings = result.Value;
                StatusMessage = "Settings loaded successfully.";
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Validates and persists the current settings to the service.</summary>
    [RelayCommand(CanExecute = nameof(IsAdminUser))]
    private async Task SaveSettingsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var result = await _systemAdminService.UpdateSettingsAsync(Settings);
            if (result.IsSuccess)
            {
                StatusMessage = "Settings saved successfully.";
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
