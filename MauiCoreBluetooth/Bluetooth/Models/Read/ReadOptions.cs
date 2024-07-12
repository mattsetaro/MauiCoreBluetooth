namespace MauiCoreBluetooth.Bluetooth.Models.Read;

/// <summary>
/// Generic read characteristic options.
/// </summary>
/// <param name="Characteristic">The characteristic to read data from.</param>
/// <param name="Timeout">How long of a timeout to wait before determining the read as unsuccessful.</param>
public record ReadOptions(Characteristic Characteristic, TimeSpan Timeout);