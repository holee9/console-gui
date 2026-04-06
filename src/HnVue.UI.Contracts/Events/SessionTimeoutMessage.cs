using CommunityToolkit.Mvvm.Messaging.Messages;

namespace HnVue.UI.Contracts.Events;

/// <summary>Message broadcast when session timeout is imminent.</summary>
public sealed class SessionTimeoutMessage : ValueChangedMessage<int>
{
    /// <summary>Initialises a new instance of <see cref="SessionTimeoutMessage"/>.</summary>
    /// <param name="secondsRemaining">Seconds remaining before automatic logout.</param>
    public SessionTimeoutMessage(int secondsRemaining) : base(secondsRemaining) { }
}
