using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using OtusCSExpert.Common.Utils.CommandHandlers;
using OtusCSExpert.Common.Utils.Server;

namespace OtusCSExpert.TestsUnit;

public class ServerTests : IDisposable
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

    [Fact]
    public void Constructor_DefaultPort_CreatesInstance()
    {
        using var server = new TcpServer(_commandHandler, IPAddress.Loopback);
        server.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomPort_CreatesInstance()
    {
        using var server = new TcpServer(_commandHandler, IPAddress.Loopback, GetFreePort());
        server.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_ListensOnPort_AcceptsConnection()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);

        client.Connected.Should().BeTrue();
        _server.Stop();
    }

    [Fact]
    public async Task Stop_AfterStart_RefusesNewConnections()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        _server.Stop();
        await Task.Delay(100);

        using var client = new TcpClient();
        Func<Task> act = () => client.ConnectAsync(IPAddress.Loopback, port);
        await act.Should().ThrowAsync<SocketException>();
    }

    [Fact]
    public void Stop_WhenNotStarted_DoesNotThrow()
    {
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, GetFreePort());
        Action act = () => _server.Stop();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenNotStarted_DoesNotThrow()
    {
        var server = new TcpServer(_commandHandler, IPAddress.Loopback, GetFreePort());
        Action act = () => server.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_WhenStarted_DoesNotThrow()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        Action act = () => _server.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var server = new TcpServer(_commandHandler, IPAddress.Loopback, GetFreePort());
        Action act = () =>
        {
            server.Dispose();
            server.Dispose();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartAsync_AfterStop_ServerTaskCompletes()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        var serverTask = _server.StartAsync();
        await Task.Delay(100);

        _server.Stop();

        // Server task should complete (not hang) after Stop
        var completed = await Task.WhenAny(serverTask, Task.Delay(2000));
        completed.Should().Be(serverTask);
    }

    [Fact]
    public async Task StartAsync_ClientSendsValidCommand_ServerRemainsRunning()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        var stream = client.GetStream();
        await stream.WriteAsync(Encoding.UTF8.GetBytes("SET user:1 testvalue"));
        await Task.Delay(200);

        // Server should still accept new connections after processing
        using var client2 = new TcpClient();
        Func<Task> act = () => client2.ConnectAsync(IPAddress.Loopback, port);
        await act.Should().NotThrowAsync();

        _server.Stop();
    }

    [Fact]
    public async Task StartAsync_ClientDisconnectsWithoutData_ServerRemainsRunning()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(IPAddress.Loopback, port);
        } // graceful disconnect without sending data

        await Task.Delay(200);

        // Server should still accept new connections
        using var client2 = new TcpClient();
        Func<Task> act = () => client2.ConnectAsync(IPAddress.Loopback, port);
        await act.Should().NotThrowAsync();

        _server.Stop();
    }

    [Fact]
    public async Task StartAsync_MultipleSequentialClients_HandlesAll()
    {
        int port = GetFreePort();
        _server = new TcpServer(_commandHandler, IPAddress.Loopback, port);
        _ = _server.StartAsync();
        await Task.Delay(100);

        for (int i = 0; i < 3; i++)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);
            var stream = client.GetStream();
            await stream.WriteAsync(Encoding.UTF8.GetBytes($"GET key:{i}"));
            await Task.Delay(100);
        }

        _server.Stop();
    }
}
