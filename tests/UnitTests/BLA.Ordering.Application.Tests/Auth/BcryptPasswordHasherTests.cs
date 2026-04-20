using BLA.Ordering.Infrastructure.Auth;
using FluentAssertions;

namespace BLA.Ordering.Application.Tests.Auth;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ValidPlainPassword_ReturnsBcryptFormattedHash()
    {
        // Act
        var hash = _hasher.Hash("SecurePass1!");

        // Assert
        hash.Should().StartWith("$2");
        hash.Length.Should().BeGreaterThanOrEqualTo(59);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _hasher.Hash("SecurePass1!");

        // Act
        var result = _hasher.Verify("SecurePass1!", hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.Hash("SecurePass1!");

        // Act
        var result = _hasher.Verify("WrongPassword9@", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashesDueToSalting()
    {
        // Act
        var hash1 = _hasher.Hash("SecurePass1!");
        var hash2 = _hasher.Hash("SecurePass1!");

        // Assert
        hash1.Should().NotBe(hash2);
        _hasher.Verify("SecurePass1!", hash1).Should().BeTrue();
        _hasher.Verify("SecurePass1!", hash2).Should().BeTrue();
    }
}
