using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PluginTester : MonoBehaviour {

	public struct BTScanResult
	{
		public string Name;
		public double Rssi;
		public double Distance;

		public BTScanResult(string name, double rssi, double distance)
		{
			this.Name = name;
			this.Rssi = rssi;
			this.Distance = distance;
		}
	}

	public GUIManager itsGUIManager;

	void Awake()
	{
		AndroidBTScanBridge.Initialize ();
	}

	void Start()
	{
		StartCoroutine (UpdateGUIRoutine (0.5f));
	}

	void OnDisable()
	{
		AndroidBTScanBridge.KillAndroidService ();
	}

	void OnApplicationQuit()
	{
		AndroidBTScanBridge.KillAndroidService ();
	}

	private IEnumerator UpdateGUIRoutine(float theWaitBetweenUpdate)
	{
		bool adapterReady = false;
		bool adapterDiscoverable = false;
		bool scanActive = false;
		string adapterName = string.Empty;

		while (true) 
		{
			yield return new WaitForSeconds (theWaitBetweenUpdate);
			adapterReady = AndroidBTScanBridge.GetBTAdapterEnabled ();
			adapterName = AndroidBTScanBridge.GetBluetoothAdapterName ();
			scanActive = AndroidBTScanBridge.GetScanActive ();
			adapterDiscoverable = AndroidBTScanBridge.GetBluetoothAdapterDiscoverable ();
			itsGUIManager.AdapterStatusText.text = string.Format ("Bt Adapter ({0}) {1}.", adapterName, (adapterReady) ? "is ready!" : "not ready");
			itsGUIManager.AdapterDiscoverabilityText.text = string.Format ("Bt Adapter {0}.", (adapterDiscoverable) ? "is currently visible" : "is not visible to other Bt devices");
			itsGUIManager.ScanActiveText.text = string.Format ("Scan is currently {0}.", (scanActive) ? "in progress" : "not in progress");

			List<BTScanResult> aResultList = ComputeScanResults ();
			if (aResultList.Count > 0) 
			{
				itsGUIManager.ScanResultText.text = "";
				foreach (BTScanResult aScanResult in aResultList) 
				{
					itsGUIManager.ScanResultText.text += aScanResult.Name + ": " + System.Math.Round (aScanResult.Distance * 100.0d) + "cm (rssi: " + aScanResult.Rssi + ")" + System.Environment.NewLine;
				}
			}
			else 
			{
				itsGUIManager.ScanResultText.text = "No scan results.";
			}
		}
	}

	private List<BTScanResult> ComputeScanResults()
	{
		// plainResult will look like "adaptername:rssivalue;adaptername:rssivalue;" (e.g. "samsunghandy:-40;htchandy:-72;")
		string plainResult = AndroidBTScanBridge.GetScanResult ();

		if (string.IsNullOrEmpty(plainResult) || plainResult == "") 
		{
			return new List<BTScanResult> ();
		}

		List<BTScanResult> aBTScanResultList = new List<BTScanResult> ();
		string[] results = plainResult.Split (';');

		for (int i = 0; i < results.Length; i++) 
		{
			// result will look like "adaptername:rssivalue" (e.g. "samsunghandy:-40")
			string[] resultStrings = results[i].Split (':');

			if (resultStrings.Length < 2) 
			{
				continue;
			}

			double aRssi = 0d;
			double.TryParse (resultStrings [1], out aRssi);

			aBTScanResultList.Add(new BTScanResult (resultStrings [0], aRssi, AndroidBTScanBridge.CalculateDistance (aRssi)));
		}

		return aBTScanResultList;
	}

	public void EnableAdapter()
	{
		AndroidBTScanBridge.EnableBluetoothAdapter ();
	}

	public void DisableAdapter()
	{
		AndroidBTScanBridge.DisableBluetoothAdapter ();
	}

	public void SetNewAdapterName()
	{
		AndroidBTScanBridge.SetAdapterFriendlyName (itsGUIManager.AdapterNameInput.text);
	}

	public void EnableDiscoverability()
	{
		AndroidBTScanBridge.EnableBTDiscoverability ();
	}

	public void DisableDiscoverability()
	{
		AndroidBTScanBridge.DisableBTDiscoverability();
	}

	public void SetNewTargetDeviceName()
	{
		AndroidBTScanBridge.SetScanTargetDeviceName (itsGUIManager.TargetDeviceNameInput.text);
	}

	public void StartScan()
	{
		AndroidBTScanBridge.StartScan ();
	}

	public void StopScan()
	{
		AndroidBTScanBridge.StopScan();
	}
}
