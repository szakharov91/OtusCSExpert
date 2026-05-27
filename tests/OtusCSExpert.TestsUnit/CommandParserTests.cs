using System.Buffers;
using System.Text;
using FluentAssertions;
using OtusCSExpert.Common.Parsers;
using OtusCSExpert.Common.Types;

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
    public void Parse_ValidSetCommandAsBytes_ReturnsCommandKeyValue()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("SET user:1 data");

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
    public void Parse_ValidGetCommandAsBytes_ReturnsCommandKeyAndEmptyValue()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("GET user:1");

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
    public void Parse_InvalidCommandWithoutKeyAsBytes_ReturnsDefault()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("SET"); // только команда, нет ключа

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
    public void Parse_CommandWithExtraSpacesBetweenArgumentsAsBytes_ReturnsCorrectComponents()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("SET    user:1      data"); // множественные пробелы

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
    public void Parse_EmptyByteArray_ReturnsDefault()
    {
        // Arrange
        byte[] input = [];

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
    public void Parse_OnlyCommandAndValueWithoutKeyAsBytes_ReturnsDefault()
    {
        // Arrange - Некорректная команда: "SET  value" – нет ключа
        byte[] input = Encoding.UTF8.GetBytes("SET  data");

        // Act
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
    public void Parse_LeadingSpacesAsBytes_ShouldTrimAndParse()
    {
        // Arrange - Ведущие пробелы не должны мешать
        byte[] input = Encoding.UTF8.GetBytes("   GET user:1");

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
    public void Parse_TrailingSpacesAsBytes_ShouldNotAffectParsing()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("SET key value   ");

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

    [Fact]
    public void Parse_ExtraSpacesOnlyAsBytes_ReturnsDefault()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("     ");

        // Act
        ParsedCommand result = CommandParser.Parse(input);

        // Assert
        result.IsEmpty().Should().BeTrue();
    }
}
