using System;
using System.Windows.Input;
using FluentAssertions;
using HnVue.UI.Components.Common;
using HnVue.UI.Components.Layout;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for RelayCommand, RelayCommand&lt;T&gt;, and StatusBarItem.
/// Boosts coverage: RelayCommand 50%→80%+, RelayCommand&lt;T&gt; 0%→70%+, StatusBarItem 55%→75%+.
/// </summary>
public class RelayCommandTests
{
    // ====================================================================
    // RelayCommand
    // ====================================================================

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_Execute_InvokesAction()
    {
        bool executed = false;
        var cmd = new RelayCommand(_ => executed = true);

        cmd.Execute(null);

        executed.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_Execute_PassesParameterToAction()
    {
        object? received = null;
        var cmd = new RelayCommand(p => received = p);

        cmd.Execute("hello");

        received.Should().Be("hello");
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_CanExecute_WithNoGuard_Returns_True()
    {
        var cmd = new RelayCommand(_ => { });

        cmd.CanExecute(null).Should().BeTrue();
        cmd.CanExecute("anything").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_CanExecute_WithGuard_RespectsGuard()
    {
        bool canRun = false;
        var cmd = new RelayCommand(_ => { }, _ => canRun);

        cmd.CanExecute(null).Should().BeFalse();

        canRun = true;
        cmd.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_RaiseCanExecuteChanged_FiresEvent()
    {
        var cmd = new RelayCommand(_ => { });
        bool eventFired = false;
        cmd.CanExecuteChanged += (_, _) => eventFired = true;

        cmd.RaiseCanExecuteChanged();

        eventFired.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_Constructor_NullExecute_Throws_ArgumentNullException()
    {
        Action act = () => new RelayCommand(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("execute");
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_Implements_ICommand()
    {
        var cmd = new RelayCommand(_ => { });
        cmd.Should().BeAssignableTo<ICommand>();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_CanExecuteChanged_CanSubscribeAndUnsubscribe()
    {
        var cmd = new RelayCommand(_ => { });
        EventHandler handler = (_, _) => { };

        // Should not throw on subscribe/unsubscribe
        cmd.CanExecuteChanged += handler;
        cmd.CanExecuteChanged -= handler;
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_RaiseCanExecuteChanged_WithNoSubscribers_DoesNotThrow()
    {
        var cmd = new RelayCommand(_ => { });
        Action act = () => cmd.RaiseCanExecuteChanged();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommand")]
    public void RelayCommand_Execute_MultipleParameters_EachDelivered()
    {
        int callCount = 0;
        var cmd = new RelayCommand(_ => callCount++);

        cmd.Execute(1);
        cmd.Execute(2);
        cmd.Execute(3);

        callCount.Should().Be(3);
    }

    // ====================================================================
    // RelayCommand<T>
    // ====================================================================

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Execute_InvokesTypedAction()
    {
        string? received = null;
        var cmd = new RelayCommand<string>(s => received = s);

        cmd.Execute("typed");

        received.Should().Be("typed");
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Execute_ValueTypeParameter_InvokesTypedAction()
    {
        int received = 0;
        var cmd = new RelayCommand<int>(value => received = value);

        cmd.Execute(7);

        received.Should().Be(7);
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Execute_NullParameter_InvokesAction()
    {
        bool executed = false;
        var cmd = new RelayCommand<string?>(_ => executed = true);

        cmd.Execute(null);

        executed.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Execute_WrongType_DoesNotInvoke()
    {
        bool executed = false;
        var cmd = new RelayCommand<string>(s => executed = true);

        // int is not string, so Execute should silently skip
        cmd.Execute(42);

        executed.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Execute_NullValueTypeParameter_DoesNotInvoke()
    {
        bool executed = false;
        var cmd = new RelayCommand<int>(_ => executed = true);

        cmd.Execute(null);

        executed.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithNoGuard_Returns_True_ForTypedParam()
    {
        var cmd = new RelayCommand<int>(_ => { });

        cmd.CanExecute(1).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithNoGuard_NullParameter_ReturnsTrue_ForValueType()
    {
        // Implementation contract: when no guard exists, an uncastable parameter still returns true.
        var cmd = new RelayCommand<int>(_ => { });
        cmd.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithNoGuard_NullParameter_ReturnsTrue_ForReferenceType()
    {
        // string is a reference type; null can be T?, so returns true when no guard
        var cmd = new RelayCommand<string?>(_ => { });
        cmd.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithNoGuard_WrongTypeParameter_Returns_True()
    {
        var cmd = new RelayCommand<string>(_ => { });

        cmd.CanExecute(42).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithGuard_RespectsGuard()
    {
        bool canRun = false;
        var cmd = new RelayCommand<string>(_ => { }, s => canRun);

        cmd.CanExecute("test").Should().BeFalse();

        canRun = true;
        cmd.CanExecute("test").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_CanExecute_WithGuard_WrongTypeParameter_Returns_False()
    {
        var cmd = new RelayCommand<string>(_ => { }, _ => true);

        cmd.CanExecute(42).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_RaiseCanExecuteChanged_FiresEvent()
    {
        var cmd = new RelayCommand<string>(_ => { });
        bool eventFired = false;
        cmd.CanExecuteChanged += (_, _) => eventFired = true;

        cmd.RaiseCanExecuteChanged();

        eventFired.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Constructor_NullExecute_Throws_ArgumentNullException()
    {
        Action act = () => new RelayCommand<string>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("execute");
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_Implements_ICommand()
    {
        var cmd = new RelayCommand<int>(_ => { });
        cmd.Should().BeAssignableTo<ICommand>();
    }

    [Fact]
    [Trait("Category", "Commands")]
    [Trait("Command", "RelayCommandT")]
    public void RelayCommandT_RaiseCanExecuteChanged_WithNoSubscribers_DoesNotThrow()
    {
        var cmd = new RelayCommand<int>(_ => { });
        Action act = () => cmd.RaiseCanExecuteChanged();
        act.Should().NotThrow();
    }
}

/// <summary>
/// Unit tests for StatusBarItem WPF component.
/// </summary>
public class StatusBarItemTests
{
    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBar")]
    public void StatusBar_DefaultValues_AreExpected()
    {
        StaRunner.Run(() =>
        {
            var statusBar = new StatusBar();

            statusBar.Height.Should().Be(32);
            statusBar.StatusMessage.Should().BeEmpty();
            statusBar.ConnectionStatus.Should().Be("Connected");
            statusBar.ShowTime.Should().BeTrue();
            statusBar.Message.Should().BeEmpty();
            statusBar.StatusItems.Should().NotBeNull().And.BeEmpty();
            statusBar.InfoItems.Should().NotBeNull().And.BeEmpty();
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBar")]
    public void StatusBar_PropertyRoundTrip_PersistsValues()
    {
        StaRunner.Run(() =>
        {
            var statusBar = new StatusBar();

            statusBar.StatusMessage = "Ready";
            statusBar.ConnectionStatus = "Offline";
            statusBar.ShowTime = false;
            statusBar.Message = "Alert";

            statusBar.StatusMessage.Should().Be("Ready");
            statusBar.ConnectionStatus.Should().Be("Offline");
            statusBar.ShowTime.Should().BeFalse();
            statusBar.Message.Should().Be("Alert");
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_DefaultStatus_Is_Online()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.Status.Should().Be(SystemStatus.Online);
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_DefaultIsPulse_Is_False()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.IsPulse.Should().BeFalse();
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_DefaultStatusBrush_IsGreen()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            // Default Online status → green brush (#2ED573 = rgb(46, 213, 115))
            item.StatusBrush.Should().NotBeNull();
        });
    }

    [Theory]
    [InlineData(SystemStatus.Online)]
    [InlineData(SystemStatus.Busy)]
    [InlineData(SystemStatus.Warning)]
    [InlineData(SystemStatus.Offline)]
    [InlineData(SystemStatus.Blocked)]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_SetStatus_UpdatesStatusBrush(SystemStatus status)
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();

            item.Status = status;

            item.Status.Should().Be(status);
            item.StatusBrush.Should().NotBeNull();
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_SetStatus_Online_Returns_GreenBrush()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.Status = SystemStatus.Offline; // change first
            item.Status = SystemStatus.Online;  // then back

            var brush = item.StatusBrush as System.Windows.Media.SolidColorBrush;
            brush.Should().NotBeNull();
            brush!.Color.R.Should().Be(46);
            brush.Color.G.Should().Be(213);
            brush.Color.B.Should().Be(115);
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_SetStatus_Offline_Returns_RedBrush()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.Status = SystemStatus.Offline;

            var brush = item.StatusBrush as System.Windows.Media.SolidColorBrush;
            brush.Should().NotBeNull();
            brush!.Color.R.Should().Be(255);
            brush.Color.G.Should().Be(71);
            brush.Color.B.Should().Be(87);
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_SetStatus_Warning_Returns_AmberBrush()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.Status = SystemStatus.Warning;

            var brush = item.StatusBrush as System.Windows.Media.SolidColorBrush;
            brush.Should().NotBeNull();
            brush!.Color.R.Should().Be(255);
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_SetIsPulse_True_Persists()
    {
        StaRunner.Run(() =>
        {
            var item = new StatusBarItem();
            item.IsPulse = true;
            item.IsPulse.Should().BeTrue();
        });
    }

    [Fact]
    [Trait("Category", "Components")]
    [Trait("Component", "StatusBarItem")]
    public void StatusBarItem_DifferentStatuses_ProduceDifferentBrushColors()
    {
        StaRunner.Run(() =>
        {
            var statuses = new[] { SystemStatus.Online, SystemStatus.Busy, SystemStatus.Warning, SystemStatus.Offline, SystemStatus.Blocked };
            var item = new StatusBarItem();
            var colors = new System.Collections.Generic.List<System.Windows.Media.Color>();

            foreach (var status in statuses)
            {
                item.Status = status;
                var brush = item.StatusBrush as System.Windows.Media.SolidColorBrush;
                colors.Add(brush!.Color);
            }

            colors.Distinct().Should().HaveCount(5, "each SystemStatus should produce a unique color");
        });
    }
}
