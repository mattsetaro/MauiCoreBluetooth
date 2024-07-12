using MauiCoreBluetooth.Bluetooth;

namespace MauiCoreBluetooth.Extensions;

public static class CharacteristicEx
{
    public static string GetId(this Characteristic characteristic)
    {
        return characteristic.GetFirstAttribute<CharacteristicUuidAttribute>()?.Uuid ?? string.Empty;
    }
}