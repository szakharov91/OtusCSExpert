namespace OtusCSExpert.Common.Types;

public readonly ref struct ParsedCommand
{
    public ParsedCommand(ReadOnlySpan<char> command)
    {
        Command = command;
    }

    public ParsedCommand(ReadOnlySpan<char> command, ReadOnlySpan<char> key)
    {
        Command = command;
        Key = key;
    }

    public ParsedCommand(ReadOnlySpan<char> command, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        Command = command;
        Key = key;
        Value = value;
    }

    public ReadOnlySpan<char> Command { get; } = [];
    public ReadOnlySpan<char> Key { get; } = [];
    public ReadOnlySpan<char> Value { get; } = [];

    public bool IsEmpty() => Command == [] && Key == [] && Value == [];

    public static ParsedCommand Empty() => new ParsedCommand([], [], []);
}
