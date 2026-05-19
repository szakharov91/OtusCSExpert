using OtusCSExpert.Common.Types;
using OtusCSExpert.Common.Parsers;
using FluentAssertions;

namespace OtusCSExpert.TestsUnit;

public class CommandParserTests
{
    [Fact]
    public void Parse_ValidSetCommand_ReturnsCommandKeyValue()
    {
        // Arrange
        string input = "SET user:1 data";

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.Should().BeEquivalentTo("SET");
        result.Key.Should().BeEquivalentTo("user:1");
        result.Value.Should().BeEquivalentTo("data");
    }

    [Fact]
    public void Parse_ValidGetCommand_ReturnsCommandKeyAndEmptyValue()
    {
        // Arrange
        string input = "GET user:1";

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.Should().BeEquivalentTo("GET");
        result.Key.Should().BeEquivalentTo("user:1");
        result.Value.IsEmpty.Should().BeTrue("Value should be empty for GET command");
    }

    [Fact]
    public void Parse_InvalidCommandWithoutKey_ReturnsDefault()
    {
        // Arrange
        string input = "SET"; // только команда, нет ключа

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.IsEmpty.Should().BeTrue();
        result.Key.IsEmpty.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_CommandWithExtraSpacesBetweenArguments_ReturnsCorrectComponents()
    {
        // Arrange
        string input = "SET    user:1      data"; // множественные пробелы

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.Should().BeEquivalentTo("SET");
        result.Key.Should().BeEquivalentTo("user:1");
        result.Value.Should().BeEquivalentTo("data");
    }

    [Fact]
    public void Parse_EmptyString_ReturnsDefault()
    {
        // Arrange
        string input = "";

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.IsEmpty.Should().BeTrue();
        result.Key.IsEmpty.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_NullInput_ReturnsDefault()
    {
        // Arrange
        string input = null;

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.IsEmpty.Should().BeTrue();
        result.Key.IsEmpty.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyCommandAndValueWithoutKey_ReturnsDefault()
    {
        // Arrange - Некорректная команда: "SET  value" – нет ключа
        string input = "SET  data";
        
        //Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void Parse_LeadingSpaces_ShouldTrimAndParse()
    {
        // Arrange - Ведущие пробелы не должны мешать
        string input = "   GET user:1";

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.Should().BeEquivalentTo("GET");
        result.Key.Should().BeEquivalentTo("user:1");
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_TrailingSpaces_ShouldNotAffectParsing()
    {
        // Arrange
        string input = "SET key value   ";
        
        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.Command.Should().BeEquivalentTo("SET");
        result.Key.Should().BeEquivalentTo("key");
        result.Value.Should().BeEquivalentTo("value");
    }

    [Fact]
    public void Parse_ExtraSpacesOnly_ReturnsDefault()
    {
        // Arrange
        string input = "     ";

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.IsEmpty().Should().BeTrue();
    }
}
