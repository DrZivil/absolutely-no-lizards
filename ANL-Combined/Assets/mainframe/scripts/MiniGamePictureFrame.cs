using UnityEngine;
using System.Collections;
using WiimoteApi;

public class MiniGamePictureFrame : MonoBehaviour {
    public GameObject energyBar;
    public float decreaseAmount = 0.01f;
    public float decreaseAmountManual = 0.01f;

    private Wiimote remotePicture;
    private float currAccel = 0f;

    public float fireRate = 0.5F;
    private float nextFire = 0.0F;

    private bool wiiMoteInitialized = false;

    // Use this for initialization
    void Start () {
        InitWiimotes();
    }

    // Update is called once per frame
    void Update () {
        // Debug.Log("wii mote init state: " + wiiMoteInitialized);
        if (wiiMoteInitialized)
        {
            InputEvents();
        }
	}

    void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            remote.SendPlayerLED(true, false, true, false);
            remotePicture = remote;
            remotePicture.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
            currAccel = remotePicture.Accel.GetCalibratedAccelData()[2];

            wiiMoteInitialized = true;
        }
    }

    void InputEvents()
    {
        //print("Wii Remote");
        float[] currData = remotePicture.Accel.GetCalibratedAccelData();
        int currDataInt = remotePicture.ReadWiimoteData();

        // print("Math Abs: " + Mathf.Abs(currAccel - currData[2]));

        if(Mathf.Abs(currAccel - currData[2]) > 0.5f && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            currAccel = currData[2];
            // print("Value has changed: " + currData[2]);
       
            if (energyBar.GetComponent<EnergyBarRiddle>().isActive)
            {
                energyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            }
        }
    }
}