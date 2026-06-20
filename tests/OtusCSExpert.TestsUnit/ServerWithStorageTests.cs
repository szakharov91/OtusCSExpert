using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using OtusCSExpert.Common.Storage;
using OtusCSExpert.Common.Utils.CommandHandlers;
using OtusCSExpert.Common.Utils.Server;

namespace OtusCSExpert.TestsUnit;

public class ServerWithStorageTests : IDisposable
{
    private readonly ICommandHandler _commandHandler = new BlankCommandHandler();
    private TcpServer? _server;

    public void Dispose() => _server?.Dispose();

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task<int> StartServerAsync()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, new SimpleStore(), IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);
        return port;
    }

    private static async Task<byte[]> SendCommandAsync(int port, string command)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        var stream = client.GetStream();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(command));

        await Task.Delay(200);

        var buffer = new byte[4096];
        int read = 0;
        if (stream.DataAvailable)
        {
            read = await stream.ReadAsync(buffer);
        }

        return buffer[..read];
    }

    [Fact]
    public async Task SetCommand_ReturnsOk()
    {
        int port = await StartServerAsync();

        byte[] response = await SendCommandAsync(port, "SET user:1 hello");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("OK\r\n"));
    }

    [Fact]
    public async Task GetCommand_WhenKeyExists_ReturnsStoredValue()
    {
        int port = await StartServerAsync();

        await SendCommandAsync(port, "SET user:1 hello");
        byte[] response = await SendCommandAsync(port, "GET user:1");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("hello"));
    }

    [Fact]
    public async Task GetCommand_WhenKeyMissing_ReturnsNil()
    {
        int port = await StartServerAsync();

        byte[] response = await SendCommandAsync(port, "GET missing");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("(nil)\r\n"));
    }

    [Fact]
    public async Task DeleteCommand_ReturnsOk()
    {
        int port = await StartServerAsync();

        await SendCommandAsync(port, "SET user:1 hello");
        byte[] response = await SendCommandAsync(port, "DELETE user:1");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("OK\r\n"));
    }

    [Fact]
    public async Task UnknownCommand_ReturnsError()
    {
        int port = await StartServerAsync();

        byte[] response = await SendCommandAsync(port, "FOO bar");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("-ERR Unknown command\r\n"));
    }

    [Fact]
    public async Task UnknownCommand_ServerRemainsRunning()
    {
        int port = await StartServerAsync();

        await SendCommandAsync(port, "FOO bar");
        byte[] response = await SendCommandAsync(port, "SET user:1 hello");

        response.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("OK\r\n"));
    }
}
