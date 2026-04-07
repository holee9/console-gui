using CommunityToolkit.Mvvm.Messaging.Messages;

// @MX:NOTE CommunityToolkit.Mvvm messenger pattern for loosely-coupled ViewModel-to-ViewModel communication
namespace HnVue.UI.Contracts.Events;

/// <summary>Message broadcast when the user selects a patient.</summary>
public sealed class PatientSelectedMessage : ValueChangedMessage<string>
{
    /// <summary>Initialises a new instance of <see cref="PatientSelectedMessage"/>.</summary>
    /// <param name="patientId">The selected patient's identifier.</param>
    public PatientSelectedMessage(string patientId) : base(patientId) { }
}
