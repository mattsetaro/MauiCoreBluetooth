namespace MauiCoreBluetooth.Bluetooth.Models.Scan;

/// <summary>
/// The result of a Bluetooth scan.
/// </summary>
/// <param name="Status">The status of the scan attempt.</param>
/// <param name="FoundDevices">A list of found devices (if any).</param>
public record ScanResult(ScanStatus Status, List<AppleBluetoothDevice> FoundDevices);