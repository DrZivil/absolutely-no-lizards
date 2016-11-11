using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MS_Orc : MonoBehaviour {
	
	public Slider healthBarSlider;  //reference for slider
	public Image Fill;
	public Color BlockColor = Color.yellow;
	public Color AttackColor = Color.red;
	public Color IdleColor = Color.green;
	public Text gameOverText;   //reference for text
	public Transform sightStart, sightEnd, rayStart, rayEnd; //2 transforms for start and end points for the linecast
	public GameObject arrow; //arrow above the guards head for alerted status
	public float damageOutput = .00125f;
	public bool spotted, facingLeft;
	private bool isGameOver = false; //flag to see if game is over

    public GameObject FakeStreetEnergyBar;

    void Start(){
		InvokeRepeating("Patrol", 0f, Random.Range (2f, 4f)); //we use InvokeRepeating to call a function - it takes ("FunctionName", startTime, repeatTime) it loops forever until you call 'CancelInvokes'

		gameOverText.enabled = false; //disable GameOver text on start
	}
	
	// Update is called once per frame
	void Update () {
		//check if game is over i.e., health is greater than 0
//		if(!isGameOver)
//			transform.Translate(Input.GetAxis("Horizontal")*Time.deltaTime*10f, 0, 0); //get input
		RaycastStuff();
		Behaviours();
		Stances ();
	}

	void RaycastStuff() //Keep our raycast code tidy in here
	{
		Debug.DrawLine(sightStart.position, sightEnd.position, Color.magenta); //just a visual representation for the linecast
		spotted = Physics2D.Linecast(sightStart.position, sightEnd.position, 1 << LayerMask.NameToLayer("Player")); //we assign the bool 'spotted' with a linecast, that returns true or false when it touches the Player
	}

	void Behaviours()
	{
		if(spotted && healthBarSlider.value > 0 ) //checking if spotted bool is true
		{
			arrow.SetActive(true); //SetActive toggles the gameobject on and off in the scene
			Debug.DrawLine(rayStart.position, rayEnd.position, Color.red);

			//if player triggers fire object and health is greater than 0
			if (!Input.GetKey (KeyCode.P) && !Input.GetKey (KeyCode.L)) {
				healthBarSlider.value -= damageOutput;  //reduce health
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            } else if (Input.GetKey (KeyCode.P)) {
				healthBarSlider.value -= damageOutput * 0.125f; //blocking
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            } else if (Input.GetKey (KeyCode.L)) {
				healthBarSlider.value -= damageOutput * 1.25f; //attacking
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            }
		}
		else
		{
			arrow.SetActive(false);
		}
	}

	void Patrol()
	{
		facingLeft = !facingLeft; //each time the Patrol function is called, facingLeft bool switches between true and false, this is just a shorthand way of flipping it to the opposite
		
		if(facingLeft) //checking if the facingLeft bool is true
		{
			transform.eulerAngles = new Vector2(0, 0); //if facing left, set the rotation of the enemy to (0, 0) which is his default
		}
		else //if facingLeft bool is false
		{
			transform.eulerAngles = new Vector2(0, 180); //if not facing left, set the rotation of the enemy to (180, 0) which flips him to face right
		}
	}

	//Check if player enters/stays on the fire
	void OnTriggerStay(Collider other){
		if (spotted && other.gameObject.name == "Human") {

		}
		else{
			isGameOver = true;    //set game over to true
			gameOverText.enabled = true; //enable GameOver text
		}
	}

	void Stances() {
		if (Input.GetKey (KeyCode.P)) {
			Fill.color = BlockColor;
		}
		else if (Input.GetKey (KeyCode.L)) {
			Fill.color = AttackColor;
		}
		else {
			Fill.color = IdleColor;
		}
	}
}
