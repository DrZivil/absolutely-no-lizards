using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
	public Slider healthBarSlider;
	public float runSpeed = 4f;
	public float jumpForce = 500f;
	public float damageOutput = .00125f;
	public bool grounded;
    public GameObject FakeStreetEnergyBar;
    public bool interact = false; //bool for checking if player is grounded so they can jump, and bool for interact, so that player can only interact when in range of thing to interact with
	public Transform groundedEnd, lineStart, lineEnd; //transform variable for the end points of the linecasts

	RaycastHit2D interacted; //a variable type that stores a collider that was hit during linecast

	void Update()
	{
		Movement(); //call the function every frame
		RaycastStuff(); //call the function every frame
	}

	void RaycastStuff()
	{
		//Just a debug visual representation of the Linecast, can only see this in scene view! Doesn't actually do anything!
		Debug.DrawLine(lineStart.position, lineEnd.position, Color.magenta);
		Debug.DrawLine(this.transform.position, groundedEnd.position, Color.magenta);

		//we assign the bool 'ground' with a linecast, that returns true or false when the end of line 'jumpCheck' touches the ground
		grounded = Physics2D.Linecast(this.transform.position, groundedEnd.position, 1 << LayerMask.NameToLayer("Ground"));  

		//Using linecast which takes (start point, end point, layermask) so we can make it only detect objects with specified layers
		//its wrapped in an if statement, so that while the tip of the Linecast (interactCheck.position) is touching an object with layer 'Guard', the code inside executes
		if(Physics2D.Linecast(lineStart.position, lineEnd.position, 1 << LayerMask.NameToLayer("Enemy")))
		{
			//we store the collider object the Linecast hit so that we can do something with that specific object, ie. the guard
			//each time the linecast touches a new object with layer "guard", it updates 'interacted' with that specific object instance
			interacted = Physics2D.Linecast(lineStart.position, lineEnd.position, 1 << LayerMask.NameToLayer("Enemy")); 
			interact = true; //since the linecase is touching the guard and we are in range, we can now interact!
		}
		else
		{
			interact = false; //if the linecast is not touching a guard, we cannot interact
		}

//		Physics2D.IgnoreLayerCollision(8, 10); //if we want certain layers to ignore each others collision, we use this! the number is the layer number in the layers list
	}


	void Movement() //function that stores all the movement
	{
//		transform.Translate (Input.GetAxis ("Horizontal") * Time.deltaTime * 10f, 0, 0);
		if(Input.GetKey (KeyCode.D))
		{
			transform.Translate(Vector3.right * runSpeed * Time.deltaTime); 
			transform.eulerAngles = new Vector2(0, 0); //this sets the rotation of the gameobject
		}

		if(Input.GetKey (KeyCode.A))
		{
			transform.Translate(Vector3.right * runSpeed * Time.deltaTime);
			transform.eulerAngles = new Vector2(0, 180); //this sets the rotation of the gameobject
		}
		if(Input.GetKey(KeyCode.Space) && grounded) // If the jump button is pressed and the player is grounded then the player jumps 
		{
			this.GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce);
		}
		
		if(Input.GetKeyDown(KeyCode.E) && interact) //if you press E and interact is set to true
		{
			if (Input.GetKey (KeyCode.P)) {
				healthBarSlider.value -= damageOutput * 0.5f; //blocking
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            } else if (Input.GetKey (KeyCode.L)) {
				healthBarSlider.value -= damageOutput * 1.25f; //attacking
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            } else {
				healthBarSlider.value -= damageOutput;  //reduce health
                FakeStreetEnergyBar.GetComponent<EnergyBarRiddle>().setEnergyBarManual();
            }
		}



		if(Input.GetKeyDown (KeyCode.JoystickButton1) && grounded) // If the jump button is pressed and the player is grounded then the player jumps 
		{
			this.GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce);
		}

		if(Input.GetKeyDown (KeyCode.Joystick1Button0) && interact) //if you press E and interact is set to true
		{
//			interacted.collider.gameObject.GetComponent<Animation>().Play (); //access the gameobject of the collider stored in 'interacted' back in the linecast code, and tell its animation component to play the default animation
			healthBarSlider.value -=.0125f;
		}
	}
}
