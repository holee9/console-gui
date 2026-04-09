using FluentAssertions;
using HnVue.Common.Configuration;
using Xunit;

namespace HnVue.Common.Tests.Configuration;

[Trait("SWR", "SWR-CS-030")]
public sealed class HnVueOptionsTests
{
    // ── Validate Success ──────────────────────────────────────────────────────

    [Fact]
    public void Validate_AllDefaultValues_ReturnsNull()
    {
        var options = new HnVueOptions();

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Valid_AllValidCustomValues_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                LockoutDurationMinutes = 15,
            },
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = "STATION1",
                ListenPort = 11112,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    // ── Validate Security Section ───────────────────────────────────────────────

    [Fact]
    public void Validate_SessionTimeoutMinutes_Zero_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = 0,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.SessionTimeoutMinutes must be at least 1.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_SessionTimeoutMinutes_Negative_ReturnsError(int negativeValue)
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = negativeValue,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.SessionTimeoutMinutes must be at least 1.");
    }

    [Fact]
    public void Validate_MaxFailedLoginAttempts_Zero_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                MaxFailedLoginAttempts = 0,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.MaxFailedLoginAttempts must be at least 1.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validate_MaxFailedLoginAttempts_Negative_ReturnsError(int negativeValue)
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                MaxFailedLoginAttempts = negativeValue,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.MaxFailedLoginAttempts must be at least 1.");
    }

    [Fact]
    public void Validate_LockoutDurationMinutes_Zero_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                LockoutDurationMinutes = 0,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.LockoutDurationMinutes must be at least 1.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_LockoutDurationMinutes_Negative_ReturnsError(int negativeValue)
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                LockoutDurationMinutes = negativeValue,
            },
        };

        var error = options.Validate();

        error.Should().Be("Security.LockoutDurationMinutes must be at least 1.");
    }

    // ── Validate DICOM Section ──────────────────────────────────────────────────

    [Fact]
    public void Validate_LocalAeTitle_Null_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = null!,
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.LocalAeTitle is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_LocalAeTitle_WhiteSpace_ReturnsError(string whitespace)
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = whitespace,
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.LocalAeTitle is required.");
    }

    [Fact]
    public void Validate_LocalAeTitle_Exactly16Chars_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = "1234567890123456", // Exactly 16 chars
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Validate_LocalAeTitle_MoreThan16Chars_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = "12345678901234567", // 17 chars
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.LocalAeTitle must be 16 characters or fewer.");
    }

    [Fact]
    public void Validate_ListenPort_Zero_ReturnsError()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                ListenPort = 0,
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.ListenPort must be between 1 and 65535.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ListenPort_Negative_ReturnsError(int negativePort)
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                ListenPort = negativePort,
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.ListenPort must be between 1 and 65535.");
    }

    [Fact]
    public void Validate_ListenPort_65535_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                ListenPort = 65535,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Theory]
    [InlineData(65536)]
    [InlineData(70000)]
    [InlineData(100000)]
    public void Validate_ListenPort_ExceedsMax_ReturnsError(int invalidPort)
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                ListenPort = invalidPort,
            },
        };

        var error = options.Validate();

        error.Should().Be("Dicom.ListenPort must be between 1 and 65535.");
    }

    // ── Validate Multiple Errors ────────────────────────────────────────────────

    [Fact]
    public void Validate_MultipleErrors_ReturnsFirstError()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = 0,
                MaxFailedLoginAttempts = -1,
            },
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = "",
                ListenPort = 0,
            },
        };

        var error = options.Validate();

        // Should return the first encountered error
        error.Should().Be("Security.SessionTimeoutMinutes must be at least 1.");
    }

    // ── Validate Edge Cases ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_SessionTimeoutMinutes_One_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = 1,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Validate_MaxFailedLoginAttempts_One_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                MaxFailedLoginAttempts = 1,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Validate_LockoutDurationMinutes_One_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                LockoutDurationMinutes = 1,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Validate_ListenPort_One_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                ListenPort = 1,
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }

    [Fact]
    public void Validate_LocalAeTitle_OneChar_ReturnsNull()
    {
        var options = new HnVueOptions
        {
            Dicom = new HnVueOptions.DicomOptions
            {
                LocalAeTitle = "A",
            },
        };

        var error = options.Validate();

        error.Should().BeNull();
    }
}
