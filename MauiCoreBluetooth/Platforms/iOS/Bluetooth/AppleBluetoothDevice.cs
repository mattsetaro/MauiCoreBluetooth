using CoreBluetooth;
using Foundation;
using MauiCoreBluetooth.Bluetooth.Models;
using MauiCoreBluetooth.Bluetooth.Models.Connect;
using MauiCoreBluetooth.Bluetooth.Models.Disconnect;
using MauiCoreBluetooth.Bluetooth.Models.Read;
using MauiCoreBluetooth.Bluetooth.Models.Write;
using MauiCoreBluetooth.Extensions;

namespace MauiCoreBluetooth.Bluetooth;

public class AppleBluetoothDevice : CBPeripheralDelegate
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Unique UUID of the device.
    /// </summary>
    public string Address => Peripheral.Identifier.ToString();

    /// <summary>
    /// All manufacturer specific data that was advertised by the device.
    /// Limited due to CoreBluetooth limitations.
    /// </summary>
    public byte[] AdvertisedData { get; set; } = [];

    /// <summary>
    /// The entire advertisement data that was advertised by the device.
    /// </summary>
    public NSDictionary FullAdvertisementData { get; init; } = NSDictionary.FromDictionary([]);

    /// <summary>
    /// The CoreBluetooth peripheral object for this device.
    /// </summary>
    private CBPeripheral Peripheral { get; set; }

    /// <summary>
    /// Instance of the central manager delegate.
    /// </summary>
    private readonly CentralManagerDelegate _centralManagerDelegate = CentralManagerDelegate.Instance;

    /// <summary>
    /// Map the raw characteristic UUIDs to the appropriate enum member.
    /// </summary>
    private readonly Dictionary<string, Characteristic> _mappedCharacteristics;

    /// <summary>
    /// Map the characteristics to their CoreBluetooth characteristic.
    /// </summary>
    private readonly Dictionary<Characteristic, CBCharacteristic> _characteristics = new();

    /// <summary>
    /// List of characteristics that are currently being read.
    /// </summary>
    private readonly List<CBCharacteristic> _readCharacteristics = [];

    public AppleBluetoothDevice(CBPeripheral peripheral)
    {
        Peripheral = peripheral;

        // Map the characteristics to their UUIDs.
        _mappedCharacteristics = EnumEx.GetValues<Characteristic>()
            .ToDictionary(characteristic => characteristic.GetId()
                .ToUpper());
    }
    
    public bool IsConnected => Peripheral.State == CBPeripheralState.Connected;

    public bool IsConnecting { get; set; }
    
    public async Task<ConnectResult> Connect()
    {
        try
        {
            CoreFoundation.OSLog.Default.Log("Attempting to connect to device.");
            _centralManagerDelegate.CompletionSources.Connect = new TaskCompletionSource<Tuple<ConnectResult, object>>();

            AppleBleManager.CentralManager?.ConnectPeripheral(Peripheral, new PeripheralConnectionOptions
            {
                NotifyOnConnection = true,
                NotifyOnDisconnection = true,
                NotifyOnNotification = true
            });


            var res = await _centralManagerDelegate.CompletionSources.Connect.Task;

            if (res.Item1 is ConnectResult.Connected)
            {
                Peripheral = res.Item2 as CBPeripheral ?? throw new InvalidOperationException();
                Peripheral.Delegate = this;
                CoreFoundation.OSLog.Default.Log("Connected to device.");
            }

            return res.Item1;
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while attempting to connect to a device");
            AppleBleManager.CentralManager?.CancelPeripheralConnection(Peripheral);
        }

        return ConnectResult.Exception;
    }

    
    public async Task<ServiceDiscoveryResult> DiscoverServices()
    {
        try
        {
            _centralManagerDelegate.CompletionSources.ServiceDiscovery = new TaskCompletionSource<ServiceDiscoveryResult>();

            CoreFoundation.OSLog.Default.Log("Discovering services...");
            Peripheral.DiscoverServices();

            // Wait for the services to be discovered, callback on the peripheral delegate will set the result.
            var services = await _centralManagerDelegate.CompletionSources.ServiceDiscovery.Task;

            if (services == ServiceDiscoveryResult.Success)
            {
                // Reset TCS.
                _centralManagerDelegate.CompletionSources.ServiceDiscovery = new TaskCompletionSource<ServiceDiscoveryResult>();

                // Cache the characteristics for later use.
                foreach (var service in Peripheral.Services ?? [])
                {
                    Peripheral.DiscoverCharacteristics(service);
                    await Task.Delay(200);

                    foreach (var chara in service.Characteristics ?? [])
                    {
                        try
                        {
                            _characteristics.Add(_mappedCharacteristics[chara.UUID.ToString().ToUpper()], chara);
                            CoreFoundation.OSLog.Default.Log("Discovered characteristic: " + chara.UUID);
                        }
                        catch (Exception)
                        {
                            CoreFoundation.OSLog.Default.Log($"Error while mapping characteristic {chara.UUID}.");
                        }
                    }
                }

                CoreFoundation.OSLog.Default.Log("Successfully discovered all services and characteristics.");
                return ServiceDiscoveryResult.Success;
            }

            CoreFoundation.OSLog.Default.Log("Failed to discover services on device.");
            return ServiceDiscoveryResult.Failure;
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while attempting to discover services on a device");
        }

        return ServiceDiscoveryResult.Exception;
    }
    
    public async Task<DisconnectResult> Disconnect()
    {
        try
        {
            _centralManagerDelegate.CompletionSources.Disconnect = new TaskCompletionSource<DisconnectResult>();

            AppleBleManager.CentralManager?.CancelPeripheralConnection(Peripheral);

            var res = await _centralManagerDelegate.CompletionSources.Disconnect.Task;

            if (res is DisconnectResult.Disconnected)
                CoreFoundation.OSLog.Default.Log("Disconnected from device.");
            else
                CoreFoundation.OSLog.Default.Log("Failed to disconnect from device.");

            return res;
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while attempting to disconnect from a device");
        }

        return DisconnectResult.Exception;
    }
    

    public async Task<ReadResult> ReadCharacteristic(ReadOptions options)
    {
        try
        {
            _centralManagerDelegate.CompletionSources.ReadCharacteristic = new TaskCompletionSource<ReadResult>();
            _characteristics.TryGetValue(options.Characteristic, out var characteristic);

            if (characteristic is null)
            {
                CoreFoundation.OSLog.Default.Log($"Characteristic with UUID {options.Characteristic.GetId()} not found on device with address {Address}");
                return new ReadResult(ReadStatus.CharacteristicNotFound, []);
            }

            CoreFoundation.OSLog.Default.Log($"Reading characteristic with UUID {options.Characteristic.GetId()} on device with address {Address}");
            Peripheral.ReadValue(characteristic);

            // Keep track of which characteristic we are reading from because the callback is shared with incoming notifications.
            _readCharacteristics.Add(characteristic);

            var res = await _centralManagerDelegate.CompletionSources.ReadCharacteristic.Task;

            CoreFoundation.OSLog.Default.Log("Read result: " + res.Status);
            CoreFoundation.OSLog.Default.Log("Read data: " + BitConverter.ToString(res.Data) + " | Length: " + res.Data.Length);

            return res;
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while attempting to read a characteristic");
        }

        return new ReadResult(ReadStatus.Exception, []);
    }
    
    public async Task<WriteResult> WriteCharacteristic(WriteOptions options)
    {
        try
        {
            _centralManagerDelegate.CompletionSources.WriteCharacteristic = new TaskCompletionSource<WriteResult>();
            _characteristics.TryGetValue(options.Characteristic, out var characteristic);

            if (characteristic is null)
            {
                CoreFoundation.OSLog.Default.Log($"Characteristic with UUID {options.Characteristic.GetId()} not found on device with address {Address}");
                return WriteResult.CharacteristicNotFound;
            }

            CoreFoundation.OSLog.Default.Log($"Writing to characteristic with UUID {options.Characteristic.GetId()} on device with address {Address}");
            var data = NSData.FromArray(options.Data);
            Peripheral.WriteValue(data, characteristic, options.WithResponse ? CBCharacteristicWriteType.WithResponse : CBCharacteristicWriteType.WithoutResponse);

            var res = options.WithResponse ? await _centralManagerDelegate.CompletionSources.WriteCharacteristic.Task : WriteResult.Success;

            CoreFoundation.OSLog.Default.Log("Write result: " + res);
            CoreFoundation.OSLog.Default.Log("Write data: " + BitConverter.ToString(options.Data) + " | Length: " + options.Data.Length);
            return res;
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while attempting to write to a characteristic");
        }

        return WriteResult.Exception;
    }


    #region Callbacks


    public override void DiscoveredService(CBPeripheral peripheral, NSError? error)
    {
        if (error is not null)
        {
            CoreFoundation.OSLog.Default.Log($"Error while discovering services on device with address {Address}: {error.LocalizedDescription}");
            _centralManagerDelegate.CompletionSources.ServiceDiscovery.TrySetResult(ServiceDiscoveryResult.Failure);
            return;
        }

        CoreFoundation.OSLog.Default.Log("DiscoveredService | Peripheral: " + peripheral.Name);

        // Wait to discover all services before returning.
        CoreFoundation.OSLog.Default.Log("Discovered all services.");
        _centralManagerDelegate.CompletionSources.ServiceDiscovery.TrySetResult(ServiceDiscoveryResult.Success);
    }


    public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
    {
        if (error is not null)
        {
            CoreFoundation.OSLog.Default.Log($"Error while reading characteristic on device with address {Address}: {error.LocalizedDescription}");
            _centralManagerDelegate.CompletionSources.ReadCharacteristic.TrySetResult(new ReadResult(ReadStatus.Failure, []));
            return;
        }

        CoreFoundation.OSLog.Default.Log("UpdatedCharacteristicValue | Peripheral: " + peripheral.Name + " | Characteristic: " + characteristic.UUID);

        var data = characteristic.Value?.ToArray() ?? [];

        if (_readCharacteristics.Contains(characteristic))
        {
            _readCharacteristics.Remove(characteristic);
            _centralManagerDelegate.CompletionSources.ReadCharacteristic.TrySetResult(new ReadResult(ReadStatus.Success, data));
        }

        // TODO: Handle incoming notifications.
    }


    public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
    {
        if (error is not null)
        {
            CoreFoundation.OSLog.Default.Log($"Error while writing to characteristic on device with address {Address}: {error.LocalizedDescription}");
            _centralManagerDelegate.CompletionSources.WriteCharacteristic.TrySetResult(WriteResult.Failure);
            return;
        }

        CoreFoundation.OSLog.Default.Log("WroteCharacteristicValue | Peripheral: " + peripheral.Name + " | Characteristic: " + characteristic.UUID);

        _centralManagerDelegate.CompletionSources.WriteCharacteristic.TrySetResult(WriteResult.Success);
    }

    #endregion
}