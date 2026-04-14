using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Sequential test collection for Security tests that use BCrypt.
/// BCrypt is CPU-intensive; running these tests in parallel causes
/// timing-related flaky failures under load.
/// </summary>
[CollectionDefinition("Security-Sequential", DisableParallelization = true)]
public sealed class SecuritySequentialCollection;
