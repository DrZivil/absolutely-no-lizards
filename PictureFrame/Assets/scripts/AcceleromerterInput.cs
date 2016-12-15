using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using SocketIO;

public class AcceleromerterInput : NetworkBehaviour {

    public GameObject president;
    private Image presidentImage;

    public Text debug;

    // Energy Bar Stuff
    public GameObject energyBar;
    private Slider energySlider;

    public float decreaseAmount = 0.005f;
    public float decreaseAmountManual = 0.02f;
    public float repeatRate = 0.1f;

    private float curr_energy = 0f;
    private float curr_timer;
    private float last_try = 0f;
    public bool isActive;

    // Energy Bar Stuff ende


    // Socket Connection
    private SocketIOComponent socket;

    public float peakLevel = 0.6f;
    public float endCountTime = 0.025f;


    private float avrgTime = 0.5f;
    private int shakeCount;
    private int shakeDir;
    Vector3 avrgAcc = Vector3.zero;
    int countPos;
    int countNeg;
    int lastPeak;
    int firstPeak;
    bool counting;
    float timer;


    void Start()
    {
        curr_energy = 1f;
        isActive = true;

        //InvokeRepeating("setEnergyBar", 0, 0.2f);

        presidentImage = president.GetComponent<Image>();
        energySlider = energyBar.GetComponent<Slider>();

        initSocketIO();
    }

    void Update()
    {
        if (ShakeDetector())
        {
            setEnergyBarManual();
        }
        socketEvents();
    }

    void initSocketIO()
    {
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        socket.On("curr energy pic", updateEnergyBar);
    }

    void socketEvents()
    {
        JSONObject currEnergyJson = new JSONObject(curr_energy);
        socket.Emit("curr energy pic", currEnergyJson);
    }

    void updateEnergyBar(SocketIOEvent e)
    {
        Debug.Log(e.data);
        Debug.Log(e.name);
        Debug.Log(e.ToString());
        float currValue = presidentImage.color.a - decreaseAmount;

        if (currValue < 0.9)
        {
            energyBar.gameObject.SetActive(true);
        }

        // energy Level 0-1
        if (currValue > 0)
        {
            presidentImage.color = new Color(1f, 1f, 1f, currValue);
            energySlider.value = currValue;
            curr_energy = currValue;
        }
        else
        {
            currValue = 0;
            energySlider.value = currValue;
            presidentImage.color = new Color(1f, 1f, 1f, currValue);
            isActive = false;
        }
    }

    public void setEnergyBar()
    {
        float currValue = presidentImage.color.a - decreaseAmount;

        if (currValue < 0.9)
        {
            energyBar.gameObject.SetActive(true);
        }

        // energy Level 0-1
        if (currValue > 0)
        {
            presidentImage.color = new Color(1f, 1f, 1f, currValue);
            energySlider.value = currValue;
            curr_energy = currValue;
        }
        else
        {
            currValue = 0;
            energySlider.value = currValue;
            presidentImage.color = new Color(1f, 1f, 1f, currValue);
            isActive = false;
        }
    }

    public void setEnergyBarManual()
    {
        if (isActive)
        {
            float currValue = presidentImage.color.a - decreaseAmountManual;
            //Debug.Log("currScaleY: " + currScaleY);
            //Debug.Log("Decrease Amount: " + decreaseAmountManual);


            if (currValue > 0)
            {
                presidentImage.color = new Color(1f, 1f, 1f, currValue);
                energySlider.value = currValue;
                curr_energy = currValue;
            }
            else
            {
                currValue = 0;
                presidentImage.color = new Color(1f, 1f, 1f, currValue);
                energySlider.value = currValue;
                isActive = false;
            }
        }
    }

    bool ShakeDetector()
    {
        // read acceleration:
        Vector3 curAcc = Input.acceleration;
        // update average value:
        avrgAcc = Vector3.Lerp(avrgAcc, curAcc, avrgTime * Time.deltaTime);


        // calculate peak size:
        curAcc -= avrgAcc;

        // variable peak is zero when no peak detected...
        int peak = 0;

        // or +/- 1 according to the peak polarity:
        if (curAcc.y > peakLevel)
        {
            peak = 1;
        }
        if (curAcc.y < -peakLevel)
        {
            peak = -1;
        }

        // do nothing if peak is the same of previous frame:
        if (peak == lastPeak)
        {
            return false;
        }

        // peak changed state: process it
        lastPeak = peak; // update lastPeak

        if (peak != 0)
        { // if a peak was detected...
            timer = 0; // clear end count timer...

            // and increment corresponding count
            if (peak > 0){
                countPos++;
            }
            else
            {
                countNeg++;
            }

            if (!counting)
            { // if it's the first peak...
                counting = true; // start shake counting
                firstPeak = peak; // save the first peak direction
            }
        }
        else // but if no peak detected...
        if (counting)
        { // and it was counting...

            timer += Time.deltaTime; // increment timer

            //Debug.Log("timer: " + timer + " Endcount Time: " + endCountTime);

            if (timer > endCountTime)
            { // if endCountTime reached...
                //Debug.Log("is Shaking");
                counting = false; // finish counting...
                shakeDir = firstPeak; // inform direction of first shake...
                if (countPos > countNeg)
                {
                    shakeCount = countPos;
                }
                else
                {
                    shakeCount = countNeg;
                }
                // zero counters and become ready for next shake count
                countPos = 0;
                countNeg = 0;

                return true; // count finished
            }
        }
        return false;
    }
}
