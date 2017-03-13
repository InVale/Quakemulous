using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketWithSpeed : MonoBehaviour
{


	[Header ("Tir 1 (speed 10)")]
	public GameObject T1_Rocket;
	public float T1_RocketFireSpeed = 1;
	public float T1_RocketSpeed = 10;
	public float T1_RocketExplosionRadius = 1;
	public float T1_RocketKnockback = 10;
	public float T1_RocketDammage = 100;

	[Header ("Tir 2 (speed 8)")]
	public GameObject T2_Rocket;
	public float T2_RocketFireSpeed = 1;
	public float T2_RocketSpeed = 10;
	public float T2_RocketExplosionRadius = 1;
	public float T2_RocketKnockback = 10;
	public float T2_RocketDammage = 100;

	[Header ("Tir 3 (speed 0)")]
	public GameObject T3_Rocket;
	public float T3_RocketFireSpeed = 1;
	public float T3_RocketSpeed = 10;
	public float T3_RocketExplosionRadius = 1;
	public float T3_RocketKnockback = 10;
	public float T3_RocketDammage = 100;

	float theSpeed;
	Interactions playerInteract;

	void Start ()
	{
		{
			playerInteract = this.GetComponent<Interactions> ();
		}
	}

	void Update ()
	{
		theSpeed = this.GetComponent<CCC> ().CurrentSpeed;

		if (theSpeed >= 9) {
			//Debug.Log ("tir1"); 

			playerInteract.RocketPrefab = T1_Rocket;

			playerInteract.RocketFireSpeed = T1_RocketFireSpeed;
			playerInteract.RocketSpeed = T1_RocketSpeed;
			playerInteract.RocketExplosionRadius = T1_RocketExplosionRadius;
			playerInteract.RocketKnockback = T1_RocketKnockback;
			playerInteract.RocketDammage = T1_RocketDammage;

		}

		if ((theSpeed < 9) && (theSpeed > 1)) {
			//Debug.Log ("tir2"); 

			playerInteract.RocketPrefab = T2_Rocket;

			playerInteract.RocketFireSpeed = T2_RocketFireSpeed;
			playerInteract.RocketSpeed = T2_RocketSpeed;
			playerInteract.RocketExplosionRadius = T2_RocketExplosionRadius;
			playerInteract.RocketKnockback = T2_RocketKnockback;
			playerInteract.RocketDammage = T2_RocketDammage;
		}

		if (theSpeed <= 1) {
			//Debug.Log ("tir3"); 

			playerInteract.RocketPrefab = T3_Rocket;

			playerInteract.RocketFireSpeed = T3_RocketFireSpeed;
			playerInteract.RocketSpeed = T3_RocketSpeed;
			playerInteract.RocketExplosionRadius = T3_RocketExplosionRadius;
			playerInteract.RocketKnockback = T3_RocketKnockback;
			playerInteract.RocketDammage = T3_RocketDammage;
		}
	}
}
