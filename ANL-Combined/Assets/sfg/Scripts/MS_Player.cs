using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MS_Player : MonoBehaviour {
	
	public Slider healthBarSlider;  //reference for slider
	public Text gameOverText;   //reference for text
	public float runSpeed = 4f;
	public bool grounded;
	private bool isGameOver = false; //flag to see if game is over
	
	void Start(){
		gameOverText.enabled = false; //disable GameOver text on start
	}
	
	// Update is called once per frame
	void Update () {
//		Raycast ();
		//check if game is over i.e., health is greater than 0
		if (!isGameOver) {
			transform.Translate (Input.GetAxis ("Horizontal") * runSpeed * Time.deltaTime,0,0); //get input
		}
		if(Input.GetKeyDown (KeyCode.Space) && grounded) {
			GetComponent<Rigidbody2D>().AddForce(Vector2.up * 200f);
		}
	}

//	void Raycast() {
//		grounded = Physics2D.Linecast(transform.position, jumpCheck.position, 1 << LayerMask.NameToLayer("Ground"));
//	}
		
	//Check if player enters/stays on the fire
	void OnTriggerStay(Collider other){
		//if player triggers fire object and health is greater than 0
		if(other.gameObject.name=="Lizard" && healthBarSlider.value>0 && !Input.GetKey (KeyCode.F) && !Input.GetKey (KeyCode.R)){
			healthBarSlider.value -=.00125f;  //reduce health
		} 
		else if (other.gameObject.name=="Lizard" && healthBarSlider.value>0 && Input.GetKey (KeyCode.F)){
			healthBarSlider.value -=.0000f; //blocking
		} 
		else if (other.gameObject.name=="Lizard" && healthBarSlider.value>0 && Input.GetKey (KeyCode.R)){
			healthBarSlider.value -=.0033f; //attacking
		}
		else{
			isGameOver = true;    //set game over to true
			gameOverText.enabled = true; //enable GameOver text
		}
	}
}
