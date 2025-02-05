using Newtonsoft.Json;
using System;

namespace BiliExtract.Lib;

public readonly struct AdbDevice(string serialNumber, AdbDeviceState state, string product, string model, string device, bool supportShellV2, bool supportStatV2, bool supportLsV2)
{
    public string SerialNumber { get; } = serialNumber;
    public AdbDeviceState State { get; } = state;
    public string Product { get; } = product;
    public string Model { get; } = model;
    public string Device { get; } = device;

    public bool SupportShellV2 { get; } = supportShellV2;
    public bool SupportStatV2 { get; } = supportStatV2;
    public bool SupportLsV2 { get; } = supportLsV2;

    public override string ToString()
    {
        return $"SerialNumber: {SerialNumber}, State: {State}, Product: {Product}, Model: {Model}, Device: {Device}";
    }
}

[method: JsonConstructor]
public struct RichLogViewStyle()
{
    public static readonly RichLogViewStyle DarkThemeDefault = new()
    {
        ExceptionTextColor = new(202, 144, 89),
        FilenameColor = new(98, 153, 213),
        LogLevelDebugColor = new(98, 153, 213),
        LogLevelInfoColor = new(181, 207, 168),
        LogLevelWarningColor = new(202, 144, 89),
        LogLevelErrorColor = new(202, 144, 89),
        NormalTextColor = new(212, 212, 212),
        NumberColor = new(98, 153, 213),
        StringColor = new(202, 144, 89),
        TimeHMSColor = new(106, 154, 86),
        TimeYMDColor = new(98, 153, 213)
    };

    public static readonly RichLogViewStyle LightThemeDefault = new()
    {
        ExceptionTextColor = new(159, 26, 25),
        FilenameColor = new(61, 0, 254),
        LogLevelDebugColor = new(39, 76, 164),
        LogLevelInfoColor = new(25, 135, 88),
        LogLevelWarningColor = new(159, 26, 25),
        LogLevelErrorColor = new(159, 26, 25),
        NormalTextColor = new(0, 0, 0),
        NumberColor = new(61, 0, 254),
        StringColor = new(159, 26, 25),
        TimeHMSColor = new(8, 129, 3),
        TimeYMDColor = new(61, 0, 254)
    };

    public string Font { get; set; } = "Consolas";

    public RGBColor ExceptionTextColor { get; set; }
    public TextStyle ExceptionTextStyle { get; set; } = TextStyle.Italic;
    public RGBColor FilenameColor { get; set; }
    public RGBColor LogLevelDebugColor { get; set; }
    public TextStyle LogLevelDebugStyle { get; set; } = TextStyle.Normal;
    public RGBColor LogLevelInfoColor { get; set; }
    public TextStyle LogLevelInfoStyle { get; set; } = TextStyle.Normal;
    public RGBColor LogLevelWarningColor { get; set; }
    public TextStyle LogLevelWarningStyle { get; set; } = TextStyle.Normal;
    public RGBColor LogLevelErrorColor { get; set; }
    public TextStyle LogLevelErrorStyle { get; set; } = TextStyle.Bold;
    public RGBColor NormalTextColor { get; set; }
    public RGBColor NumberColor { get; set; }
    public RGBColor StringColor { get; set; }
    public RGBColor TimeHMSColor { get; set; }
    public RGBColor TimeYMDColor { get; set; }
}

[method: JsonConstructor]
public readonly struct RGBColor(byte r, byte g, byte b)
{
    public static readonly RGBColor Green = new(142, 255, 0);
    public static readonly RGBColor Pink = new(186, 0, 255);
    public static readonly RGBColor Purple = new(101, 0, 255);
    public static readonly RGBColor Red = new(255, 0, 0);
    public static readonly RGBColor Teal = new(0, 212, 255);
    public static readonly RGBColor White = new(255, 255, 255);

    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;

    #region Equality

    public override bool Equals(object? obj)
    {
        return obj is RGBColor color && R == color.R && G == color.G && B == color.B;
    }

    public override int GetHashCode() => (R, G, B).GetHashCode();

    public static bool operator ==(RGBColor left, RGBColor right) => left.Equals(right);

    public static bool operator !=(RGBColor left, RGBColor right) => !left.Equals(right);

    #endregion

    public override string ToString() => $"{nameof(R)}: {R}, {nameof(G)}: {G}, {nameof(B)}: {B}";
}

public readonly struct Size(double width, double height)
{
    public double Width { get; } = width;
    public double Height { get; } = height;
}

[method: JsonConstructor]
public readonly struct TempFileHandle(string path, DateTime registerTime)
{
    public static TempFileHandle Empty => new();

    public string Path { get; } = path;
    public DateTime RegisterTime { get; } = registerTime;

    #region Equality

    public override bool Equals(object? obj)
    {
        return obj is TempFileHandle handle && Path == handle.Path && RegisterTime == handle.RegisterTime;
    }

    public static bool operator ==(TempFileHandle left, TempFileHandle right) => left.Equals(right);

    public static bool operator !=(TempFileHandle left, TempFileHandle right) => !left.Equals(right);

    #endregion
}

[method: JsonConstructor]
public struct TempFileHandleData()
{
    public string? Hash { get; set; }
    public TempFileState State { get; set; }
}

[method: JsonConstructor]
public readonly struct TempFileHandleStore(TempFileHandle handle, string hash)
{
    public TempFileHandle Handle { get; } = handle;
    public string Hash { get; } = hash;
}
