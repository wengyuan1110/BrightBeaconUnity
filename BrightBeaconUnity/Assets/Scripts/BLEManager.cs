using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    public RectTransform uiBeaconContainer;
    public GameObject beaconInfoTemplate;

    public static AndroidJavaObject bleLib = null;
    public Dictionary<string, BRTBeacon> beaconTable = new Dictionary<string, BRTBeacon>();

    private float N_FACTOR = 4f;


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

    //Message receive from java jar, gameobject name must be "Main Camera" currently
    public void onUpdateBeacon(string message)
    {
        List<BRTBeacon> beacons = JsonConvert.DeserializeObject<List<BRTBeacon>>(message);
        updateDevice(beacons);
    }

    //Message receive from java jar, gameobject name must be "Main Camera" currently
    public void onNewBeacon(string message)
    {
        BRTBeacon beacon = JsonConvert.DeserializeObject<BRTBeacon>(message);
        addDevice(beacon);
    }

    //Message receive from java jar, gameobject name must be "Main Camera" currently
    public void onGoneBeacon(string message)
    {
        BRTBeacon beacon = JsonConvert.DeserializeObject<BRTBeacon>(message);
        removeDevice(beacon);
    }

    //Message receive from java jar, gameobject name must be "Main Camera" currently
    public void OnLogMessage(string message)
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

        beaconTable.Add(name, beacon);
        RectTransform beaconInfo = GameObject.Instantiate(beaconInfoTemplate).GetComponent<RectTransform>();
        beaconInfo.Find("UUID").GetComponent<TextMeshProUGUI>().text = "UUID: " + beacon.uuid;
        beaconInfo.Find("Name").GetComponent<TextMeshProUGUI>().text = "Name: " + beacon.name;
        beaconInfo.Find("RSSI").GetComponent<TextMeshProUGUI>().text = "RSSI: " + beacon.rssi;
        beaconInfo.Find("MPower").GetComponent<TextMeshProUGUI>().text = "MPower: " + beacon.measuredPower;
        beaconInfo.Find("MAC").GetComponent<TextMeshProUGUI>().text = "MAC: " + beacon.macAddress;
        beaconInfo.Find("DIS").GetComponent<TextMeshProUGUI>().text = "Distance: " + RSSIToDistance(beacon);
        beaconInfo.name = name;


        beaconInfo.GetComponent<RectTransform>().SetParent(uiBeaconContainer);
        uiBeaconContainer.sizeDelta = new Vector2(uiBeaconContainer.sizeDelta.x, uiBeaconContainer.sizeDelta.y + 200);
        beaconInfo.SetAsFirstSibling();
        beaconInfo.localScale = Vector3.one;

    }

    private void removeDevice(BRTBeacon beacon)
    {
        if (!beacon.isBrightBeacon)
            return;

        string name = beacon.macAddress.Replace("-", "");
        if (!beaconTable.ContainsKey(name))
            return;

        beaconTable.Remove(name);

        DestroyImmediate(uiBeaconContainer.Find(name).gameObject);
        uiBeaconContainer.sizeDelta = new Vector2(uiBeaconContainer.sizeDelta.x, uiBeaconContainer.sizeDelta.y - 200);
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
                    beaconTable[name] = beacon;
                    RectTransform beaconInfo = uiBeaconContainer.Find(name).GetComponent<RectTransform>();
                    beaconInfo.Find("RSSI").GetComponent<TextMeshProUGUI>().text = "RSSI: " + beacon.rssi;
                    beaconInfo.Find("DIS").GetComponent<TextMeshProUGUI>().text = "Distance: " + RSSIToDistance(beacon);
                }
            }
        }
    }

    private float RSSIToDistance(BRTBeacon beaconInfo)
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
}
