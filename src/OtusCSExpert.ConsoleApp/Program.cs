using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OtusCSExpert.Common.Types;
using OtusCSExpert.Common.Utils.CommandHandlers;
using OtusCSExpert.Common.Utils.Server;

Trace.Listeners.Clear();
var consoleListener = new ConsoleTraceListener();
Trace.Listeners.Add(consoleListener);
Trace.AutoFlush = true;

Console.WriteLine("Hello, OTUS!");

var cts = new CancellationTokenSource();

_ = Task.Run(async () =>
{
    using IServer server = new TcpServer(new ConsoleHandler());
    await server.StartAsync(cts.Token);
});

Console.ReadLine();

cts.Cancel();

public class ConsoleHandler : ICommandHandler
{
    public void Execute(ParsedCommand command)
    {
        Console.WriteLine($"{command.Command} {command.Key} {command.Value}");
    }
}
