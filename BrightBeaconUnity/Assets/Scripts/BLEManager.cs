using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BLEManager : MonoBehaviour
{
    private static BLEManager instance;
    public static BLEManager Instance
    {
        get
        {
            if (!instance)
                instance = FindObjectOfType<BLEManager>();

            if (!instance)
            {
                GameObject go = GameObject.Find("Main Camera");
                instance = go.AddComponent<BLEManager>();
            }

            return instance;
        }
    }


    public static float N_FACTOR = 4f;
    public static AndroidJavaObject bleLib = null;
    public Dictionary<string, BRTBeacon> beaconTable = new Dictionary<string, BRTBeacon>();


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start to initialize");
        AndroidJavaClass ble = new AndroidJavaClass("com.ncs.ble.BLEApplication");
        bleLib = ble.CallStatic<AndroidJavaObject>("getInstance");
    }

    public void StartScan()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        bleLib.Call("startBTScan");
#endif
    }

    //Message receive from java jar, gameobject name must be "BLEObject" currently
    public void onUpdateBeacon(string message)
    {
        List<BRTBeacon> beacons = JsonConvert.DeserializeObject<List<BRTBeacon>>(message);
        updateDevice(beacons);
    }

    //Message receive from java jar, gameobject name must be "BLEObject" currently
    public void onNewBeacon(string message)
    {
        BRTBeacon beacon = JsonConvert.DeserializeObject<BRTBeacon>(message);
        addDevice(beacon);
    }

    //Message receive from java jar, gameobject name must be "BLEObject" currently
    public void onGoneBeacon(string message)
    {
        BRTBeacon beacon = JsonConvert.DeserializeObject<BRTBeacon>(message);
        removeDevice(beacon);
    }

    //Message receive from java jar, gameobject name must be "BLEObject" currently
    public void onLogMessage(string message)
    {
        Debug.Log(message);
    }

    private void addDevice(BRTBeacon beacon)
    {
        if (!beacon.isBrightBeacon)
            return;

        string name = beacon.macAddress.Replace("-", "");
        if (beaconTable.ContainsKey(name))
            return;

        beacon.distance = RSSIToDistance(beacon);
        beaconTable.Add(name, beacon);
        BLEUIManager.Instance.AddDeviceUI(beacon);
    }

    private void removeDevice(BRTBeacon beacon)
    {
        if (!beacon.isBrightBeacon)
            return;

        string name = beacon.macAddress.Replace("-", "");
        if (!beaconTable.ContainsKey(name))
            return;

        beaconTable.Remove(name);
        BLEUIManager.Instance.RemoveDeviceUI(beacon);
    }

    private void updateDevice(List<BRTBeacon> beacons)
    {
        foreach (var beacon in beacons)
        {
            if (beacon.isBrightBeacon)
            {
                string name = beacon.macAddress.Replace("-", "");
                if (beaconTable.ContainsKey(name))
                {
                    beacon.distance = RSSIToDistance(beacon);
                    beaconTable[name] = beacon;
                    BLEUIManager.Instance.UpdateDeviceUI(beacon);
                }
            }
        }
    }

    public static float RSSIToDistance(BRTBeacon beaconInfo)
    {
        if (beaconInfo.rssi == 0)
        {
            return -1.0f;
        }
        else
        {
            //Distance = 10^((Measured Power - Instant RSSI)/10*N).
            return Mathf.Pow(10, (beaconInfo.measuredPower - beaconInfo.rssi) / (10 * N_FACTOR));
        }
    }
}

[System.Serializable]
public class BRTBeacon
{
    public string uuid = "";
    public string name = "";
    public string macAddress = "";
    public int major;
    public int minor;
    public int measuredPower;
    public int rssi;
    public int battery;
    public int temperature;
    public bool isBrightBeacon;
    public int hardwareType;
    public int firmwareNum;
    [JsonIgnore]
    public float distance;
}
