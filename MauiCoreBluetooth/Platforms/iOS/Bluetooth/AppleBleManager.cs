using CoreBluetooth;
using CoreFoundation;

namespace MauiCoreBluetooth.Bluetooth;

public static class AppleBleManager
{
    public static CBCentralManager? CentralManager { get; private set; }

    public static void Initialize()
    {
        CentralManager = new CBCentralManager(CentralManagerDelegate.Instance, DispatchQueue.CurrentQueue);
    }
}