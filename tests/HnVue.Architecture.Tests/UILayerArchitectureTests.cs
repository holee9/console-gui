using System.IO;
using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HnVue.Architecture.Tests;

/// <summary>
/// Architecture boundary tests for the HnVue UI layer.
///
/// Three test strategies are used:
/// 1. NetArchTest assembly scan — for HnVue.UI.Contracts and HnVue.UI.ViewModels
///    (these compile cleanly and can be loaded as assemblies).
/// 2. csproj XML parse — for HnVue.UI, which contains pre-existing XAML compilation
///    errors. We validate its ProjectReference declarations directly instead.
/// 3. Sanity check — verifies assemblies are reachable via ProjectReference.
///
/// Violations of these tests indicate an unintended architectural coupling.
/// Never suppress these tests; fix the coupling instead.
/// </summary>
public class UILayerArchitectureTests
{
    // Assemblies loaded via ProjectReference (compile cleanly).
    private static readonly Assembly ContractsAssembly =
        typeof(global::HnVue.UI.Contracts.AssemblyMarker).Assembly;

    private static readonly Assembly ViewModelsAssembly =
        typeof(global::HnVue.UI.ViewModels.AssemblyMarker).Assembly;

    // Paths to csproj files — resolved relative to this test assembly's location.
    // tests/HnVue.Architecture.Tests/bin/Release/net8.0-windows/ -> up 5 levels to repo root.
    private static readonly string UICsprojPath = ResolveCsprojPath("HnVue.UI", "HnVue.UI.csproj");
    private static readonly string ContractsCsprojPath = ResolveCsprojPath("HnVue.UI.Contracts", "HnVue.UI.Contracts.csproj");

    // Business module assembly name prefixes forbidden from UI layer references.
    private static readonly string[] ForbiddenBusinessAssemblyNames =
    [
        "HnVue.Data",
        "HnVue.Security",
        "HnVue.Workflow",
        "HnVue.Imaging",
        "HnVue.Dicom",
        "HnVue.Dose",
        "HnVue.PatientManagement",
        "HnVue.Incident",
        "HnVue.Update",
        "HnVue.SystemAdmin",
        "HnVue.CDBurning",
    ];

    // -------------------------------------------------------------------------
    // Test 1: HnVue.UI.csproj must not reference business modules
    // Validated via ProjectReference XML parsing (avoids dependency on XAML compile).
    // -------------------------------------------------------------------------

    /// <summary>
    /// HnVue.UI.csproj must not declare a ProjectReference to any business module.
    /// Allowed ProjectReferences: HnVue.Common, HnVue.UI.Contracts.
    /// Forbidden: all domain/infrastructure modules in ForbiddenBusinessAssemblyNames.
    /// </summary>
    [Fact]
    public void UI_Should_Not_Depend_On_Business_Modules()
    {
        UICsprojPath.Should().NotBeNullOrEmpty(
            because: "HnVue.UI.csproj must be discoverable from the test output directory");

        File.Exists(UICsprojPath).Should().BeTrue(
            because: $"Expected HnVue.UI.csproj at '{UICsprojPath}'");

        var doc = XDocument.Load(UICsprojPath);
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Collect all ProjectReference Include paths
        var projectReferences = doc.Descendants(ns + "ProjectReference")
            .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        foreach (var forbidden in ForbiddenBusinessAssemblyNames)
        {
            var violation = projectReferences.FirstOrDefault(
                r => r.Contains(forbidden, StringComparison.OrdinalIgnoreCase));

            violation.Should().BeNull(
                because: $"HnVue.UI.csproj must not reference '{forbidden}'. " +
                         "Found ProjectReference: '{0}'. " +
                         "Use an interface from HnVue.UI.Contracts instead and " +
                         "register the implementation in HnVue.App (DI composition root).");
        }
    }

    // -------------------------------------------------------------------------
    // Test 2: HnVue.UI.ViewModels assembly must not depend on business modules
    // -------------------------------------------------------------------------

    /// <summary>
    /// HnVue.UI.ViewModels must only depend on HnVue.UI.Contracts and HnVue.Common.
    /// It must not reference any business module directly.
    /// </summary>
    [Fact]
    public void ViewModels_Should_Only_Depend_On_Contracts_And_Common()
    {
        var types = Types.InAssembly(ViewModelsAssembly);

        foreach (var forbiddenNs in ForbiddenBusinessAssemblyNames)
        {
            var result = types
                .ShouldNot()
                .HaveDependencyOn(forbiddenNs)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"HnVue.UI.ViewModels must not depend on '{forbiddenNs}'. " +
                         "ViewModels communicate with business modules exclusively through " +
                         "interfaces declared in HnVue.UI.Contracts.");
        }
    }

    // -------------------------------------------------------------------------
    // Test 3: HnVue.UI.Contracts assembly must have no implementation dependencies
    // -------------------------------------------------------------------------

    /// <summary>
    /// HnVue.UI.Contracts must not depend on any business module or the UI implementation.
    /// It defines pure interface contracts and may only reference HnVue.Common models.
    /// </summary>
    [Fact]
    public void Contracts_Should_Have_No_Implementation_Dependencies()
    {
        var types = Types.InAssembly(ContractsAssembly);

        foreach (var forbiddenNs in ForbiddenBusinessAssemblyNames)
        {
            var result = types
                .ShouldNot()
                .HaveDependencyOn(forbiddenNs)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"HnVue.UI.Contracts must not depend on '{forbiddenNs}'. " +
                         "Contracts defines interfaces only; concrete module namespaces " +
                         "must never appear here.");
        }

        // Contracts must not have a ProjectReference to HnVue.UI (the view implementation).
        // We validate this via csproj XML parsing rather than assembly scanning,
        // because HnVue.UI.Contracts' own namespace starts with "HnVue.UI" which would
        // cause false positives when using HaveDependencyOn("HnVue.UI").
        var contractsCsprojPath = ContractsCsprojPath;
        if (!string.IsNullOrEmpty(contractsCsprojPath) && File.Exists(contractsCsprojPath))
        {
            var doc = XDocument.Load(contractsCsprojPath);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var refs = doc.Descendants(ns + "ProjectReference")
                .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
                .ToList();

            var uiImplRef = refs.FirstOrDefault(
                r => r.Contains("HnVue.UI.csproj", StringComparison.OrdinalIgnoreCase) &&
                     !r.Contains("HnVue.UI.Contracts", StringComparison.OrdinalIgnoreCase) &&
                     !r.Contains("HnVue.UI.ViewModels", StringComparison.OrdinalIgnoreCase));

            uiImplRef.Should().BeNull(
                because: "HnVue.UI.Contracts.csproj must not have a ProjectReference to HnVue.UI. " +
                         "Contracts is an abstraction layer and must not depend on the view implementation.");
        }
    }

    // -------------------------------------------------------------------------
    // Test 4: Sanity checks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that the test infrastructure itself is set up correctly.
    /// If this test fails, a ProjectReference or AssemblyMarker is missing.
    /// </summary>
    [Fact]
    public void Architecture_Test_Infrastructure_Is_Correctly_Configured()
    {
        ContractsAssembly.Should().NotBeNull(
            "HnVue.UI.Contracts assembly must be reachable via ProjectReference");

        ViewModelsAssembly.Should().NotBeNull(
            "HnVue.UI.ViewModels assembly must be reachable via ProjectReference");

        File.Exists(UICsprojPath).Should().BeTrue(
            $"HnVue.UI.csproj must be resolvable for XML-based architecture checks. " +
            $"Resolved path: '{UICsprojPath}'");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string ResolveCsprojPath(string projectFolder, string csprojFile)
    {
        // Walk up from the test output directory to find the repository root.
        // Expected depth from assembly location to repo root:
        //   net8.0-windows -> Release -> bin -> HnVue.Architecture.Tests -> tests -> repo_root
        //   = 5 levels up, search up to 8 levels for safety.
        var dir = new DirectoryInfo(
            Path.GetDirectoryName(typeof(UILayerArchitectureTests).Assembly.Location)!);

        for (int i = 0; i < 8; i++)
        {
            if (dir is null)
                break;

            var candidate = Path.Combine(
                dir.FullName, "src", projectFolder, csprojFile);

            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return string.Empty;
    }
}
