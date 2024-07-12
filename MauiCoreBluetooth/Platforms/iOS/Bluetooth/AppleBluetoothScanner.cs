using System.Runtime.CompilerServices;
using CoreBluetooth;
using MauiCoreBluetooth.Bluetooth.Models.Scan;

[assembly: InternalsVisibleTo("IPS.Seed.iOS")]
namespace MauiCoreBluetooth.Bluetooth;

internal class AppleBluetoothScanner
{
    private readonly CentralManagerDelegate _centralManagerDelegate = CentralManagerDelegate.Instance;
    public bool IsRadioEnabled => AppleBleManager.CentralManager?.State == CBManagerState.PoweredOn;

    public async Task<ScanResult> Scan(ScanOptions options)
    {
        try
        {
            if (AppleBleManager.CentralManager is null)
            {
                CoreFoundation.OSLog.Default.Log("Initializing CBCentralManager.");
                AppleBleManager.Initialize();
                await Task.Delay(500);
            }
            if (!IsRadioEnabled)
            {
                CoreFoundation.OSLog.Default.Log("Attempting to scan when the radio is disabled.");
                return new ScanResult(ScanStatus.RadioDisabled, []);
            }


            CoreFoundation.OSLog.Default.Log("Starting scan... | Manager state: " + AppleBleManager.CentralManager?.State);

            // Clear the list of discovered devices from the last scan.
            _centralManagerDelegate.DiscoveredDevices.Clear();

            // Start the scan.
            AppleBleManager.CentralManager?.ScanForPeripherals((CBUUID[])[], new PeripheralScanningOptions());

            // Wait for the specified duration.
            await Task.Delay(TimeSpan.FromSeconds(options.ScanDurationSeconds));

            // Stop the scan.
            AppleBleManager.CentralManager?.StopScan();

            var res = _centralManagerDelegate.DiscoveredDevices.ToList();
            CoreFoundation.OSLog.Default.Log($"Scan stopped. Found {res.Count} devices.");

            // Return the result of the scan.
            return new ScanResult(res.Count > 0 ? ScanStatus.Success : ScanStatus.NoDevicesFound, res);
        }
        catch (Exception e)
        {
            CoreFoundation.OSLog.Default.Log("Exception while scanning for devices");
            AppleBleManager.CentralManager?.StopScan();
        }

        return new ScanResult(ScanStatus.Exception, []);
    }
}