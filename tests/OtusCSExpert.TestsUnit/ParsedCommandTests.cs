using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OtusCSExpert.Common.Parsers;
using OtusCSExpert.Common.Types;

namespace OtusCSExpert.TestsUnit;

public class ParsedCommandTests
{
    [Fact]
    public void ParsedCommand_WithDefaultCtor()
    {
        // Arrange  Act
        var parsedCommand = new ParsedCommand();

        // Assert
        parsedCommand.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ParsedCommand_WithOneArgCtor()
    {
        // Arrange  Act
        var parsedCommand = new ParsedCommand("SET".AsSpan());

        // Assert
        parsedCommand.Command.Should().BeEquivalentTo("SET");
        parsedCommand.Key.IsEmpty.Should().BeTrue();
        parsedCommand.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ParsedCommand_WithTwoArgCtor()
    {
        // Arrange  Act
        var parsedCommand = new ParsedCommand("SET".AsSpan(), "User:1".AsSpan());

        // Assert
        parsedCommand.Command.Should().BeEquivalentTo("SET");
        parsedCommand.Key.Should().BeEquivalentTo("User:1");
        parsedCommand.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ParsedCommand_WithThreeArgCtor()
    {
        // Arrange  Act
        var parsedCommand = new ParsedCommand("SET".AsSpan(), "User:1".AsSpan(), "data".AsSpan());

        // Assert
        parsedCommand.Command.Should().BeEquivalentTo("SET");
        parsedCommand.Key.Should().BeEquivalentTo("User:1");
        parsedCommand.Value.Should().BeEquivalentTo("data");
    }

    [Fact]
    public void ParsedCommand_Empty()
    {
        // Arrange  Act
        var parsedCommand = ParsedCommand.Empty();

        // Assert
        parsedCommand.Command.IsEmpty.Should().BeTrue();
        parsedCommand.Key.IsEmpty.Should().BeTrue();
        parsedCommand.Value.IsEmpty.Should().BeTrue();
    }
}
