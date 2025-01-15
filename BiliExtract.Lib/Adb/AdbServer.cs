using BiliExtract.Lib.Settings;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Adb;

public class AdbServer
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();

    public string Host { get; }
    public int Port { get; }
    public int Version => GetServerVersion();

    public bool IsStarted
    {
        get
        {
            try
            {
                GetServerVersion();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public AdbServer()
    {
        Host = _adbSettings.Data.Host;
        Port = _adbSettings.Data.Port;
        return;
    }

    private static AdbSocket GetSocket()
    {
        return new AdbSocket();
    }

    public Task StartAsync()
    {
        if (IsStarted)
        {
            return Task.CompletedTask;
        }

        try
        {
            AdbCommandHandler.Execute("start-server", out _, out _);
        }
        catch
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to start ADB server.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (!IsStarted)
        {
            return Task.CompletedTask;
        }

        GetSocket().SendCommand("host:kill");
        return Task.CompletedTask;
    }

    private int GetServerVersion()
    {
        using var socket = GetSocket();
        socket.SendCommand("host:version");
        return socket.ReadIntHex();
    }
}
