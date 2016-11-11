using UnityEngine;
using System.Collections;

public class lizzardMovement : MonoBehaviour {

	// Use this for initialization
	Rigidbody2D rig2d;
	Animator anim;
	Transform enemy;
	float horizontal;
	public int maxSpeed =70 ;
	Vector3 movement;
	int counter = 0;

    public bool dead = false;

	public bool damage;
	public float noDamage = 1;
	float noDamageTimer;
	void Start () {
		rig2d = GetComponent<Rigidbody2D> ();
		anim = GetComponentInChildren<Animator> ();
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject pl in players) {
			if (pl.transform != this.transform) {
				enemy = pl.transform;
			}
		}
	}
	void Update(){
		ScaleCheck ();

        if (!dead)
        {
		    if(counter%20 == 0){
			    counter = 0;
			    horizontal = Mathf.Sin(Random.Range(-1f,1f));	
		    }

		    Damage ();
		    UpdateAnimator ();

		    counter++;
        
        }
	}
	void UpdateAnimator(){
		anim.SetFloat ("Movement",Mathf.Abs(horizontal));
	}
	void FixedUpdate () {
        //horizontal = Input.GetAxis ("Horizontal");

        if (!dead)
        {
            Vector3 movement = new Vector3 (horizontal,0,0);

		    rig2d.AddForce (movement * maxSpeed);
	    }
    }

    void ScaleCheck(){
		if (transform.position.x < enemy.position.x)
			transform.localScale = new Vector3 (-1, 1, 1);
		else
			transform.localScale = Vector3.one;
	}
	void Damage(){
		noDamageTimer += Time.deltaTime;
		if (damage) {
			if (noDamageTimer > noDamage) {
				movement = Vector3.zero;
				damage = false;
				noDamageTimer = 0;
			}
		}

	}
}
