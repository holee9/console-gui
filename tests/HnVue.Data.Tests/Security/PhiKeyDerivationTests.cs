using HnVue.Data.Security;

namespace HnVue.Data.Tests.Security;

/// <summary>
/// Tests deterministic HKDF derivation used for PHI column encryption keys.
/// </summary>
public sealed class PhiKeyDerivationTests
{
    [Fact]
    public void DeriveKey_SameInput_ReturnsSameKey()
    {
        var key1 = PhiKeyDerivation.DeriveKey("sqlcipher-password");
        var key2 = PhiKeyDerivation.DeriveKey("sqlcipher-password");

        key1.Should().Equal(key2);
        key1.Should().HaveCount(32);
    }

    [Fact]
    public void DeriveKey_DifferentInput_ReturnsDifferentKey()
    {
        var key1 = PhiKeyDerivation.DeriveKey("sqlcipher-password-a");
        var key2 = PhiKeyDerivation.DeriveKey("sqlcipher-password-b");

        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void DeriveKey_EmptyInput_ThrowsArgumentException()
    {
        var act = () => PhiKeyDerivation.DeriveKey(string.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
