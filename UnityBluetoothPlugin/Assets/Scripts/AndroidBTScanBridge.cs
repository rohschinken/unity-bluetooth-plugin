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

	/// <summary>
	/// Initializes and starts the Android Service which handles all the Bluetoothadapter interaction
	/// </summary>
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
	}

	/// <summary>
	/// Stops and kills the Android Service.
	/// If you want to use Bluetooth features after you called this function you need to call Initialize() again.
	/// It is recommended to call this function before the Application quits. (it shouldn't be necessary, though)
	/// </summary>
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
	/// <summary>
	/// Enables the Bluetooth adapter on the Android device.
	/// Requires permission (user promp)
	/// </summary>
	public static void EnableBluetoothAdapter()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("enableBluetoothAdapter");
	}

	/// <summary>
	/// Disables the Bluetooth adapter on the Android device.
	/// </summary>
	public static void DisableBluetoothAdapter()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("disableBluetoothAdapter");
	}

	/// <summary>
	/// Enables or Disables the Bluetooth adapter on the Android device.
	/// </summary>
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

	/// <summary>
	/// Sets the name of the Bluetooth adapter on the Android device (= friendly name).
	/// </summary>
	/// <param name="theNewAdapterName">The new adapter name.</param>
	public static void SetAdapterFriendlyName(string theNewAdapterName)
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("setAdapterFriendlyName", theNewAdapterName);
	}

	/// <summary>
	/// Makes the Android device visible to other Bluetooth devices for an unspecified period of time.
	/// Requires permission (user promp)
	/// </summary>
	public static void EnableBTDiscoverability()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("enableBTDiscoverability");
	}

	/// <summary>
	/// Makes the Android device visible to other Bluetooth devices for exactly 1 second.
	/// Afterwards the device is not visible anymore.
	/// Requires permission (user promp)
	/// </summary>
	public static void DisableBTDiscoverability()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("disableBTDiscoverability");
	}

	/// <summary>
	/// Sets the bluetooth name of the scan target device.
	/// </summary>
	/// <param name="theDeviceName">The bluetooth device name.</param>
	public static void SetScanTargetDeviceName(string theDeviceName)
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("setScanTargetDeviceName", theDeviceName);
	}

	/// <summary>
	/// Starts scanning for other Bluetooth devices.
	/// One complete scan takes ~12 seconds. Once the scan is complete it will restart automatically.
	/// If a target device name is specified, the scan will restart immediately when a Bluetooth device with a matching name has been found.
	/// </summary>
	public static void StartScan()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("startScan");
	}

	/// <summary>
	/// Stops scanning for other Bluetooth devices.
	/// </summary>
	public static void StopScan()
	{
		if (_staticBridge == null) return;

		_staticBridge.CallStatic ("stopScan");
	}

	/// <summary>
	/// Starts or stops scanning for other Bluetooth devices.
	/// </summary>
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
	/// <summary>
	/// Gets if the Android Bluetooth Service is active.
	/// </summary>
	/// <returns><c>true</c>, if the service is running, <c>false</c> otherwise.</returns>
	public static bool GetBTServiceRunning()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("serviceConnected");
	}

	/// <summary>
	/// Gets if the Android Bluetooth Adapter is enabled. 
	/// </summary>
	/// <returns><c>true</c>, if adapter is enabled, <c>false</c> otherwise.</returns>
	public static bool GetBTAdapterEnabled()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("bluetoothAdapterEnabled");
	}

	/// <summary>
	/// Gets the name (Bluetooth friendly name) of the Android Bluetooth Adapter.
	/// </summary>
	/// <returns>The adapter name.</returns>
	public static string GetBluetoothAdapterName()
	{
		if (_staticBridge == null) return null;

		return _staticBridge.CallStatic<string> ("getBluetoothAdapterName");
	}

	/// <summary>
	/// Gets if the Android device is discoverable for other Bluetooth devices.
	/// </summary>
	/// <returns><c>true</c>, if adapter is visible, <c>false</c> otherwise.</returns>
	public static bool GetBluetoothAdapterDiscoverable()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("bluetoothAdapterDiscoverable");
	}

	/// <summary>
	/// Gets the name of the scan target device.
	/// </summary>
	/// <returns>The scan target device name.</returns>
	public static string GetScanTargetDeviceName()
	{
		if (_staticBridge == null) return null;

		return _staticBridge.CallStatic<string> ("getScanTargetDeviceName");
	}

	/// <summary>
	/// Gets if the scan is active.
	/// </summary>
	/// <returns><c>true</c>, if the scan is active/running, <c>false</c> otherwise.</returns>
	public static bool GetScanActive()
	{
		if (_staticBridge == null) return false;

		return _staticBridge.GetStatic<bool> ("scanActive");
	}

	/// <summary>
	/// Returns the result of the last scan cycle.
	/// The result string has the following format:
	/// "adaptername:rssivalue;adaptername:rssivalue;" (e.g. "myPhone:-40;otherPhone:-72;")
	/// </summary>
	/// <returns>The scan result as string.</returns>
	public static string GetScanResult()
	{
		if (_staticBridge == null) return string.Empty;

		return _staticBridge.GetStatic<string> ("scanResult");
	}
	#endregion
}
