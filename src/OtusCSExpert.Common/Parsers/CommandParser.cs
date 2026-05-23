using System.Buffers;
using System.Text;
using OtusCSExpert.Common.Types;

namespace OtusCSExpert.Common.Parsers;

public static class CommandParser
{
    /// <summary> Перегрузка для явной последовательности байт </summary>
    public static ParsedCommand Parse(ReadOnlySpan<byte> byteSequence)
    {
        if (byteSequence.IsEmpty)
            return ParsedCommand.Empty();
 
        int maxCharCount = Encoding.UTF8.GetCharCount(byteSequence);
        char[] buffer = ArrayPool<char>.Shared.Rent(maxCharCount);
        
        try
        {
            int actualCharCount = Encoding.UTF8.GetChars(byteSequence, buffer);
            return Parse(buffer.AsSpan(0, actualCharCount).TrimStart());
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary> Перегрузка для последовательности символов </summary>
    public static ParsedCommand Parse(ReadOnlySpan<char> rawCommand)
    {
        // Первый токен — команда, всегда должна быть. Если её нет, то это некорректная строка.
        int idx = rawCommand.IndexOf(' ');
        if (idx < 0)
            return ParsedCommand.Empty(); // "SET" — ключ отсутствует

        ReadOnlySpan<char> command = rawCommand.Slice(0, idx);
        ReadOnlySpan<char> rest = rawCommand.Slice(idx + 1); // шагаем через первый пробел

        // Считаем суммарный gap между command и следующим токеном.
        // Нужно чтобы отличить "GET key" (gap=1) от "SET  data" (gap≥2, ключ пропущен).
        int beforeTrim = rest.Length;
        rest = rest.TrimStart(' ');
        int gap = beforeTrim - rest.Length + 1; // +1 — уже шагнутый пробел выше

        // Второй токен — ключ, может быть, а может и не быть. Если его нет, то это некорректная строка.
        int idx2 = rest.IndexOf(' ');

        if (idx2 < 0)
        {
            // Ровно два токена
            if (gap >= 2)
                return ParsedCommand.Empty(); // "SET  data" — ключ пропущен

            return new ParsedCommand(command, rest); // "GET user:1"
        }

        ReadOnlySpan<char> key = rest.Slice(0, idx2);
        ReadOnlySpan<char> value = rest.Slice(idx2 + 1).Trim(); // убираем хвостовые пробелы

        return new ParsedCommand(command, key, value);
    }

    /// <summary> должен разбирать входящую последовательность байт, представляющую собой строку вида "COMMAND KEY VALUE" </summary>
    /// <param name="command">"COMMAND KEY VALUE" (ex. "SET user:1 data")</param>
    public static ParsedCommand Parse(string? rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand))
            return ParsedCommand.Empty();

        return Parse(rawCommand.AsSpan().TrimStart());
    }
}
