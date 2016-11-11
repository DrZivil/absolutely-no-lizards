using UnityEngine;
using System.Collections;

public class umanMovement : MonoBehaviour {

	// Use this for initialization
	Rigidbody2D rig2d;
	Animator anim;
	Transform enemy;
	float horizontal;
	public int maxSpeed =70 ;
	Vector3 movement;
	public float atackRate = 0.3f;
	bool attack;
	float attackTimere = 0f;
	int  timePressed = 0 ;
	public bool damage;
	public float noDamage = 1;
	float noDamageTimer;
	void Start () {
		rig2d = GetComponent<Rigidbody2D> ();
		anim = GetComponentInChildren<Animator> ();
		GameObject[] players = GameObject.FindGameObjectsWithTag ("lizzard");
		foreach (GameObject pl in players) {
			if (pl.transform != this.transform) {
				enemy = pl.transform;
			}
		}
	}
	void Update(){
		ScaleCheck ();
		Attack ();
		Damage ();
		UpdateAnimator ();
	}
	void UpdateAnimator(){
		anim.SetFloat ("Movement",Mathf.Abs(horizontal));
		anim.SetBool ("Attack1",attack);
	}
	void FixedUpdate () {
		horizontal = Input.GetAxis ("Horizontal");

		Vector3 movement = new Vector3 (horizontal,0,0);

		rig2d.AddForce (movement * maxSpeed);
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

	void Attack(){
		if(	Input.GetButton("punch")){
			attack = true;
			attackTimere = 0;
			timePressed++;
		}
		if(attack){
			attackTimere += Time.deltaTime;

			if(attackTimere > atackRate || timePressed >= 4){
				attackTimere = 0;
				attack = false;
				timePressed = 0;
			}
		}
	}
}
