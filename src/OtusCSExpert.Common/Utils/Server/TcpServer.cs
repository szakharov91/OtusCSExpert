using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OtusCSExpert.Common.Parsers;
using OtusCSExpert.Common.Storage;
using OtusCSExpert.Common.Types;
using OtusCSExpert.Common.Utils.CommandHandlers;

namespace OtusCSExpert.Common.Utils.Server;

public static class ErrorResponses
{
    public static class AsString
    {
        public const string OkResponse = "OK\r\n";
        public const string NilResponse = "(nil)\r\n";
        public const string UnknownCommandResponse = "-ERR Unknown command\r\n";
    }
}

public class TcpServer : IServer
{
    private readonly ICommandHandler _dataHandler;
    private readonly IStoragable _storage;
    private readonly int _port;
    private readonly IPAddress _ipAddress;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly int _bufferSize = 8 * 1024;
    private static readonly byte[] _okResponse = Encoding.UTF8.GetBytes("OK\r\n");
    private static readonly byte[] _nilResponse = Encoding.UTF8.GetBytes("(nil)\r\n");
    private static readonly byte[] _unknownCommandResponse = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");

    private Socket? _listener;
    private bool _isDisposed;

    #region ctor, finalizers, properties

    public TcpServer(ICommandHandler dataHandler, IStoragable storage, IPAddress ipAddress, int port = 8080)
    {
        _dataHandler = dataHandler;
        _storage = storage;
        _port = port;
        _ipAddress = ipAddress;
    }

    /// <summary> Финализатор (деструктор) - только для освобождения НЕУПРАВЛЯЕМЫХ ресурсов </summary>
    ~TcpServer()
    {
        // Освобождаем только неуправляемые ресурсы
        Dispose(false);
        Trace.TraceInformation($"[GC] TcpServer finalizer called at {DateTime.Now:HH:mm:ss.fff}");
    }

    #endregion

    #region public methods

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
        var combinedToken = linkedCts.Token;

        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(new IPEndPoint(_ipAddress, _port));
        _listener.Listen();
        Trace.TraceInformation($"Socket listening on {_ipAddress}:{_port}");

        // В методе StartAsync после вызова Listen организуйте бесконечный асинхронный цикл (while(true))
        while (true)
        {
            if(combinedToken.IsCancellationRequested)
                break; // выходим по отмене из цикла безопасно

            try
            {
                var clientSocket = await _listener.AcceptAsync(); // не используем using, т.к. обрабатываем клиента в ProcessClientAsync try-finally
                _ = ProcessClientAsync(clientSocket, combinedToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
        }
    }

    public void Stop() => Dispose();

    public void Dispose()
    {
        // Освобождаем и управляемые, и неуправляемые ресурсы
        Dispose(true);
        Trace.TraceInformation($"[Dispose] TcpServer disposed at {DateTime.Now:HH:mm:ss.fff}");
        // Подавляем финализатор (объект уже очищен)
        GC.SuppressFinalize(this);
    }

    #endregion

    #region protected methods

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Освобождаем УПРАВЛЯЕМЫЕ ресурсы (только из Dispose(), не из финализатора)
            if (_listener != null)
            {
                _cts.Cancel();
                _listener.Close(0); // немедленное закрытие
                _listener = null;
            }
        }

        // Освобождаем НЕУПРАВЛЯЕМЫЕ ресурсы (всегда, даже в финализаторе)

        _isDisposed = true;
    }

    #endregion

    #region private methods

    private async Task ProcessClientAsync(Socket clientSocket, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientSocket, nameof(clientSocket));

        byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

        try
        {
            while (!cancellationToken.IsCancellationRequested) // сразу завязываем цикл на токен
            {
                var memory = buffer.AsMemory();

                int bytesReceived = await clientSocket.ReceiveAsync(memory, SocketFlags.None, _cts.Token);
                if (bytesReceived == 0)
                    break;

                ReadOnlyMemory<byte> readOnlyData = memory[..bytesReceived];
                var result = CommandParser.Parse(readOnlyData);

                _dataHandler.Execute(result);

                string key = result.Key.ToString();
                byte[] response = _nilResponse;

                switch (result.Command)
                {
                    case "GET":
                        response = _storage.Get(key) ?? _nilResponse;
                        break;
                    case "SET":
                        int byteCount = Encoding.UTF8.GetByteCount(result.Value);
                        byte[] valueBytes = new byte[byteCount];
                        Encoding.UTF8.GetBytes(result.Value, valueBytes);
                        _storage.Set(key, valueBytes);
                        response = _okResponse;
                        break;
                    case "DELETE":
                        _storage.Delete(key);
                        response = _okResponse;
                        break;
                    default:
                        await clientSocket.SendAsync(_unknownCommandResponse);
                        continue;
                }

                await clientSocket.SendAsync(response);
            }
        }
        finally
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close(); // Dispose можно не вызывать, так как он вызовется внутри Close()
            ArrayPool<byte>.Shared.Return(buffer, true);
            Trace.TraceInformation($"[{DateTime.Now:HH:mm:ss}] Client processing completed");
        }       
    }

    #endregion
}
