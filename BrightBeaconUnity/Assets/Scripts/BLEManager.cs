using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;

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

        /*
        using(StreamReader sw = new StreamReader(Application.dataPath+"/demo.json"))
        {
            string line = sw.ReadToEnd();
            Debug.Log(line);
            BRTBeacon brt = JsonConvert.DeserializeObject<BRTBeacon>(line);
            Debug.Log(brt.uuid);
        }

        BRTBeacon b1 = new BRTBeacon { name="A",uuid="12183921u981",measuredPower=-65, rssi=-59, macAddress="sajdiasdas"};
        AddDevice(b1);
        BRTBeacon b2 = new BRTBeacon { name = "B", uuid = "218393921u981", measuredPower = -65, rssi = -73, macAddress = "zxccoewmas" };
        AddDevice(b2);
        BRTBeacon b3 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas"};
        BRTBeacon b4 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b5 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b6 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b7 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b8 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b9 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };
        BRTBeacon b10 = new BRTBeacon { name = "C", uuid = "9304-921u981", measuredPower = -65, rssi = -64, macAddress = "pqwjeowqdas" };

        AddDevice(b3);
        AddDevice(b4);
        AddDevice(b5);
        AddDevice(b6);
        AddDevice(b7);
        AddDevice(b8);
        AddDevice(b9);
        AddDevice(b10);
        */
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
        Debug.Log("****************** updated beacons: " + beacons.Count + " ******************");
        int brightBeaconsCount = 0;
        int brightBeaconsName = 0;
        foreach (var beacon in beacons)
        {
            if (beacon.name == "A02" || beacon.name == "A03" || beacon.name.Contains("BrtBeacon"))
                brightBeaconsName++;

            if (beacon.isBrightBeacon)
            {
                brightBeaconsCount++;
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
        Debug.Log("****************** updated bright beacons by name: " + brightBeaconsName + " ******************");
        Debug.Log("****************** updated bright beacons: " + brightBeaconsCount + " ******************");
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

        /*
        if (beaconInfo.rssi == 0)
        {
            return -1.0f;
        }
        else
        {
            float ratio = beaconInfo.rssi / beaconInfo.measuredPower;
            float rssiCorrection = 0.96f + Mathf.Pow(Mathf.Abs(beaconInfo.rssi), 3.0f) % 10.0f / 150.0f;
            return ratio <= 1.0f ? Mathf.Pow(ratio, 9.98f) * rssiCorrection : (0.103f + 0.89978f * Mathf.Pow(ratio, 7.71f)) * rssiCorrection;
        }
        */

        //if (rssi == 0)
        //{
        //    return -1.0f;
        //}

        //float ratio = rssi * 1.0f / measurePower;
        //if (ratio < 1.0)
        //{
        //    return Mathf.Pow(ratio, 10);
        //}
        //else
        //{
        //    var distance = (0.89976f) * Mathf.Pow(ratio, 7.7095f) + 0.111f;
        //    return distance;
        //}
        return 0;
    }

    public static string formatProximityUUID(string proximityUUID)
    {
        if (proximityUUID == null)
        {
            return "";
        }
        else
        {
            string withoutDashes = proximityUUID.Replace("-", "").ToLower();
            if (withoutDashes.Length != 32) {
                Debug.Log("Proximity UUID must be 32 characters without dashes");
                return null;
            }
            return string.Format("%s-%s-%s-%s-%s", withoutDashes.Substring(0, 8), withoutDashes.Substring(8, 12), withoutDashes.Substring(12, 16), withoutDashes.Substring(16, 20), withoutDashes.Substring(20, 32));
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
