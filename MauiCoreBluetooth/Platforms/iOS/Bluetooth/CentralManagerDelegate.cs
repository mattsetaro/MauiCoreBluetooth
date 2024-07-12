using System.Collections.Concurrent;
using CoreBluetooth;
using Foundation;
using MauiCoreBluetooth.Bluetooth.Models.Connect;
using MauiCoreBluetooth.Bluetooth.Models.Disconnect;

namespace MauiCoreBluetooth.Bluetooth;

internal class CentralManagerDelegate : CBCentralManagerDelegate
{
    internal static CentralManagerDelegate Instance { get; } = new();

    internal ConcurrentQueue<AppleBluetoothDevice> DiscoveredDevices { get; } = new();
    internal BleCompletionSources CompletionSources = new();
    private CentralManagerDelegate()
    {
    }

    public override void UpdatedState(CBCentralManager central)
    {
        CoreFoundation.OSLog.Default.Log($"Central manager state updated: {central.State}");
    }

    #region Scan
    public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData,
        NSNumber RSSI)
    {

        // If for some reason the device was null, don't add it as a found device.
        if (peripheral.Name is null || !peripheral.Name.Contains("Spirio", StringComparison.CurrentCultureIgnoreCase))
            return;

        // Don't add duplicate devices to the list.
        var alreadyDiscovered = DiscoveredDevices.Any(device => device.Address == peripheral.Identifier.ToString());
        if (alreadyDiscovered)
            return;

        // Get the manufacturer specific data from the advertisement data.
        var manufacturerData = advertisementData.ObjectForKey(CBAdvertisement.DataManufacturerDataKey) as NSData;
        var manufacturerDataBytes = manufacturerData?.ToArray() ?? [];

        // Create an instance of the device.
        var device = new AppleBluetoothDevice(peripheral)
        {
            AdvertisedData = manufacturerDataBytes,
            FullAdvertisementData = advertisementData,
            Name = peripheral.Name,
        };

        CoreFoundation.OSLog.Default.Log($"Found device with address {device.Address}");

        // Add it to the list of all found devices.
        DiscoveredDevices.Enqueue(device);
    }

    #endregion

    #region Connect

    public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
    {
        CompletionSources.Connect.TrySetResult(new Tuple<ConnectResult, object>(ConnectResult.Connected, peripheral));
    }

    public override void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError? error)
    {
        CompletionSources.Connect.TrySetResult(new Tuple<ConnectResult, object>(ConnectResult.Failure, error ?? new NSError()));
    }

    #endregion

    #region Disconnect

    public override void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError? error)
    {
        if (error is not null)
        {
            CoreFoundation.OSLog.Default.Log($"Failed to disconnect from device: {error}");
            CompletionSources.Disconnect.TrySetResult(DisconnectResult.Failure);
        }
        else
        {
            CoreFoundation.OSLog.Default.Log("Disconnected from device");
            CompletionSources.Disconnect.TrySetResult(DisconnectResult.Disconnected);
        }
    }

    #endregion
}