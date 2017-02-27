using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

	public Transform Contact;
	public LayerMask HardObjects;

	[HideInInspector]
	public float ExplosionRadius = 1;
	[HideInInspector]
	public float Knockback = 1;
	[HideInInspector]
	public float Dammage = 1;
	[HideInInspector]
	public GameObject ID;

	void OnCollisionEnter (Collision Collision) {

		if (Collision.collider.tag == "Door") {
			Collision.collider.transform.parent.parent.parent.GetComponent<Door>().OpenDoor();
		}
		else {
			Collider[] _sphereHit = Physics.OverlapSphere (Contact.position, ExplosionRadius);
			if (_sphereHit.Length > 0) 
			{
				for (int i = 0; i < _sphereHit.Length; i++)
				{
					if (_sphereHit[i].tag == "Player" && _sphereHit[i].gameObject != ID) 
					{
						Vector3 _originToPoint = _sphereHit[i].transform.position - Contact.position;
						RaycastHit Hit;
						Physics.Raycast(Contact.position, _originToPoint, out Hit);
						if (HardObjects == (HardObjects | (1 << Hit.collider.gameObject.layer))) {
							float _knockbackForce  = Knockback * ((ExplosionRadius - _originToPoint.magnitude) / ExplosionRadius);
							Vector3 _imprimedKnockback = _knockbackForce * _originToPoint.normalized;
							float _appliedDammage = Dammage * ((ExplosionRadius - _originToPoint.magnitude) / ExplosionRadius);
							_sphereHit[i].gameObject.GetComponent<CCC>().TakeKnockback(_imprimedKnockback, _appliedDammage);
						}
					}
				}
			}
		}

		Destroy(transform.gameObject);
	}
}
