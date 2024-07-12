namespace MauiCoreBluetooth.Bluetooth.Models.Read;

/// <summary>
/// The result of a read characteristic request.
/// </summary>
/// <param name="Status">The status of the read request.</param>
/// <param name="Data">The data read from the read request (if any).</param>
public record ReadResult(ReadStatus Status, byte[] Data);