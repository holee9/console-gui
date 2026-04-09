using System;
using FluentAssertions;
using HnVue.UI.Services;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for ThemeRollbackService.
/// Tests static property and method behavior without a live WPF Application instance.
/// </summary>
public class ThemeRollbackServiceTests
{
    // ====================================================================
    // CanRollback — static property
    // ====================================================================

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void CanRollback_WhenNoPreviousThemeFile_Returns_False()
    {
        // The test environment has no WPF Application and no Themes/HnVueTheme.previous.xaml
        // resource, so CanRollback should be false (ResourceDictionary source load fails).
        var result = ThemeRollbackService.CanRollback;
        result.Should().BeFalse();
    }

    // ====================================================================
    // RollbackToPrevious — static method
    // ====================================================================

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void RollbackToPrevious_WhenCannotRollback_Returns_False()
    {
        // Without a previous theme file, CanRollback == false.
        // RollbackToPrevious should short-circuit and return false without
        // attempting to access Application.Current (which is null in tests).
        var result = ThemeRollbackService.RollbackToPrevious();
        result.Should().BeFalse();
    }

    // ====================================================================
    // SaveCurrentAsSnapshot — static method (placeholder)
    // ====================================================================

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void SaveCurrentAsSnapshot_DoesNotThrow()
    {
        // SaveCurrentAsSnapshot is a placeholder for the deployment pipeline;
        // it must not throw even without any WPF infrastructure.
        Action act = () => ThemeRollbackService.SaveCurrentAsSnapshot();
        act.Should().NotThrow();
    }

    // ====================================================================
    // Type and contract validation
    // ====================================================================

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_IsPublicClass()
    {
        typeof(ThemeRollbackService).IsPublic.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_CanRollback_IsStaticProperty()
    {
        var prop = typeof(ThemeRollbackService).GetProperty(nameof(ThemeRollbackService.CanRollback));
        prop.Should().NotBeNull();
        prop!.GetMethod.Should().NotBeNull();
        prop.GetMethod!.IsStatic.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_RollbackToPrevious_IsStaticMethod()
    {
        var method = typeof(ThemeRollbackService).GetMethod(nameof(ThemeRollbackService.RollbackToPrevious));
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_RollbackToPrevious_Returns_Bool()
    {
        var method = typeof(ThemeRollbackService).GetMethod(nameof(ThemeRollbackService.RollbackToPrevious));
        method!.ReturnType.Should().Be(typeof(bool));
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_SaveCurrentAsSnapshot_IsStaticMethod()
    {
        var method = typeof(ThemeRollbackService).GetMethod(nameof(ThemeRollbackService.SaveCurrentAsSnapshot));
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_SaveCurrentAsSnapshot_Returns_Void()
    {
        var method = typeof(ThemeRollbackService).GetMethod(nameof(ThemeRollbackService.SaveCurrentAsSnapshot));
        method!.ReturnType.Should().Be(typeof(void));
    }
}
