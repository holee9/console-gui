using System;
using System.Security.Cryptography;
using HnVue.Security;
using FluentAssertions;
using Xunit;

namespace HnVue.Security.Tests;

public class PhiEncryptionServiceTests
{
    private static byte[] GenerateKey() => RandomNumberGenerator.GetBytes(32);
    private readonly PhiEncryptionService _service = new(GenerateKey());

    [Fact]
    public void Encrypt_Decrypt_RoundTrips_Correctly()
    {
        var plaintext = "홍길동 (Patient Name)";
        var encrypted = _service.Encrypt(plaintext);
        var decrypted = _service.Decrypt(encrypted);
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertext_EachTime()
    {
        var plaintext = "Same input";
        var enc1 = _service.Encrypt(plaintext);
        var enc2 = _service.Encrypt(plaintext);
        enc1.Should().NotBe(enc2, "random nonce should produce different ciphertext");
    }

    [Fact]
    public void Encrypt_NullOrEmpty_ReturnsSameInput()
    {
        _service.Encrypt(null!).Should().BeNull();
        _service.Encrypt(string.Empty).Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_NullOrEmpty_ReturnsSameInput()
    {
        _service.Decrypt(null!).Should().BeNull();
        _service.Decrypt(string.Empty).Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_InvalidData_ThrowsFormatException()
    {
        var act = () => _service.Decrypt("not-valid-base64!!!");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Constructor_InvalidKeyLength_Throws()
    {
        var act = () => new PhiEncryptionService(new byte[16]);
        act.Should().Throw<ArgumentException>().WithMessage("*32 bytes*");
    }

    [Fact]
    public void Constructor_NullKey_Throws()
    {
        var act = () => new PhiEncryptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void Encrypt_KoreanText_RoundTripsCorrectly()
    {
        var koreanName = "김환자";
        var encrypted = _service.Encrypt(koreanName);
        _service.Decrypt(encrypted).Should().Be(koreanName);
    }

    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void Encrypt_DateOfBirth_RoundTripsCorrectly()
    {
        var dob = "1990-01-15";
        var encrypted = _service.Encrypt(dob);
        _service.Decrypt(encrypted).Should().Be(dob);
    }

    [Fact]
    public void Decrypt_WithWrongKey_Throws()
    {
        var key1 = GenerateKey();
        var key2 = GenerateKey();
        var svc1 = new PhiEncryptionService(key1);
        var svc2 = new PhiEncryptionService(key2);

        var encrypted = svc1.Encrypt("secret data");
        var act = () => svc2.Decrypt(encrypted);
        act.Should().Throw<AuthenticationTagMismatchException>();
    }
}
