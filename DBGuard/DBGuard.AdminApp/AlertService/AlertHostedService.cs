using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DBGuard.BLL.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DBGuard.AdminApp.AlertService;

public class AlertHostedService : BackgroundService
{
    private readonly ILogger<AlertHostedService> _logger;
    private readonly IAlertService _alertService;
    private TcpListener? _listener;

    public AlertHostedService(ILogger<AlertHostedService> logger, IAlertService alertService)
    {
        _logger = logger;
        _alertService = alertService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int port = 8082;

        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        _logger.LogInformation("TCP server started on port {Port}", port);

        while (!stoppingToken.IsCancellationRequested)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync(stoppingToken);

            _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
        }
    }

    private async Task HandleClientAsync(
        TcpClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            await using NetworkStream stream = client.GetStream();

            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] oneByte = new byte[1];
                int read = await stream.ReadAsync(oneByte, 0, 1, cancellationToken);

                if (read == 0)
                    return;

                int type = oneByte[0];

                switch (type)
                {
                    case 1:
                        await HandleSQLInjectionAsync(stream, cancellationToken);
                        break;
                    
                    case 2:
                        await HandleBulkOperationAsync(stream, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown packet type: {Type}", type);
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Client handling failed");
        }
        finally
        {
            client.Close();
        }
    }

    private async Task HandleSQLInjectionAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        int queryLength = await ReadInt32Async(stream, cancellationToken);
        string query = await ReadStringAsync(stream, queryLength, cancellationToken);
        
        float accuracy = await ReadFloatAsync(stream, cancellationToken);

        int usernameLength = await ReadInt32Async(stream, cancellationToken);
        string username = await ReadStringAsync(stream, usernameLength, cancellationToken);
        
        int ipLength = await ReadInt32Async(stream, cancellationToken);
        string ip = await ReadStringAsync(stream, ipLength, cancellationToken);

        await _alertService.SendSQLInjectionAlertAsync(query, accuracy, username, ip);
    }
    
    private async Task HandleBulkOperationAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        byte[] countBuffer = await ReadExactAsync(stream, 1, cancellationToken);
        int tableCount = countBuffer[0];

        int tablesLength = await ReadInt32Async(stream, cancellationToken);
        string tables = await ReadStringAsync(stream, tablesLength, cancellationToken);
        
        long rowCount = await ReadInt64Async(stream, cancellationToken);
        
        int usernameLength = await ReadInt32Async(stream, cancellationToken);
        string username = await ReadStringAsync(stream, usernameLength, cancellationToken);

        int ipLength = await ReadInt32Async(stream, cancellationToken);
        string ip = await ReadStringAsync(stream, ipLength, cancellationToken);

        await _alertService.SendBulkOperationAlertAsync(
            tables,
            rowCount,
            username,
            ip);
    }
    
    private static async Task<string> ReadStringAsync(
        NetworkStream stream,
        int length,
        CancellationToken cancellationToken)
    {
        byte[] buffer = await ReadExactAsync(stream, length, cancellationToken);
        return Encoding.UTF8.GetString(buffer);
    }
    
    private static async Task<float> ReadFloatAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        byte[] buffer = await ReadExactAsync(stream, 4, cancellationToken);

        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }

    private static async Task<int> ReadInt32Async(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        byte[] buffer = await ReadExactAsync(stream, 4, cancellationToken);

        return BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }
    
    private static async Task<long> ReadInt64Async(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        byte[] buffer = await ReadExactAsync(stream, 8, cancellationToken);

        return BinaryPrimitives.ReadInt64LittleEndian(buffer);
    }

    private static async Task<byte[]> ReadExactAsync(
        NetworkStream stream,
        int length,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[length];
        int offset = 0;

        while (offset < length)
        {
            int read = await stream.ReadAsync(
                buffer.AsMemory(offset, length - offset),
                cancellationToken);

            if (read == 0)
                throw new IOException("Socket closed");

            offset += read;
        }

        return buffer;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();

        await base.StopAsync(cancellationToken);
    }
}