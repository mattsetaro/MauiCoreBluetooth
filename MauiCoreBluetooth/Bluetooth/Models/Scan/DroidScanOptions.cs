namespace MauiCoreBluetooth.Bluetooth.Models.Scan;

public class DroidScanOptions
{
    public DroidScanFilter Filter { get; init; } = new();
    public DroidScanSettings Settings { get; init; } = new();
};


public record DroidScanFilter(
    string? Address = null,
    string? DeviceName = null,
    DroidManufacturerData? ManufacturerData = null,
    DroidServiceData? ServiceData = null,
    DroidServiceUuid? ServiceUuid = null,
    int AdvertisingDataType = -1,
    DroidAdvertisingDataTypeWithData? AdvertisingDataTypeWithData = null,
    string? ServiceSolicitationUuid = null);

public record DroidManufacturerData(int ManufacturerId, byte[] ManufacturerData, byte[]? ManufacturerDataMask = null);
public record DroidServiceData(string ParcelUuid, byte[] ServiceData, byte[]? ServiceDataMask = null);
public record DroidServiceUuid(string ServiceUuid, string? ServiceUuidMask = null);
public record DroidAdvertisingDataTypeWithData(int AdvertisingDataType, byte[] AdvertisingData, byte[] AdvertisingDataMask);


public record DroidScanSettings(
    bool Legacy = false,
    DroidPhy Phy = DroidPhy.Null,
    DroidScanCallbackType CallbackType = DroidScanCallbackType.Null,
    DroidMatchMode MatchMode = DroidMatchMode.Null,
    long ReportDelay = -1,
    DroidScanMode ScanMode = DroidScanMode.Null,
    int NumOfMatches = -1);

public enum DroidPhy
{
    Null,
    Le1M,
    Le1MMask,
    Le2M,
    Le2MMask,
    LeCoded,
    LeCodedMask
}

public enum DroidScanCallbackType
{
    Null,
    AllMatches,
    FirstMatch,
    MatchLost,
    AllMatchesAutoBatch
}

public enum DroidMatchMode
{
    Null,
    Aggressive,
    Sticky
}

public enum DroidScanMode
{
    Null,
    Opportunistic,
    LowPower,
    Balanced,
    LowLatency
}