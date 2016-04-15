package at.rohschinken.unitybt;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.IBinder;
import android.util.Log;

public class BluetoothBridge extends BroadcastReceiver
{
    private static BluetoothBridge _instance;
    private static Activity _unityActivity;
    private static Intent _serviceIntent;
    public static boolean serviceConnected = false;
    public static boolean bluetoothAdapterEnabled = false;
    public static boolean bluetoothAdapterDiscoverable = false;
    public static boolean scanActive = false;
    public static String scanResult = "";

    public void onReceive(Context context, Intent intent)
    {
        Log.d("#_#_BRIDGE", "onReceive()");
    }

    public static void createInstance(Activity unityActivity)
    {
        Log.d("BT_BRIDGE", "tryCreateInstance");
        if (_unityActivity == null) {
            _unityActivity = unityActivity;
        }
        if (_instance == null)
        {
            _instance = new BluetoothBridge();

            Log.d("BT_BRIDGE", "doCreateInstance");

            BluetoothService.unityActivity = _unityActivity;

            _serviceIntent = new Intent(_unityActivity, BluetoothService.class);

            _unityActivity.bindService(_serviceIntent, _serviceConnection, Context.BIND_AUTO_CREATE);
        }
    }

    public static void stopService()
    {
        if ((_unityActivity == null) || (_serviceIntent == null)) {
            return;
        }
        _unityActivity.unbindService(_serviceConnection);
    }

    private static ServiceConnection _serviceConnection = new ServiceConnection()
    {
        public void onServiceDisconnected(ComponentName name)
        {
            BluetoothBridge.serviceConnected = false;
        }

        public void onServiceConnected(ComponentName name, IBinder service)
        {
            BluetoothBridge.serviceConnected = true;
        }
    };

    public static void enableBluetoothAdapter()
    {
        BluetoothService.enableBluetoothAdapter();
    }

    public static void disableBluetoothAdapter()
    {
        BluetoothService.disableBluetoothAdapter();
    }

    public static void enableBTDiscoverability()
    {
        BluetoothService.enableBluetoothDiscoverability();
    }

    public static void disableBTDiscoverability()
    {
        BluetoothService.disableBluetoothDiscoverability();
    }

    public static void startScan()
    {
        BluetoothService.startScan();
    }

    public static void stopScan()
    {
        BluetoothService.stopScan();
    }

    public static void setAdapterFriendlyName(String newName)
    {
        BluetoothService.setAdapterFriendlyName(newName);
    }

    public static void setScanTargetDeviceName(String theNewTargetDeviceName)
    {
        BluetoothService.setScanTargetDeviceName(theNewTargetDeviceName);
    }

    public static String getBluetoothAdapterName()
    {
        return BluetoothService.getBluetoothAdapterName();
    }

    public static String getScanTargetDeviceName()
    {
        return BluetoothService.getScanTargetDeviceName();
    }
}
