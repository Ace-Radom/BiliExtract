using BiliExtract.Lib.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BiliExtract.Lib.Adb;

public class AdbSocket : IDisposable
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();
    private readonly Encoding _encoding = Encoding.ASCII;

    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _tcpStream;

    private byte[] _buf = new byte[65536];

    public AdbSocket()
    {
        var host = _adbSettings.Data.Host;
        var port = _adbSettings.Data.Port;
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Connecting to ADB server. [host={host},port={port}]");
        _tcpClient = new TcpClient(host, port);
        _tcpStream = _tcpClient.GetStream();

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

    public void Write(byte[] data, int size)
    {
        Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Write to ADB socket. [size={size}]");
        _tcpStream.Write(data, 0, size);
        return;
    }

    public void Write(byte[] data)
    {
        Write(data, data.Length);
        return;
    }

    public void WriteString(string text)
    {
        var size = _encoding.GetBytes(text, 0, text.Length, _buf, 0);
        Write(_buf, size);
        return;
    }

    public void WriteSyncString(string text)
    {
        WriteInt(text.Length);
        WriteString(text);
        return;
    }

    public void WriteSyncStringHex(string text)
    {
        WriteString($"{text.Length:X04}");
        WriteString(text);
        return;
    }

    public void WriteInt(int num)
    {
        var bytes = BitConverter.GetBytes(num);
        Write(bytes);
        return;
    }

    public void Read(byte[] data, int size)
    {
        var readCount = 0;
        while (readCount < size)
        {
            readCount += _tcpStream.Read(data, readCount, size - readCount);
        }
        return;
    }

    public string ReadString(int length)
    {
        Read(_buf, length);
        return _encoding.GetString(_buf, 0, length);
    }

    public string ReadSyncString()
    {
        var length = ReadInt();
        return ReadString(length);
    }

    public string ReadSyncStringHex()
    {
        var length = ReadIntHex();
        return ReadString(length);
    }

    public int ReadInt()
    {
        Read(_buf, 4);
        return BitConverter.ToInt32(_buf, 0);
    }

    public int ReadIntHex()
    {
        var hex = ReadString(4);
        return Convert.ToInt32(hex, 16);
    }

    public string[] ReadAllLines()
    {
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

    public void SendCommand(string command)
    {
        WriteSyncStringHex(command);
        var response = ReadString(4);
        switch (response)
        {
            case "OKAY":
                return;
            case "FAIL":
                var message = ReadSyncStringHex();
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB server responsed failed. [cmd=\"{command}\",msg=\"{message}\"]");
                throw new InvalidOperationException("ADB server responsed failed");
            default:
                throw new InvalidOperationException("Unexcepted ADB response");
        }
    }

    public string SendSyncCommand(string command, string parameter, bool readResponse = true)
    {
        WriteString(command);
        WriteSyncString(parameter);

        if (!readResponse)
        {
            return string.Empty;
        }

        var response = ReadString(4);
        if (response == "FAIL")
        {
            var message = ReadSyncStringHex();
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB server responsed failed. [cmd=\"{command}\",msg=\"{message}\"]");
            throw new InvalidOperationException("ADB server responsed failed");
        }

        return response;
    }
}
