using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnergyBarRiddle : MonoBehaviour
    {

    public GameObject energyBar;
    public GameObject background;
    public GameObject border;

    public GameObject arduinoMain;

    public float decreaseAmount = 0.005f;
    public float decreaseAmountManual = 0.005f;
    public float repeatRate = 0.1f;

    public Color disabledBar = Color.magenta;
    public Color activeBar = Color.red;

    public bool isActive;

    private float curr_energy = 0f;
    private float curr_timer;
    private float last_try = 0f;

    public float stage = 1;


    // Use this for initialization
    void Start ()
    {
        curr_energy = 1f;
        isActive = true;
        InvokeRepeating("setEnergyBar", 0, 0.2f);
	}

    public float getCurrEnergy()
    {
        return curr_energy;
    }


	
	// Update is called once per frame
	void Update ()
    {

    }

    public void setCurrEnergy(float energy)
    {
        curr_energy = energy;
    }


    public void setEnergyBar()
    {
        float currScaleY = energyBar.transform.localScale.y - decreaseAmount;

        // energy Level 0-1
        if (currScaleY > 0)
        {
            energyBar.transform.localScale = new Vector3(this.transform.localScale.x, currScaleY, this.transform.localScale.z);
            curr_energy = currScaleY;
        }
        else
        {
            // print("Ende Energie");
            background.GetComponent<Image>().color = disabledBar;
            isActive = false;
        }
    }
    public void setEnergyBarManual()
    {
        if (isActive)
        {
            float currScaleY = energyBar.transform.localScale.y - decreaseAmountManual;
            //Debug.Log("currScaleY: " + currScaleY);
            //Debug.Log("Decrease Amount: " + decreaseAmountManual);

            // energy Level 0-1
            if (currScaleY > 0)
            {
                energyBar.transform.localScale = new Vector3(this.transform.localScale.x, currScaleY, this.transform.localScale.z);
                curr_energy = currScaleY;
            }
            else
            {
                // print("Ende Energie");
                energyBar.transform.localScale = new Vector3(this.transform.localScale.x, 0, this.transform.localScale.z);
                curr_energy = 0;
                background.GetComponent<Image>().color = disabledBar;
                isActive = false;
            }
        }
    }
}
