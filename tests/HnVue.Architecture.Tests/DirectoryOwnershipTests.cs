using System.IO;
using FluentAssertions;
using Xunit;

namespace HnVue.Architecture.Tests;

/// <summary>
/// Directory ownership architecture tests enforcing role-matrix v2.0 boundaries.
///
/// These tests prevent cross-team file placement violations:
/// 1. DesignTime/ files must not use ViewModel namespaces (Coordinator invasion prevention)
/// 2. DesignTime/ files must only exist in the UI project
/// 3. ViewModel files must not reference XAML types (View-ViewModel separation)
///
/// Triggered by S08-R1 incident: Coordinator created files in DesignTime/ directory
/// which is Design team's exclusive ownership per role-matrix.md.
/// </summary>
public class DirectoryOwnershipTests
{
    private static readonly string? RepoRoot = ResolveRepoRoot();

    // XAML-related using patterns that ViewModels must not import.
    // System.Windows.Input is ALLOWED (ICommand is standard MVVM).
    // System.Windows.Media.Imaging is ALLOWED (image display ViewModels).
    // Prohibited: Controls, Shapes, Data binding, FrameworkElement, Xaml reader.
    private static readonly string[] ForbiddenXamlUsingPatterns =
    [
        "using System.Windows.Controls",
        "using System.Windows.Shapes",
        "using System.Windows.Data",
        "using System.Windows.FrameworkElement",
        "using System.Windows.DependencyObject",
        "using System.Xaml",
        "using PresentationFramework",
        "using System.Windows.Threading",
    ];

    // -------------------------------------------------------------------------
    // Test 1: DesignTime files must not use ViewModel namespaces
    // Prevents Coordinator from placing ViewModel files in Design's directory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Files in src/HnVue.UI/DesignTime/ must use the HnVue.UI.DesignTime namespace,
    /// not HnVue.UI.ViewModels. This prevents Coordinator from accidentally placing
    /// ViewModel implementations in the Design team's directory.
    /// </summary>
    [Fact]
    public void DesignTime_Files_Must_Not_Use_ViewModel_Namespaces()
    {
        RepoRoot.Should().NotBeNullOrEmpty("Repository root must be discoverable");

        var designTimeDir = Path.Combine(RepoRoot!, "src", "HnVue.UI", "DesignTime");

        if (!Directory.Exists(designTimeDir))
        {
            // No DesignTime directory — pass vacuously
            return;
        }

        var violations = new List<string>();

        foreach (var file in Directory.GetFiles(designTimeDir, "*.cs"))
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);

            // Check for ViewModel namespace usage
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("namespace ", StringComparison.Ordinal)
                    && trimmed.Contains("HnVue.UI.ViewModels", StringComparison.Ordinal))
                {
                    violations.Add(
                        $"{fileName} uses namespace '{trimmed}' — " +
                        "DesignTime files must use 'HnVue.UI.DesignTime' namespace");
                }

                // Also check using directives for ViewModel namespace
                if (trimmed.StartsWith("using ", StringComparison.Ordinal)
                    && trimmed.Contains("HnVue.UI.ViewModels", StringComparison.Ordinal)
                    && !trimmed.Contains("HnVue.UI.ViewModels.Models", StringComparison.Ordinal))
                {
                    violations.Add(
                        $"{fileName} imports '{trimmed}' — " +
                        "DesignTime files should not depend on ViewModel implementation types");
                }
            }
        }

        violations.Should().BeEmpty(
            because: "DesignTime/ files belong to Design team and must use HnVue.UI.DesignTime " +
                     "namespace. Coordinator must place ViewModels in HnVue.UI.ViewModels project. " +
                     "Violations:\n" + string.Join("\n", violations));
    }

    // -------------------------------------------------------------------------
    // Test 2: DesignTime directory must only exist in UI project
    // Prevents DesignTime files from leaking into other projects
    // -------------------------------------------------------------------------

    /// <summary>
    /// The DesignTime/ directory must only exist within src/HnVue.UI/.
    /// It must not exist in src/HnVue.UI.ViewModels/, src/HnVue.UI.Contracts/,
    /// or any other project. DesignTime mock data is a UI-layer concern.
    /// </summary>
    [Fact]
    public void DesignTime_Directory_Must_Only_Exist_In_UI_Project()
    {
        RepoRoot.Should().NotBeNullOrEmpty("Repository root must be discoverable");

        var srcDir = Path.Combine(RepoRoot!, "src");
        Directory.Exists(srcDir).Should().BeTrue("src/ directory must exist");

        // Only HnVue.UI is allowed to have a DesignTime directory
        var allowedOwner = "HnVue.UI";

        var violations = new List<string>();

        foreach (var projectDir in Directory.GetDirectories(srcDir))
        {
            var projectName = Path.GetFileName(projectDir);

            // Skip the allowed owner
            if (projectName.Equals(allowedOwner, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var designTimeDir = Path.Combine(projectDir, "DesignTime");
            if (Directory.Exists(designTimeDir))
            {
                violations.Add(
                    $"DesignTime directory found in '{projectName}' — " +
                    "only HnVue.UI is allowed to have a DesignTime/ directory");
            }
        }

        violations.Should().BeEmpty(
            because: "DesignTime/ directory is Design team property and must only exist " +
                     "in HnVue.UI project. Other projects should not contain mock data. " +
                     "Violations:\n" + string.Join("\n", violations));
    }

    // -------------------------------------------------------------------------
    // Test 3: ViewModels must not reference XAML types
    // Enforces View-ViewModel separation
    // -------------------------------------------------------------------------

    /// <summary>
    /// ViewModel .cs files must not import WPF UI-specific namespaces.
    /// Allowed: System.Windows.Input (ICommand), System.Windows.Media.Imaging (image display).
    /// Prohibited: Controls, Shapes, Data binding, FrameworkElement, Xaml parsing, Dispatcher.
    /// Uses file-based scanning because NetArchTest granularity is too coarse for
    /// System.Windows which includes ICommand (legitimate MVVM usage).
    /// </summary>
    [Fact]
    public void ViewModels_Must_Not_Reference_Xaml_Types()
    {
        RepoRoot.Should().NotBeNullOrEmpty("Repository root must be discoverable");

        var viewModelsDir = Path.Combine(RepoRoot!, "src", "HnVue.UI.ViewModels");

        if (!Directory.Exists(viewModelsDir))
        {
            return;
        }

        var violations = new List<string>();

        foreach (var file in Directory.GetFiles(viewModelsDir, "*.cs", SearchOption.AllDirectories))
        {
            // Skip generated files in obj/
            if (file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                continue;
            }

            var relativePath = file.Substring(viewModelsDir.Length + 1);
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                foreach (var pattern in ForbiddenXamlUsingPatterns)
                {
                    if (trimmed.StartsWith(pattern, StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{relativePath}: '{trimmed}' — " +
                            "ViewModels must not import WPF UI types");
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            because: "ViewModels must be platform-agnostic and not reference WPF UI types " +
                     "(Controls, Shapes, Data binding, etc.). ICommand and Media.Imaging are allowed. " +
                     "UI-specific logic belongs in HnVue.UI Views or code-behind. " +
                     "Violations:\n" + string.Join("\n", violations));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string? ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(
            Path.GetDirectoryName(typeof(DirectoryOwnershipTests).Assembly.Location)!);

        for (int i = 0; i < 8; i++)
        {
            if (dir is null) break;

            if (File.Exists(Path.Combine(dir.FullName, "HnVue.sln")))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }
}
