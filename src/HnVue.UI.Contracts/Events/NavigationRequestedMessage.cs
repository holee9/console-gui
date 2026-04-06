using CommunityToolkit.Mvvm.Messaging.Messages;
using HnVue.UI.Contracts.Navigation;

namespace HnVue.UI.Contracts.Events;

/// <summary>Message requesting shell navigation to a target view.</summary>
public sealed class NavigationRequestedMessage : ValueChangedMessage<NavigationToken>
{
    /// <summary>Initialises a new instance of <see cref="NavigationRequestedMessage"/>.</summary>
    /// <param name="token">The navigation target.</param>
    /// <param name="parameter">An optional parameter passed to the target ViewModel.</param>
    public NavigationRequestedMessage(NavigationToken token, object? parameter = null) : base(token)
    {
        Parameter = parameter;
    }

    /// <summary>Gets the optional navigation parameter.</summary>
    public object? Parameter { get; }
}
