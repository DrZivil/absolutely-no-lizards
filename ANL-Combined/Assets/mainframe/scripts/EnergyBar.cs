using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;
using SocketIO;

// Input
using MidiJack;

// output
using Midi;

using UnityEngine.SceneManagement;

public class EnergyBar : MonoBehaviour
{

    public bool gameStarted = false;

    public GameObject energyBar;
    public GameObject[] slaveEnergyBars;
    public GameObject TVRemoteEnergyBar;
    public GameObject MoneyReaderEnergyBar;
    public GameObject ControllerEnergyBar;
    public GameObject FakeStreetEnergyBar;
    public GameObject BeatEmUpEnergyBar;

    public GameObject FakeWiiEnergyBar;

    public GameObject resetButton;


    public bool streetFighterActive = false;

    public float repeat_rate = 0.1f;
    public float charge_rate = 0.2f;
    public float main_charge_rate = 0.3f;
    public bool gameOver = false;

    public float fireRateTV = 0.5F;
    private float nextFireTV = 0.0F;


    public float fireRateMoney = 0.5F;
    private float nextFireMoney = 0.0F;

    public float fireRateEnergy = 0.25F;
    private float nextFireEnergy = 0.0F;


    public float fireRateStreetFighter = 0.5F;
    private float nextFireStreetFighter = 0.0F;

    private int currWheelIndex;

    private float curr_energy = 0;
    private int slave_index = 0;
    private GameObject curr_slave_bar;
    private int prev_bars_down = 0;

    private int gameMasterToGameRatio = 5000;
    private int gameMasterToGameRatioActive = 1000;

    SerialPort sp = new SerialPort("COM4", 9600);

    private SocketIOComponent socket;

    // variable for Slow Opdate Time [s];
    private float nextUpdate = 0.1f;

    private float lastEmit;
    public float emitRate = 0.2f;

    // Coroutines for emitting
    private IEnumerator emitPictureFrame;

    private int green = 60;
    private int red = 15;
    private int amber = 63;
    private int yellow = 62;
    private int off = 0;

    OutputDevice outputDevice;

    public AudioClip alertSound;
    public AudioClip alertVoice;

    AudioSource mainAudio;
    public bool playAudio;

    private bool launchpadError;

    // Use this for initialization
    void Start()
    {
        Debug.Log("init Stuff");
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();

        mainAudio = GetComponent<AudioSource>();

       // Debug.Log(OutputDevice.InstalledDevices.Count);
        // Launchpad Stuff
        if(OutputDevice.InstalledDevices.Count > 1)
        {
            outputDevice = OutputDevice.InstalledDevices[1];
            //Debug.Log(outputDevice.Name);
            if (outputDevice.IsOpen) outputDevice.Close();
            if (!outputDevice.IsOpen) outputDevice.Open();
            LightLaunchpad();
        }

        SocketIOListeners();

        //ConnectArduino();

        // InvokeRepeating("setEnergyBar", 1f, repeat_rate);
        curr_slave_bar = slaveEnergyBars[slave_index];

        // Initial Border Color for first Bar
        GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;
        curr_slave_bg.GetComponent<Image>().color = Color.white;
        energyBar.transform.localScale = new Vector3(this.transform.localScale.x, curr_energy, this.transform.localScale.z);
    }

    public void LightLaunchpad()
    {
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)0, yellow);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)1, yellow);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)8, yellow);

    }

    public void SocketIOListeners()
    {
        socket.On("start game", StartGame);
        socket.On("reset game", ResetGame);
        socket.On("update", updateVariables);

        socket.On("shake pic", DecreasePicEnergy);
    }

    public void StartGame(SocketIOEvent e)
    {
        Debug.Log("Game Started");
        gameStarted = true;
    }

    public void ResetGame(SocketIOEvent e)
    {
        Debug.Log("Reset Game");
        gameStarted = false;
        SetFullEnergyToClients();
        SceneManager.LoadScene("Lizard Interface", LoadSceneMode.Single);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("Lizard Interface", LoadSceneMode.Single);
    }

    public void DecreasePicEnergy(SocketIOEvent e)
    {
        if (gameStarted)
        {
            Debug.Log("Decrease Pic Energy");
            FakeWiiEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
        }
    }

    private void updateVariables(SocketIOEvent e)
    {
        //string text = e.data["passiveMON"].ToString().Replace("\"", "");
        //float passiveMONval = float.Parse(text);
        Debug.Log("update variables: " + getFloat(e.data["passiveMON"].ToString()));
        MoneyReaderEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmount = getFloat(e.data["passiveMON"].ToString()) / gameMasterToGameRatio;
        MoneyReaderEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmountManual = getFloat(e.data["activeMON"].ToString()) / gameMasterToGameRatioActive;
        TVRemoteEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmount = getFloat(e.data["passiveTV"].ToString()) / gameMasterToGameRatio;
        TVRemoteEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmountManual = getFloat(e.data["activeTV"].ToString()) / gameMasterToGameRatioActive;
        FakeWiiEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmount = getFloat(e.data["passivePIC"].ToString()) / gameMasterToGameRatio;
        FakeWiiEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmountManual = getFloat(e.data["activePIC"].ToString()) / gameMasterToGameRatioActive;
        FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmount = getFloat(e.data["passiveBEU"].ToString()) / gameMasterToGameRatio;
        FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmountManual = getFloat(e.data["activeBEU"].ToString()) / gameMasterToGameRatioActive;
        BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmount = getFloat(e.data["passiveBEU"].ToString()) / gameMasterToGameRatio;
        BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().decreaseAmountManual = getFloat(e.data["activeBEU"].ToString()) / gameMasterToGameRatioActive;
        main_charge_rate = getFloat(e.data["lizardCharge"].ToString()) / 500;
    }

    public float getFloat(string str)
    {
        float value;
        value = float.Parse(str.Replace("\"", ""));
        return value;
    }


    // Update is called once per frame
    void Update()
    {

        //Debug.Log(gameStarted);
        if (!gameOver && gameStarted)
        {

            if(Time.time > emitRate + lastEmit)
            {
                socketEvents();
                lastEmit = Time.time;
            }

            keyEvents();
            //ArduinoEvents();

            if (allBarsDown())
            {
                gameOver = true;
                Debug.Log("Game over Bro");
                socket.Emit("game over");
                //resetButton.SetActive(true);
                OnApplicationQuit();
            }
        }
    }

    void SetFullEnergyToClients()
    {
        JSONObject currEnergyPicJson = new JSONObject(1);
        JSONObject currEnergyJson = new JSONObject(1);
        socket.Emit("curr energy pic", currEnergyPicJson);
        socket.Emit("hide bars");
        socket.Emit("curr energy", currEnergyJson);
    }

    void socketEvents()
    {
        float currEnergy = TVRemoteEnergyBar.GetComponent<EnergyBarRiddle>().getCurrEnergy();
        JSONObject currEnergyJson = new JSONObject(currEnergy);
        socket.Emit("curr energy", currEnergyJson);

        emitPictureFrame = WaitAndEmitPictureFrame(0.1f);
        StartCoroutine(emitPictureFrame);
    }

    private IEnumerator WaitAndEmitPictureFrame(float waitTime)
    {
        Debug.Log("emit energy pictureframe");
        yield return new WaitForSeconds(waitTime);
        float currEnergyPic = FakeWiiEnergyBar.GetComponent<EnergyBarRiddle>().getCurrEnergy();
        JSONObject currEnergyPicJson = new JSONObject(currEnergyPic);
        socket.Emit("pic energy", currEnergyPicJson);
    }

    void ConnectArduino()
    {
        if (sp != null)
        {
            if (sp.IsOpen)
            {
                sp.Close();
                print("Cloasing because it was allready open");
            }
            else
            {
                sp.Open();
                sp.ReadTimeout = 16;
                print("Port Opened COM4 Arduino Port 9600");
            }
        }
    }

    void OnApplicationQuit()
    {
        ClearExclamation();

        if (outputDevice.IsOpen) outputDevice.Close();
        //sp.Close();
    }

    public bool oneBarDown()
    {
        // Check if all energybars are up
        int k = 0;
        for (int i = 0; i < slaveEnergyBars.Length; i++)
        {
            if (!slaveEnergyBars[i].GetComponent<EnergyBarRiddle>().isActive)
            {
                // todo: comment in after debug:
                // sp.WriteLine(i.ToString());
                //Debug.Log(i.ToString());
                //sp.BaseStream.Flush();
                k++;
            }
        }
        if (k > 0)
        {
            if (!launchpadError)
            {
                launchpadErrorMode();
                launchpadError = true;
            }
            return true;
        }
        else
        {
            if (launchpadError)
            {
                launchpadActiveMove();
                launchpadError = false;
            }
            return false;
        }
    }

    public void launchpadActiveMove()
    {
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)0, yellow);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)1, yellow);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)8, yellow);
        CancelInvoke("FLashExclamation");
        CancelInvoke("ClearExclamation");
        if (playAudio)
        {
            CancelInvoke("AlertSound");
        }
    }

    public void launchpadErrorMode()
    {
        //outputDevice = OutputDevice.InstalledDevices[1];
        //Debug.Log(outputDevice.Name);
        //if (!outputDevice.IsOpen) outputDevice.Open();


        InvokeRepeating("FLashExclamation", 0f, 0.5f);
        InvokeRepeating("ClearExclamation", 0.25f, 0.5f);

        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)0, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)1, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)8, 0);

        if (playAudio)
        {
            InvokeRepeating("AlertSound", 0, alertSound.length + alertVoice.length);
        }
    }

    public void ClearExclamation()
    {
        Debug.Log("Clear Exclamation: " + outputDevice.Name + " - isOPen: " + outputDevice.IsOpen);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)3, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)4, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)19, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)20, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)35, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)36, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)51, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)52, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)67, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)68, 0);

        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)99, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)100, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)115, 0);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)116, 0);
    }

    public void FLashExclamation()
    {
        Debug.Log("Flash Exclamation: " + outputDevice.Name + " - isOPen: " + outputDevice.IsOpen);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)3, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)4, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)19, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)20, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)35, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)36, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)51, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)52, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)67, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)68, red);

        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)99, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)100, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)115, red);
        outputDevice.SendNoteOn(Channel.Channel1, (Pitch)116, red);
    }

    IEnumerator alertSounds()
    {
        mainAudio.clip = alertSound;
        mainAudio.Play();
        yield return new WaitForSeconds(alertSound.length);
        mainAudio.clip = alertVoice;
        mainAudio.Play();
    }

    public bool allBarsDown()
    {
        // Check if all energybars are up
        int k = 0;
        for (int i = 0; i < slaveEnergyBars.Length; i++)
        {
            if (!slaveEnergyBars[i].GetComponent<EnergyBarRiddle>().isActive)
            {
                if (sp.IsOpen)
                {
                    sp.WriteLine(i.ToString());
                    //Debug.Log(i.ToString());
                    sp.BaseStream.Flush();
                    k++;
                }
            }
        }

        // If one Bar goes down send a signal to Arduino
        if (prev_bars_down < k)
        {
            sp.WriteLine("buzz_down");
            sp.BaseStream.Flush();
            prev_bars_down = k;
        }

        // If all Bars are down send True -> Game Over Condition
        if (k >= slaveEnergyBars.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void ArduinoEvents()
    {
        if (sp.IsOpen)
        {
            try
            {
                int mappingNr = sp.ReadByte();

                // Debug.Log(mappingNr);

                if (mappingNr == 8 && Time.time > nextFireTV)
                {
                    nextFireTV = Time.time + fireRateTV;
                    TVRemoteEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
                }
                if (mappingNr == 5 && Time.time > nextFireMoney)
                {
                    nextFireMoney = Time.time + fireRateMoney;
                    MoneyReaderEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
                }
                if (mappingNr == 3)
                {
                    ActivateBarNumber(mappingNr);
                }
                if (mappingNr == 10 || mappingNr == 20 || mappingNr == 30 || mappingNr == 40)
                {
                    switchBars(mappingNr / 10);
                }
                if (mappingNr == 17)
                {
                    //Debug.Log("bring down one");
                    FakeWiiEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
                }
                if (mappingNr == 18 && Time.time > nextFireEnergy)
                {
                    nextFireEnergy = Time.time + fireRateEnergy;
                    //Debug.Log("Energy Up Button");
                    float currScaleY = energyBar.transform.localScale.y + charge_rate;
                    if (currScaleY > 1)
                    {
                        currScaleY = 1;
                    }
                    energyBar.transform.localScale = new Vector3(this.transform.localScale.x, currScaleY, this.transform.localScale.z);
                }

            }
            catch (System.TimeoutException)
            {
            }
        }
    }

    void ActivateBarNumber(int number)
    {
        //Debug.Log(number);

        if (!slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive)
        {
            int sendNumber = slave_index + 4;
            float stage = slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().stage;
            if (stage > 0.1)
            {
                slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().stage = stage - 0.1f;
                stage = stage - 0.1f;
            }

            if (sp.IsOpen)
            {
                sp.WriteLine("buzz_up");
                sp.WriteLine(sendNumber.ToString());
                sp.BaseStream.Flush();
                prev_bars_down--;
            }

            slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().energyBar.transform.localScale = new Vector3(this.transform.localScale.x, stage, this.transform.localScale.z);
            slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive = true;
            slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().setEnergyBar();
            slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().background.GetComponent<Image>().color = slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().activeBar;


            // causal beat em up stuff
            //Debug.Log(slaveEnergyBars[slave_index].transform.name);
            if (slaveEnergyBars[slave_index].transform.name == "Slave Energybar - Beat em up")
            {

                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().energyBar.transform.localScale = new Vector3(this.transform.localScale.x, stage, this.transform.localScale.z);
                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().isActive = true;
                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBar();
                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().background.GetComponent<Image>().color = slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().activeBar;

                GameObject lizzard = GameObject.Find("Lizzard");
                lizzard.GetComponent<lizzardMovement>().damage = false;
                lizzard.GetComponent<lizzardMovement>().dead = false;
                lizzard.GetComponent<damageScript>().lizardLife = stage;
                lizzard.transform.root.GetComponentInChildren<Animator>().SetTrigger("Spawn");
            }

            // print("curr Energy[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().getCurrEnergy());
            // print("isActive[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().isActive);
            // print("local Scale Y[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().energyBar.transform.localScale.y);

        }
    }

    void switchBars(int barNumber)
    {
        if (currWheelIndex != barNumber)
        {
            slave_index = (barNumber - 1) % slaveEnergyBars.Length;
            int j = 0;

            //Debug.Log("curr wheel index: " +  currWheelIndex);
            //Debug.Log("curr bar number: " +  barNumber);

            curr_slave_bar = slaveEnergyBars[slave_index];

            GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;

            // reset Border of other Bars
            for (int i = 0; i < slaveEnergyBars.Length; i++)
            {
                GameObject other_bg = slaveEnergyBars[i].GetComponent<EnergyBarRiddle>().border;
                other_bg.GetComponent<Image>().color = Color.black;
            }
            // set Border for Bars
            curr_slave_bg.GetComponent<Image>().color = Color.white;

            currWheelIndex = barNumber;
        }

    }

    void keyEvents()
    {
        /**
        * Key Input Handling
        */

        // Load Main Energy Bar
        //if (Input.GetKeyDown(KeyCode.Space) && !oneBarDown())
        if (MidiMaster.GetKeyUp(0, 0) && !oneBarDown())
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.CNeg1, yellow);

        }
        if (MidiMaster.GetKeyDown(0, 0) && !oneBarDown())
            {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.CNeg1, green);

            float currScaleY = energyBar.transform.localScale.y + main_charge_rate;
            if (currScaleY > 1)
            {
                currScaleY = 1;
            }
            energyBar.transform.localScale = new Vector3(this.transform.localScale.x, currScaleY, this.transform.localScale.z);
        }

        // Switch through Energy Bars to charge
        //if (Input.GetKeyDown(KeyCode.Tab))
        if (MidiMaster.GetKeyUp(0, 8) && !oneBarDown())
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.GSharpNeg1, yellow);
        }
        if (MidiMaster.GetKeyDown(0, 8) && !oneBarDown())
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.GSharpNeg1, green);

            slave_index = (slave_index + 1) % slaveEnergyBars.Length;
            int j = 0;
            if (!slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive)
            {
                while (!slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive && j < slaveEnergyBars.Length)
                {
                    slave_index = (slave_index + 1) % slaveEnergyBars.Length;
                    j++;
                }
                if (j == slaveEnergyBars.Length)
                {
                    //Debug.Log("Lizard Time is over");
                    gameOver = true;
                }
            }
            curr_slave_bar = slaveEnergyBars[slave_index];

            GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;

            // reset Border of other Bars
            for (int i = 0; i < slaveEnergyBars.Length; i++)
            {
                GameObject other_bg = slaveEnergyBars[i].GetComponent<EnergyBarRiddle>().border;
                other_bg.GetComponent<Image>().color = Color.black;
            }
            // set Border for Bars
            curr_slave_bg.GetComponent<Image>().color = Color.white;
        }

        // Charge selected Energy Bar
        //if (Input.GetKeyDown(KeyCode.LeftShift) && curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive)
        if (MidiMaster.GetKeyUp(0, 1) && curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive && !oneBarDown())
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.CSharpNeg1, yellow);
        }
        if (MidiMaster.GetKeyDown(0, 1) && curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive && !oneBarDown())
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.CSharpNeg1, green);
            GameObject curr_slave_energy = curr_slave_bar.GetComponent<EnergyBarRiddle>().energyBar;
            float new_energy = curr_slave_energy.transform.localScale.y + energyBar.transform.localScale.y;

            // remove consumed energy from Main Energy Bar
            float new_main_energy = energyBar.transform.localScale.y - (1 - curr_slave_energy.transform.localScale.y);
            if (new_main_energy < 0)
            {
                new_main_energy = 0;
            }

            // Transfert energy from Main Bar to Riddle Bar
            energyBar.transform.localScale = new Vector3(this.transform.localScale.x, new_main_energy, this.transform.localScale.z);
            if (new_energy > 1)
            {
                new_energy = 1;
            }

            Debug.Log(curr_slave_bar.transform.name); //todo -> figure out why this does not work
            if (curr_slave_bar.transform.name == "Slave Energybar - Beat em up" && curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive)
            {
                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().setCurrEnergy(new_energy);
                BeatEmUpEnergyBar.GetComponent<EnergyBarRiddle>().energyBar.transform.localScale = new Vector3(this.transform.localScale.x, new_energy, this.transform.localScale.z);
            }

            if (curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive)
            {
                curr_slave_bar.GetComponent<EnergyBarRiddle>().setCurrEnergy(new_energy);
                print("curr Energy[keys]: " + curr_slave_bar.GetComponent<EnergyBarRiddle>().getCurrEnergy());
                curr_slave_energy.transform.localScale = new Vector3(this.transform.localScale.x, new_energy, this.transform.localScale.z);
            }
        }
    }
}