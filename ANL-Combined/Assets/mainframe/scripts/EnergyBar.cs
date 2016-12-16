using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;
using SocketIO;

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

    // Use this for initialization
    void Start()
    {
        Debug.Log("init Stuff");
        string test = "42";
        Debug.Log(test);
        float testFloat = float.Parse(test);
        Debug.Log(testFloat);

        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();

        SocketIOListeners();

        //ConnectArduino();

        //InvokeRepeating("setEnergyBar", 1f, repeat_rate);
        curr_slave_bar = slaveEnergyBars[slave_index];

        // Initial Border Color for first Bar
        GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;
        curr_slave_bg.GetComponent<Image>().color = Color.white;
        energyBar.transform.localScale = new Vector3(this.transform.localScale.x, curr_energy, this.transform.localScale.z);
    }

    public void SocketIOListeners()
    {
        socket.On("start game", StartGame);
        socket.On("reset game", ResetGame);
        socket.On("update", updateVariables);
    }

    public void StartGame(SocketIOEvent e)
    {
        Debug.Log("Game Started");
        gameStarted = true;
    }

    public void ResetGame(SocketIOEvent e)
    {
        Debug.Log("Reset Game");
        SceneManager.LoadScene("Lizard Interface", LoadSceneMode.Single);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("Lizard Interface", LoadSceneMode.Single);
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
        main_charge_rate = getFloat(e.data["lizardCharge"].ToString()) / 1000;
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

            //socketEvents();

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

    void socketEvents()
    {
        // todo activate when arduino is connected 
        //int mappingNr = sp.ReadByte();
        float currEnergy = GameObject.Find("Slave Energybar - TV").transform.GetComponent<EnergyBarRiddle>().getCurrEnergy();
        float currEnergyPic = GameObject.Find("Slave Energybar - Picture Frame").transform.GetComponent<EnergyBarRiddle>().getCurrEnergy();
        JSONObject currEnergyPicJson = new JSONObject(currEnergyPic);
        JSONObject currEnergyJson = new JSONObject(currEnergy);
        //Debug.Log("Socket Events");
        socket.Emit("curr energy", currEnergyJson);
        socket.Emit("curr energy pic", currEnergyPicJson);
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
        sp.Close();
    }

    public bool oneBarDown()
    {
        // Check if all energybars are up
        int k = 0;
        for (int i = 0; i < slaveEnergyBars.Length; i++)
        {
            if (!slaveEnergyBars[i].GetComponent<EnergyBarRiddle>().isActive)
            {
                sp.WriteLine(i.ToString());
                //Debug.Log(i.ToString());
                sp.BaseStream.Flush();
                k++;
            }
        }
        if (k > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
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
        if (Input.GetKeyDown(KeyCode.Space) && !oneBarDown())
        {
            float currScaleY = energyBar.transform.localScale.y + main_charge_rate;
            if (currScaleY > 1)
            {
                currScaleY = 1;
            }
            energyBar.transform.localScale = new Vector3(this.transform.localScale.x, currScaleY, this.transform.localScale.z);
        }

        // Switch through Energy Bars to charge
        if (Input.GetKeyDown(KeyCode.Tab))
        {
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive)
        {
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