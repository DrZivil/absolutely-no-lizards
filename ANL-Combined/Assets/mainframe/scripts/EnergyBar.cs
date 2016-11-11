﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;
using SocketIO;
using WiimoteApi;

public class EnergyBar : MonoBehaviour
    {

    public GameObject energyBar;
    public GameObject[] slaveEnergyBars;
    public GameObject TVRemoteEnergyBar;
    public GameObject MoneyReaderEnergyBar;
    public GameObject ControllerEnergyBar;
    public GameObject FakeStreetEnergyBar;

    public GameObject FakeWiiEnergyBar;

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

    Wiimote theRemote;

    SerialPort sp = new SerialPort("COM4", 9600);

    private SocketIOComponent socket;

    // variable for Slow Opdate Time [ms];
    private float nextUpdate = 300;

    // Use this for initialization
    void Start ()
    {
        InitWiimotes();

        InitSocketIO();
        ConnectArduino();

        //InvokeRepeating("setEnergyBar", 1f, repeat_rate);
        curr_slave_bar = slaveEnergyBars[slave_index];

        // Initial Border Color for first Bar
        GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;
        curr_slave_bg.GetComponent<Image>().color = Color.white;
        energyBar.transform.localScale = new Vector3(this.transform.localScale.x, curr_energy, this.transform.localScale.z);
    }


    //init wiimotes
    void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        //Debug.Log(WiimoteManager.Wiimotes);
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            Debug.Log("remotes da");
            remote.SendPlayerLED(true, false, true, false);
            theRemote = remote;
        }
    }
    void FinishedWithWiimotes()
    {
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            WiimoteManager.Cleanup(remote);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (!gameOver)
        {
            if(Time.time > nextUpdate)
            {
                slowUpdate();
                nextUpdate = Time.time + nextUpdate;
            }

            keyEvents();
            ArduinoEvents();

            if (allBarsDown())
            {
                gameOver = true;
                Debug.Log("Game over Bro");
                OnApplicationQuit();
            }
        }
    }

    void slowUpdate()
    {
        socketEvents();
    }

    void InitSocketIO()
    {
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();

        //socket.On("open", TestOpenSocket);
        //socket.On("error", TestErrorSocket);
        //socket.On("close", TestCloseSocket);
    }

    public void TestOpenSocket(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
    }
    public void TestErrorSocket(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
    }

    public void TestCloseSocket(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
    }


    void socketEvents()
    {
        // todo activate when arduino is connected 
        //int mappingNr = sp.ReadByte();
        float currEnergy = GameObject.Find("Slave Energybar - TV").transform.GetComponent<EnergyBarRiddle>().getCurrEnergy();
        JSONObject currEnergyJson = new JSONObject(currEnergy);
        socket.Emit("curr energy", currEnergyJson);
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
        if(prev_bars_down < k)
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

                    Debug.Log(mappingNr);
                if(mappingNr != 40)
                {
                }

                if (mappingNr == 8 && Time.time > nextFireTV)
                 {
                    nextFireTV = Time.time + fireRateTV;
                    TVRemoteEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
                }
                if(mappingNr == 5 && Time.time > nextFireMoney)
                {
                    nextFireMoney = Time.time + fireRateMoney;
                    MoneyReaderEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
                }
                if(mappingNr == 3)
                {
                    ActivateBarNumber(mappingNr);
                }
                if(mappingNr == 10 || mappingNr == 20 || mappingNr == 30 || mappingNr == 40)
                {
                    switchBars(mappingNr / 10);
                }
                if(mappingNr == 17)
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

        if(!slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive)
        {
            int sendNumber = slave_index + 4;
            float stage = slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().stage;
            if(stage > 0.1)
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

            // print("curr Energy[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().getCurrEnergy());
            // print("isActive[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().isActive);
            // print("local Scale Y[arduino]: " + slaveEnergyBars[number].GetComponent<EnergyBarRiddle>().energyBar.transform.localScale.y);

        }
    }

    void switchBars(int barNumber)
    {
        if(currWheelIndex != barNumber)
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
            if(currScaleY > 1)
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
                while(!slaveEnergyBars[slave_index].GetComponent<EnergyBarRiddle>().isActive && j < slaveEnergyBars.Length)
                {
                    slave_index  = (slave_index + 1) % slaveEnergyBars.Length;
                    j++;
                }
                if(j == slaveEnergyBars.Length)
                {
                    //Debug.Log("Lizard Time is over");
                    gameOver = true;
                }
            }
            curr_slave_bar = slaveEnergyBars[slave_index];

            GameObject curr_slave_bg = curr_slave_bar.GetComponent<EnergyBarRiddle>().border;

            // reset Border of other Bars
            for(int i = 0 ; i < slaveEnergyBars.Length; i++)
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
            if(new_main_energy < 0)
            {
                new_main_energy = 0;
            }

            // Transfert energy from Main Bar to Riddle Bar
            energyBar.transform.localScale = new Vector3(this.transform.localScale.x, new_main_energy, this.transform.localScale.z);
            if (new_energy > 1)
            {
                new_energy = 1;
            }
            if (curr_slave_bar.GetComponent<EnergyBarRiddle>().isActive)
            {
                curr_slave_bar.GetComponent<EnergyBarRiddle>().setCurrEnergy(new_energy);
                if(curr_slave_bar.transform.name == "Slave Energybar - Beat em up")
                {
                    GameObject beat_em_up_bar = GameObject.Find("Slave Energybar - Beat em up 2");
                    beat_em_up_bar.GetComponent<EnergyBarRiddle>().setCurrEnergy(new_energy);
                    Debug.Log(curr_slave_bar.transform);
                    Debug.Log(beat_em_up_bar.transform);
                    beat_em_up_bar.GetComponent<EnergyBarRiddle>().energyBar.transform.localScale = new Vector3(this.transform.localScale.x, new_energy, this.transform.localScale.z);
                }
                print("curr Energy[keys]: " + curr_slave_bar.GetComponent<EnergyBarRiddle>().getCurrEnergy());
                curr_slave_energy.transform.localScale = new Vector3(this.transform.localScale.x, new_energy, this.transform.localScale.z);
            }
        }


        // todo: put in sp events and adapt after arduino is connected - allso give lizzard new energy als lizardLife
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            GameObject lizzard = GameObject.Find("Lizzard");
            lizzard.GetComponent<lizzardMovement>().damage = false;
            lizzard.GetComponent<lizzardMovement>().dead = false;
            lizzard.transform.root.GetComponentInChildren<Animator>().SetTrigger("Spawn");
            switchBars(1);
            ActivateBarNumber(3);
        }
    }
}