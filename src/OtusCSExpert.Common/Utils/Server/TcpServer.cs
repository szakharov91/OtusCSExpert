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
using OtusCSExpert.Common.Types;
using OtusCSExpert.Common.Utils.CommandHandlers;

namespace OtusCSExpert.Common.Utils.Server;

public class TcpServer : IServer
{
    private readonly ICommandHandler _dataHandler;
    private readonly int _port;
    private readonly IPAddress _ipAddress = IPAddress.Loopback;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly int _bufferSize = 8 * 1024;

    private Socket? _listener;
    private bool _isDisposed;

    #region ctor, finalizers, properties

    public TcpServer(ICommandHandler dataHandler, int port = 8080)
    {
        _dataHandler = dataHandler;
        _port = port;
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
