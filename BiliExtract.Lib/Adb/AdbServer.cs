using BiliExtract.Lib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Adb;

public class AdbServer
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();

    public IPAddress Host { get; }
    public int Port { get; }
    public int Version => GetServerVersion();
    public bool IsStarted => GetServerVersion() != int.MinValue;

    private AdbSocket Socket => new(Host, Port);

    public AdbServer()
    {
        Host = _adbSettings.Data.ServerHost;
        Port = _adbSettings.Data.ServerPort;
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

        Socket.ExecuteCommand("host:kill");
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
            int ret = socket.ExecuteCommand("host:devices-l");
            HandleAdbSocketExecuteCommandReturnCode(ret, socket.LastError);
            response = socket.ReadSyncStringHex();
            HandleAdbSocketReadStringResponse(response, socket.LastError);
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

    public AdbDevice GetDeviceBySerialNumber(string serialNumber) => GetDevices()
        .Where(d => d.SerialNumber == serialNumber)
        .First();

    public bool PairWirelessDevice(IPAddress ipAddress, int port, string pairingCode, out string response)
    {
        if (_adbSettings.Data.CheckServerStartedBeforeOperate && !IsStarted)
        {
            response = string.Empty;
            return false;
        }

        if (pairingCode.Length != 6 || !pairingCode.All(char.IsDigit))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Illegal pairing code. [code=\"{pairingCode}\"]");
            throw new InvalidOperationException("Illegal pairing code");
        }

        using var socket = Socket;
        int ret = socket.ExecuteCommand($"host:pair:{pairingCode}:{ipAddress}:{port}");
        HandleAdbSocketExecuteCommandReturnCode(ret, socket.LastError);
        response = socket.ReadSyncStringHex();
        HandleAdbSocketReadStringResponse(response, socket.LastError);

        // successful pair response should be something like:
        //   - "Successfully paired to {ip}:{port} [guid={guid}]"
        // unsuccessful pair response can be like:
        //   - "Failed: Unable to start pairing client."
        //   - ...

        if (!response.Contains("Successful"))
        {
            return false;
        }
        return true;
    }

    public bool ConnectWirelessDevice(IPAddress ipAddress, int port, out string response)
    {
        if (_adbSettings.Data.CheckServerStartedBeforeOperate && !IsStarted)
        {
            response = string.Empty;
            return false;
        }

        using var socket = Socket;
        int ret = socket.ExecuteCommand($"host:connect:{ipAddress}:{port}");
        HandleAdbSocketExecuteCommandReturnCode(ret, socket.LastError);
        response = socket.ReadSyncStringHex();
        HandleAdbSocketReadStringResponse(response, socket.LastError);

        // successful connect response should be something like:
        //   - "connected to {ip}:{port}"
        //   - "already connected to {ip}:{port}"
        // unsuccessful connect response can be like:
        //   - "cannot connect to {ip}:{port}: {msg} ({errno})"
        //   - ...

        if (!response.Contains("connected"))
        {
            return false;
        }
        return true;
    }

    public bool DisconnectWirelessDevice(IPAddress ipAddress, int port, out string response)
    {
        if (_adbSettings.Data.CheckServerStartedBeforeOperate && !IsStarted)
        {
            response = string.Empty;
            return false;
        }

        using var socket = Socket;
        int ret = socket.ExecuteCommand($"host:disconnect:{ipAddress}:{port}");
        HandleAdbSocketExecuteCommandReturnCodeAllowFail(ret, socket.LastError);
        // if disconnect failed (e.g device not found) ADB server responses failed
        response = socket.ReadSyncStringHex();
        HandleAdbSocketReadStringResponse(response, socket.LastError);

        // successful disconnect response should be something like:
        //   - "disconnected {ip}:{port}"
        // unsuccessful disconnect response can be like:
        //   - "no such device '{ip}:{port}'"
        //   - ...

        if (!response.Contains("disconnected"))
        {
            return false;
        }
        return true;
    }

    private int GetServerVersion()
    {
        using var socket = Socket;
        socket.ExecuteCommand("host:version");
        return socket.ReadIntHex();
    }

    private static void HandleAdbSocketExecuteCommandReturnCode(int ret, string? lastError)
    {
        if (ret != 0 && lastError is not null)
        {
            throw ret switch
            {
                -1 => new InvalidOperationException($"Socket operation error: {lastError}"),
                1 => new InvalidOperationException($"Command execute failed: {lastError}"),
                _ => new Exception($"Unexpected error: {lastError}"),
            };
        }
        return;
    }

    private static void HandleAdbSocketExecuteCommandReturnCodeAllowFail(int ret, string? lastError)
    {
        if (ret != 0 && ret != 1 && lastError is not null)
        {
            throw ret switch
            {
                -1 => new InvalidOperationException($"Socket operation error: {lastError}"),
                _ => new Exception($"Unexpected error: {lastError}"),
            };
        }
        return;
    }

    private static void HandleAdbSocketReadStringResponse(string response, string? lastError)
    {
        if (string.IsNullOrEmpty(response) && lastError is not null)
        {
            throw new InvalidOperationException($"Read string from socket failed: {lastError}");
        }
        return;
    }
}
