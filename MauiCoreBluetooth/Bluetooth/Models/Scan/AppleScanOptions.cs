namespace MauiCoreBluetooth.Bluetooth.Models.Scan;

public record AppleScanOptions(bool AllowDuplicates = false, string[]? ServiceUuids = null);