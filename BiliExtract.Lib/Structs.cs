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

public readonly struct Size(double width, double height)
{
    public double Width { get; } = width;
    public double Height { get; } = height;
}
