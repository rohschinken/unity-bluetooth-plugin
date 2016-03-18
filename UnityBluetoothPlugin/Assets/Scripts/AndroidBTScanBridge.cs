using UnityEngine;
using System.Collections;

public static class AndroidBTScanBridge
{
	private static AndroidJavaClass _staticBridge;
	private static bool _supported = false;

	static AndroidBTScanBridge()
	{
		#if UNITY_ANDROID
		_supported = true;
		Initialize();
		#else
		_supported = false;
		#endif
	}


	public static void Initialize()
	{
		if (!_supported) 
		{
			return;
		}

		// get unity android activity
		AndroidJavaClass unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		AndroidJavaObject unityActivityObject = unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

		// Accessing the class to call a static method on it
		_staticBridge = new AndroidJavaClass("at.rohschinken.unitybt.BluetoothBridge");

		// Create an instance of our bridge (because it also needs to receive data from the service as a broadcastreceiver)
		_staticBridge.CallStatic("createInstance", unityActivityObject);


		// TODO: wait for activity (or broadcastreceiver) to be created (onCreate) before trying to access an instance
//		_adapterInterface = bridgeClass.CallStatic<AndroidJavaObject> ("getInstance");
	}

	public static void KillAndroidService()
	{
		_staticBridge.CallStatic ("stopService");
		_staticBridge = null;
	}

	#region distance computation
	// http://developer.radiusnetworks.com/2014/12/04/fundamentals-of-beacon-ranging.html
	public static double CalculateDistance(double rssi){
		// -65 is just an educated guess which replaced the device specific transmitter power in dB
		return CalculateDistance(-65, rssi);
	}
	public static double CalculateDistance(int txPower, double rssi) {
		if (rssi == 0) {
			return -1.0d; // if we cannot determine distance, return -1.
		}

		double ratio = rssi*1.0d/txPower;
		if (ratio < 1.0d) {
			return System.Math.Pow(ratio,10.0d);
		}
		else {
			// The three constants in the formula (0.89976, 7.7095 and 0.111) are based on a best fit curve based on
			// a number of measured signal strengths at various known distances from a Nexus4.
			double accuracy =  (0.89976d)*System.Math.Pow(ratio,7.7095d) + 0.111d;
			return accuracy;
		}
	}
	#endregion

	#region public API method calls
	public static void EnableBluetoothAdapter()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("enableBluetoothAdapter");
	}

	public static void DisableBluetoothAdapter()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("disableBluetoothAdapter");
	}

	public static void ToggleBluetoothAdapter()
	{
		if (_staticBridge == null) return;

		if (GetBTAdapterEnabled ()) 
		{
			_staticBridge.CallStatic ("disableBluetoothAdapter");
		} 
		else 
		{
			_staticBridge.CallStatic ("enableBluetoothAdapter");
		}
	}

	public static void SetAdapterFriendlyName(string theNewAdapterName)
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("setAdapterFriendlyName", theNewAdapterName);
	}

	public static void EnableBTDiscoverability()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("enableBTDiscoverability");
	}

	/// <summary>
	/// Disables the BT discoverability. 
	/// Warning: Changes discoverability to 1 sec. and thus will show a dialog to the user
	/// </summary>
	public static void DisableBTDiscoverability()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("disableBTDiscoverability");
	}

	public static void SetScanTargetDeviceName(string theDeviceName)
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("setScanTargetDeviceName", theDeviceName);
	}

	public static void StartScan()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("startScan");
	}

	public static void StopScan()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("stopScan");
	}

	public static void ToggleScan()
	{
		if (_staticBridge == null) return;

		if (GetScanActive ()) 
		{
			_staticBridge.CallStatic ("stopScan");
		} 
		else 
		{
			_staticBridge.CallStatic ("startScan");
		}
	}
	#endregion
		
	#region public API getter calls
	public static bool GetBTServiceRunning()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("serviceConnected");
	}

	public static bool GetBTAdapterEnabled()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("bluetoothAdapterEnabled");
	}

	public static string GetBluetoothAdapterName()
	{
		if (_staticBridge == null) return null;

		return _staticBridge.CallStatic<string> ("getBluetoothAdapterName");
	}

	public static bool GetBluetoothAdapterDiscoverable()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("bluetoothAdapterDiscoverable");
	}

	public static string GetScanTargetDeviceName()
	{
		if (_staticBridge == null) return null;

		return _staticBridge.CallStatic<string> ("getScanTargetDeviceName");
	}

	public static bool GetScanActive()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("scanActive");
	}

	public static string GetScanResult()
	{
		if (_staticBridge == null) return string.Empty;

		return _staticBridge.GetStatic<string> ("scanResult");
	}

//	public static string GetPairedDevices()
//	{
//		if (_adapterInterface == null) return null;
//
//		return _adapterInterface.Call<string> ("getPairedDevices");
//	}
	#endregion
}
