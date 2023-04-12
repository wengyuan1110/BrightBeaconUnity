using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BLEUIManager : MonoBehaviour
{
    private static BLEUIManager instance;
    public static BLEUIManager Instance
    {
        get
        {
            if (!instance)
                instance = FindObjectOfType<BLEUIManager>();

            return instance;
        }
    }

    //scrollview
    public RectTransform uiBeaconContainer;
    public GameObject beaconInfoTemplate;
    public Button scanTriggerButton;

    public bool isScanningOn;

    private void Start()
    {
        scanTriggerButton.onClick.AddListener(() => {
            isScanningOn = !isScanningOn;
            BLEManager.Instance.TriggerScan(isScanningOn);
        });
        BLEManager.Instance.startScanEvent += () =>
        {
            isScanningOn = true;
            scanTriggerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Scanning... Stop Now";
        };
        BLEManager.Instance.stopScanEvent += () =>
        {
            isScanningOn = false;
            scanTriggerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Scan";
        };
        BLEManager.Instance.onUpdateBeaconEvent += (beacon) => UpdateDeviceUI(beacon);
        BLEManager.Instance.onNewBeaconEvent += (beacon) => AddDeviceUI(beacon);
        BLEManager.Instance.onGoneBeaconEvent += (beacon) => RemoveDeviceUI(beacon);
    }


    public void AddDeviceUI(BRTBeacon beacon)
    {
        Debug.Log("AddDeviceUI");
        string beaconName = beacon.macAddress.Replace("-", "");
        RectTransform beaconInfo = GameObject.Instantiate(beaconInfoTemplate).GetComponent<RectTransform>();
        beaconInfo.Find("UUID").GetComponent<TextMeshProUGUI>().text = "UUID: " + beacon.uuid;
        beaconInfo.Find("Name").GetComponent<TextMeshProUGUI>().text = "Name: " + beacon.name;
        beaconInfo.Find("RSSI").GetComponent<TextMeshProUGUI>().text = "RSSI: " + beacon.rssi;
        beaconInfo.Find("MPower").GetComponent<TextMeshProUGUI>().text = "MPower: " + beacon.measuredPower;
        beaconInfo.Find("MAC").GetComponent<TextMeshProUGUI>().text = "MAC: " + beacon.macAddress;
        beaconInfo.Find("DIS").GetComponent<TextMeshProUGUI>().text = "Distance: " + beacon.distance;
        beaconInfo.name = beaconName;


        beaconInfo.GetComponent<RectTransform>().SetParent(uiBeaconContainer);
        uiBeaconContainer.sizeDelta = new Vector2(uiBeaconContainer.sizeDelta.x, uiBeaconContainer.sizeDelta.y + 200);
        beaconInfo.SetAsFirstSibling();
        beaconInfo.localScale = Vector3.one;
    }

    public void RemoveDeviceUI(BRTBeacon beacon)
    {
        string beaconName = beacon.macAddress.Replace("-", "");
        DestroyImmediate(uiBeaconContainer.Find(beaconName).gameObject);
        uiBeaconContainer.sizeDelta = new Vector2(uiBeaconContainer.sizeDelta.x, uiBeaconContainer.sizeDelta.y - 200);
    }


    public void UpdateDeviceUI(BRTBeacon beacon)
    {
        string beaconName = beacon.macAddress.Replace("-", "");
        RectTransform beaconInfo = uiBeaconContainer.Find(beaconName).GetComponent<RectTransform>();
        beaconInfo.Find("RSSI").GetComponent<TextMeshProUGUI>().text = "RSSI: " + beacon.rssi;
        beaconInfo.Find("DIS").GetComponent<TextMeshProUGUI>().text = "Distance: " + beacon.distance;
    }
}
