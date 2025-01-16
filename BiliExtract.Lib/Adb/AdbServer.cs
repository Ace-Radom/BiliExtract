using BiliExtract.Lib.Settings;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Adb;

public class AdbServer
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();

    public string Host { get; }
    public int Port { get; }
    public int Version
    {
        get
        {
            try
            {
                return GetServerVersion();
            }
            catch
            {
                return -1;
            }
        }
    }
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

    private AdbSocket Socket => new(Host, Port);

    public AdbServer()
    {
        Host = _adbSettings.Data.Host;
        Port = _adbSettings.Data.Port;
        return;
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

        Socket.SendCommand("host:kill");
        return Task.CompletedTask;
    }

    public AdbDevice[] GetDevices()
    {
        if (_adbSettings.Data.CheckServerStartedBeforeOperate && !IsStarted)
        {
            return [];
        }

        string response;
        using (var socket = Socket)
        {
            socket.SendCommand("host:devices-l");
            response = socket.ReadSyncStringHex();
        }

        var lines = response.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var devices = new List<AdbDevice>();
        foreach (var line in lines)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string product = string.Empty;
            string model = string.Empty;
            string device = string.Empty;

            var state = parts[1] switch
            {
                "device" => AdbDeviceState.Connected,
                "offline" => AdbDeviceState.Offline,
                "unauthorized" => AdbDeviceState.Unauthorized,
                _ => AdbDeviceState.Unknown,
            };

            for (int i = 2; i < parts.Length; i++)
            {
                var halves = parts[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (halves.Length == 2)
                {
                    switch (halves[0])
                    {
                        case "product":
                            product = halves[1];
                            break;
                        case "model":
                            model = halves[1];
                            break;
                        case "device":
                            device = halves[1];
                            break;
                    }
                }
            }

            devices.Add(new AdbDevice(parts[0], state, product, model, device));
        }

        return devices.ToArray();
    }

    private int GetServerVersion()
    {
        using var socket = Socket;
        socket.SendCommand("host:version");
        return socket.ReadIntHex();
    }
}
