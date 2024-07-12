namespace MauiCoreBluetooth.Bluetooth.Models.Scan;

/// <summary>
/// Generic BLE scan options that can be used to tweak the scan's behavior.
/// </summary>
/// <param name="ScanDurationSeconds">How long a scan should occur for.</param>
/// <param name="DroidScanOptions">Android-specific scan options.</param>
/// <param name="IosScanOptions">iOS-specific scan options.</param>
public record ScanOptions(int ScanDurationSeconds, DroidScanOptions DroidScanOptions, AppleScanOptions IosScanOptions);