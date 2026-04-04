using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the patient list and search screen.
/// Provides patient search, selection, and registration entry points.
/// </summary>
public sealed partial class PatientListViewModel : ObservableObject
{
    private readonly IPatientService _patientService;

    /// <summary>Initialises a new instance of <see cref="PatientListViewModel"/>.</summary>
    /// <param name="patientService">Service used to search and manage patient records.</param>
    public PatientListViewModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    /// <summary>Gets or sets the free-text query used to filter patients.</summary>
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    /// <summary>Gets the collection of patient records matching the current search query.</summary>
    public ObservableCollection<PatientRecord> Patients { get; } = new();

    /// <summary>Gets or sets the patient record currently selected in the list.</summary>
    [ObservableProperty]
    private PatientRecord? _selectedPatient;

    /// <summary>Gets or sets a value indicating whether a search or load operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Raised when the user selects a patient record.</summary>
    public event EventHandler<PatientRecord>? PatientSelected;

    /// <summary>Raised when the user requests the patient registration form.</summary>
    public event EventHandler? RegisterPatientRequested;

    /// <summary>Searches for patients whose name or ID matches <see cref="SearchQuery"/>.</summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _patientService.SearchAsync(SearchQuery);
            Patients.Clear();

            if (result.IsSuccess)
            {
                foreach (var patient in result.Value)
                {
                    Patients.Add(patient);
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Opens the patient registration dialog.</summary>
    [RelayCommand]
    private void RegisterPatient()
    {
        RegisterPatientRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raises <see cref="PatientSelected"/> with the given patient.</summary>
    /// <param name="patient">The patient record chosen by the user.</param>
    [RelayCommand]
    private void SelectPatient(PatientRecord patient)
    {
        SelectedPatient = patient;
        PatientSelected?.Invoke(this, patient);
    }
}
