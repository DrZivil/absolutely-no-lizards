using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuardLogic : MonoBehaviour {

	public Slider healthBarSlider;  //reference for slider
	public Transform sightStart, sightEnd, rayStart, rayEnd; //2 transforms for start and end points for the linecast
	public GameObject arrow; //arrow above the guards head for alerted status
	public float damageOutput = .00125f;
	public bool spotted, facingLeft; //bool for if player is spotted, bool for if guard is facing left so we can rotate his sprite based on his facing direction

	void Start ()
	{
		InvokeRepeating("Patrol", 0f, Random.Range (2f, 4f)); //we use InvokeRepeating to call a function - it takes ("FunctionName", startTime, repeatTime) it loops forever until you call 'CancelInvokes'
	}
	
	// Update is called once per frame
	void Update () 
	{
		RaycastStuff();
		Behaviours();
	}

	void RaycastStuff() //Keep our raycast code tidy in here
	{
		Debug.DrawLine(sightStart.position, sightEnd.position, Color.magenta); //just a visual representation for the linecast
		spotted = Physics2D.Linecast(sightStart.position, sightEnd.position, 1 << LayerMask.NameToLayer("Player")); //we assign the bool 'spotted' with a linecast, that returns true or false when it touches the Player
	}

	void Behaviours()
	{
		if(spotted) //checking if spotted bool is true
		{
			arrow.SetActive(true); //SetActive toggles the gameobject on and off in the scene

			Debug.DrawLine(rayStart.position, rayEnd.position, Color.red);
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
}
