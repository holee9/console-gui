using System.IO;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace HnVue.Architecture.Tests;

/// <summary>
/// Naming convention and service interface architecture tests.
///
/// Strategy: csproj XML parsing + source file discovery
/// These tests validate architecture rules without requiring full assembly compilation
/// of modules that may have platform-specific dependencies (EF Core, SQLite, etc.).
///
/// Test 5: Repository classes follow {Name}Repository / I{Name}Repository naming pattern.
/// Test 6: Service classes in Common.Abstractions have a corresponding interface.
/// </summary>
public class NamingConventionTests
{
    // Paths resolved relative to the test assembly output location.
    private static readonly string? RepoRoot = ResolveRepoRoot();

    // -------------------------------------------------------------------------
    // Test 5: Repository Naming Convention
    // All repository implementation classes must end with "Repository"
    // and a corresponding I{Name}Repository interface must exist in HnVue.Common.Abstractions.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Repository implementation classes in HnVue.Data must follow the {Name}Repository pattern.
    /// A corresponding I{Name}Repository interface must exist in HnVue.Common.Abstractions.
    /// </summary>
    [Fact]
    public void Repository_Classes_Must_Follow_Naming_Convention()
    {
        RepoRoot.Should().NotBeNullOrEmpty(
            because: "Repository root must be discoverable from the test output directory");

        var dataRepoDir = Path.Combine(RepoRoot!, "src", "HnVue.Data", "Repositories");
        Directory.Exists(dataRepoDir).Should().BeTrue(
            because: $"HnVue.Data/Repositories directory must exist at '{dataRepoDir}'");

        var repoFiles = Directory.GetFiles(dataRepoDir, "*Repository.cs");
        repoFiles.Should().NotBeEmpty(
            because: "HnVue.Data/Repositories must contain at least one repository implementation");

        var abstractionsDir = Path.Combine(RepoRoot!, "src", "HnVue.Common", "Abstractions");
        Directory.Exists(abstractionsDir).Should().BeTrue(
            because: $"HnVue.Common/Abstractions directory must exist at '{abstractionsDir}'");

        foreach (var repoFile in repoFiles)
        {
            var className = Path.GetFileNameWithoutExtension(repoFile);

            // Verify file name ends with "Repository"
            className.Should().EndWith("Repository",
                because: $"Repository implementation file '{className}.cs' must end with 'Repository'");

            // Verify corresponding interface exists: {Name}Repository -> I{Name}Repository
            var interfaceName = $"I{className}.cs";
            var interfacePath = Path.Combine(abstractionsDir, interfaceName);

            File.Exists(interfacePath).Should().BeTrue(
                because: $"Interface '{interfaceName}' must exist in HnVue.Common.Abstractions " +
                         $"for repository implementation '{className}'");
        }
    }

    // -------------------------------------------------------------------------
    // Test 6: Service Interface Completeness
    // All IXxxService interfaces in HnVue.Common.Abstractions must have at least
    // one corresponding implementation class (xXxxService.cs) somewhere in src/.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Service interfaces declared in HnVue.Common.Abstractions must have a corresponding
    /// implementation class. This enforces the service interface contract pattern.
    /// </summary>
    [Fact]
    public void Service_Interfaces_Must_Have_Implementations()
    {
        RepoRoot.Should().NotBeNullOrEmpty(
            because: "Repo root must be discoverable");

        var abstractionsDir = Path.Combine(RepoRoot!, "src", "HnVue.Common", "Abstractions");
        Directory.Exists(abstractionsDir).Should().BeTrue(
            because: $"HnVue.Common/Abstractions must exist at '{abstractionsDir}'");

        var serviceInterfaces = Directory.GetFiles(abstractionsDir, "I*Service.cs");
        serviceInterfaces.Should().NotBeEmpty(
            because: "HnVue.Common/Abstractions must define at least one IXxxService interface");

        var srcRoot = Path.Combine(RepoRoot!, "src");
        var violations = new List<string>();

        foreach (var interfaceFile in serviceInterfaces)
        {
            var interfaceName = Path.GetFileNameWithoutExtension(interfaceFile); // e.g. "ISecurityService"
            var expectedImplName = interfaceName.Substring(1); // e.g. "SecurityService"

            // Search for any C# file named {Name}Service.cs anywhere in src/
            var implementations = Directory.GetFiles(
                srcRoot, $"{expectedImplName}.cs", SearchOption.AllDirectories);

            if (implementations.Length == 0)
            {
                violations.Add($"No implementation found for {interfaceName} (expected {expectedImplName}.cs in src/)");
            }
        }

        violations.Should().BeEmpty(
            because: "Every IXxxService interface must have at least one implementation class. " +
                     "Missing implementations:\n" + string.Join("\n", violations));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string? ResolveRepoRoot()
    {
        // Walk up from the test output directory to find the repository root.
        // Expected depth: net8.0-windows -> Release -> bin -> HnVue.Architecture.Tests -> tests -> repo_root
        var dir = new DirectoryInfo(
            Path.GetDirectoryName(typeof(NamingConventionTests).Assembly.Location)!);

        for (int i = 0; i < 8; i++)
        {
            if (dir is null)
                break;

            // Repo root has HnVue.sln at root
            if (File.Exists(Path.Combine(dir.FullName, "HnVue.sln")))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }
}
