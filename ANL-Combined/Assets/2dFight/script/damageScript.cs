using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class damageScript : MonoBehaviour {

	public float lizardLife = 100f;
	public int damage = 10;

    public GameObject FakeStreetEnergyBar;
    public GameObject FakeStreetEnergyBarBeatEmUp;

    void start()
    {
    }

	void OnTriggerEnter2D(Collider2D col){
        //Debug.Log("col.transform.root: " + col.transform.root);
        //Debug.Log("transform.root: " + transform.root);
        //Debug.Log("col.tag: " + col.tag);
        //Debug.Log("!col.isTrigger: " + !col.isTrigger);
        if (col.transform.root != transform.root && col.tag != "Ground")
        {
			if (!transform.GetComponent<lizzardMovement> ().damage) {
				transform.GetComponent<lizzardMovement> ().damage = true;
				transform.root.GetComponentInChildren<Animator> ().SetTrigger ("Damage");
			}
		}
		killIt ();
	}

	public void killIt(){
        FakeStreetEnergyBar = GameObject.Find("Slave Energybar - Beat em up");
        FakeStreetEnergyBarBeatEmUp = GameObject.Find("Slave Energybar - Beat em up 2");
        FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
        FakeStreetEnergyBarBeatEmUp.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
        //Debug.Log("Curr Energy: " + FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().getCurrEnergy());
        if(FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().getCurrEnergy() == 0)
        {
            lizardLife = 0;
            transform.root.GetComponentInChildren<Animator>().SetTrigger("Dead");
            transform.GetComponent<lizzardMovement>().damage = true;
            transform.GetComponent<lizzardMovement>().dead = true;
        }
		//healtBar.size = lizardLife/100;
	}

	void Update(){
		
		if(lizardLife <= 0 ){
			//SceneManager.LoadScene ("1");
		}
	}
}
