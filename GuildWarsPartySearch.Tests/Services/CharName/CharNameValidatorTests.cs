using FluentAssertions;
using GuildWarsPartySearch.Server.Services.CharName;

namespace GuildWarsPartySearch.Tests.Services.CharName;

[TestClass]
public sealed class CharNameValidatorTests
{
    private readonly CharNameValidator charNameValidator;

    public CharNameValidatorTests()
    {
        this.charNameValidator = new CharNameValidator();
    }

    [TestMethod]
    [DataRow("-eq", false)] //Invalid chars
    [DataRow(";_;_;", false)] //Invalid chars
    [DataRow("D@ddy UwU", false)] //Invalid chars
    [DataRow("Kormir Fr3@kin Sucks", false)] // Invalid chars
    [DataRow("Myself", false)] // Only one word
    [DataRow("Fentanyl Ascetic", true)] //Correct
    [DataRow("Kormir", false)] // Only one word
    [DataRow("Kormir Sucks", true)] // Correct
    [DataRow("Bob'); DROP TABLE", false)] // Invalid chars
    [DataRow("Why are we still here, just to suffer", false)] // Too long + Invalid chars
    [DataRow("Why are we still here just to suffer", false)] // Too long
    [DataRow(null, false)] // Null
    [DataRow("", false)] // Empty
    [DataRow(" ", false)] // Whitespace
    [DataRow("   ", false)] // Whitespace
    [DataRow("Me", false)] // Too short
    public void ValidateNames_ShouldReturnExpected(string name, bool expected)
    {
        var result = this.charNameValidator.Validate(name);
        result.Should().Be(expected);
    }
}
