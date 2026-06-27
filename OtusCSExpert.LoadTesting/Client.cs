using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OtusCSExpert.LoadTesting;

public interface ISimpleTcpClient: IDisposable
{
    Task ConnectAsync();
    Task<byte[]> SetAsync(string key, byte[] value);
    Task<byte[]> GetAsync(string key);
}

public class SimpleTcpClient : ISimpleTcpClient
{
    private readonly TcpClient _client;
    private readonly string _host;
    private readonly int _port;
    private readonly int _bufferSize = 4096;
    private bool _disposedValue;

    public SimpleTcpClient(string host, int port)
    {
        _client = new TcpClient();
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        await _client.ConnectAsync(IPAddress.Parse(_host), _port);
    }

    public async Task<byte[]> GetAsync(string key)
    {
        return await SendCommand(Encoding.UTF8.GetBytes($"GET {key}"));
    }

    public async Task<byte[]> SetAsync(string key, byte[] value)
    {
        byte[] mergedData = Encoding.UTF8.GetBytes($"SET {key} ").Concat(value).ToArray();
        return await SendCommand(mergedData);
    }

    private async Task<byte[]> SendCommand(byte[] data)
    {
        using var stream = _client.GetStream();
        await stream.WriteAsync(data);

        var buffer = new byte[_bufferSize];
        int read = 0;
        if (stream.DataAvailable)
        {
            read = await stream.ReadAsync(buffer);
        }

        return buffer[..read];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _client.Close();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
