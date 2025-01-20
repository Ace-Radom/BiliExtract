using Newtonsoft.Json;

namespace BiliExtract.Lib;

public readonly struct AdbDevice(string serialNumber, AdbDeviceState state, string product, string model, string device)
{
    public string SerialNumber { get; } = serialNumber;
    public AdbDeviceState State { get; } = state;
    public string Product { get; } = product;
    public string Model { get; } = model;
    public string Device { get; } = device;

    public override string ToString()
    {
        return $"SerialNumber: {SerialNumber}, State: {State}, Product: {Product}, Model: {Model}, Device: {Device}";
    }
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
