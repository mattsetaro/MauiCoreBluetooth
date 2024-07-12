namespace MauiCoreBluetooth.Bluetooth;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
sealed class CharacteristicUuidAttribute(string uuid) : Attribute
{
    public string Uuid { get; } = uuid;
}