using System.Runtime.CompilerServices;
using MauiCoreBluetooth.Bluetooth.Models;
using MauiCoreBluetooth.Bluetooth.Models.Connect;
using MauiCoreBluetooth.Bluetooth.Models.Disconnect;
using MauiCoreBluetooth.Bluetooth.Models.Read;
using MauiCoreBluetooth.Bluetooth.Models.Write;

[assembly: InternalsVisibleTo("IPS.Seed.BLE.Core")]
namespace MauiCoreBluetooth.Bluetooth;

public class BleCompletionSources
{
    internal TaskCompletionSource<Tuple<ConnectResult, object>> Connect = new();
    internal TaskCompletionSource<DisconnectResult> Disconnect = new();
    internal TaskCompletionSource<ServiceDiscoveryResult> ServiceDiscovery = new();
    internal TaskCompletionSource<ReadResult> ReadCharacteristic = new();
    internal TaskCompletionSource<WriteResult> WriteCharacteristic = new();
}