package at.rohschinken.unitybt;

import android.app.Activity;
import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Handler;
import android.os.IBinder;
import android.util.Log;

public class BluetoothService extends Service
{
    private final Handler handler = new Handler();
    public static BluetoothService instance;
    public static Activity unityActivity;
    private static BluetoothAdapter _bluetoothAdapter;
    private static final int REQUEST_ENABLE_BT = 1;
    private static final int REQUEST_ENABLE_BT_DISCOVERY = 2;
    private static boolean _keepScanAlive = false;
    private static String _scanResult = "";
    public static boolean enableBT = false;
    public static boolean disableBT = false;
    public static boolean makeDiscoverable = false;
    public static boolean undoMakeDistoverable = false;
    public static boolean startScan = false;
    public static boolean stopScan = false;
    public static String targetDeviceName = "";

    public void onCreate()
    {
        Log.d("BT_SERVICE", "onCreate()");
        _bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        if (instance == null) {
            instance = this;
        }
    }

    public void onDestroy()
    {
        Log.d("BT_SERVICE", "onDestroy()");
        _bluetoothAdapter = null;
        enableBT = false;
        disableBT = false;
        makeDiscoverable = false;
        undoMakeDistoverable = false;
        startScan = false;
        stopScan = false;
        targetDeviceName = "";
        unityActivity = null;
        this.handler.removeCallbacks(this.sendData);

        instance = null;

        BluetoothBridge.serviceConnected = false;
    }

    public IBinder onBind(Intent intent)
    {
        Log.d("BT_SERVICE", "onBind()");
        BluetoothBridge.serviceConnected = true;
        if (instance == null) {
            instance = this;
        }
        this.handler.removeCallbacks(this.sendData);
        this.handler.postDelayed(this.sendData, 250L);

        return null;
    }

    private Runnable sendData = new Runnable()
    {
        public void run()
        {
            Log.d("BT_SERVICE", "run()");
            if ((!BluetoothBridge.bluetoothAdapterEnabled) && (BluetoothService._bluetoothAdapter != null) && (BluetoothService._bluetoothAdapter.isEnabled())) {
                BluetoothBridge.bluetoothAdapterEnabled = true;
            }
            if ((!BluetoothBridge.bluetoothAdapterDiscoverable) && (BluetoothService._bluetoothAdapter != null) && (BluetoothService._bluetoothAdapter.getScanMode() == 23)) {
                BluetoothBridge.bluetoothAdapterDiscoverable = true;
            }
            if ((BluetoothBridge.bluetoothAdapterDiscoverable) && (BluetoothService._bluetoothAdapter != null) && (BluetoothService._bluetoothAdapter.getScanMode() != 23)) {
                BluetoothBridge.bluetoothAdapterDiscoverable = false;
            }
            Intent sendIntent = new Intent();

            sendIntent.addFlags(65572);

            sendIntent.setAction("at.rohschinken.unitybt.IntentToUnity");

            BluetoothService.this.sendBroadcast(sendIntent);

            BluetoothService.this.handler.removeCallbacks(this);
            BluetoothService.this.handler.postDelayed(this, 250L);
        }
    };

    private void doScan()
    {
        if (!_bluetoothAdapter.isEnabled()) {
            return;
        }
        Log.d("BT_SERVICE", "doScan()");
        IntentFilter filterBTDeviceScan = new IntentFilter();
        filterBTDeviceScan.addAction(BluetoothAdapter.ACTION_DISCOVERY_STARTED);
        filterBTDeviceScan.addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
        filterBTDeviceScan.addAction(BluetoothDevice.ACTION_FOUND);

        registerReceiver(new BroadcastReceiver()
        {
            public void onReceive(Context context, Intent intent)
            {
                String action = intent.getAction();
                if (action.equals(BluetoothAdapter.ACTION_DISCOVERY_STARTED))
                {
                    Log.d("BT_SERVICE", "...discovery started");

                    BluetoothBridge.scanActive = true;
                    BluetoothService._scanResult = "";
                }
                if (action.equals(BluetoothAdapter.ACTION_DISCOVERY_FINISHED))
                {
                    Log.d("BT_SERVICE", "...discovery finished");

                    BluetoothBridge.scanActive = false;

                    BluetoothService.this.unregisterReceiver(this);
                    if (BluetoothService._keepScanAlive) {
                        BluetoothService.this.doScan();
                    }
                }
                if (action.equals(BluetoothDevice.ACTION_FOUND))
                {
                    Log.d("BT_SERVICE", "...found something");

                    String aBTDeviceName = intent.getStringExtra(BluetoothDevice.EXTRA_NAME);
                    Short aBTDeviceRSSI = Short.valueOf(intent.getShortExtra(BluetoothDevice.EXTRA_RSSI, (short)0));

                    BluetoothService._scanResult += aBTDeviceName + ":" + aBTDeviceRSSI.toString() + ";";

                    Log.d("BT_SERVICE", aBTDeviceName);
                    Log.d("BT_SERVICE", aBTDeviceRSSI.toString());

                    BluetoothBridge.scanResult = BluetoothService._scanResult;
                    if (aBTDeviceName.equals(BluetoothService.targetDeviceName)) {
                        BluetoothService._bluetoothAdapter.cancelDiscovery();
                    }
                }
            }
        }, filterBTDeviceScan);

        _bluetoothAdapter.startDiscovery();
    }

    public static void enableBluetoothAdapter()
    {
        Log.d("BT_SERVICE", "enableBluetoothAdapter()");
        Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        enableBtIntent.addFlags(268435456);
        unityActivity.startActivity(enableBtIntent);
    }

    public static void disableBluetoothAdapter()
    {
        Log.d("BT_SERVICE", "disableBluetoothAdapter()");
        BluetoothBridge.bluetoothAdapterEnabled = false;
        if (_bluetoothAdapter == null) {
            return;
        }
        _bluetoothAdapter.disable();
    }

    public static void enableBluetoothDiscoverability()
    {
        Log.d("BT_SERVICE", "enableBluetoothDiscoverability()");

        Intent enableBtDiscoveryIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
        enableBtDiscoveryIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 0);
        enableBtDiscoveryIntent.addFlags(268435456);
        unityActivity.startActivity(enableBtDiscoveryIntent);
    }

    public static void disableBluetoothDiscoverability()
    {
        Log.d("BT_SERVICE", "disableBluetoothDiscoverability()");

        Intent enableBtDiscoveryIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
        enableBtDiscoveryIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 1);
        enableBtDiscoveryIntent.addFlags(268435456);
        unityActivity.startActivity(enableBtDiscoveryIntent);
    }

    public static void startScan()
    {
        Log.d("BT_SERVICE", "startScan()");
        _keepScanAlive = true;
        if (instance != null) {
            instance.doScan();
        }
    }

    public static void stopScan()
    {
        Log.d("BT_SERVICE", "stopScan()");
        _keepScanAlive = false;
        _bluetoothAdapter.cancelDiscovery();
        BluetoothBridge.scanResult = "";
    }

    public static void setAdapterFriendlyName(String newName)
    {
        Log.d("BT_SERVICE", "setAdapterFriendlyName()");
        if (_bluetoothAdapter == null) {
            return;
        }
        _bluetoothAdapter.setName(newName);
    }

    public static void setScanTargetDeviceName(String theNewTargetDeviceName)
    {
        Log.d("BT_SERVICE", "setScanTargetDeviceName()");
        targetDeviceName = theNewTargetDeviceName;
    }

    public static String getBluetoothAdapterName()
    {
        Log.d("BT_SERVICE", "getBluetoothAdapterName()");
        if (_bluetoothAdapter == null) {
            return "";
        }
        return _bluetoothAdapter.getName();
    }

    public static String getScanTargetDeviceName()
    {
        Log.d("BT_SERVICE", "getScanTargetDeviceName()");
        return targetDeviceName;
    }
}
