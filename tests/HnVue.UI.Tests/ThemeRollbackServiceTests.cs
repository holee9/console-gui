using System;
using System.Linq;
using System.Reflection;
using System.Windows;
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
    private const string CurrentThemePath = "/HnVue.UI;component/Themes/HnVueTheme.xaml";
    private const string PreviousThemePath = "/HnVue.UI;component/Themes/HnVueTheme.previous.xaml";

    // ====================================================================
    // CanRollback — static property
    // ====================================================================

    [StaFact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void CanRollback_WhenPreviousThemeResourceExists_Returns_True()
    {
        var result = ThemeRollbackService.CanRollback;
        result.Should().BeTrue();
    }

    // ====================================================================
    // RollbackToPrevious — static method
    // ====================================================================

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void RollbackToPrevious_WhenApplicationIsNull_Returns_False()
    {
        var result = ThemeRollbackService.RollbackToPrevious();
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Services")]
    [Trait("Service", "ThemeRollbackService")]
    public void ThemeRollbackService_WhenPreviousThemeExists_CanRollbackAndReplaceCurrentTheme()
    {
        StaRunner.Run(() =>
        {
            using var scope = ThemeRollbackTestScope.Create();
            var app = scope.Application;

            ThemeRollbackService.CanRollback.Should().BeTrue();

            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(CurrentThemePath, UriKind.Relative)
            });

            var result = ThemeRollbackService.RollbackToPrevious();

            result.Should().BeTrue();
            app.Resources.MergedDictionaries
                .Select(dict => dict.Source?.OriginalString)
                .Should()
                .Contain(PreviousThemePath)
                .And
                .NotContain(CurrentThemePath);
        });
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

    private sealed class ThemeRollbackTestScope : IDisposable
    {
        private static readonly FieldInfo AppInstanceField =
            typeof(Application).GetField("_appInstance", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly FieldInfo AppCreatedField =
            typeof(Application).GetField("_appCreatedInThisAppDomain", BindingFlags.NonPublic | BindingFlags.Static)!;

        private readonly System.Collections.Generic.List<ResourceDictionary> _originalMergedDictionaries;
        private readonly bool _createdApplication;

        private ThemeRollbackTestScope(Application application, bool createdApplication)
        {
            Application = application;
            _originalMergedDictionaries = application.Resources.MergedDictionaries.ToList();
            _createdApplication = createdApplication;
        }

        public Application Application { get; }

        public static ThemeRollbackTestScope Create()
        {
            var createdApplication = Application.Current is null;
            var application = Application.Current ?? new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown,
            };

            return new ThemeRollbackTestScope(application, createdApplication);
        }

        public void Dispose()
        {
            Application.Resources.MergedDictionaries.Clear();
            foreach (var dict in _originalMergedDictionaries)
            {
                Application.Resources.MergedDictionaries.Add(dict);
            }

            if (_createdApplication)
            {
                Application.Shutdown();
                AppInstanceField.SetValue(null, null);
                AppCreatedField.SetValue(null, false);
            }
        }
    }
}
