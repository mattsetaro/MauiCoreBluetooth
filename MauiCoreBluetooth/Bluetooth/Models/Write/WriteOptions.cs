using System.Text;

namespace MauiCoreBluetooth.Bluetooth.Models.Write;

/// <summary>
/// Generic write characteristic options.
/// </summary>
public record WriteOptions()
{
    /// <summary>
    /// The characteristic to write data to.
    /// </summary>
    public Characteristic Characteristic { get; }

    /// <summary>
    /// Raw byte data to be written to the target characteristic.
    /// </summary>
    public byte[] Data { get; } = [];

    /// <summary>
    /// Write with or without response.
    /// </summary>
    public bool WithResponse { get; } = true;

    /// <summary>
    /// How long to wait for a response before timing out.
    /// </summary>
    public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Default constructor for WriteOptions.
    /// </summary>
    /// <param name="characteristic">The characteristic to write data to.</param>
    /// <param name="withResponse">Write with or without response.</param>
    /// <param name="data">Raw byte data to be written to the target characteristic.</param>
    public WriteOptions(Characteristic characteristic, bool withResponse, byte[] data) : this()
    {
        Characteristic = characteristic;
        Data = data;
        WithResponse = withResponse;
    }

    /// <summary>
    /// Overload constructor for WriteOptions.
    /// </summary>
    /// <param name="characteristic">The characteristic to write data to.</param>
    /// <param name="withResponse">Write with or without response.</param>
    /// <param name="data">An array of string values to be converted to UTF8 bytes.</param>
    public WriteOptions(Characteristic characteristic, bool withResponse, params string[] data) : this()
    {
        Characteristic = characteristic;

        // Convert the strings to bytes.
        var byteList = new List<byte>();
        foreach (string str in data)
            byteList.AddRange(Encoding.UTF8.GetBytes(str));

        Data = byteList.ToArray();
        WithResponse = withResponse;
    }
}