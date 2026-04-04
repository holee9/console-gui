using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

[Trait("SWR", "SWR-UPD-010")]
public sealed class CodeSignVerifierTests
{
    [Fact]
    public async Task VerifyHash_CorrectHash_ReturnsSuccess()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "test content");
            var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(tempFile)));

            var result = await CodeSignVerifier.VerifyHashAsync(tempFile, hash);

            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task VerifyHash_WrongHash_ReturnsSignatureVerificationFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "some content");

            var result = await CodeSignVerifier.VerifyHashAsync(tempFile, new string('A', 64));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.SignatureVerificationFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task VerifyHash_NonExistentFile_ReturnsUpdatePackageCorrupt()
    {
        var result = await CodeSignVerifier.VerifyHashAsync(
            "C:/nonexistent/path/package.zip", new string('A', 64));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
    }

    [Fact]
    public async Task VerifyHash_CaseInsensitiveHashComparison_Succeeds()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "content");
            var hashBytes = SHA256.HashData(File.ReadAllBytes(tempFile));
            var upperHash = Convert.ToHexString(hashBytes).ToUpperInvariant();
            var lowerHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            var resultUpper = await CodeSignVerifier.VerifyHashAsync(tempFile, upperHash);
            var resultLower = await CodeSignVerifier.VerifyHashAsync(tempFile, lowerHash);

            resultUpper.IsSuccess.Should().BeTrue();
            resultLower.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task VerifyHash_NullFilePath_ThrowsArgumentNullException()
    {
        var act = async () => await CodeSignVerifier.VerifyHashAsync(null!, "hash");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task VerifyHash_NullExpectedHash_ThrowsArgumentNullException()
    {
        var act = async () => await CodeSignVerifier.VerifyHashAsync("path", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
