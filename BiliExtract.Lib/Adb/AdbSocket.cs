using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BiliExtract.Lib.Adb;

public class AdbSocket : IDisposable
{
    private readonly Encoding _encoding = Encoding.ASCII;

    private readonly TcpClient? _tcpClient;
    private readonly NetworkStream? _tcpStream;

    private readonly byte[] _buf = new byte[65536];

    private string? _lastError;

    public string? LastError => _lastError;

    public AdbSocket(IPAddress host, int port)
    {
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Connecting to ADB server. [host={host},port={port}]");
        try
        {
            _tcpClient = new TcpClient(host.ToString(), port);
            _tcpStream = _tcpClient.GetStream();
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to connect to ADB server.", ex);
            _lastError = $"Failed to connect to ADB server: {ex.Message}";
        }
        return;
    }

    public void Dispose()
    {
        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpStream?.Close();
        _tcpClient?.Dispose();
        return;
    }

    public int Write(byte[] data, int size)
    {
        _lastError = null;
        if (_tcpStream is null)
        {
            _lastError = "Write to null TCP stream";
            return -1;
        }

        _tcpStream.Write(data, 0, size);
        Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Wrote to ADB socket. [size={size}]");
        return size;
    }

    public int Write(byte[] data)
    {
        return Write(data, data.Length);
    }

    public int WriteString(string text)
    {
        var size = _encoding.GetBytes(text, 0, text.Length, _buf, 0);
        return Write(_buf, size);
    }

    public int WriteSyncString(string text)
    {
        int size = 0;
        size += WriteInt(text.Length);
        size += WriteString(text);
        return size;
    }

    public int WriteSyncStringHex(string text)
    {
        int size = 0;
        size += WriteString($"{text.Length:X04}");
        size += WriteString(text);
        return size;
    }

    public int WriteInt(int num)
    {
        var bytes = BitConverter.GetBytes(num);
        return Write(bytes);
    }

    public int Read(byte[] data, int size)
    {
        _lastError = null;
        if (_tcpStream is null)
        {
            _lastError = "Read from null TCP stream";
            return -1;
        }
        if (size < 0)
        {
            _lastError = "Illegal read size";
            return -1;
        }

        var readCount = 0;
        while (readCount < size)
        {
            readCount += _tcpStream.Read(data, readCount, size - readCount);
        }
        Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Read from ADB socket [size={readCount}]");
        return readCount;
    }

    public string ReadString(int length)
    {
        if (Read(_buf, length) == -1)
        {
            return string.Empty;
        }
        return _encoding.GetString(_buf, 0, length);
    }

    public string ReadSyncString()
    {
        var length = ReadInt();
        if (length < 0)
        {
            return string.Empty;
        }
        return ReadString(length);
    }

    public string ReadSyncStringHex()
    {
        var length = ReadIntHex();
        if (length < 0)
        {
            return string.Empty;
        }
        return ReadString(length);
    }

    public int ReadInt()
    {
        if (Read(_buf, 4) == -1)
        {
            return int.MinValue;
        }
        return BitConverter.ToInt32(_buf, 0);
    }

    public int ReadIntHex()
    {
        var hex = ReadString(4);
        if (string.IsNullOrEmpty(hex))
        {
            return int.MinValue;
        }
        return Convert.ToInt32(hex, 16);
    }

    public string[] ReadAllLines()
    {
        _lastError = null;
        if (_tcpStream is null)
        {
            _lastError = "Read from null TCP stream";
            return [];
        }

        var lines = new List<string>();
        using var reader = new StreamReader(_tcpStream, _encoding);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
            {
                break;
            }
            lines.Add(line.Trim());
        }
        return lines.ToArray();
    }

    public int ExecuteCommand(string command)
    {
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Executing ADB server command. [cmd=\"{command}\"]");

        _lastError = null;
        int ret = WriteSyncStringHex(command);
        if (ret == -1)
        {
            return -1;
        }
        var response = ReadString(4);
        if (string.IsNullOrEmpty(response))
        {
            return -1;
        }
        switch (response)
        {
            case "OKAY":
                return 0;
            case "FAIL":
                var message = ReadSyncStringHex();
                if (string.IsNullOrEmpty(message))
                {
                    return -1;
                }
                _lastError = $"Command execute failed: {message}";
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB server responsed failed. [cmd=\"{command}\",msg=\"{message}\"]");
                return 1;
            default:
                _lastError = $"Unexpected response: {response}";
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB server responsed unexpectedly. [cmd=\"{command}\",response=\"{response}\"]");
                return 2;
        }
    }

    public int ExecuteSyncCommand(string command, string parameter, out string response)
    {
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Executing ADB server command. [cmd=\"{command}\"]");

        _lastError = null;
        response = string.Empty;
        int ret = WriteString(command);
        if (ret == -1)
        {
            return -1;
        }
        ret = WriteSyncString(parameter);
        if (ret == -1)
        {
            return -1;
        }

        response = ReadString(4);
        if (string.IsNullOrEmpty(response))
        {
            return -1;
        }
        if (response == "FAIL")
        {
            response = ReadSyncStringHex();
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB server responsed failed. [cmd=\"{command}\",msg=\"{response}\"]");
            return 1;
        }

        return 0;
    }
}
